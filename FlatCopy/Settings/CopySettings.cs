// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global
// ReSharper disable CollectionNeverUpdated.Global
namespace FlatCopy.Settings;

public sealed class CopySettings
{
    public string TargetFolder { get; set; }

    public string SearchPattern { get; set; }

    public string[] SkipExtensions { get; set; }

    public List<string> SkipFolders { get; set; }

    public OverwriteSettings Overwrite { get; set; }

    public bool CreateHardLinks { get; set; }

    public Dictionary<string, SourceSettings> Sources { get; set; }

    public CopySettings()
    {
        TargetFolder = string.Empty;
        SearchPattern = "*";
        SkipExtensions = [];
        SkipFolders = new List<string>();
        Overwrite = OverwriteSettings.No;
        Sources = new Dictionary<string, SourceSettings>();
    }
}