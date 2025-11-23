using System.IO.Compression;
using System.Text;
using Xunit;

namespace ArchiveExtractor.Tests;

public class ArchiveTests : IDisposable
{
    private readonly string _testDirectory;
    private readonly string _extractDirectory;

    public ArchiveTests()
    {
        _testDirectory = Path.Combine(Path.GetTempPath(), $"ArchiveTests_{Guid.NewGuid()}");
        _extractDirectory = Path.Combine(Path.GetTempPath(), $"ExtractTests_{Guid.NewGuid()}");
        Directory.CreateDirectory(_testDirectory);
        Directory.CreateDirectory(_extractDirectory);
    }

    public void Dispose()
    {
        if (Directory.Exists(_testDirectory))
            Directory.Delete(_testDirectory, true);
        if (Directory.Exists(_extractDirectory))
            Directory.Delete(_extractDirectory, true);
    }

    [Fact]
    public void Open_WithNullPath_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => Archive.Open((string)null!));
    }

    [Fact]
    public void Open_WithEmptyPath_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => Archive.Open(string.Empty));
    }

    [Fact]
    public void Open_WithNonExistentFile_ThrowsFileNotFoundException()
    {
        var nonExistentPath = Path.Combine(_testDirectory, "nonexistent.zip");
        Assert.Throws<FileNotFoundException>(() => Archive.Open(nonExistentPath));
    }

    [Fact]
    public void Open_WithNullStream_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => Archive.Open((Stream)null!));
    }

    [Fact]
    public void Open_WithZipFile_ReturnsReader()
    {
        // Arrange
        var zipPath = CreateTestZipArchive();

        // Act
        using var reader = Archive.Open(zipPath);

        // Assert
        Assert.NotNull(reader);
        var entries = reader.Entries.ToList();
        Assert.NotEmpty(entries);
    }

    [Fact]
    public void Entries_ReturnsAllFiles()
    {
        // Arrange
        var zipPath = CreateTestZipArchive();

        // Act
        using var reader = Archive.Open(zipPath);
        var entries = reader.Entries.ToList();

        // Assert
        Assert.Equal(2, entries.Count);
        Assert.Contains(entries, e => e.Key.Contains("test1.txt"));
        Assert.Contains(entries, e => e.Key.Contains("test2.txt"));
    }

    [Fact]
    public void ExtractToDirectory_ExtractsAllFiles()
    {
        // Arrange
        var zipPath = CreateTestZipArchive();
        var extractPath = Path.Combine(_extractDirectory, "extracted1");

        // Act
        using var reader = Archive.Open(zipPath);
        reader.ExtractToDirectory(extractPath);

        // Assert
        Assert.True(File.Exists(Path.Combine(extractPath, "test1.txt")));
        Assert.True(File.Exists(Path.Combine(extractPath, "test2.txt")));
    }

    [Fact]
    public void ExtractToDirectory_WithOverwrite_OverwritesFiles()
    {
        // Arrange
        var zipPath = CreateTestZipArchive();
        var extractPath = Path.Combine(_extractDirectory, "extracted2");
        Directory.CreateDirectory(extractPath);
        File.WriteAllText(Path.Combine(extractPath, "test1.txt"), "old content");

        // Act
        using var reader = Archive.Open(zipPath);
        reader.ExtractToDirectory(extractPath, overwrite: true);

        // Assert
        var content = File.ReadAllText(Path.Combine(extractPath, "test1.txt"));
        Assert.Contains("Test file 1", content);
    }

    [Fact]
    public void ExtractEntry_ExtractsSingleFile()
    {
        // Arrange
        var zipPath = CreateTestZipArchive();
        var extractPath = Path.Combine(_extractDirectory, "single.txt");

        // Act
        using var reader = Archive.Open(zipPath);
        var entry = reader.Entries.First(e => e.Key.Contains("test1.txt"));
        reader.ExtractEntry(entry, extractPath);

        // Assert
        Assert.True(File.Exists(extractPath));
        var content = File.ReadAllText(extractPath);
        Assert.Contains("Test file 1", content);
    }

    [Fact]
    public void ExtractToDirectory_StaticMethod_ExtractsAllFiles()
    {
        // Arrange
        var zipPath = CreateTestZipArchive();
        var extractPath = Path.Combine(_extractDirectory, "static_extract");

        // Act
        Archive.ExtractToDirectory(zipPath, extractPath);

        // Assert
        Assert.True(File.Exists(Path.Combine(extractPath, "test1.txt")));
        Assert.True(File.Exists(Path.Combine(extractPath, "test2.txt")));
    }

    [Fact]
    public void ListEntries_ReturnsAllEntries()
    {
        // Arrange
        var zipPath = CreateTestZipArchive();

        // Act
        var entries = Archive.ListEntries(zipPath).ToList();

        // Assert
        Assert.Equal(2, entries.Count);
        Assert.All(entries, e => Assert.False(e.IsDirectory));
    }

    [Fact]
    public void ArchiveEntry_Properties_AreAccessible()
    {
        // Arrange
        var zipPath = CreateTestZipArchive();

        // Act
        using var reader = Archive.Open(zipPath);
        var entry = reader.Entries.First();

        // Assert
        Assert.NotNull(entry.Key);
        Assert.True(entry.Size > 0);
        Assert.False(entry.IsDirectory);
        Assert.NotNull(entry.LastModifiedTime);
    }

    [Fact]
    public void OpenEntryStream_AllowsReadingContent()
    {
        // Arrange
        var zipPath = CreateTestZipArchive();

        // Act
        using var reader = Archive.Open(zipPath);
        var entry = reader.Entries.First(e => e.Key.Contains("test1.txt"));
        using var stream = entry.OpenEntryStream();
        using var streamReader = new StreamReader(stream);
        var content = streamReader.ReadToEnd();

        // Assert
        Assert.Contains("Test file 1", content);
    }

    [Fact]
    public void Dispose_DisposesResources()
    {
        // Arrange
        var zipPath = CreateTestZipArchive();
        var reader = Archive.Open(zipPath);

        // Act
        reader.Dispose();

        // Assert
        Assert.Throws<ObjectDisposedException>(() => reader.Entries.ToList());
    }

    [Fact]
    public void Open_WithStream_ReturnsReader()
    {
        // Arrange
        var zipPath = CreateTestZipArchive();
        using var fileStream = File.OpenRead(zipPath);

        // Act
        using var reader = Archive.Open(fileStream);

        // Assert
        Assert.NotNull(reader);
        var entries = reader.Entries.ToList();
        Assert.NotEmpty(entries);
    }

    private string CreateTestZipArchive()
    {
        var zipPath = Path.Combine(_testDirectory, $"test_{Guid.NewGuid()}.zip");
        
        using (var zip = ZipFile.Open(zipPath, ZipArchiveMode.Create))
        {
            var entry1 = zip.CreateEntry("test1.txt");
            using (var writer = new StreamWriter(entry1.Open(), Encoding.UTF8))
            {
                writer.Write("Test file 1 content");
            }

            var entry2 = zip.CreateEntry("test2.txt");
            using (var writer = new StreamWriter(entry2.Open(), Encoding.UTF8))
            {
                writer.Write("Test file 2 content");
            }
        }

        return zipPath;
    }
}
