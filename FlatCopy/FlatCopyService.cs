namespace FlatCopy;

public record FlatCopyParams(string Name, CopyParams CopyParams, SearchParams SearchParams, string[] SubFoldersOnly, string[] SkipSubFolders, string DestDirectory);

public sealed class FlatCopyService(IDirectoryCopyService _directoryCopyService)
{
    public string[] FlatCopy(FlatCopyParams flatCopyParams)
    {
        DirectoryCopyParams directoryCopyParams = new DirectoryCopyParams(flatCopyParams.SearchParams, flatCopyParams.CopyParams, flatCopyParams.DestDirectory);

        if (flatCopyParams.SubFoldersOnly.Length > 0)
        {
            return CopySubFoldersOnly(flatCopyParams.Name, directoryCopyParams, flatCopyParams.SubFoldersOnly);
        }

        if (flatCopyParams.SkipSubFolders.Length > 0)
        {
            return CopySkipSubFolders(flatCopyParams.Name, directoryCopyParams, flatCopyParams.SkipSubFolders);
        }

        return _directoryCopyService.CopyDirectory(directoryCopyParams, flatCopyParams.Name);
    }

    private string[] CopySubFoldersOnly(string name, DirectoryCopyParams directoryCopyParams, string[] subFoldersOnly)
    {
        return [];
    }
    
    private string[] CopySkipSubFolders(string name, DirectoryCopyParams directoryCopyParams, string[] skipSubFolders)
    {
        return [];
    }
}