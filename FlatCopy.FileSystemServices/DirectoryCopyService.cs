using FlatCopy.FileSystemServices.FileSystem;

namespace FlatCopy.FileSystemServices;

internal sealed class DirectoryCopyService(IDirectoryScannerService _directoryScannerService, IFileCopyService _fileCopyService, IArchiveCopyService _archiveCopyService)
    : IDirectoryCopyService
{
    public List<string> CopyDirectory(DirectoryCopyParams directoryCopyParams, params string[] customPrefixes)
    {
        string customPrefix = customPrefixes.Length > 0
            ? string.Join('_', customPrefixes) + "_"
            : string.Empty;

        List<string> result = new List<string>(5000);
        foreach (FileItem sourceItem in _directoryScannerService.EnumerateFiles(directoryCopyParams.SearchParams))
        {
            string fileName = sourceItem.RelativePath.Replace(Path.DirectorySeparatorChar, '_');
            string destFileName = Path.Combine(directoryCopyParams.DestDirectory, customPrefix + fileName);

            if (sourceItem.IsArchive)
            {
                List<string> extractedFiles = _archiveCopyService.ExtractFiles(sourceItem.FullPath, destFileName, directoryCopyParams.CopyParams.Overwrite);
                result.AddRange(extractedFiles);
            }
            else
            {
                _fileCopyService.CopyFile(sourceItem.FullPath, destFileName, directoryCopyParams.CopyParams);
                result.Add(destFileName);
            }
        }

        return result;
    }
}