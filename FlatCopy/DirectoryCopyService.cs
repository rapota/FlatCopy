namespace FlatCopy;

public sealed class DirectoryCopyService(IDirectoryScannerService _directoryScannerService, IFileCopyService _fileCopyService) : IDirectoryCopyService
{
    public List<string> CopyDirectory(DirectoryCopyParams directoryCopyParams, params string[] customPrefixes)
    {
        string customPrefix = customPrefixes.Length > 0
            ? string.Join('_', customPrefixes) + "_"
            : string.Empty;

        List<string> result = new List<string>(5000);
        foreach (SourceItem sourceItem in _directoryScannerService.EnumerateFiles(directoryCopyParams.SearchParams))
        {
            string fileName = sourceItem.RelativePath.Replace(Path.DirectorySeparatorChar, '_');
            string destFileName = Path.Combine(directoryCopyParams.DestDirectory, customPrefix + fileName);

            _fileCopyService.CopyFile(sourceItem.SourcePath, destFileName, directoryCopyParams.CopyParams);

            result.Add(destFileName);
        }

        return result;
    }
}