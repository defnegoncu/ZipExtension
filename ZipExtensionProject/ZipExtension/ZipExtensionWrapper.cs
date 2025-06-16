using System.IO.Abstractions;
using System.IO.Compression;

namespace ZipExtension;

/// <inheritdoc cref="ZipExtension"/>
public class ZipExtensionWrapper
{
    /// <summary>
    /// Gets the file system used by this wrapper.
    /// </summary>
    public IFileSystem FileSystem { get; private set; }

    
    public ZipExtensionWrapper(IFileSystem fileSystem)
    {
        FileSystem = fileSystem;
    }
    
    /// <inheritdoc cref="ZipExtension.GetZipArchive"/>
    public ZipArchive GetZipArchive(string archivePath)
    {
        return ZipExtension.GetZipArchive(FileSystem, archivePath);
    }

    /// <inheritdoc cref="ZipExtension.ExtractToDirectory"/>
    public void ExtractToDirectory(ZipArchive archive, string destination)
    {
        ZipExtension.ExtractToDirectory(archive, FileSystem, destination);
    }

    /// <inheritdoc cref="ZipExtension.CreateFromDirectoryAsync"/>
    public async Task CreateFromDirectoryAsync(string sourcePath, string destinationPath)
    {
        await ZipExtension.CreateFromDirectoryAsync(FileSystem, sourcePath, destinationPath);
    }
}
