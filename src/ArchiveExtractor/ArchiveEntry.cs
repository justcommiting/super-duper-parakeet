using SharpCompress.Archives;

namespace ArchiveExtractor;

/// <summary>
/// Implements IArchiveEntry using SharpCompress.
/// </summary>
internal class ArchiveEntry : IArchiveEntry
{
    private readonly SharpCompress.Archives.IArchiveEntry _entry;

    public ArchiveEntry(SharpCompress.Archives.IArchiveEntry entry)
    {
        _entry = entry ?? throw new ArgumentNullException(nameof(entry));
    }

    public string Key => _entry.Key ?? string.Empty;

    public long Size => _entry.Size;

    public long CompressedSize => _entry.CompressedSize;

    public bool IsDirectory => _entry.IsDirectory;

    public DateTime? LastModifiedTime => _entry.LastModifiedTime;

    public Stream OpenEntryStream()
    {
        return _entry.OpenEntryStream();
    }
}
