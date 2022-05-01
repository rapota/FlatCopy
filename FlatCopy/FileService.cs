using Microsoft.Extensions.Logging;

namespace FlatCopy;

public class FileService
{
    private readonly ILogger<FileService> _logger;

    public FileService(ILogger<FileService> logger)
    {
        _logger = logger;
    }

    public void Copy(string sourceFileName, string destFileName, OverwriteOption overwrite, bool createHardLinks)
    {
        if (overwrite == OverwriteOption.Yes)
        {
            CopyInternal(sourceFileName, destFileName, true, createHardLinks);
        }
        else if (overwrite == OverwriteOption.Newer)
        {
            if (File.Exists(destFileName))
            {
                DateTime sourceTime = File.GetLastWriteTimeUtc(sourceFileName);
                DateTime destTime = File.GetLastWriteTimeUtc(destFileName);
                if (sourceTime > destTime)
                {
                    CopyInternal(sourceFileName, destFileName, true, createHardLinks);
                    _logger.LogInformation("Overwritten file to {path}", destFileName);
                }
                else
                {
                    _logger.LogDebug("Skipped file {path}", destFileName);
                }
            }
            else
            {
                CopyInternal(sourceFileName, destFileName, false, createHardLinks);
                _logger.LogInformation("Copied file to {path}", destFileName);
            }
        }
        else
        {
            if (File.Exists(destFileName))
            {
                _logger.LogDebug("Skipped file {path}", destFileName);
            }
            else
            {
                CopyInternal(sourceFileName, destFileName, false, createHardLinks);
                _logger.LogInformation("Copied file to {path}", destFileName);
            }
        }
    }

    public int DeleteExtraFiles(IEnumerable<string> files, string targetFolder, bool isParallel)
    {
        HashSet<string> resultSet = files.ToHashSet();

        bool TryDeleteExtraFile(string filePath)
        {
            if (!resultSet.Contains(filePath))
            {
                DeleteFileInternal(filePath);
                _logger.LogInformation("Deleted extra file {path}", filePath);
                return true;
            }

            return false;
        }

        IEnumerable<string> results = Directory.EnumerateFiles(targetFolder);
        if (isParallel)
        {
            results = results.AsParallel();
        }

        return results
            .Select(TryDeleteExtraFile)
            .Count(x => x);
    }

    private static void CopyInternal(string sourceFileName, string destFileName, bool overwrite, bool createHardLinks)
    {
        if (createHardLinks)
        {
            FileManagementFunctions.CreateHardLink(destFileName, sourceFileName);
        }
        else
        {
            File.Copy(sourceFileName, destFileName, overwrite);
        }
    }

    private static void DeleteFileInternal(string filePath)
    {
        FileInfo fileInfo = new FileInfo(filePath);
        if (fileInfo.IsReadOnly)
        {
            fileInfo.IsReadOnly = false;
        }

        fileInfo.Delete();
    }
}