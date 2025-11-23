namespace ArchiveExtractor;

/// <summary>
/// Represents a reader for archive files.
/// </summary>
public interface IArchiveReader : IDisposable
{
    /// <summary>
    /// Gets all entries in the archive.
    /// </summary>
    IEnumerable<IArchiveEntry> Entries { get; }

    /// <summary>
    /// Extracts all entries to the specified destination directory.
    /// </summary>
    /// <param name="destinationDirectory">The directory where files will be extracted.</param>
    /// <param name="overwrite">Whether to overwrite existing files.</param>
    void ExtractToDirectory(string destinationDirectory, bool overwrite = false);

    /// <summary>
    /// Extracts a specific entry to the specified file path.
    /// </summary>
    /// <param name="entry">The entry to extract.</param>
    /// <param name="destinationPath">The destination file path.</param>
    /// <param name="overwrite">Whether to overwrite existing file.</param>
    void ExtractEntry(IArchiveEntry entry, string destinationPath, bool overwrite = false);
}
