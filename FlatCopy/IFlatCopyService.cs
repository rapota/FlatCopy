namespace FlatCopy;

public record FlatCopyParams(string Name, CopyParams CopyParams, SearchParams SearchParams, string DestDirectory);

public interface IFlatCopyService
{
    string[] FlatCopy(FlatCopyParams flatCopyParams);
}