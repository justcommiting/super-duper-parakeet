# ArchiveExtractor

A memory-safe, easy-to-use C# library for extracting and reading all major archive file formats. Think 7-Zip or WinRAR, but as a simple API.

## Features

- ✅ **Memory-Safe**: Built with .NET's managed memory and proper resource disposal
- 📦 **Multiple Formats**: ZIP, TAR, GZIP, 7-Zip, RAR, BZip2, and combinations (TAR.GZ, TAR.BZ2)
- 🎯 **Simple API**: Intuitive, fluent interface that's easy to understand
- 🔒 **Safe by Default**: Comprehensive error handling and input validation
- 🚀 **High Performance**: Built on top of SharpCompress library
- 📝 **Well-Documented**: Clear XML documentation for all public APIs

## Supported Archive Formats

| Format | Extension | Read | Extract |
|--------|-----------|------|---------|
| ZIP | `.zip` | ✅ | ✅ |
| TAR | `.tar` | ✅ | ✅ |
| GZIP | `.gz` | ✅ | ✅ |
| TAR+GZIP | `.tar.gz`, `.tgz` | ✅ | ✅ |
| 7-Zip | `.7z` | ✅ | ✅ |
| RAR | `.rar` | ✅ | ✅ |
| BZip2 | `.bz2` | ✅ | ✅ |
| TAR+BZip2 | `.tar.bz2`, `.tbz`, `.tbz2` | ✅ | ✅ |

## Installation

Add the ArchiveExtractor project to your solution or reference the compiled DLL.

```bash
dotnet add reference path/to/ArchiveExtractor.csproj
```

## Quick Start

### Extract an entire archive

```csharp
using ArchiveExtractor;

// Automatically detects archive type from extension
Archive.ExtractToDirectory("myfiles.zip", "extracted/");

// Or specify the type explicitly
Archive.ExtractToDirectory("myfiles.7z", "extracted/", archiveType: ArchiveType.SevenZip);
```

### List archive contents

```csharp
using ArchiveExtractor;

var entries = Archive.ListEntries("myfiles.zip");

foreach (var entry in entries)
{
    Console.WriteLine($"{entry.Key} - {entry.Size} bytes");
}
```

### Read archive with full control

```csharp
using ArchiveExtractor;

using (var archive = Archive.Open("myfiles.tar.gz"))
{
    foreach (var entry in archive.Entries)
    {
        Console.WriteLine($"File: {entry.Key}");
        Console.WriteLine($"  Size: {entry.Size} bytes");
        Console.WriteLine($"  Compressed: {entry.CompressedSize} bytes");
        Console.WriteLine($"  Modified: {entry.LastModifiedTime}");
        
        // Extract specific entry
        archive.ExtractEntry(entry, $"output/{entry.Key}");
    }
}
```

### Extract from a stream

```csharp
using ArchiveExtractor;

using (var fileStream = File.OpenRead("myfiles.zip"))
using (var archive = Archive.Open(fileStream))
{
    archive.ExtractToDirectory("extracted/");
}
```

### Read file contents directly

```csharp
using ArchiveExtractor;

using (var archive = Archive.Open("myfiles.zip"))
{
    var entry = archive.Entries.First(e => e.Key == "readme.txt");
    
    using (var stream = entry.OpenEntryStream())
    using (var reader = new StreamReader(stream))
    {
        string content = reader.ReadToEnd();
        Console.WriteLine(content);
    }
}
```

## API Reference

### Archive (Static Class)

The main entry point for all archive operations.

#### Methods

- **`Open(string filePath, ArchiveType archiveType = ArchiveType.Auto)`**
  - Opens an archive file for reading
  - Returns: `IArchiveReader`

- **`Open(Stream stream, ArchiveType archiveType = ArchiveType.Auto)`**
  - Opens an archive from a stream
  - Returns: `IArchiveReader`

- **`ExtractToDirectory(string archiveFilePath, string destinationDirectory, bool overwrite = false, ArchiveType archiveType = ArchiveType.Auto)`**
  - Extracts an entire archive to a directory

- **`ListEntries(string archiveFilePath, ArchiveType archiveType = ArchiveType.Auto)`**
  - Lists all entries in an archive
  - Returns: `IEnumerable<IArchiveEntry>`

### IArchiveReader

Represents an open archive for reading and extraction.

#### Properties

- **`Entries`**: Gets all entries in the archive

#### Methods

- **`ExtractToDirectory(string destinationDirectory, bool overwrite = false)`**
  - Extracts all entries to the specified directory

- **`ExtractEntry(IArchiveEntry entry, string destinationPath, bool overwrite = false)`**
  - Extracts a specific entry to a file

### IArchiveEntry

Represents a file entry within an archive.

#### Properties

- **`Key`**: The path/name of the entry within the archive
- **`Size`**: Uncompressed size in bytes
- **`CompressedSize`**: Compressed size in bytes
- **`IsDirectory`**: Whether this entry is a directory
- **`LastModifiedTime`**: Last modification timestamp

#### Methods

- **`OpenEntryStream()`**: Opens a stream to read the entry's content

### ArchiveType Enum

- `Auto` - Automatically detect from file extension or content
- `Zip` - ZIP archive
- `Tar` - TAR archive
- `GZip` - GZIP compressed file
- `TarGz` - TAR+GZIP archive
- `SevenZip` - 7-Zip archive
- `Rar` - RAR archive
- `BZip2` - BZip2 compressed file
- `TarBz2` - TAR+BZip2 archive

## Error Handling

The library provides clear exceptions for common error cases:

```csharp
using ArchiveExtractor;

try
{
    Archive.ExtractToDirectory("myfiles.zip", "output/");
}
catch (FileNotFoundException ex)
{
    Console.WriteLine($"Archive not found: {ex.Message}");
}
catch (NotSupportedException ex)
{
    Console.WriteLine($"Unsupported archive format: {ex.Message}");
}
catch (IOException ex)
{
    Console.WriteLine($"I/O error: {ex.Message}");
}
```

## Memory Safety

The library is designed to be memory-safe:

- ✅ Implements `IDisposable` properly for resource cleanup
- ✅ Uses `using` statements in all examples
- ✅ No unsafe code or unmanaged memory
- ✅ Null checking on all public methods
- ✅ Proper exception handling throughout

## Building

```bash
# Build the library
dotnet build src/ArchiveExtractor/ArchiveExtractor.csproj

# Run tests
dotnet test

# Build everything
dotnet build
```

## Testing

The library includes comprehensive unit tests covering:

- All supported archive formats
- Error conditions and edge cases
- Resource disposal
- Memory safety
- API usability

```bash
dotnet test tests/ArchiveExtractor.Tests/ArchiveExtractor.Tests.csproj
```

## License

This project is open source. See LICENSE file for details.

## Dependencies

- [SharpCompress](https://github.com/adamhathcock/sharpcompress) - A compression library for .NET

## Contributing

Contributions are welcome! Please ensure:

1. All tests pass
2. Code follows existing style
3. XML documentation is provided for public APIs
4. Memory safety is maintained