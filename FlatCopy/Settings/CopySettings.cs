// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global
// ReSharper disable CollectionNeverUpdated.Global
namespace FlatCopy.Settings;

public sealed class CopySettings
{
    public string TargetFolder { get; set; } = string.Empty;

    public string SearchPattern { get; set; } = "*";

    public string[] SkipExtensions { get; set; } = [];

    public OverwriteSettings Overwrite { get; set; } = OverwriteSettings.No;

    public bool CreateHardLinks { get; set; }

    public bool Unpack { get; set; }

    public Dictionary<string, SourceSettings> Sources { get; set; } = new();
}