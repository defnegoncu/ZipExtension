using System.IO.Abstractions;
using System.IO.Compression;

namespace ZipExtension;

public class ZipExtension
{
    /// <summary>
    /// Gets the file system used by this wrapper.
    /// </summary>
    public IFileSystem FileSystem { get; private set; }

    
    public ZipExtension(IFileSystem fileSystem)
    {
        FileSystem = fileSystem;
    }
    
    /// <summary>
    /// Opens a zip archive in read mode.
    /// </summary>
    /// <param name="archivePath">The path to the zip archive file.</param>
    /// <returns>A <see cref="ZipArchive"/> instance representing the archive.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="fileSystemReference"/> or <paramref name="archivePath"/> is null.</exception>
    /// <exception cref="FileNotFoundException">Thrown if the specified file does not exist.</exception>
    /// <exception cref="UnauthorizedAccessException">Thrown if the caller does not have the required permissions.</exception>
    /// <exception cref="InvalidDataException">Thrown if the file is not a valid zip archive.</exception>
    public ZipArchive OpenReadZipArchive(string archivePath)
    {
        var zipStream = FileSystem.File.OpenRead(archivePath);
        return new ZipArchive(zipStream, ZipArchiveMode.Read);
    }

    /// <summary>
    /// Extracts all entries from a zip archive to the specified directory.
    /// </summary>
    /// <param name="archive">The zip archive to extract.</param>
    /// <param name="destination">The directory to extract the archive's contents to.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="destination"/> is null.</exception>
    /// <exception cref="UnauthorizedAccessException">Thrown if the caller does not have write access to the destination directory.</exception>
    /// <exception cref="IOException">Thrown if an I/O error occurs during extraction.</exception>
    public void ExtractZipArchiveToDirectory(ZipArchive archive, string destination)
    {
        EnsureDestinationDirectoryExists(destination);

        foreach (var entry in archive.Entries)
        {
            var extractedEntryPath = NormalizeExtractedEntryPathForWindowsAndUnix(entry);
            var fullPath = Path.Combine(destination, extractedEntryPath);
            CreateEntryDirectory(fullPath);
            
            
            using var destinationStream = FileSystem.File.Create(fullPath);
            using var sourceStream = entry.Open();
            sourceStream.CopyTo(destinationStream);
        }
    }

    /// <summary>
    /// Creates a zip archive from the contents of a directory asynchronously.
    /// </summary>
    /// <param name="sourcePath">The source directory to compress.</param>
    /// <param name="destinationPath">The path of the resulting zip archive.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="source"/> or <paramref name="destination"/> is null.</exception>
    /// <exception cref="DirectoryNotFoundException">Thrown if the source directory does not exist.</exception>
    /// <exception cref="UnauthorizedAccessException">Thrown if the caller does not have the required permissions.</exception>
    /// <exception cref="IOException">Thrown if an I/O error occurs during compression.</exception>
    public async Task CreateZipArchiveFromDirectoryAsync(string sourcePath, string destinationPath)
    {
        ValidatePath(sourcePath);
        ValidatePath(destinationPath);
        using var archive = GetWritableZipArchive(destinationPath);
        
        var files = GetAllFilesExceptSymbolicLinksToPreventLoops( sourcePath);
        foreach (var file in files)
        {
            var relativePath = Path.GetRelativePath(sourcePath, file);
            await AddFileToArchiveAsync(archive, relativePath, file);
        }
    }
    private void EnsureDestinationDirectoryExists(string destination)
    {
        if (!FileSystem.Directory.Exists(destination))
        {
            FileSystem.Directory.CreateDirectory(destination);
        }
    }
    private string NormalizeExtractedEntryPathForWindowsAndUnix(ZipArchiveEntry entry)
    {
        return Path.DirectorySeparatorChar switch
        {
            //adjust paths for unpacking on unix when packed on windows
            '/' => entry.FullName.Replace("\\", "/"),
            //adjust paths for unpacking on windows when packed on unix
            '\\' => entry.FullName.Replace("/", "\\"),
            _ => entry.FullName
        };
    }
    private void CreateEntryDirectory( string fullPath)
    {
        var directoryName = Path.GetDirectoryName(fullPath);
        if (directoryName != null) FileSystem.Directory.CreateDirectory(directoryName);
    }
    private void ValidatePath(string path)
    {
        if (path == null)
            throw new ArgumentNullException(nameof(path), "Path cannot be null.");
        if (path.IndexOfAny(Path.GetInvalidPathChars()) >= 0)
        {
            throw new ArgumentException($"The path is invalid: {path}");
        }
    }
    private async Task AddFileToArchiveAsync( ZipArchive archive,
        string relativePath, string file)
    {
        var entry = archive.CreateEntry(relativePath);
        await using var entryStream = entry.Open();
        await using var fileStream = FileSystem.File.OpenRead(file);
        await fileStream.CopyToAsync(entryStream);
    }
    private string[] GetAllFilesExceptSymbolicLinksToPreventLoops( string source)
    {
        return FileSystem.Directory.GetFiles(source, "*", SearchOption.AllDirectories)
            .Where(file => !FileSystem.File.GetAttributes(file).HasFlag(FileAttributes.ReparsePoint))
            .ToArray();
    }
    private ZipArchive GetWritableZipArchive( string archivePath)
    {
        var zipStream = FileSystem.File.OpenWrite(archivePath);
        return new ZipArchive(zipStream, ZipArchiveMode.Create);
    }
}
