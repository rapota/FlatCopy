namespace FlatCopy;

public record FlatCopyParams(string Name, CopyParams CopyParams, SearchParams SearchParams, string DestDirectory);

public interface IFlatCopyService
{
    List<string> FlatCopy(FlatCopyParams flatCopyParams);

    long DeleteExtraFiles(IEnumerable<string> files, string path, string searchPattern);
}