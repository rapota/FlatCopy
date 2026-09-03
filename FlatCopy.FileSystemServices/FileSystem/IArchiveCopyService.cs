namespace FlatCopy.FileSystemServices.FileSystem;

internal interface IArchiveCopyService
{
    List<string> ExtractFiles(string archivePath, string destFileName, OverwriteParams overwrite);
}