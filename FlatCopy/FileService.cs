using System;
using System.IO;
using Microsoft.Extensions.Logging;

namespace FlatCopy
{
    internal class FileService
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

        private void CopyInternal(string sourceFileName, string destFileName, bool overwrite, bool createHardLinks)
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
    }
}