using FlatCopy.FileSystemServices.FileSystem;

namespace FlatCopy.FileSystemServices.Tests;

public class DirectoryScannerServiceTests
{
    private readonly IFileSystemApiMock _fileSystemMock;
    private readonly DirectoryScannerService _directoryScannerService;

    public DirectoryScannerServiceTests()
    {
        _fileSystemMock = IFileSystemApi.Mock();
        _directoryScannerService = new DirectoryScannerService(_fileSystemMock, Mock.Logger<DirectoryScannerService>());
    }

    [Test]
    public async Task EnumerateNestedTest()
    {
        string[] files =
        [
            @"C:\inp\file1.txt",
            @"C:\inp\sub\file2.txt"
        ];

        _fileSystemMock.EnumerateFiles(@"C:\inp", "*").Returns(files);

        SearchParams searchParams = new(
        new QueryParams(
            @"C:\inp",
            "*"),
        [],
        [],
        []);

        IEnumerable<FileItem> result = _directoryScannerService.EnumerateFiles(searchParams);
        List<FileItem> sourceItems = result.ToList();

        FileItem[] expected =
        [
            new(@"C:\inp\file1.txt", "file1.txt"),
            new(@"C:\inp\sub\file2.txt", @"sub\file2.txt")
        ];

        await Assert.That(sourceItems).IsEquivalentTo(expected);
    }

    [Test]
    public async Task SkipExtensionTest()
    {
        string[] files =
        [
            @"C:\inp\file1.txt",
            @"C:\inp\file2.zip",
            @"C:\inp\file3.txt",
            @"C:\inp\file4.exe"
        ];

        _fileSystemMock.EnumerateFiles(@"C:\inp", "*.*").Returns(files);

        SearchParams searchParams = new(
        new QueryParams(
            @"C:\inp",
            "*.*"),
        [".zip", ".EXE"],
        [],
        []);

        IEnumerable<FileItem> result = _directoryScannerService.EnumerateFiles(searchParams);
        List<FileItem> sourceItems = result.ToList();

        FileItem[] expected =
        [
            new(@"C:\inp\file1.txt", "file1.txt"),
            new(@"C:\inp\file3.txt", "file3.txt")
        ];

        await Assert.That(sourceItems).IsEquivalentTo(expected);
    }

    [Test]
    public async Task EnumerateSubFoldersOnlyTest()
    {
        string[] subFiles1 =
        [
            @"C:\inp\sub1\file1.txt",
            @"C:\inp\sub1\file2.txt"
        ];
        _fileSystemMock.DirectoryExists(@"C:\inp\sub1").Returns(true);
        _fileSystemMock.EnumerateFiles(@"C:\inp\sub1", "*.*").Returns(subFiles1);

        string[] subFiles2 =
        [
            @"C:\inp\sub2\subsub\file3.txt",
            @"C:\inp\sub2\subsub\file4.txt"
        ];
        _fileSystemMock.DirectoryExists(@"C:\inp\sub2\subsub").Returns(true);
        _fileSystemMock.EnumerateFiles(@"C:\inp\sub2\subsub", "*.*").Returns(subFiles2);

        SearchParams searchParams = new(
            new QueryParams(
            @"C:\inp",
            "*.*"),
            [],
            ["sub1", @"sub2\subsub"],
            []);

        IEnumerable<FileItem> result = _directoryScannerService.EnumerateFiles(searchParams);
        List<FileItem> sourceItems = result.ToList();

        FileItem[] expected =
        [
            new(@"C:\inp\sub1\file1.txt", @"sub1\file1.txt"),
            new(@"C:\inp\sub1\file2.txt", @"sub1\file2.txt"),
            new(@"C:\inp\sub2\subsub\file3.txt", @"sub2\subsub\file3.txt"),
            new(@"C:\inp\sub2\subsub\file4.txt", @"sub2\subsub\file4.txt")
        ];

        await Assert.That(sourceItems).IsEquivalentTo(expected);
    }

    [Test]
    public async Task SkipSubFoldersTest()
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

        _fileSystemMock.EnumerateFiles(@"C:\inp", "*.*").Returns(files);

        SearchParams searchParams = new(
            new QueryParams(
            @"C:\inp",
            "*.*"),
            [],
            [],
            ["SUB1", @"sub3\sub31"]);

        IEnumerable<FileItem> result = _directoryScannerService.EnumerateFiles(searchParams);
        List<FileItem> sourceItems = result.ToList();

        FileItem[] expected =
        [
            new(@"C:\inp\file0.txt", "file0.txt"),
            new(@"C:\inp\sub1file0.txt", "sub1file0.txt"),
            new(@"C:\inp\sub2\file2.txt", @"sub2\file2.txt"),
            new(@"C:\inp\sub3\file3.txt", @"sub3\file3.txt")
        ];
        await Assert.That(sourceItems).IsEquivalentTo(expected);
    }
}