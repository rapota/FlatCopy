namespace FlatCopy;

public record DirectoryCopyParams(SearchParams SearchParams, CopyParams CopyParams, string DestDirectory);

public interface IDirectoryCopyService
{
    List<string> CopyDirectory(DirectoryCopyParams directoryCopyParams, params string[] customPrefixes);
}