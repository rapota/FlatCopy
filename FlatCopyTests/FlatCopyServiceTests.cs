using FlatCopy;
using FlatCopy.FileSystem;
using FlatCopy.Settings;
using Microsoft.Extensions.Logging;

namespace FlatCopyTests;

public class FlatCopyServiceTests
{
    private readonly Mock<IDirectoryCopyService> _directoryCopyMock;
    private readonly Mock<IFileSystemApi> _fileSystemMock;
    private readonly FlatCopyService _flatCopyService;

    public FlatCopyServiceTests()
    {
        _directoryCopyMock = new Mock<IDirectoryCopyService>();
        _fileSystemMock = new Mock<IFileSystemApi>();
        _flatCopyService = new FlatCopyService(_directoryCopyMock.Object, _fileSystemMock.Object, Mock.Of<ILogger<FlatCopyService>>());
    }

    [Fact]
    public void CopyFromMissingDirectory()
    {
        CopyParams copyParams = new(false, OverwriteOption.No);
        SearchParams searchParams = new(@"C:\inp", "*", [], [], []);
        FlatCopyParams flatCopyParams = new("Name", copyParams, searchParams, @"C:\out");

        _fileSystemMock.Setup(x => x.DirectoryExists(@"C:\inp")).Returns(false);

        List<string> copy = _flatCopyService.FlatCopy(flatCopyParams);

        copy.Should().BeEmpty();

        DirectoryCopyParams directoryCopyParams = new DirectoryCopyParams(flatCopyParams.SearchParams, flatCopyParams.CopyParams, flatCopyParams.DestDirectory);
        _directoryCopyMock.Verify(x => x.CopyDirectory(directoryCopyParams), Times.Never);
    }

    [Fact]
    public void CopyToMissingDirectoryTest()
    {
        CopyParams copyParams = new(false, OverwriteOption.No);
        SearchParams searchParams = new(@"C:\inp", "*", [], [], []);
        FlatCopyParams flatCopyParams = new("Name", copyParams, searchParams, @"C:\out");

        _fileSystemMock.Setup(x => x.DirectoryExists(@"C:\inp")).Returns(true);
        _fileSystemMock.Setup(x => x.DirectoryExists(@"C:\out")).Returns(false);

        DirectoryCopyParams directoryCopyParams = new DirectoryCopyParams(flatCopyParams.SearchParams, flatCopyParams.CopyParams, flatCopyParams.DestDirectory);
        _directoryCopyMock.Setup(x => x.CopyDirectory(directoryCopyParams, "Name")).Returns([@"C:\out\file.txt"]);

        List<string> copy = _flatCopyService.FlatCopy(flatCopyParams);

        copy.Should().BeEquivalentTo([@"C:\out\file.txt"]);
        _fileSystemMock.Verify(x => x.CreateDirectory(@"C:\out"), Times.Once);
        _directoryCopyMock.Verify(x => x.CopyDirectory(directoryCopyParams, "Name"), Times.Once);
    }

    [Fact]
    public void CopyDirectoryTest()
    {
        CopyParams copyParams = new(false, OverwriteOption.No);
        SearchParams searchParams = new(@"C:\inp", "*", [], [], []);
        FlatCopyParams flatCopyParams = new("Name", copyParams, searchParams, @"C:\out");

        _fileSystemMock.Setup(x => x.DirectoryExists(@"C:\inp")).Returns(true);
        _fileSystemMock.Setup(x => x.DirectoryExists(@"C:\out")).Returns(true);

        DirectoryCopyParams directoryCopyParams = new DirectoryCopyParams(flatCopyParams.SearchParams, flatCopyParams.CopyParams, flatCopyParams.DestDirectory);
        _directoryCopyMock.Setup(x => x.CopyDirectory(directoryCopyParams, "Name")).Returns([@"C:\out\file.txt"]);

        List<string> copy = _flatCopyService.FlatCopy(flatCopyParams);

        copy.Should().BeEquivalentTo([@"C:\out\file.txt"]);
    }

    [Fact]
    public void DeleteExtraFilesTest()
    {
        List<string> existingFiles = [
            @"C:\file1.txt",
            @"C:\file2.txt",
            @"C:\file3.txt"];
        _fileSystemMock.Setup(x => x.EnumerateFiles("C:", "*")).Returns(existingFiles);

        _flatCopyService.DeleteExtraFiles([@"C:\File1.txt", @"C:\file2.txt"], "C:", "*");

        _fileSystemMock.Verify(x => x.DeleteFile(@"C:\file1.txt"), Times.Never);
        _fileSystemMock.Verify(x => x.DeleteFile(@"C:\file2.txt"), Times.Never);
        _fileSystemMock.Verify(x => x.DeleteFile(@"C:\file3.txt"), Times.Once);
    }
}