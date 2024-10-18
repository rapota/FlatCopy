namespace FlatCopy;

public record SearchParams(string SourceFolder, string SearchPattern, string[] SkipExtensions, string[] SubFoldersOnly, string[] SkipSubFolders);

public record SourceItem(string SourcePath, string RelativePath);

public interface IDirectoryScannerService
{
    IEnumerable<SourceItem> EnumerateFiles(SearchParams searchParams);
}