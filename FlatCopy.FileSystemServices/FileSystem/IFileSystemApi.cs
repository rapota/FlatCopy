namespace FlatCopy.FileSystemServices.FileSystem;

public record FileInformation(DateTimeOffset LastWriteTimeUtc, long Length);

internal interface IFileSystemApi
{
    bool FileExists(string path);

    bool DirectoryExists(string path);

    void CreateDirectory(string path);

    FileInformation GetFileInformation(string path);

    IEnumerable<string> EnumerateFiles(string path, string searchPattern);

    void CreateHardLink(string fileName, string existingFileName);

    void CopyFile(string sourceFileName, string destFileName);

    void CopyFile(string sourceFileName, string destFileName, bool overwrite);

    void DeleteFile(string filePath);
}