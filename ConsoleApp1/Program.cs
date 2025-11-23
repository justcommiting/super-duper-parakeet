using System;
using System.IO;
using System.Linq;
using ArchiveExtractor;

class Program
{
    static int Main(string[] args)
    {
        if (args.Length == 0 || args.Contains("--help") || args.Contains("-h"))
        {
            PrintHelp();
            return 0;
        }

        try
        {
            var archivePath = args[0];
            if (!File.Exists(archivePath))
            {
                Console.Error.WriteLine($"Error: archive not found: {archivePath}");
                return 2;
            }

            string? dest = null;
            bool listOnly = false;
            bool overwrite = false;
            string? entryToExtract = null;
            string? password = null;

            for (int i = 1; i < args.Length; i++)
            {
                var a = args[i];
                switch (a)
                {
                    case "--dest":
                    case "-d":
                        if (i + 1 < args.Length)
                        {
                            dest = args[++i];
                        }
                        break;
                    case "--list":
                    case "-l":
                        listOnly = true;
                        break;
                    case "--overwrite":
                    case "-o":
                        overwrite = true;
                        break;
                    case "--entry":
                    case "-e":
                        if (i + 1 < args.Length)
                        {
                            entryToExtract = args[++i];
                        }
                        break;
                    case "--password":
                    case "-p":
                        if (i + 1 < args.Length && !args[i + 1].StartsWith("-"))
                        {
                            password = args[++i];
                        }
                        else
                        {
                            // prompt for password without echo
                            password = ReadPasswordFromConsole();
                        }
                        break;
                    default:
                        Console.Error.WriteLine($"Unknown option: {a}");
                        PrintHelp();
                        return 3;
                }
            }

            if (listOnly)
            {
                ListArchive(archivePath, password);
                return 0;
            }

            if (!string.IsNullOrEmpty(entryToExtract))
            {
                // Extract a single entry
                using var archive = Archive.Open(archivePath, ArchiveType.Auto, password);
                var match = archive.Entries.FirstOrDefault(e => string.Equals(e.Key, entryToExtract, StringComparison.OrdinalIgnoreCase)
                                                               || e.Key.EndsWith(entryToExtract, StringComparison.OrdinalIgnoreCase)
                                                               || e.Key.Contains(entryToExtract, StringComparison.OrdinalIgnoreCase));
                if (match == null)
                {
                    Console.Error.WriteLine($"Entry not found in archive: {entryToExtract}");
                    return 4;
                }

                string outPath;
                if (string.IsNullOrEmpty(dest))
                {
                    // place next to current directory with same file name
                    var fileName = Path.GetFileName(match.Key);
                    outPath = Path.Combine(Directory.GetCurrentDirectory(), fileName);
                }
                else
                {
                    // if dest is a directory, combine
                    if (Directory.Exists(dest) || dest.EndsWith(Path.DirectorySeparatorChar.ToString()) || dest.EndsWith(Path.AltDirectorySeparatorChar.ToString()))
                    {
                        var fileName = Path.GetFileName(match.Key);
                        outPath = Path.Combine(dest, fileName);
                    }
                    else
                    {
                        // treat dest as file path
                        outPath = dest;
                    }
                }

                // Ensure destination directory exists
                var parent = Path.GetDirectoryName(outPath);
                if (!string.IsNullOrEmpty(parent) && !Directory.Exists(parent))
                    Directory.CreateDirectory(parent);

                archive.ExtractEntry(match, outPath, overwrite: overwrite);
                Console.WriteLine($"Extracted '{match.Key}' -> {outPath}");
                return 0;
            }

            // Extract entire archive
            var extractDir = dest ?? Path.Combine(Directory.GetCurrentDirectory(), Path.GetFileNameWithoutExtension(archivePath));
            if (!Directory.Exists(extractDir))
                Directory.CreateDirectory(extractDir);

            Archive.ExtractToDirectory(archivePath, extractDir, overwrite: overwrite, password: password);
            Console.WriteLine($"Extracted archive to: {extractDir}");
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Fatal error: {ex.Message}");
            return 100;
        }
    }

    static void ListArchive(string path, string password)
    {
        try
        {
            var entries = Archive.ListEntries(path, ArchiveType.Auto, password);
            Console.WriteLine($"Entries in {path}:");
            foreach (var e in entries)
            {
                Console.WriteLine($"  {e.Key} ({e.Size} bytes)");
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Unable to list archive: {ex.Message}");
        }
    }

    static string ReadPasswordFromConsole()
    {
        Console.Write("Password: ");
        var pwd = string.Empty;
        ConsoleKeyInfo key;
        while ((key = Console.ReadKey(intercept: true)).Key != ConsoleKey.Enter)
        {
            if (key.Key == ConsoleKey.Backspace)
            {
                if (pwd.Length > 0)
                {
                    pwd = pwd[..^1];
                    Console.Write("\b \b");
                }
            }
            else
            {
                pwd += key.KeyChar;
                Console.Write('*');
            }
        }
        Console.WriteLine();
        return pwd;
    }

    static void PrintHelp()
    {
        Console.WriteLine("Usage: extract <archive> [options]\n");
        Console.WriteLine("Options:");
        Console.WriteLine("  -h, --help         Show this help message");
        Console.WriteLine("  -l, --list         List entries in the archive");
        Console.WriteLine("  -d, --dest <path>  Destination directory or file for extraction");
        Console.WriteLine("  -e, --entry <path> Extract a single entry (name or partial match)");
        Console.WriteLine("  -o, --overwrite    Overwrite existing files");
        Console.WriteLine("  -p, --password     Password for encrypted archives (use no value to prompt)");
        Console.WriteLine("\nExamples:");
        Console.WriteLine("  extract sample.zip -l");
        Console.WriteLine("  extract sample.zip -d out_dir");
        Console.WriteLine("  extract sample.zip -e docs/readme.txt -d readme.txt");
    }
}