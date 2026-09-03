using System.IO.Compression;
using FlatCopy.FileSystemServices.FileSystem;

namespace FlatCopy.FileSystemServices.Tests.FileSystem;

public class ArchiveCopyServiceTests : IDisposable
{
    private readonly string _tempDirectory;
    private readonly string _testArchiveName;
    private readonly IFileSystemApiMock _fileSystemMock;
    private readonly ArchiveCopyService _archiveCopyService;

    public ArchiveCopyServiceTests()
    {
        _tempDirectory = CreateTempDirectory();
        _testArchiveName = Path.Combine(_tempDirectory, "test.zip");

        _fileSystemMock = IFileSystemApi.Mock();
        _archiveCopyService = new ArchiveCopyService(_fileSystemMock.Object, Mock.Logger<ArchiveCopyService>());
    }

    public void Dispose()
    {
        Directory.Delete(_tempDirectory, true);
    }

    [Test]
    public async Task ExtractFiles_SkipsDirectoryEntries_Test()
    {
        // Arrange
        EntryFixture item1 = new("file1.txt", "body", DateTimeOffset.UtcNow);
        EntryFixture item2 = new("nested/", "", DateTimeOffset.UtcNow);
        EntryFixture item3 = new("nested/file2.txt", "body", DateTimeOffset.UtcNow);
        CreateTestArchive(item1, item2, item3);

        string targetArchive = Path.Combine(_tempDirectory, "dest.zip");
        string expectedFile1 = targetArchive + "_file1.txt";
        string expectedFile2 = targetArchive + "_nested_file2.txt";

        // Act
        List<string> extractedFiles = _archiveCopyService.ExtractFiles(_testArchiveName, targetArchive, OverwriteParams.No);

        // Assert
        await Assert.That(extractedFiles).IsEquivalentTo([expectedFile1, expectedFile2]);
        await Assert.That(File.Exists(expectedFile1)).IsTrue();
        await Assert.That(File.Exists(expectedFile2)).IsTrue();
    }

    [Test]
    public async Task ExtractFiles_NoOverwrite_SkipsExistingFile()
    {
        // Arrange
        EntryFixture item = new("file1.txt", "body", DateTimeOffset.UtcNow);
        CreateTestArchive(item);

        string targetArchive = Path.Combine(_tempDirectory, "dest.zip");
        string expectedFile = targetArchive + "_file1.txt";
        _fileSystemMock.FileExists(expectedFile).Returns(true);

        // Act
        List<string> extractedFiles = _archiveCopyService.ExtractFiles(_testArchiveName, targetArchive, OverwriteParams.No);

        // Assert
        await Assert.That(extractedFiles).IsEquivalentTo([expectedFile]);
        await Assert.That(File.Exists(expectedFile)).IsFalse();
    }

    [Test]
    public async Task ExtractFiles_OverwriteNewer_ExtractsMissingDestinationFile()
    {
        // Arrange
        EntryFixture item = new("file1.txt", "new body", DateTimeOffset.UtcNow);
        CreateTestArchive(item);

        string targetArchive = Path.Combine(_tempDirectory, "dest.zip");
        string expectedFile = targetArchive + "_file1.txt";
        _fileSystemMock.FileExists(expectedFile).Returns(false);

        // Act
        List<string> extractedFiles = _archiveCopyService.ExtractFiles(_testArchiveName, targetArchive, OverwriteParams.Newer);

        // Assert
        await Assert.That(extractedFiles).IsEquivalentTo([expectedFile]);
        await Assert.That(File.Exists(expectedFile)).IsTrue();
        await Assert.That(await File.ReadAllTextAsync(expectedFile)).IsEqualTo("new body");
    }

    [Test]
    public async Task ExtractFiles_OverwriteNewer_OverwritesExistingOlderFile()
    {
        // Arrange
        EntryFixture item = new("file1.txt", "new body", DateTimeOffset.UtcNow);
        CreateTestArchive(item);

        string targetArchive = Path.Combine(_tempDirectory, "dest.zip");
        string expectedFile = targetArchive + "_file1.txt";
        _fileSystemMock.FileExists(expectedFile).Returns(true);
        _fileSystemMock.GetFileInformation(expectedFile).Returns(new FileInformation(DateTimeOffset.UtcNow.AddDays(-1), "body".Length));

        // Act
        List<string> extractedFiles = _archiveCopyService.ExtractFiles(_testArchiveName, targetArchive, OverwriteParams.Newer);

        // Assert
        await Assert.That(extractedFiles).IsEquivalentTo([expectedFile]);
        await Assert.That(File.Exists(expectedFile)).IsTrue();
        await Assert.That(await File.ReadAllTextAsync(expectedFile)).IsEqualTo("new body");
    }

    [Test]
    public async Task ExtractFiles_OverwriteNewer_SkipsExistingSameFile()
    {
        // Arrange
        EntryFixture item = new("file1.txt", "body", DateTimeOffset.UtcNow.AddDays(-1));
        CreateTestArchive(item);

        string targetArchive = Path.Combine(_tempDirectory, "dest.zip");
        string expectedFile = targetArchive + "_file1.txt";
        _fileSystemMock.FileExists(expectedFile).Returns(true);
        _fileSystemMock.GetFileInformation(expectedFile).Returns(new FileInformation(DateTimeOffset.UtcNow, "body".Length));

        // Act
        List<string> extractedFiles = _archiveCopyService.ExtractFiles(_testArchiveName, targetArchive, OverwriteParams.Newer);

        // Assert
        await Assert.That(extractedFiles).IsEquivalentTo([expectedFile]);
        await Assert.That(File.Exists(expectedFile)).IsFalse();
    }

    [Test]
    public async Task ExtractFiles_OverwriteYes_AlwaysOverwritesExistingFile()
    {
        // Arrange
        EntryFixture item = new("file1.txt", "new body", DateTimeOffset.UtcNow);
        CreateTestArchive(item);

        string targetArchive = Path.Combine(_tempDirectory, "dest.zip");
        string expectedFile = targetArchive + "_file1.txt";
        _fileSystemMock.FileExists(expectedFile).Returns(true);
        _fileSystemMock.GetFileInformation(expectedFile).Returns(new FileInformation(DateTimeOffset.UtcNow.AddDays(-1), "new body".Length));

        // Act
        List<string> extractedFiles = _archiveCopyService.ExtractFiles(_testArchiveName, targetArchive, OverwriteParams.Yes);

        // Assert
        await Assert.That(extractedFiles).IsEquivalentTo([expectedFile]);
        await Assert.That(File.Exists(expectedFile)).IsTrue();
        await Assert.That(await File.ReadAllTextAsync(expectedFile)).IsEqualTo("new body");
    }

    private static string CreateTempDirectory()
    {
        string tempDirectory = Path.Combine(Path.GetTempPath(), $"ArchiveCopyServiceTests_{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDirectory);
        return tempDirectory;
    }

    private record EntryFixture(string Name, string Content, DateTimeOffset LastWriteTimeUtc);

    private void CreateTestArchive(params EntryFixture[] entries)
    {
        CreateArchive(_testArchiveName, entries);
    }

    private static void CreateArchive(string archivePath, params EntryFixture[] entries)
    {
        using ZipArchive archive = ZipFile.Open(archivePath, ZipArchiveMode.Create);
        foreach (EntryFixture item in entries)
        {
            ZipArchiveEntry entry = archive.CreateEntry(item.Name);
            entry.LastWriteTime = item.LastWriteTimeUtc;

            using StreamWriter writer = new(entry.Open());
            writer.Write(item.Content);
        }
    }
}
