using FlatCopy;
using FlatCopy.Settings;

namespace FlatCopyTests;

public class DirectoryCopyServiceTests
{
    private readonly Mock<IDirectoryScannerService> _directoryScannerServiceMock;
    private readonly Mock<IFileCopyService> _fileCopyService;
    private readonly DirectoryCopyService _directoryCopyService;

    public DirectoryCopyServiceTests()
    {
        _directoryScannerServiceMock = new Mock<IDirectoryScannerService>();
        _fileCopyService = new Mock<IFileCopyService>();

        _directoryCopyService = new DirectoryCopyService(_directoryScannerServiceMock.Object, _fileCopyService.Object);
    }

    [Fact]
    public void CopyDirectoryTest()
    {
        SourceItem[] sourceItems = new[]
        {
            new SourceItem(@"C:\inp\file0.txt","file0.txt"),
            new SourceItem(@"C:\inp\sub\file1.txt",@"sub\file1.txt")
        };

        SearchParams searchParams = new SearchParams(
            @"C:\inp",
            "*",
            [],
            [],
            []);
        CopyParams copyParams = new CopyParams(false, OverwriteOption.No);
        DirectoryCopyParams directoryCopyParams = new(searchParams, copyParams, @"C:\out");

        _directoryScannerServiceMock.Setup(x => x.EnumerateFiles(searchParams)).Returns(sourceItems);

        List<string> copiedFiles = _directoryCopyService.CopyDirectory(directoryCopyParams, "prefix");

        copiedFiles.Should().BeEquivalentTo([
            @"C:\out\prefix_file0.txt",
            @"C:\out\prefix_sub_file1.txt",
        ]);
    }
}