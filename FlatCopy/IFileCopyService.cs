using FlatCopy.Settings;

namespace FlatCopy;

public interface IFileCopyService
{
    void CopyFile(string sourceFile, string destFileName, bool createHardLinks, OverwriteOption overwrite);
}