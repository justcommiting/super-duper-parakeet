namespace ArchiveExtractor;

/// <summary>
/// Represents an entry (file or directory) within an archive.
/// </summary>
public interface IArchiveEntry
{
    /// <summary>
    /// Gets the key (path) of the entry within the archive.
    /// </summary>
    string Key { get; }

    /// <summary>
    /// Gets the uncompressed size of the entry in bytes.
    /// </summary>
    long Size { get; }

    /// <summary>
    /// Gets the compressed size of the entry in bytes.
    /// </summary>
    long CompressedSize { get; }

    /// <summary>
    /// Gets a value indicating whether this entry is a directory.
    /// </summary>
    bool IsDirectory { get; }

    /// <summary>
    /// Gets the last modified time of the entry.
    /// </summary>
    DateTime? LastModifiedTime { get; }

    /// <summary>
    /// Opens a stream to read the entry's content.
    /// </summary>
    /// <returns>A readable stream for the entry's content.</returns>
    Stream OpenEntryStream();
}
