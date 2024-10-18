using FlatCopy.FileSystem;
using FlatCopy.Settings;
using Microsoft.Extensions.Logging;

namespace FlatCopy;

public sealed class FileCopyService(IFileSystemApi _fileSystemApi, ILogger<FileCopyService> _logger) : IFileCopyService
{
    public void CopyFile(string sourceFile, string destFileName, CopyParams copyParams) =>
        CopyFile(sourceFile, destFileName, copyParams.CreateHardLinks, copyParams.Overwrite);

    private void CopyFile(string sourceFile, string destFileName, bool createHardLinks, OverwriteOption overwrite)
    {
        if (createHardLinks)
        {
            CreateHardLink(sourceFile, destFileName, overwrite);
        }
        else
        {
            CopyFile(sourceFile, destFileName, overwrite);
        }
    }

    private void CreateHardLink(string sourceFile, string destFileName, OverwriteOption overwrite)
    {
        if (overwrite == OverwriteOption.No)
        {
            if (!_fileSystemApi.FileExists(destFileName))
            {
                _fileSystemApi.CreateHardLink(destFileName, sourceFile);
            }
        }
        else if (overwrite == OverwriteOption.Newer)
        {
            if (_fileSystemApi.FileExists(destFileName))
            {
                FileInformation sourceInformation = _fileSystemApi.GetFileInformation(sourceFile);
                FileInformation destInformation = _fileSystemApi.GetFileInformation(destFileName);

                if (sourceInformation.LastWriteTimeUtc > destInformation.LastWriteTimeUtc
                    || sourceInformation.Length != destInformation.Length)
                {
                    _fileSystemApi.DeleteFile(destFileName);
                    _fileSystemApi.CreateHardLink(destFileName, sourceFile);
                }
                else
                {
                    _logger.LogDebug("Skipped file {path}", destFileName);
                }
            }
            else
            {
                _fileSystemApi.CreateHardLink(destFileName, sourceFile);
            }
        }
        else if (overwrite == OverwriteOption.Yes)
        {
            if (_fileSystemApi.FileExists(destFileName))
            {
                _fileSystemApi.DeleteFile(destFileName);
            }

            _fileSystemApi.CreateHardLink(destFileName, sourceFile);
        }
    }

    private void CopyFile(string sourceFile, string destFileName, OverwriteOption overwrite)
    {
        if (overwrite == OverwriteOption.No)
        {
            if (!_fileSystemApi.FileExists(destFileName))
            {
                _fileSystemApi.CopyFile(sourceFile, destFileName);
            }
        }
        else if (overwrite == OverwriteOption.Newer)
        {
            if (_fileSystemApi.FileExists(destFileName))
            {
                FileInformation sourceInformation = _fileSystemApi.GetFileInformation(sourceFile);
                FileInformation destInformation = _fileSystemApi.GetFileInformation(destFileName);

                if (sourceInformation.LastWriteTimeUtc > destInformation.LastWriteTimeUtc
                    || sourceInformation.Length != destInformation.Length)
                {
                    _fileSystemApi.CopyFile(sourceFile, destFileName, true);
                }
                else
                {
                    _logger.LogDebug("Skipped file {path}", destFileName);
                }
            }
            else
            {
                _fileSystemApi.CopyFile(sourceFile, destFileName);
            }
        }
        else if (overwrite == OverwriteOption.Yes)
        {
            _fileSystemApi.CopyFile(sourceFile, destFileName, true);
        }
    }
}
