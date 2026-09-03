namespace FlatCopy.FileSystemServices;

public record FlatCopyParams(string Name, SearchParams SearchParams, CopyParams CopyParams, string DestDirectory);

public interface IFlatCopyService
{
    List<string> FlatCopy(FlatCopyParams flatCopyParams);

    long DeleteExtraFiles(IEnumerable<string> files, string path, string searchPattern);
}