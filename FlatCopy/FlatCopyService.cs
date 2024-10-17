using FlatCopy.FileSystem;
using Microsoft.Extensions.Logging;

namespace FlatCopy;

public sealed class FlatCopyService(IDirectoryCopyService _directoryCopyService, IFileSystemApi _fileSystemApi, ILogger<FlatCopyService> _logger) : IFlatCopyService
{
    public string[] FlatCopy(FlatCopyParams flatCopyParams)
    {
        if (!_fileSystemApi.DirectoryExists(flatCopyParams.SearchParams.SourceFolder))
        {
            _logger.LogWarning("Directory not found: {directory}", flatCopyParams.SearchParams.SourceFolder);
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
}