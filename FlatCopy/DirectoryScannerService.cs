using FlatCopy.FileSystem;

namespace FlatCopy;

public sealed class DirectoryScannerService(IFileSystemApi _fileSystemApi) : IDirectoryScannerService
{
    public IEnumerable<SourceItem> EnumerateFiles(SearchParams searchParams) =>
        EnumerateFiles(searchParams.SourceFolder, searchParams.SearchPattern, searchParams.SkipExtensions);

    public IEnumerable<SourceItem> EnumerateFiles(string path, string searchPattern, string[] skipExtensions)
    {
        HashSet<string> se = skipExtensions.ToHashSet(StringComparer.OrdinalIgnoreCase);

        IEnumerable<string> files = _fileSystemApi.EnumerateFiles(path, searchPattern);
        foreach (string filePath in files)
        {
            string extension = Path.GetExtension(filePath);
            if (se.Contains(extension))
            {
                continue;
            }

            string relativePath = Path.GetRelativePath(path, filePath);

            yield return new SourceItem(filePath, relativePath);
        }
    }
}