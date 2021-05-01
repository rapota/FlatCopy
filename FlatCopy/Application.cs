using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace FlatCopy
{
    internal class Application
    {
        private readonly ILogger<Application> _logger;
        private readonly CopyOptions _options;
        private readonly FileService _fileService;

        public Application(ILogger<Application> logger, CopyOptions options, FileService fileService)
        {
            _logger = logger;
            _options = options;
            _fileService = fileService;
        }

        public void Run()
        {
            Stopwatch sw = Stopwatch.StartNew();
            List<string> copiedFiles = CopyFiles();
            sw.Stop();

            Stopwatch swd = Stopwatch.StartNew();
            int count = DeleteExtraFiles(copiedFiles);
            swd.Stop();

            _logger.LogInformation("Processed {count} files for {elapsed}", copiedFiles.Count, sw.Elapsed);
            _logger.LogInformation("Deleted {count} extra files for {elapsed}", count, swd.Elapsed);
        }
        private static string CalculateTargetFile(string filePath, string sourceFolder, string targetFolder)
        {
            string relativeName = filePath.Remove(0, sourceFolder.Length);

            string normalizedName = relativeName
                .TrimStart(Path.DirectorySeparatorChar)
                .Replace(Path.DirectorySeparatorChar, '_');

            return Path.Combine(targetFolder, normalizedName);
        }

        private static IEnumerable<(string SourceFile, string TargetFile)> GetPairsForFolder(string sourceFolder, string searchPattern, string targetFolder)
        {
            return Directory.EnumerateFiles(searchPattern, searchPattern, SearchOption.AllDirectories)
                .Select(x => (x, CalculateTargetFile(x, sourceFolder, targetFolder)));
        }

        private string[] CopyFolder(string sourceFolder)
        {
            if (!Directory.Exists(_options.TargetFolder))
            {
                _logger.LogWarning("Directory not found: {directory}", sourceFolder);
                return Array.Empty<string>();
            }

            IEnumerable<string> files = Directory.EnumerateFiles(sourceFolder, _options.SearchPattern, SearchOption.AllDirectories);
            if (_options.IsParallel)
            {
                files = files.AsParallel();
            }

            return files
                .Select(x =>
                {
                    string targetFile = CalculateTargetFile(x, sourceFolder, _options.TargetFolder);
                    _fileService.Copy(x, targetFile, _options.Overwrite, _options.CreateHardLinks);
                    return targetFile;
                })
                .ToArray();
        }

        private List<string> CopyFiles()
        {
            string[] folders = _options.SourceFolders.Split(';', StringSplitOptions.RemoveEmptyEntries);
            _logger.LogInformation("Source folders: {folders}", folders);

            if (!Directory.Exists(_options.TargetFolder))
            {
                Directory.CreateDirectory(_options.TargetFolder);
                _logger.LogInformation("Created target folder: {folder}", _options.TargetFolder);
            }

            IEnumerable<string> sourceFolders = folders;
            if (_options.IsParallel)
            {
                sourceFolders = sourceFolders.AsParallel();
            }

            return sourceFolders
                .SelectMany(CopyFolder)
                .ToList();
        }

        private int DeleteExtraFiles(IEnumerable<string> files)
        {
            HashSet<string> resultSet = files.ToHashSet();

            bool TryDeleteExtraFile(string filePath)
            {
                if (!resultSet.Contains(filePath))
                {
                    File.Delete(filePath);
                    _logger.LogInformation("Deleted extra file {path}", filePath);
                    return true;
                }

                return false;
            }

            IEnumerable<string> results = Directory.EnumerateFiles(_options.TargetFolder, _options.SearchPattern, SearchOption.TopDirectoryOnly);
            if (_options.IsParallel)
            {
                return results
                    .AsParallel()
                    .Select(TryDeleteExtraFile)
                    .Count(x => x);
            }

            return results
                .Select(TryDeleteExtraFile)
                .Count(x => x);
        }
    }
}
