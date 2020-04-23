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

        public Application(ILogger<Application> logger, CopyOptions options)
        {
            _logger = logger;
            _options = options;
        }

        public void Run()
        {
            _logger.LogInformation("Source: {path}", _options.SourceFolder);

            var sw = Stopwatch.StartNew();
            List<string> copiedFiles = CopyFiles();
            sw.Stop();

            var swd = Stopwatch.StartNew();
            int count = DeleteExtraFiles(copiedFiles);
            swd.Stop();

            _logger.LogInformation("Processed {count} files for {elapsed}", copiedFiles.Count, sw.Elapsed);
            _logger.LogInformation("Deleted {count} extra files for {elapsed}", count, swd.Elapsed);
        }

        private List<string> CopyFiles()
        {
            if (!Directory.Exists(_options.TargetFolder))
            {
                Directory.CreateDirectory(_options.TargetFolder);
            }

            IEnumerable<string> files = Directory.EnumerateFiles(_options.SourceFolder, _options.SearchPattern, SearchOption.AllDirectories);
            return _options.IsParallel
                ? files.AsParallel().Select(ProcessFile).ToList()
                : files.Select(ProcessFile).ToList();
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
            return _options.IsParallel
                ? results.Select(TryDeleteExtraFile).Count(x => x)
                : results.AsParallel().Select(TryDeleteExtraFile).Count(x => x);
        }

        protected void CopyInternal(string sourceFileName, string destFileName, bool overwrite)
        {
            if (_options.CreateHardLinks)
            {
                FileManagementFunctions.CreateHardLink(destFileName, sourceFileName);
            }
            else
            {
                File.Copy(sourceFileName, destFileName, overwrite);
            }
        }

        protected void CopyFile(string sourceFileName, string destFileName)
        {
            if (_options.Overwrite)
            {
                CopyInternal(sourceFileName, destFileName, true);
            }
            else
            {
                if (File.Exists(destFileName))
                {
                    _logger.LogDebug("Skipped file {path}", destFileName);
                }
                else
                {
                    CopyInternal(sourceFileName, destFileName, false);
                    _logger.LogInformation("Copied file to {path}", destFileName);
                }
            }
        }

        private string ProcessFile(string filePath)
        {
            string relativeName = filePath.Remove(0, _options.SourceFolder.Length);

            string normalizedName = relativeName
                .TrimStart(Path.DirectorySeparatorChar)
                .Replace(Path.DirectorySeparatorChar, '_');

            string targetFile = Path.Combine(_options.TargetFolder, normalizedName);

            CopyFile(filePath, targetFile);

            return targetFile;
        }
    }
}
