using ArchiveExtractor;
using System.IO.Compression;
using System.Text;

namespace ArchiveExtractor.Example;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("=== ArchiveExtractor Example ===\n");

        // Create a sample ZIP archive for demonstration
        var sampleZipPath = CreateSampleArchive();
        Console.WriteLine($"Created sample archive: {sampleZipPath}\n");

        // Example 1: List archive contents
        Console.WriteLine("Example 1: List Archive Contents");
        Console.WriteLine("----------------------------------");
        ListArchiveContents(sampleZipPath);
        Console.WriteLine();

        // Example 2: Extract entire archive
        Console.WriteLine("Example 2: Extract Entire Archive");
        Console.WriteLine("----------------------------------");
        ExtractEntireArchive(sampleZipPath);
        Console.WriteLine();

        // Example 3: Extract specific file
        Console.WriteLine("Example 3: Extract Specific File");
        Console.WriteLine("----------------------------------");
        ExtractSpecificFile(sampleZipPath);
        Console.WriteLine();

        // Example 4: Read file contents directly
        Console.WriteLine("Example 4: Read File Contents Directly");
        Console.WriteLine("---------------------------------------");
        ReadFileContents(sampleZipPath);
        Console.WriteLine();

        // Cleanup
        CleanupExampleFiles(sampleZipPath);
        
        Console.WriteLine("\nExample completed successfully!");
    }

    static string CreateSampleArchive()
    {
        var tempPath = Path.GetTempPath();
        var zipPath = Path.Combine(tempPath, $"sample_{Guid.NewGuid()}.zip");

        using (var zip = ZipFile.Open(zipPath, ZipArchiveMode.Create))
        {
            // Create a text file
            var textEntry = zip.CreateEntry("documents/readme.txt");
            using (var writer = new StreamWriter(textEntry.Open(), Encoding.UTF8))
            {
                writer.WriteLine("Welcome to ArchiveExtractor!");
                writer.WriteLine("This is a sample archive for demonstration purposes.");
                writer.WriteLine("The library makes it easy to work with archive files.");
            }

            // Create another file
            var dataEntry = zip.CreateEntry("data/info.txt");
            using (var writer = new StreamWriter(dataEntry.Open(), Encoding.UTF8))
            {
                writer.WriteLine("Archive Information:");
                writer.WriteLine($"Created: {DateTime.Now}");
                writer.WriteLine("Format: ZIP");
            }

            // Create a third file
            var logEntry = zip.CreateEntry("logs/app.log");
            using (var writer = new StreamWriter(logEntry.Open(), Encoding.UTF8))
            {
                writer.WriteLine("[INFO] Application started");
                writer.WriteLine("[INFO] Processing data");
                writer.WriteLine("[INFO] Application completed successfully");
            }
        }

        return zipPath;
    }

    static void ListArchiveContents(string archivePath)
    {
        var entries = Archive.ListEntries(archivePath);
        
        Console.WriteLine($"Found {entries.Count()} files in archive:");
        foreach (var entry in entries)
        {
            Console.WriteLine($"  • {entry.Key}");
            Console.WriteLine($"    Size: {entry.Size} bytes (compressed: {entry.CompressedSize} bytes)");
            Console.WriteLine($"    Modified: {entry.LastModifiedTime}");
        }
    }

    static void ExtractEntireArchive(string archivePath)
    {
        var extractPath = Path.Combine(Path.GetTempPath(), $"extracted_{Guid.NewGuid()}");
        
        Archive.ExtractToDirectory(archivePath, extractPath);
        
        Console.WriteLine($"Extracted archive to: {extractPath}");
        Console.WriteLine("Extracted files:");
        
        foreach (var file in Directory.GetFiles(extractPath, "*", SearchOption.AllDirectories))
        {
            var relativePath = Path.GetRelativePath(extractPath, file);
            Console.WriteLine($"  • {relativePath}");
        }
    }

    static void ExtractSpecificFile(string archivePath)
    {
        using var archive = Archive.Open(archivePath);
        var entry = archive.Entries.First(e => e.Key.Contains("readme.txt"));
        
        var extractPath = Path.Combine(Path.GetTempPath(), "extracted_readme.txt");
        archive.ExtractEntry(entry, extractPath, overwrite: true);
        
        Console.WriteLine($"Extracted '{entry.Key}' to: {extractPath}");
        Console.WriteLine("File contents:");
        Console.WriteLine(File.ReadAllText(extractPath));
    }

    static void ReadFileContents(string archivePath)
    {
        using var archive = Archive.Open(archivePath);
        
        Console.WriteLine("Reading file contents without extracting:");
        
        foreach (var entry in archive.Entries)
        {
            Console.WriteLine($"\n--- {entry.Key} ---");
            using var stream = entry.OpenEntryStream();
            using var reader = new StreamReader(stream);
            var content = reader.ReadToEnd();
            Console.WriteLine(content);
        }
    }

    static void CleanupExampleFiles(string archivePath)
    {
        try
        {
            if (File.Exists(archivePath))
                File.Delete(archivePath);

            var tempPath = Path.GetTempPath();
            foreach (var dir in Directory.GetDirectories(tempPath, "extracted_*"))
            {
                try { Directory.Delete(dir, true); } catch { }
            }

            foreach (var file in Directory.GetFiles(tempPath, "extracted_readme.txt"))
            {
                try { File.Delete(file); } catch { }
            }
        }
        catch
        {
            // Ignore cleanup errors
        }
    }
}
