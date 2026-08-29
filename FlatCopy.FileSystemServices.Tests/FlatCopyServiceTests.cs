using FlatCopy.FileSystemServices.FileSystem;

namespace FlatCopy.FileSystemServices.Tests;

public class FlatCopyServiceTests
{
    private readonly IDirectoryCopyServiceMock _directoryCopyMock;
    private readonly IFileSystemApiMock _fileSystemMock;
    private readonly FlatCopyService _flatCopyService;

    public FlatCopyServiceTests()
    {
        _directoryCopyMock = IDirectoryCopyService.Mock();
        _fileSystemMock = IFileSystemApi.Mock();
        _flatCopyService = new FlatCopyService(_directoryCopyMock.Object, _fileSystemMock.Object, Mock.Logger<FlatCopyService>());
    }

    [Test]
    public async Task CopyFromMissingDirectory()
    {
        CopyParams copyParams = new(false, OverwriteParams.No);
        SearchParams searchParams = new(new QueryParams(@"C:\inp", "*"), [], [], []);
        FlatCopyParams flatCopyParams = new("Name", copyParams, searchParams, @"C:\out");

        _fileSystemMock.DirectoryExists(@"C:\inp").Returns(false);

        List<string> copy = _flatCopyService.FlatCopy(flatCopyParams);

        await Assert.That(copy).IsEmpty();

        DirectoryCopyParams directoryCopyParams = new DirectoryCopyParams(flatCopyParams.SearchParams, flatCopyParams.CopyParams, flatCopyParams.DestDirectory);
        _directoryCopyMock.CopyDirectory(directoryCopyParams).WasNeverCalled();
    }

    [Test]
    public async Task CopyToMissingDirectoryTest()
    {
        CopyParams copyParams = new(false, OverwriteParams.No);
        SearchParams searchParams = new(new QueryParams(@"C:\inp", "*"), [], [], []);
        FlatCopyParams flatCopyParams = new("Name", copyParams, searchParams, @"C:\out");

        _fileSystemMock.DirectoryExists(@"C:\inp").Returns(true);
        _fileSystemMock.DirectoryExists(@"C:\out").Returns(false);

        DirectoryCopyParams directoryCopyParams = new DirectoryCopyParams(flatCopyParams.SearchParams, flatCopyParams.CopyParams, flatCopyParams.DestDirectory);
        _directoryCopyMock.CopyDirectory(directoryCopyParams, "Name").Returns([@"C:\out\file.txt"]);

        List<string> copy = _flatCopyService.FlatCopy(flatCopyParams);

        string[] expected = [@"C:\out\file.txt"];
        await Assert.That(copy).IsEquivalentTo(expected);
        _fileSystemMock.CreateDirectory(@"C:\out").WasCalled(Times.Once);
        _directoryCopyMock.CopyDirectory(directoryCopyParams, "Name").WasCalled(Times.Once);
    }

    [Test]
    public async Task CopyDirectoryTest()
    {
        CopyParams copyParams = new(false, OverwriteParams.No);
        SearchParams searchParams = new(new QueryParams(@"C:\inp", "*"), [], [], []);
        FlatCopyParams flatCopyParams = new("Name", copyParams, searchParams, @"C:\out");

        _fileSystemMock.DirectoryExists(@"C:\inp").Returns(true);
        _fileSystemMock.DirectoryExists(@"C:\out").Returns(true);

        DirectoryCopyParams directoryCopyParams = new DirectoryCopyParams(flatCopyParams.SearchParams, flatCopyParams.CopyParams, flatCopyParams.DestDirectory);
        _directoryCopyMock.CopyDirectory(directoryCopyParams, "Name").Returns([@"C:\out\file.txt"]);

        List<string> copy = _flatCopyService.FlatCopy(flatCopyParams);

        string[] expected = [@"C:\out\file.txt"];
        await Assert.That(copy).IsEquivalentTo(expected);
    }

    [Test]
    public async Task DeleteExtraFilesTest()
    {
        List<string> existingFiles = [
            @"C:\file1.txt",
            @"C:\file2.txt",
            @"C:\file3.txt"];
        _fileSystemMock.EnumerateFiles("C:", "*").Returns(existingFiles);

        _flatCopyService.DeleteExtraFiles([@"C:\File1.txt", @"C:\file2.txt"], "C:", "*");

        _fileSystemMock.DeleteFile(@"C:\file1.txt").WasNeverCalled();
        _fileSystemMock.DeleteFile(@"C:\file2.txt").WasNeverCalled();
        _fileSystemMock.DeleteFile(@"C:\file3.txt").WasCalled(Times.Once);
    }
}