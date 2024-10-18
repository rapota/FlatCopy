// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global
// ReSharper disable CollectionNeverUpdated.Global
namespace FlatCopy.Settings;

public sealed class CopyOptions
{
    public string TargetFolder { get; set; }

    public string SearchPattern { get; set; }

    public string[] SkipExtensions { get; set; }

    public List<string> SkipFolders { get; set; }

    public OverwriteOption Overwrite { get; set; }

    public bool CreateHardLinks { get; set; }

    public List<string> SourceFolders { get; set; }

    public Dictionary<string, CopySource> Sources { get; set; }

    public CopyOptions()
    {
        TargetFolder = string.Empty;
        SourceFolders = new List<string>();
        SearchPattern = "*";
        SkipExtensions = [];
        SkipFolders = new List<string>();
        Overwrite = OverwriteOption.No;
        Sources = new Dictionary<string, CopySource>();
    }
}