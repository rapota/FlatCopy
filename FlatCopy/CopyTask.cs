using FlatCopy.Settings;

namespace FlatCopy;

public sealed class CopyTask
{
    public string Name { get; }
    
    public string SourceFolder { get; }

    public string SearchPattern { get; }

    public string[] SkipExtensions { get; }

    public OverwriteOption Overwrite { get; }

    public bool CreateHardLinks { get; }

    public string[] SubFoldersOnly { get; }

    public string[] SkipSubFolders { get; }

    public bool CopyParallel { get; }

    private CopyTask(string name, string sourceFolder, string searchPattern, string[] skipExtensions, OverwriteOption overwrite, bool createHardLinks, string[] subFoldersOnly, string[] skipSubFolders, bool copyParallel)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);
        ArgumentException.ThrowIfNullOrEmpty(sourceFolder);
        ArgumentException.ThrowIfNullOrEmpty(searchPattern);
        ArgumentNullException.ThrowIfNull(skipExtensions);
        ArgumentNullException.ThrowIfNull(subFoldersOnly);
        ArgumentNullException.ThrowIfNull(skipSubFolders);

        Name = name;
        SourceFolder = sourceFolder;
        SearchPattern = searchPattern;
        SkipExtensions = skipExtensions;
        Overwrite = overwrite;
        CreateHardLinks = createHardLinks;
        SubFoldersOnly = subFoldersOnly;
        SkipSubFolders = skipSubFolders;
        CopyParallel = copyParallel;
    }

    public static CopyTask FromOptions(string name, CopyOptions copyOptions, CopySource copySource) =>
        new(
            name,
            copySource.SourceFolder,
            copySource.SearchPattern ?? copyOptions.SearchPattern,
            copySource.SkipExtensions ?? copyOptions.SkipExtensions,
            copySource.Overwrite ?? copyOptions.Overwrite,
            copySource.CreateHardLinks ?? copyOptions.CreateHardLinks,
            copySource.SubFoldersOnly ?? [],
            copySource.SkipSubFolders ?? [],
            copySource.CopyParallel ?? copyOptions.IsParallel);
}