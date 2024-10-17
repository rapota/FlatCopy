using Microsoft.Extensions.Logging;

namespace FlatCopy.FileSystem;

internal sealed class FileSystemApi(ILogger<FileSystemApi> _logger) : IFileSystemApi
{
    public bool FileExists(string path) =>
        File.Exists(path);

    public bool DirectoryExists(string path) =>
        Directory.Exists(path);
    
    public void CreateDirectory(string path) =>
        Directory.CreateDirectory(path);

    public FileInformation GetFileInformation(string fileName)
    {
        FileInfo fileInfo = new(fileName);
        return new FileInformation(fileInfo.LastWriteTimeUtc, fileInfo.Length);
    }

    public IEnumerable<string> EnumerateFiles(string path, string searchPattern) =>
        Directory.EnumerateFiles(path, searchPattern, SearchOption.AllDirectories);

    public void CreateHardLink(string fileName, string existingFileName)
    {
        FileSystemFunctions.CreateHardLink(fileName, existingFileName);
        _logger.LogInformation("Created hard link at {path}", fileName);
    }

    public void CopyFile(string sourceFileName, string destFileName)
    {
        File.Copy(sourceFileName, destFileName);
        _logger.LogInformation("File copied to {path}", destFileName);
    }

    public void CopyFile(string sourceFileName, string destFileName, bool overwrite)
    {
        File.Copy(sourceFileName, destFileName, overwrite);
        _logger.LogInformation("File copied to {path}", destFileName);
    }

    public void DeleteFile(string filePath)
    {
        FileInfo fileInfo = new(filePath);
        if (fileInfo.IsReadOnly)
        {
            fileInfo.IsReadOnly = false;
        }

        fileInfo.Delete();

        _logger.LogInformation("File deleted at {path}", filePath);
    }
}