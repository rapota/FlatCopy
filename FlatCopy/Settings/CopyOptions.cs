namespace FlatCopy.Settings;

public class CopyOptions
{
    public string TargetFolder { get; set; }

    public List<string> SourceFolders { get; set; }

    public string SearchPattern { get; set; }

    public List<string> SkipExtensions { get; set; }

    public List<string> SkipFolders { get; set; }

    public OverwriteOption Overwrite { get; set; }

    public bool IsParallel { get; set; }

    public bool CreateHardLinks { get; set; }

    public CopyOptions()
    {
        TargetFolder = string.Empty;
        SourceFolders = new List<string>();
        SearchPattern = string.Empty;
        SkipExtensions = new List<string>();
        SkipFolders = new List<string>();
        Overwrite = OverwriteOption.No;
    }
}