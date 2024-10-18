using FlatCopy.Settings;
using System.Xml.Linq;

namespace FlatCopy;

public static class FlatCopyParamsHelper
{
    public static FlatCopyParams ToFlatCopyParams(string name, CopyOptions copyOptions, CopySource copySource)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);
        ArgumentException.ThrowIfNullOrEmpty(copyOptions.TargetFolder);

        return new FlatCopyParams(
            name,
            ToCopyParams(copyOptions, copySource),
            ToSearchParams(copyOptions, copySource),
            copyOptions.TargetFolder);
    }

    private static SearchParams ToSearchParams(CopyOptions copyOptions, CopySource copySource)
    {
        ArgumentException.ThrowIfNullOrEmpty(copySource.SourceFolder);
        ArgumentNullException.ThrowIfNull(copyOptions.SearchPattern);
        ArgumentNullException.ThrowIfNull(copyOptions.SkipExtensions);

        return new SearchParams(
            copySource.SourceFolder,
            copySource.SearchPattern ?? copyOptions.SearchPattern,
            copySource.SkipExtensions ?? copyOptions.SkipExtensions,
            copySource.SubFoldersOnly ?? [],
            copySource.SkipSubFolders ?? []);
    }

    private static CopyParams ToCopyParams(CopyOptions copyOptions, CopySource copySource) =>
        new(
            copySource.CreateHardLinks ?? copyOptions.CreateHardLinks,
            copySource.Overwrite ?? copyOptions.Overwrite);
}