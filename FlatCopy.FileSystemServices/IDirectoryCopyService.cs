namespace FlatCopy.FileSystemServices;

public record DirectoryCopyParams(SearchParams SearchParams, CopyParams CopyParams, string DestDirectory);

internal interface IDirectoryCopyService
{
    List<string> CopyDirectory(DirectoryCopyParams directoryCopyParams, params string[] customPrefixes);
}