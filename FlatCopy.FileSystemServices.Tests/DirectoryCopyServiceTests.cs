namespace FlatCopy.FileSystemServices.Tests;

public class DirectoryCopyServiceTests
{
    [Test]
    public async Task CopyDirectoryTest()
    {
        FileItem[] sourceItems =
        [
            new(@"C:\inp\file0.txt","file0.txt"),
            new(@"C:\inp\sub\file1.txt",@"sub\file1.txt")
        ];

        SearchParams searchParams = new(
            new QueryParams(
                @"C:\inp",
                "*"),
            [],
            [],
            []);
        CopyParams copyParams = new(false, OverwriteParams.No);
        DirectoryCopyParams directoryCopyParams = new(searchParams, copyParams, @"C:\out");

        var directoryScannerServiceMock = IDirectoryScannerService.Mock();
        directoryScannerServiceMock.EnumerateFiles(searchParams).Returns(sourceItems);

        var fileCopyServiceMock = IFileCopyService.Mock();

        var directoryCopyService = new DirectoryCopyService(directoryScannerServiceMock, fileCopyServiceMock);
        List<string> copiedFiles = directoryCopyService.CopyDirectory(directoryCopyParams, "prefix");

        string[] expected = [
            @"C:\out\prefix_file0.txt",
            @"C:\out\prefix_sub_file1.txt"];

        await Assert.That(copiedFiles).IsEquivalentTo(expected);
    }
}