using SharpCompress.Archives;
using SharpCompress.Common;

namespace ArchiveExtractor;

/// <summary>
/// Implements IArchiveReader using SharpCompress.
/// </summary>
internal class ArchiveReader : IArchiveReader
{
    private readonly IArchive _archive;
    private bool _disposed;

    public ArchiveReader(IArchive archive)
    {
        _archive = archive ?? throw new ArgumentNullException(nameof(archive));
    }

    public IEnumerable<IArchiveEntry> Entries
    {
        get
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(ArchiveReader));

            return _archive.Entries
                .Where(e => !e.IsDirectory)
                .Select(e => new ArchiveEntry((SharpCompress.Archives.IArchiveEntry)e));
        }
    }

    public void ExtractToDirectory(string destinationDirectory, bool overwrite = false)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(ArchiveReader));

        if (string.IsNullOrWhiteSpace(destinationDirectory))
            throw new ArgumentException("Destination directory cannot be null or empty.", nameof(destinationDirectory));

        Directory.CreateDirectory(destinationDirectory);

        var options = new ExtractionOptions
        {
            ExtractFullPath = true,
            Overwrite = overwrite
        };

        foreach (var entry in _archive.Entries.Where(e => !e.IsDirectory))
        {
            entry.WriteToDirectory(destinationDirectory, options);
        }
    }

    public void ExtractEntry(IArchiveEntry entry, string destinationPath, bool overwrite = false)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(ArchiveReader));

        if (entry == null)
            throw new ArgumentNullException(nameof(entry));

        if (string.IsNullOrWhiteSpace(destinationPath))
            throw new ArgumentException("Destination path cannot be null or empty.", nameof(destinationPath));

        if (entry is not ArchiveEntry archiveEntry)
            throw new ArgumentException("Entry must be from this archive reader.", nameof(entry));

        var directory = Path.GetDirectoryName(destinationPath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        if (File.Exists(destinationPath) && !overwrite)
            throw new IOException($"File already exists: {destinationPath}");

        using var entryStream = entry.OpenEntryStream();
        using var fileStream = File.Create(destinationPath);
        entryStream.CopyTo(fileStream);
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _archive?.Dispose();
            _disposed = true;
        }
    }
}
