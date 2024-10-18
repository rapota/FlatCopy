using FlatCopy.FileSystem;

namespace FlatCopy;

public sealed class DirectoryScannerService(IFileSystemApi _fileSystemApi) : IDirectoryScannerService
{
    public IEnumerable<SourceItem> EnumerateFiles(SearchParams searchParams) =>
        searchParams.SubFoldersOnly.Length == 0 && searchParams.SkipSubFolders.Length == 0
            ? EnumerateFiles(searchParams.SourceFolder, searchParams.SearchPattern, searchParams.SkipExtensions)
            : EnumerateFiles(searchParams.SourceFolder, searchParams.SearchPattern, searchParams.SkipExtensions, searchParams.SubFoldersOnly, searchParams.SkipSubFolders);

    private IEnumerable<SourceItem> EnumerateFiles(string path, string searchPattern, string[] skipExtensions, string[] subFoldersOnly, string[] skipSubFolders)
    {
        subFoldersOnly = subFoldersOnly
            .Select(x => Path.Combine(path, x))
            .Select(x =>
                x.EndsWith(Path.DirectorySeparatorChar)
                    ? x
                    : x + Path.DirectorySeparatorChar)
            .ToArray();

        skipSubFolders = skipSubFolders
            .Select(x => Path.Combine(path, x))
            .Select(x =>
                x.EndsWith(Path.DirectorySeparatorChar)
                    ? x
                    : x + Path.DirectorySeparatorChar)
            .ToArray();

        bool IsSubFolder(string folder)
        {
            return subFoldersOnly.Any(x => folder.StartsWith(x, StringComparison.OrdinalIgnoreCase));
        }

        bool IsSkipFolder(string folder)
        {
            return skipSubFolders.Any(x => folder.StartsWith(x, StringComparison.OrdinalIgnoreCase));
        }

        foreach (SourceItem sourceItem in EnumerateFiles(path, searchPattern, skipExtensions))
        {
            if (subFoldersOnly.Length > 0 && !IsSubFolder(sourceItem.SourcePath))
            {
                continue;
            }

            if (skipSubFolders.Length > 0 && IsSkipFolder(sourceItem.SourcePath))
            {
                continue;
            }

            yield return sourceItem;
        }
    }

    private IEnumerable<SourceItem> EnumerateFiles(string path, string searchPattern, string[] skipExtensions)
    {
        string searchPath = Path.TrimEndingDirectorySeparator(path);
        HashSet<string> se = skipExtensions.ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (string filePath in _fileSystemApi.EnumerateFiles(path, searchPattern))
        {
            string extension = Path.GetExtension(filePath);
            if (se.Contains(extension))
            {
                continue;
            }

            string relativePath = Path.GetRelativePath(searchPath, filePath);

            yield return new SourceItem(filePath, relativePath);
        }
    }
}