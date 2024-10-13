namespace FlatCopy.FileSystem;

public interface IFileSystemApi
{
    bool DirectoryExists(string path);

    IEnumerable<string> EnumerateFiles(string path, string searchPattern);

    void CreateHardLink(string fileName, string existingFileName);

    void CopyFile(string sourceFileName, string destFileName, bool overwrite);

    void DeleteFileInternal(string filePath);
}