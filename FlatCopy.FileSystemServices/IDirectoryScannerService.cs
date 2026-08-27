namespace FlatCopy.FileSystemServices;

public record QueryParams(string SearchPath, string SearchPattern);

public record SearchParams(QueryParams QueryParams, string[] SkipExtensions, string[] SubFoldersOnly, string[] SkipSubFolders);

public record FileItem(string FullPath, string RelativePath);

public interface IDirectoryScannerService
{
    IEnumerable<FileItem> EnumerateFiles(SearchParams searchParams);
}