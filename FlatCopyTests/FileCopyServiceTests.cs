using FlatCopy;
using FlatCopy.FileSystem;
using FlatCopy.Settings;
using Microsoft.Extensions.Logging;

namespace FlatCopyTests;

public class FileCopyServiceTests
{
    private readonly Mock<IFileSystemApi> _fileSystemMock;
    private readonly FileCopyService _fileCopyService;

    public FileCopyServiceTests()
    {
        _fileSystemMock = new Mock<IFileSystemApi>();
        _fileCopyService = new FileCopyService(_fileSystemMock.Object, Mock.Of<ILogger<FileCopyService>>());
    }

    [Fact]
    public void CopyNewFileTest()
    {
        _fileSystemMock.Setup(x => x.FileExists(@"C:\out.txt")).Returns(false);

        _fileCopyService.CopyFile(@"C:\file.txt", @"C:\out.txt", new CopyParams(false, OverwriteOption.No));

        _fileSystemMock.Verify(x => x.CopyFile(@"C:\file.txt", @"C:\out.txt"), Times.Once);
    }

    [Fact]
    public void CopyExistingFileTest()
    {
        _fileSystemMock.Setup(x => x.FileExists(@"C:\out.txt")).Returns(true);

        _fileCopyService.CopyFile(@"C:\file.txt", @"C:\out.txt", new CopyParams(false, OverwriteOption.No));

        _fileSystemMock.Verify(x => x.CopyFile(@"C:\file.txt", @"C:\out.txt"), Times.Never);
    }

    [Fact]
    public void OverwriteNewFileTest()
    {
        _fileSystemMock.Setup(x => x.FileExists(@"C:\out.txt")).Returns(false);

        _fileCopyService.CopyFile(@"C:\file.txt", @"C:\out.txt", new CopyParams(false, OverwriteOption.Newer));

        _fileSystemMock.Verify(x => x.CopyFile(@"C:\file.txt", @"C:\out.txt"), Times.Once);
    }

    [Fact]
    public void OverwriteUpdatedFileTest()
    {
        _fileSystemMock.Setup(x => x.FileExists(@"C:\out.txt")).Returns(true);
        _fileSystemMock.Setup(x => x.GetFileInformation(@"C:\file.txt")).Returns(new FileInformation(DateTime.UtcNow.AddMinutes(1), 1));
        _fileSystemMock.Setup(x => x.GetFileInformation(@"C:\out.txt")).Returns(new FileInformation(DateTime.UtcNow, 1));

        _fileCopyService.CopyFile(@"C:\file.txt", @"C:\out.txt", new CopyParams(false, OverwriteOption.Newer));

        _fileSystemMock.Verify(x => x.CopyFile(@"C:\file.txt", @"C:\out.txt", true), Times.Once);
    }

    [Fact]
    public void OverwriteSameFileTest()
    {
        DateTime dt = DateTime.UtcNow;
        _fileSystemMock.Setup(x => x.FileExists(@"C:\out.txt")).Returns(true);
        _fileSystemMock.Setup(x => x.GetFileInformation(@"C:\file.txt")).Returns(new FileInformation(dt, 1));
        _fileSystemMock.Setup(x => x.GetFileInformation(@"C:\out.txt")).Returns(new FileInformation(dt, 1));

        _fileCopyService.CopyFile(@"C:\file.txt", @"C:\out.txt", new CopyParams(false, OverwriteOption.Newer));

        _fileSystemMock.Verify(x => x.CopyFile(@"C:\file.txt", @"C:\out.txt", true), Times.Never);
    }

    [Fact]
    public void AlwaysOverwriteSameFileTest()
    {
        DateTime dt = DateTime.UtcNow;
        _fileSystemMock.Setup(x => x.FileExists(@"C:\out.txt")).Returns(true);
        _fileSystemMock.Setup(x => x.GetFileInformation(@"C:\file.txt")).Returns(new FileInformation(dt, 1));
        _fileSystemMock.Setup(x => x.GetFileInformation(@"C:\out.txt")).Returns(new FileInformation(dt, 1));

        _fileCopyService.CopyFile(@"C:\file.txt", @"C:\out.txt", new CopyParams(false, OverwriteOption.Yes));

        _fileSystemMock.Verify(x => x.CopyFile(@"C:\file.txt", @"C:\out.txt", true), Times.Once);
    }
}