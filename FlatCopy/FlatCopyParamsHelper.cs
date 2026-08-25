using FlatCopy.FileSystemServices;
using FlatCopy.Settings;

namespace FlatCopy;

public static class FlatCopyParamsHelper
{
    public static OverwriteParams ToOverwriteParams(OverwriteSettings overwrite) =>
        overwrite switch
        {
            OverwriteSettings.No => OverwriteParams.No,
            OverwriteSettings.Newer => OverwriteParams.Newer,
            OverwriteSettings.Yes => OverwriteParams.Yes,
            _ => throw new ArgumentOutOfRangeException(nameof(overwrite), overwrite, null)
        };

    public static FlatCopyParams ToFlatCopyParams(string name, CopySettings copySettings, SourceSettings sourceSettings)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);
        ArgumentException.ThrowIfNullOrEmpty(copySettings.TargetFolder);

        return new FlatCopyParams(
            name,
            ToCopyParams(copySettings, sourceSettings),
            ToSearchParams(copySettings, sourceSettings),
            copySettings.TargetFolder);
    }

    private static SearchParams ToSearchParams(CopySettings copySettings, SourceSettings sourceSettings)
    {
        ArgumentException.ThrowIfNullOrEmpty(sourceSettings.SourceFolder);
        ArgumentNullException.ThrowIfNull(copySettings.SearchPattern);
        ArgumentNullException.ThrowIfNull(copySettings.SkipExtensions);

        return new SearchParams(
            sourceSettings.SourceFolder,
            sourceSettings.SearchPattern ?? copySettings.SearchPattern,
            sourceSettings.SkipExtensions ?? copySettings.SkipExtensions,
            sourceSettings.SubFoldersOnly ?? [],
            sourceSettings.SkipSubFolders ?? []);
    }

    private static CopyParams ToCopyParams(CopySettings copySettings, SourceSettings sourceSettings) =>
        new(
            sourceSettings.CreateHardLinks ?? copySettings.CreateHardLinks,
            ToOverwriteParams(sourceSettings.Overwrite ?? copySettings.Overwrite));
}