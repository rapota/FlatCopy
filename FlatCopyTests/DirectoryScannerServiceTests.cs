using FlatCopy;
using FlatCopy.FileSystem;

namespace FlatCopyTests;

public class DirectoryScannerServiceTests
{
    private readonly Mock<IFileSystemApi> _fileSystemMock;
    private readonly DirectoryScannerService _directoryScannerService;

    public DirectoryScannerServiceTests()
    {
        _fileSystemMock = new Mock<IFileSystemApi>();
        _directoryScannerService = new DirectoryScannerService(_fileSystemMock.Object);
    }

    [Fact]
    public void NestedCopyTest()
    {
        string[] files =
        [
            @"C:\inp\file1.txt",
            @"C:\inp\sub\file2.txt"
        ];

        _fileSystemMock.Setup(x => x.EnumerateFiles(@"C:\inp", "*")).Returns(files);

        SearchParams searchParams = new(
            @"C:\inp",
            "*",
            [],
            [],
            []);

        IEnumerable<SourceItem> result = _directoryScannerService.EnumerateFiles(searchParams);
        List<SourceItem> sourceItems = result.ToList();

        sourceItems.Should().BeEquivalentTo([
            new SourceItem(@"C:\inp\file1.txt", "file1.txt"),
            new SourceItem(@"C:\inp\sub\file2.txt", @"sub\file2.txt")
        ]);
    }

    [Fact]
    public void SkipExtensionTest()
    {
        string[] files =
        [
            @"C:\inp\file1.txt",
            @"C:\inp\file2.zip",
            @"C:\inp\file3.txt",
            @"C:\inp\file4.exe"
        ];

        _fileSystemMock.Setup(x => x.EnumerateFiles(@"C:\inp", "*.*")).Returns(files);

        SearchParams searchParams = new(
            @"C:\inp",
            "*.*",
            [".zip", ".EXE"],
            [],
            []);

        IEnumerable<SourceItem> result = _directoryScannerService.EnumerateFiles(searchParams);
        List<SourceItem> sourceItems = result.ToList();

        sourceItems.Should().BeEquivalentTo([
            new SourceItem(@"C:\inp\file1.txt", "file1.txt"),
            new SourceItem(@"C:\inp\file3.txt", "file3.txt")
        ]);
    }

    [Fact]
    public void SubFoldersCopyTest()
    {
        string[] files =
        [
            @"C:\inp\file.txt",
            @"C:\inp\sub1file.txt",
            @"C:\inp\sub1\file1.txt",
            @"C:\inp\sub2\file2.txt",
            @"C:\inp\sub3\file3.txt",
            @"C:\inp\sub3\sub31\file4.txt"
        ];

        _fileSystemMock.Setup(x => x.EnumerateFiles(@"C:\inp", "*.*")).Returns(files);

        SearchParams searchParams = new(
            @"C:\inp",
            "*.*",
            [],
            ["sub1", "SUB2", @"sub3\sub31"],
            []);

        IEnumerable<SourceItem> result = _directoryScannerService.EnumerateFiles(searchParams);
        List<SourceItem> sourceItems = result.ToList();

        sourceItems.Should().BeEquivalentTo([
            new SourceItem(@"C:\inp\sub1\file1.txt", @"sub1\file1.txt"),
            new SourceItem(@"C:\inp\sub2\file2.txt", @"sub2\file2.txt"),
            new SourceItem(@"C:\inp\sub3\sub31\file4.txt", @"sub3\sub31\file4.txt")
        ]);
    }

    [Fact]
    public void SkipSubFoldersTest()
    {
        string[] files =
        [
            @"C:\inp\file0.txt",
            @"C:\inp\sub1file0.txt",
            @"C:\inp\sub1\file1.txt",
            @"C:\inp\sub2\file2.txt",
            @"C:\inp\sub3\file3.txt",
            @"C:\inp\sub3\sub31\file4.txt",
        ];

        _fileSystemMock.Setup(x => x.EnumerateFiles(@"C:\inp", "*.*")).Returns(files);

        SearchParams searchParams = new(
            @"C:\inp",
            "*.*",
            [],
            [],
            ["SUB1", @"sub3\sub31"]);

        IEnumerable<SourceItem> result = _directoryScannerService.EnumerateFiles(searchParams);
        List<SourceItem> sourceItems = result.ToList();

        sourceItems.Should().BeEquivalentTo([
            new SourceItem(@"C:\inp\file0.txt", "file0.txt"),
            new SourceItem(@"C:\inp\sub1file0.txt", "sub1file0.txt"),
            new SourceItem(@"C:\inp\sub2\file2.txt", @"sub2\file2.txt"),
            new SourceItem(@"C:\inp\sub3\file3.txt", @"sub3\file3.txt")
        ]);
    }
}