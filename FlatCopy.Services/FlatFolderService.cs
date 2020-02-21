using System.Collections.Generic;
using System.IO;
using FileHeap;

namespace FlatCopy.Services
{
    public static class FlatFolderService
    {
        public static IEnumerable<FileLink> GetFileLinks(string sourceFolder, string targetFolder, string pattern)
        {
            foreach (string sourceFile in Directory.EnumerateFiles(sourceFolder, pattern, SearchOption.AllDirectories))
            {
                string relativeName = sourceFile.Remove(0, sourceFolder.Length);

                string normilizedName = relativeName
                    .TrimStart(Path.DirectorySeparatorChar)
                    .Replace(Path.DirectorySeparatorChar, '_');

                string targetFile = Path.Combine(targetFolder, normilizedName);

                yield return new FileLink(sourceFile, targetFile);
            }
        }
    }
}