namespace FlatCopy.FileSystemServices;

public record QueryParams(string SearchPath, string SearchPattern, bool SearchArchives);

public record SearchParams(QueryParams QueryParams, string[] SkipExtensions, string[] SubFoldersOnly, string[] SkipSubFolders);

public record FileItem(string FullPath, string RelativePath, bool IsArchive = false);

internal interface IDirectoryScannerService
{
    IEnumerable<FileItem> EnumerateFiles(SearchParams searchParams);
}