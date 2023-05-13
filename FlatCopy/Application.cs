using System.Diagnostics;
using FlatCopy.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FlatCopy;

public class Application
{
    private readonly ILogger<Application> _logger;
    private readonly CopyOptions _options;
    private readonly FileService _fileService;

    public Application(ILogger<Application> logger, IOptions<CopyOptions> options, FileService fileService)
    {
        _logger = logger;
        _options = options.Value;
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

    private string[] CopyFolder(string sourceFolder, HashSet<string> skipExtensions, List<string> skipFolders)
    {
        if (!Directory.Exists(sourceFolder))
        {
            _logger.LogWarning("Directory not found: {directory}", sourceFolder);
            return Array.Empty<string>();
        }

        bool ShouldCopy(string filePath)
        {
            foreach (string skipFolder in skipFolders)
            {
                if (filePath.StartsWith(skipFolder, StringComparison.InvariantCultureIgnoreCase))
                {
                    return false;
                }
            }

            string extension = Path.GetExtension(filePath);
            return !skipExtensions.Contains(extension);
        }

        string CopyToTarget(string filePath)
        {
            string targetFile = CalculateTargetFile(filePath, sourceFolder, _options.TargetFolder);
            _fileService.Copy(filePath, targetFile, _options.Overwrite, _options.CreateHardLinks);
            return targetFile;
        }

        IEnumerable<string> files = Directory.EnumerateFiles(sourceFolder, _options.SearchPattern, SearchOption.AllDirectories).Where(ShouldCopy);
        return _options.IsParallel
            ? files
                .AsParallel()
                .Select(CopyToTarget)
                .ToArray()
            : files
                .Select(CopyToTarget)
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
        List<string> skipFolders = _options.SkipFolders.Select(x =>
                x.EndsWith(Path.DirectorySeparatorChar)
                    ? x
                    : x + Path.DirectorySeparatorChar)
            .ToList();

        List<string> result = new(100000);
        foreach (string sourceFolder in _options.SourceFolders)
        {
            using IDisposable? scope = _logger.BeginScope(sourceFolder);

            string[] copiedFiles = CopyFolder(sourceFolder, skipExtensions, skipFolders);
            _logger.LogInformation("Copied {count} files.", copiedFiles.LongLength);
            result.AddRange(copiedFiles);
        }

        return result;
    }
}