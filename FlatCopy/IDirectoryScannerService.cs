namespace FlatCopy;

public record SearchParams(string SourceFolder, string SearchPattern, string[] SkipExtensions);

public record SourceItem(string SourcePath, string RelativePath);

public interface IDirectoryScannerService
{
    IEnumerable<SourceItem> EnumerateFiles(SearchParams searchParams);

    IEnumerable<SourceItem> EnumerateFiles(string path, string searchPattern, string[] skipExtensions);
}