using FlatCopy.FileSystemServices.FileSystem;

namespace FlatCopy.FileSystemServices.Tests;

public class DirectoryCopyServiceTests
{
    [Test]
    public async Task CopyDirectoryTest()
    {
        // Arrange
        FileItem[] sourceItems =
        [
            new(@"C:\inp\file.zip","file.zip", true),
            new(@"C:\inp\file0.txt","file0.txt"),
            new(@"C:\inp\sub\file1.txt",@"sub\file1.txt")
        ];

        SearchParams searchParams = new(
            new QueryParams(
                @"C:\inp",
                "*",
                true),
            [],
            [],
            []);
        CopyParams copyParams = new(false, OverwriteParams.No);
        DirectoryCopyParams directoryCopyParams = new(searchParams, copyParams, @"C:\out");

        var directoryScannerServiceMock = IDirectoryScannerService.Mock();
        directoryScannerServiceMock.EnumerateFiles(searchParams).Returns(sourceItems);

        var fileCopyServiceMock = IFileCopyService.Mock();
        var archiveCopyServiceMock = IArchiveCopyService.Mock();
        archiveCopyServiceMock
            .ExtractFiles(@"C:\inp\file.zip", @"C:\out\prefix_file.zip", copyParams.Overwrite)
            .Returns([@"C:\out\prefix_file.zip_f1.txt", @"C:\out\prefix_file.zip_f2.txt"]);

        var directoryCopyService = new DirectoryCopyService(directoryScannerServiceMock, fileCopyServiceMock, archiveCopyServiceMock);
        
        // Act
        List<string> copiedFiles = directoryCopyService.CopyDirectory(directoryCopyParams, "prefix");

        // Assert
        string[] expected = [
            @"C:\out\prefix_file.zip_f1.txt",
            @"C:\out\prefix_file.zip_f2.txt",
            @"C:\out\prefix_file0.txt",
            @"C:\out\prefix_sub_file1.txt"];

        await Assert.That(copiedFiles).IsEquivalentTo(expected);
    }
}