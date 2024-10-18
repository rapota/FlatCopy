using FlatCopy.Settings;

namespace FlatCopy;

public record CopyParams(bool CreateHardLinks, OverwriteOption Overwrite);

public interface IFileCopyService
{
    void CopyFile(string sourceFile, string destFileName, CopyParams copyParams);
}