namespace FlatCopy.Settings;

public sealed class CopySource
{
    public string SourceFolder { get; set; } = string.Empty;

    public string? SearchPattern { get; set; }

    public string[]? SkipExtensions { get; set; }

    public OverwriteOption? Overwrite { get; set; }

    public bool? CreateHardLinks { get; set; }

    public string[]? SubFoldersOnly { get; set; }

    public string[]? SkipSubFolders { get; set; }
}