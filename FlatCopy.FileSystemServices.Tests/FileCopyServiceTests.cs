using FlatCopy.FileSystemServices.FileSystem;

namespace FlatCopy.FileSystemServices.Tests;

public class FileCopyServiceTests
{
    private readonly IFileSystemApiMock _fileSystemMock;
    private readonly FileCopyService _fileCopyService;

    public FileCopyServiceTests()
    {
        _fileSystemMock = IFileSystemApi.Mock();
        _fileCopyService = new FileCopyService(_fileSystemMock.Object, Mock.Logger<FileCopyService>());
    }

    [Test]
    public async Task CopyNewFileTest()
    {
        _fileSystemMock.FileExists(@"C:\out.txt").Returns(false);

        _fileCopyService.CopyFile(@"C:\file.txt", @"C:\out.txt", new CopyParams(false, OverwriteParams.No));

        _fileSystemMock.CopyFile(@"C:\file.txt", @"C:\out.txt").WasCalled(Times.Once);
    }

    [Test]
    public async Task CopyExistingFileTest()
    {
        _fileSystemMock.FileExists(@"C:\out.txt").Returns(true);

        _fileCopyService.CopyFile(@"C:\file.txt", @"C:\out.txt", new CopyParams(false, OverwriteParams.No));

        _fileSystemMock.CopyFile(@"C:\file.txt", @"C:\out.txt").WasNeverCalled();
    }

    [Test]
    public async Task OverwriteNewFileTest()
    {
        _fileSystemMock.FileExists(@"C:\out.txt").Returns(false);

        _fileCopyService.CopyFile(@"C:\file.txt", @"C:\out.txt", new CopyParams(false, OverwriteParams.Newer));

        _fileSystemMock.CopyFile(@"C:\file.txt", @"C:\out.txt").WasCalled(Times.Once);
    }

    [Test]
    public async Task OverwriteUpdatedFileTest()
    {
        _fileSystemMock.FileExists(@"C:\out.txt").Returns(true);
        _fileSystemMock.GetFileInformation(@"C:\file.txt").Returns(new FileInformation(DateTime.UtcNow.AddMinutes(1), 1));
        _fileSystemMock.GetFileInformation(@"C:\out.txt").Returns(new FileInformation(DateTime.UtcNow, 1));

        _fileCopyService.CopyFile(@"C:\file.txt", @"C:\out.txt", new CopyParams(false, OverwriteParams.Newer));

        _fileSystemMock.CopyFile(@"C:\file.txt", @"C:\out.txt", true).WasCalled(Times.Once);
    }

    [Test]
    public async Task OverwriteSameFileTest()
    {
        DateTime dt = DateTime.UtcNow;
        _fileSystemMock.FileExists(@"C:\out.txt").Returns(true);
        _fileSystemMock.GetFileInformation(@"C:\file.txt").Returns(new FileInformation(dt, 1));
        _fileSystemMock.GetFileInformation(@"C:\out.txt").Returns(new FileInformation(dt, 1));

        _fileCopyService.CopyFile(@"C:\file.txt", @"C:\out.txt", new CopyParams(false, OverwriteParams.Newer));

        _fileSystemMock.CopyFile(@"C:\file.txt", @"C:\out.txt", true).WasNeverCalled();
    }

    [Test]
    public async Task AlwaysOverwriteSameFileTest()
    {
        DateTime dt = DateTime.UtcNow;
        _fileSystemMock.FileExists(@"C:\out.txt").Returns(true);
        _fileSystemMock.GetFileInformation(@"C:\file.txt").Returns(new FileInformation(dt, 1));
        _fileSystemMock.GetFileInformation(@"C:\out.txt").Returns(new FileInformation(dt, 1));

        _fileCopyService.CopyFile(@"C:\file.txt", @"C:\out.txt", new CopyParams(false, OverwriteParams.Yes));

        _fileSystemMock.CopyFile(@"C:\file.txt", @"C:\out.txt", true).WasCalled(Times.Once);
    }
}