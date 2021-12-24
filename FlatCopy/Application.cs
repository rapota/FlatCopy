using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace FlatCopy
{
    public class Application
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
            int count = _fileService.DeleteExtraFiles(copiedFiles, _options.TargetFolder, _options.IsParallel);
            swd.Stop();

            _logger.LogInformation("Processed {count} files for {elapsed}", copiedFiles.Count, sw.Elapsed);
            _logger.LogInformation("Deleted {count} extra files for {elapsed}", count, swd.Elapsed);
        }

        public static string CalculateTargetFile(string filePath, string sourceFolder, string targetFolder)
        {
            sourceFolder = Path.TrimEndingDirectorySeparator(sourceFolder);
            string relativePath = Path.GetRelativePath(sourceFolder, filePath);
            
            string directoryName = Path.GetFileName(sourceFolder);
            string normalizedName = directoryName + "_" + relativePath.Replace(Path.DirectorySeparatorChar, '_');

            return Path.Combine(targetFolder, normalizedName);
        }

        private string[] CopyFolder(string sourceFolder, HashSet<string> skipExtensions)
        {
            if (!Directory.Exists(sourceFolder))
            {
                _logger.LogWarning("Directory not found: {directory}", sourceFolder);
                return Array.Empty<string>();
            }

            bool ShouldCopy(string filePath)
            {
                string extension = Path.GetExtension(filePath);
                return !skipExtensions.Contains(extension);
            }

            IEnumerable<string> files = Directory.EnumerateFiles(sourceFolder, _options.SearchPattern, SearchOption.AllDirectories).Where(ShouldCopy);
            if (_options.IsParallel)
            {
                return files
                    .AsParallel()
                    .Select(x =>
                    {
                        string targetFile = CalculateTargetFile(x, sourceFolder, _options.TargetFolder);
                        _fileService.Copy(x, targetFile, _options.Overwrite, _options.CreateHardLinks);
                        return targetFile;
                    })
                    .ToArray();
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
            _logger.LogInformation("Source folders: {folders}", string.Join(';', _options.SourceFolders));

            if (!Directory.Exists(_options.TargetFolder))
            {
                Directory.CreateDirectory(_options.TargetFolder);
                _logger.LogInformation("Created target folder: {folder}", _options.TargetFolder);
            }

            HashSet<string> skipExtensions = _options.SkipExtensions.ToHashSet(StringComparer.OrdinalIgnoreCase);

            List<string> result = new List<string>(100000);
            foreach (string sourceFolder in _options.SourceFolders)
            {
                using IDisposable scope = _logger.BeginScope(sourceFolder);
                
                string[] copiedFiles = CopyFolder(sourceFolder, skipExtensions);
                _logger.LogInformation("Copied {count} files.", copiedFiles.LongLength);
                result.AddRange(copiedFiles);
            }

            return result;
        }
    }
}
