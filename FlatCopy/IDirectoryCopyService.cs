namespace FlatCopy;

public record DirectoryCopyParams(SearchParams SearchParams, CopyParams CopyParams, string DestDirectory);

public interface IDirectoryCopyService
{
    string[] CopyDirectory(DirectoryCopyParams directoryCopyParams, params string[] customPrefixes);
}