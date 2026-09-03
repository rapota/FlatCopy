namespace FlatCopy.FileSystemServices;

public enum OverwriteParams
{
    No,
    Newer,
    Yes
}

public record CopyParams(bool CreateHardLinks, OverwriteParams Overwrite);

internal interface IFileCopyService
{
    void CopyFile(string sourceFile, string destFileName, CopyParams copyParams);
}