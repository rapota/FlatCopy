using Microsoft.Extensions.Logging;
using System.IO.Compression;

namespace FlatCopy.FileSystemServices.FileSystem;

internal sealed class ArchiveCopyService(IFileSystemApi _fileSystemApi, ILogger<ArchiveCopyService> _logger) : IArchiveCopyService
{
    public List<string> ExtractFiles(string archivePath, string destFileName, OverwriteParams overwrite)
    {
        List<string> result = new List<string>(250);

        using ZipArchive archive = ZipFile.OpenRead(archivePath);
        foreach (ZipArchiveEntry entry in archive.Entries)
        {
            if (string.IsNullOrEmpty(entry.Name))
            {
                continue;
            }

            string fileName = entry.FullName.Replace('/', '_');
            string resultFileName = destFileName + "_" + fileName;

            CopyFile(entry, resultFileName, overwrite);

            result.Add(resultFileName);
        }

        return result;
    }

    private void CopyFile(ZipArchiveEntry entry, string destFileName, OverwriteParams overwrite)
    {
        if (overwrite == OverwriteParams.No)
        {
            if (!_fileSystemApi.FileExists(destFileName))
            {
                entry.ExtractToFile(destFileName);
                _logger.LogInformation("File extracted to {path}", destFileName);
            }
        }
        else if (overwrite == OverwriteParams.Newer)
        {
            if (_fileSystemApi.FileExists(destFileName))
            {
                FileInformation sourceInformation = new FileInformation(entry.LastWriteTime, entry.Length);
                FileInformation destInformation = _fileSystemApi.GetFileInformation(destFileName);

                if (sourceInformation.LastWriteTimeUtc > destInformation.LastWriteTimeUtc
                    || sourceInformation.Length != destInformation.Length)
                {
                    entry.ExtractToFile(destFileName, overwrite: true);
                    _logger.LogInformation("File extracted to {path}", destFileName);
                }
                else
                {
                    _logger.LogDebug("Skipped file {path}", destFileName);
                }
            }
            else
            {
                entry.ExtractToFile(destFileName);
                _logger.LogInformation("File extracted to {path}", destFileName);
            }
        }
        else if (overwrite == OverwriteParams.Yes)
        {
            entry.ExtractToFile(destFileName, overwrite: true);
            _logger.LogInformation("File extracted to {path}", destFileName);
        }
    }
}