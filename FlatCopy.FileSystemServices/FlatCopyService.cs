using FlatCopy.FileSystemServices.FileSystem;
using Microsoft.Extensions.Logging;

namespace FlatCopy.FileSystemServices;

internal sealed class FlatCopyService(
    IDirectoryCopyService _directoryCopyService,
    IFileSystemApi _fileSystemApi,
    ILogger<FlatCopyService> _logger)
    : IFlatCopyService
{
    public List<string> FlatCopy(FlatCopyParams flatCopyParams)
    {
        if (!_fileSystemApi.DirectoryExists(flatCopyParams.SearchParams.QueryParams.SearchPath))
        {
            _logger.LogWarning("Directory not found: {directory}", flatCopyParams.SearchParams.QueryParams.SearchPath);
            return [];
        }

        if (!_fileSystemApi.DirectoryExists(flatCopyParams.DestDirectory))
        {
            _fileSystemApi.CreateDirectory(flatCopyParams.DestDirectory);
            _logger.LogInformation("Created destination directory: {directory}", flatCopyParams.DestDirectory);
        }

        DirectoryCopyParams directoryCopyParams = new DirectoryCopyParams(flatCopyParams.SearchParams, flatCopyParams.CopyParams, flatCopyParams.DestDirectory);
        return _directoryCopyService.CopyDirectory(directoryCopyParams, flatCopyParams.Name);
    }

    public long DeleteExtraFiles(IEnumerable<string> files, string path, string searchPattern)
    {
        HashSet<string> resultSet = files.ToHashSet(StringComparer.OrdinalIgnoreCase);

        long result = 0;
        foreach (string filePath in _fileSystemApi.EnumerateFiles(path, searchPattern))
        {
            if (!resultSet.Contains(filePath))
            {
                _fileSystemApi.DeleteFile(filePath);
                result++;
            }
        }

        return result;
    }
}