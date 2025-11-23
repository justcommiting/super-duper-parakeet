namespace ArchiveExtractor;

/// <summary>
/// Represents supported archive types.
/// </summary>
public enum ArchiveType
{
    /// <summary>
    /// Automatically detect archive type from file extension or content.
    /// </summary>
    Auto,

    /// <summary>
    /// ZIP archive format.
    /// </summary>
    Zip,

    /// <summary>
    /// TAR archive format.
    /// </summary>
    Tar,

    /// <summary>
    /// GZIP compressed format.
    /// </summary>
    GZip,

    /// <summary>
    /// TAR.GZ archive format.
    /// </summary>
    TarGz,

    /// <summary>
    /// 7-Zip archive format.
    /// </summary>
    SevenZip,

    /// <summary>
    /// RAR archive format.
    /// </summary>
    Rar,

    /// <summary>
    /// BZip2 compressed format.
    /// </summary>
    BZip2,

    /// <summary>
    /// TAR.BZ2 archive format.
    /// </summary>
    TarBz2
}
