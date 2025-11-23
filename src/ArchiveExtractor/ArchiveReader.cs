using SharpCompress.Archives;
using SharpCompress.Common;
using System.Collections.Concurrent;
using System.Threading.Tasks;

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

        var entries = _archive.Entries.Where(e => !e.IsDirectory).ToList();

        // Try parallel extraction for maximum throughput, fall back to sequential if it fails.
        try
        {
            ExtractEntriesInParallel(entries, destinationDirectory, overwrite);
        }
        catch
        {
            // Sequential fallback
            foreach (var entry in entries)
            {
                try
                {
                    entry.WriteToDirectory(destinationDirectory, options);
                }
                catch
                {
                    // continue extracting other entries; individual errors are ignored here
                }
            }
        }
    }

    private void ExtractEntriesInParallel(IEnumerable<SharpCompress.Archives.IArchiveEntry> entries, string destinationDirectory, bool overwrite)
    {
        var list = entries.ToList();
        var exceptions = new ConcurrentBag<Exception>();

        var maxWorkers = Math.Clamp(Environment.ProcessorCount * 2, 1, 64);
        var po = new ParallelOptions { MaxDegreeOfParallelism = maxWorkers };

        Parallel.ForEach(list, po, entry =>
        {
            try
            {
                var destPath = Path.Combine(destinationDirectory, entry.Key.Replace('/', Path.DirectorySeparatorChar));
                var dir = Path.GetDirectoryName(destPath);
                if (!string.IsNullOrEmpty(dir))
                    Directory.CreateDirectory(dir);

                if (File.Exists(destPath) && !overwrite)
                    return; // skip

                // Use asynchronous/sequential-optimized file stream for faster writes on many systems
                using var entryStream = entry.OpenEntryStream();
                using var outFs = new FileStream(destPath, FileMode.Create, FileAccess.Write, FileShare.None, 81920, FileOptions.Asynchronous | FileOptions.SequentialScan);
                entryStream.CopyTo(outFs);
            }
            catch (Exception ex)
            {
                exceptions.Add(ex);
            }
        });

        if (!exceptions.IsEmpty)
            throw new AggregateException(exceptions);
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
        // Use buffered write for performance
        using var fileStream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write, FileShare.None, 81920, FileOptions.SequentialScan);
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
