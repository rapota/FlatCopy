using FlatCopy.Settings;

namespace FlatCopy;

public static class FlatCopyParamsHelper
{
    public static FlatCopyParams ToFlatCopyParams(string name, CopyOptions copyOptions, CopySource copySource) =>
        new(
            name,
            ToCopyParams(copyOptions, copySource),
            ToSearchParams(copyOptions, copySource),
            copySource.SubFoldersOnly ?? [],
            copySource.SkipSubFolders ?? []);

    private static SearchParams ToSearchParams(CopyOptions copyOptions, CopySource copySource) =>
        new(
            copySource.SourceFolder,
            copySource.SearchPattern ?? copyOptions.SearchPattern,
            copySource.SkipExtensions ?? copyOptions.SkipExtensions);

    private static CopyParams ToCopyParams(CopyOptions copyOptions, CopySource copySource) =>
        new(
            copySource.CreateHardLinks ?? copyOptions.CreateHardLinks,
            copySource.Overwrite ?? copyOptions.Overwrite);
}