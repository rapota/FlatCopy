using FlatCopy.FileSystemServices.FileSystem;
using Microsoft.Extensions.Logging;

namespace FlatCopy.FileSystemServices;

internal sealed class DirectoryScannerService(IFileSystemApi _fileSystemApi, ILogger<DirectoryScannerService> _logger) : IDirectoryScannerService
{
    public IEnumerable<FileItem> EnumerateFiles(SearchParams searchParams)
    {
        IEnumerable<FileItem> items = searchParams.SubFoldersOnly.Length > 0
            ? EnumerateSubfolders(searchParams.QueryParams, searchParams.SubFoldersOnly)
            : QueryFiles(searchParams.QueryParams);

        if (searchParams.SkipSubFolders.Length > 0)
        {
            items = FilterBySubfolders(items, searchParams.QueryParams, searchParams.SkipSubFolders);
        }

        return FilterByExtensionItems(items, searchParams.SkipExtensions);
    }

    private static IEnumerable<FileItem> FilterBySubfolders(IEnumerable<FileItem> items, QueryParams queryParams, string[] subFolders)
    {
        List<string> skipSubFolders = subFolders
            .Select(x => Path.Combine(queryParams.SearchPath, x))
            .Select(x =>
                x.EndsWith(Path.DirectorySeparatorChar)
                    ? x
                    : x + Path.DirectorySeparatorChar)
            .ToList();

        bool IsSkipFolder(string path)
        {
            return skipSubFolders.Any(x => path.StartsWith(x, StringComparison.OrdinalIgnoreCase));
        }

        return items.Where(x => !IsSkipFolder(x.FullPath));
    }

    private IEnumerable<FileItem> FilterByExtensionItems(IEnumerable<FileItem> items, string[] skipExtensions)
    {
        HashSet<string> se = skipExtensions.ToHashSet(StringComparer.OrdinalIgnoreCase);
        foreach (FileItem sourceItem in items)
        {
            string extension = Path.GetExtension(sourceItem.FullPath);
            if (se.Contains(extension))
            {
                _logger.LogInformation("Skipping file {filePath} with extension {extension}", sourceItem.FullPath, extension);
                continue;
            }

            yield return sourceItem;
        }
    }

    private IEnumerable<FileItem> EnumerateSubfolders(QueryParams queryParams, string[] subfolders)
    {
        foreach (string subfolder in subfolders)
        {
            string searchPath = Path.Combine(queryParams.SearchPath, subfolder);

            if (!_fileSystemApi.DirectoryExists(searchPath))
            {
                _logger.LogWarning("Subfolder '{subfolder}' does not exist in path '{path}'", subfolder, queryParams.SearchPath);
                continue;
            }

            foreach (FileItem sourceItem in QueryFiles(searchPath, queryParams.SearchPattern))
            {
                string relativePath = Path.GetRelativePath(queryParams.SearchPath, sourceItem.FullPath);
                yield return new FileItem(sourceItem.FullPath, relativePath);
            }
        }
    }

    private IEnumerable<FileItem> QueryFiles(QueryParams queryParams) => QueryFiles(queryParams.SearchPath, queryParams.SearchPattern);

    private IEnumerable<FileItem> QueryFiles(string path, string searchPattern)
    {
        foreach (string filePath in _fileSystemApi.EnumerateFiles(path, searchPattern))
        {
            string relativePath = Path.GetRelativePath(path, filePath);
            yield return new FileItem(filePath, relativePath);
        }
    }
}