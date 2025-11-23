using SharpCompress.Archives;
using SharpCompress.Archives.Zip;
using SharpCompress.Archives.Tar;
using SharpCompress.Archives.SevenZip;
using SharpCompress.Archives.Rar;
using SharpCompress.Archives.GZip;
using SharpCompress.Readers;

namespace ArchiveExtractor;

/// <summary>
/// Provides a simple, memory-safe API for extracting and reading archive files.
/// Supports ZIP, TAR, GZIP, 7-Zip, RAR, BZip2, and their combinations.
/// </summary>
public static class Archive
{
    /// <summary>
    /// Opens an archive file for reading.
    /// </summary>
    /// <param name="filePath">The path to the archive file.</param>
    /// <param name="archiveType">The type of archive. Use ArchiveType.Auto to detect automatically.</param>
    /// <returns>An IArchiveReader to read and extract the archive.</returns>
    /// <exception cref="ArgumentException">Thrown when filePath is null or empty.</exception>
    /// <exception cref="FileNotFoundException">Thrown when the file does not exist.</exception>
    /// <exception cref="NotSupportedException">Thrown when the archive format is not supported.</exception>
    public static IArchiveReader Open(string filePath, ArchiveType archiveType = ArchiveType.Auto)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("File path cannot be null or empty.", nameof(filePath));

        if (!File.Exists(filePath))
            throw new FileNotFoundException("Archive file not found.", filePath);

        IArchive archive;

        if (archiveType == ArchiveType.Auto)
        {
            archiveType = DetectArchiveType(filePath);
        }

        try
        {
            archive = archiveType switch
            {
                ArchiveType.Zip => ZipArchive.Open(filePath),
                ArchiveType.Tar => TarArchive.Open(filePath),
                ArchiveType.TarGz => TarArchive.Open(filePath),
                ArchiveType.TarBz2 => TarArchive.Open(filePath),
                ArchiveType.SevenZip => SevenZipArchive.Open(filePath),
                ArchiveType.Rar => RarArchive.Open(filePath),
                ArchiveType.GZip => GZipArchive.Open(filePath),
                ArchiveType.BZip2 => ArchiveFactory.Open(filePath),
                _ => ArchiveFactory.Open(filePath)
            };
        }
        catch (Exception ex)
        {
            throw new NotSupportedException($"Unable to open archive: {ex.Message}", ex);
        }

        return new ArchiveReader(archive);
    }

    /// <summary>
    /// Opens an archive from a stream for reading.
    /// </summary>
    /// <param name="stream">The stream containing the archive data.</param>
    /// <param name="archiveType">The type of archive. Use ArchiveType.Auto to detect automatically.</param>
    /// <returns>An IArchiveReader to read and extract the archive.</returns>
    /// <exception cref="ArgumentNullException">Thrown when stream is null.</exception>
    /// <exception cref="NotSupportedException">Thrown when the archive format is not supported.</exception>
    public static IArchiveReader Open(Stream stream, ArchiveType archiveType = ArchiveType.Auto)
    {
        if (stream == null)
            throw new ArgumentNullException(nameof(stream));

        IArchive archive;

        try
        {
            archive = archiveType switch
            {
                ArchiveType.Auto => ArchiveFactory.Open(stream),
                ArchiveType.Zip => ZipArchive.Open(stream),
                ArchiveType.Tar => TarArchive.Open(stream),
                ArchiveType.TarGz => TarArchive.Open(stream),
                ArchiveType.TarBz2 => TarArchive.Open(stream),
                ArchiveType.SevenZip => SevenZipArchive.Open(stream),
                ArchiveType.Rar => RarArchive.Open(stream),
                ArchiveType.GZip => GZipArchive.Open(stream),
                ArchiveType.BZip2 => ArchiveFactory.Open(stream),
                _ => ArchiveFactory.Open(stream)
            };
        }
        catch (Exception ex)
        {
            throw new NotSupportedException($"Unable to open archive: {ex.Message}", ex);
        }

        return new ArchiveReader(archive);
    }

    /// <summary>
    /// Extracts an archive file to the specified directory.
    /// </summary>
    /// <param name="archiveFilePath">The path to the archive file.</param>
    /// <param name="destinationDirectory">The directory where files will be extracted.</param>
    /// <param name="overwrite">Whether to overwrite existing files.</param>
    /// <param name="archiveType">The type of archive. Use ArchiveType.Auto to detect automatically.</param>
    /// <exception cref="ArgumentException">Thrown when paths are null or empty.</exception>
    /// <exception cref="FileNotFoundException">Thrown when the archive file does not exist.</exception>
    public static void ExtractToDirectory(
        string archiveFilePath,
        string destinationDirectory,
        bool overwrite = false,
        ArchiveType archiveType = ArchiveType.Auto)
    {
        using var reader = Open(archiveFilePath, archiveType);
        reader.ExtractToDirectory(destinationDirectory, overwrite);
    }

    /// <summary>
    /// Lists all entries in an archive file.
    /// </summary>
    /// <param name="archiveFilePath">The path to the archive file.</param>
    /// <param name="archiveType">The type of archive. Use ArchiveType.Auto to detect automatically.</param>
    /// <returns>An enumerable of archive entries.</returns>
    /// <exception cref="ArgumentException">Thrown when filePath is null or empty.</exception>
    /// <exception cref="FileNotFoundException">Thrown when the file does not exist.</exception>
    public static IEnumerable<IArchiveEntry> ListEntries(
        string archiveFilePath,
        ArchiveType archiveType = ArchiveType.Auto)
    {
        using var reader = Open(archiveFilePath, archiveType);
        return reader.Entries.ToList();
    }

    /// <summary>
    /// Detects the archive type based on file extension.
    /// </summary>
    private static ArchiveType DetectArchiveType(string filePath)
    {
        var extension = Path.GetExtension(filePath).ToLowerInvariant();
        var fullName = Path.GetFileName(filePath).ToLowerInvariant();

        return extension switch
        {
            ".zip" => ArchiveType.Zip,
            ".tar" => ArchiveType.Tar,
            ".gz" => fullName.EndsWith(".tar.gz") ? ArchiveType.TarGz : ArchiveType.GZip,
            ".tgz" => ArchiveType.TarGz,
            ".7z" => ArchiveType.SevenZip,
            ".rar" => ArchiveType.Rar,
            ".bz2" => fullName.EndsWith(".tar.bz2") ? ArchiveType.TarBz2 : ArchiveType.BZip2,
            ".tbz" or ".tbz2" => ArchiveType.TarBz2,
            _ => ArchiveType.Auto
        };
    }
}
