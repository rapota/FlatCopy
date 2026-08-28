using FlatCopy.FileSystemServices;
using FlatCopy.Settings;

namespace FlatCopy;

internal static class FlatCopyParamsHelper
{
    public static List<FlatCopyParams> BuildTasks(this CopySettings copySettings)
    {
        List<FlatCopyParams> result = new();
        foreach (KeyValuePair<string, SourceSettings> copySource in copySettings.Sources)
        {
            FlatCopyParams flatCopyParams = copySource.Value.ToFlatCopyParams(copySource.Key, copySettings);
            result.Add(flatCopyParams);
        }
        
        return result;
    }

    private static OverwriteParams ToOverwriteParams(OverwriteSettings overwrite) =>
        overwrite switch
        {
            OverwriteSettings.No => OverwriteParams.No,
            OverwriteSettings.Newer => OverwriteParams.Newer,
            OverwriteSettings.Yes => OverwriteParams.Yes,
            _ => throw new ArgumentOutOfRangeException(nameof(overwrite), overwrite, null)
        };

    private static FlatCopyParams ToFlatCopyParams(this SourceSettings sourceSettings, string name, CopySettings copySettings)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);
        ArgumentException.ThrowIfNullOrEmpty(copySettings.TargetFolder);

        return new FlatCopyParams(
            name,
            sourceSettings.ToCopyParams(copySettings),
            sourceSettings.ToSearchParams(copySettings),
            copySettings.TargetFolder);
    }

    private static SearchParams ToSearchParams(this SourceSettings sourceSettings, CopySettings copySettings)
    {
        ArgumentException.ThrowIfNullOrEmpty(sourceSettings.SourceFolder);
        ArgumentNullException.ThrowIfNull(copySettings.SearchPattern);
        ArgumentNullException.ThrowIfNull(copySettings.SkipExtensions);

        return new SearchParams(
            new QueryParams(
                sourceSettings.SourceFolder,
                sourceSettings.SearchPattern ?? copySettings.SearchPattern),
                sourceSettings.SkipExtensions ?? copySettings.SkipExtensions,
            sourceSettings.SubFoldersOnly ?? [],
            sourceSettings.SkipSubFolders ?? []);
    }

    private static CopyParams ToCopyParams(this SourceSettings sourceSettings, CopySettings copySettings) =>
        new(
            sourceSettings.CreateHardLinks ?? copySettings.CreateHardLinks,
            ToOverwriteParams(sourceSettings.Overwrite ?? copySettings.Overwrite));
}