namespace FlatCopy.FileSystem;

internal sealed class FileSystemApi : IFileSystemApi
{
    public bool DirectoryExists(string path) =>
        Directory.Exists(path);

    public IEnumerable<string> EnumerateFiles(string path, string searchPattern) =>
        Directory.EnumerateFiles(path, searchPattern, SearchOption.AllDirectories);

    public void CreateHardLink(string fileName, string existingFileName) =>
        FileSystemFunctions.CreateHardLink(fileName, existingFileName);

    public void CopyFile(string sourceFileName, string destFileName, bool overwrite) =>
        File.Copy(sourceFileName, destFileName, overwrite);

    public void DeleteFileInternal(string filePath)
    {
        FileInfo fileInfo = new(filePath);
        if (fileInfo.IsReadOnly)
        {
            fileInfo.IsReadOnly = false;
        }

        fileInfo.Delete();
    }
}