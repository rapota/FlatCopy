using FlatCopy.Settings;

namespace FlatCopy;

public interface IFileService
{
    bool DirectoryExists(string path);

    IEnumerable<string> EnumerateFiles(string path, string searchPattern);

    void Copy(string sourceFileName, string destFileName, OverwriteOption overwrite, bool createHardLinks);

    int DeleteExtraFiles(IEnumerable<string> files, string targetFolder, bool isParallel);
}