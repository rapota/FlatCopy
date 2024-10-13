using FlatCopy.Settings;
using Microsoft.Extensions.Logging;

namespace FlatCopy;

public sealed class CopyService(ILogger<CopyService> _logger, IFileService _fileService)
{
    public string[] Copy(CopyTask copyTask)
    {
        HashSet<string> skipExtensions = copyTask.SkipExtensions.ToHashSet(StringComparer.OrdinalIgnoreCase);

        if (copyTask.SubFoldersOnly.Length == 0)
        {
            return CopyFolder(copyTask.SourceFolder, copyTask.SearchPattern, "");
        }

        List<string> results = new(10000);
        foreach (string subfolder in copyTask.SubFoldersOnly)
        {
            string subfolderPath = Path.Combine(copyTask.SourceFolder, subfolder);
            string[] result = CopyFolder(subfolderPath, copyTask.SearchPattern, "");
            results.AddRange(result);
        }

        return results.ToArray();
    }

    public string[] CopyFolder(string sourceFolder, string searchPattern, string targetFolder)
    {
        if (!_fileService.DirectoryExists(sourceFolder))
        {
            _logger.LogWarning("Directory not found: {directory}", sourceFolder);
            return [];
        }

        List<string> results = new(5000);

        IEnumerable<string> files = _fileService.EnumerateFiles(sourceFolder, searchPattern);
        foreach (string filePath in files)
        {
            string targetFile = CalculateTargetFile(filePath, sourceFolder, targetFolder);
            _fileService.Copy(filePath, targetFile, OverwriteOption.No, false);

            results.Add(targetFile);
        }

        return results.ToArray();
    }

    public static string CalculateTargetFile(string filePath, string sourceFolder, string targetFolder)
    {
        sourceFolder = Path.TrimEndingDirectorySeparator(sourceFolder);
        string relativePath = Path.GetRelativePath(sourceFolder, filePath);

        string directoryName = Path.GetFileName(sourceFolder);
        string normalizedName = directoryName + "_" + relativePath.Replace(Path.DirectorySeparatorChar, '_');

        return Path.Combine(targetFolder, normalizedName);
    }
}