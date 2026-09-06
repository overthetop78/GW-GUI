namespace GWGUI.Infrastructure.HostTools;

/// <summary>Storage boundary for managed Host Tools installations.</summary>
public interface IInstallationFileSystem
{
    bool FileExists(string? path);
    bool DirectoryExists(string path);
    IEnumerable<string> EnumerateDirectories(string path);
    IEnumerable<string> EnumerateFiles(string path, string pattern, SearchOption option);
    IEnumerable<string> EnumerateEntries(string path);
    void CreateDirectory(string path);
    Stream CreateFile(string path);
    void MoveDirectory(string source, string destination);
    void MoveFile(string source, string destination);
    void DeleteDirectory(string path, bool recursive = false);
}

internal sealed class InstallationFileSystem : IInstallationFileSystem
{
    public bool FileExists(string? path) => File.Exists(path);
    public bool DirectoryExists(string path) => Directory.Exists(path);
    public IEnumerable<string> EnumerateDirectories(string path) => Directory.EnumerateDirectories(path);
    public IEnumerable<string> EnumerateFiles(string path, string pattern, SearchOption option) => Directory.EnumerateFiles(path, pattern, option);
    public IEnumerable<string> EnumerateEntries(string path) => Directory.EnumerateFileSystemEntries(path);
    public void CreateDirectory(string path) => Directory.CreateDirectory(path);
    public Stream CreateFile(string path) => File.Create(path);
    public void MoveDirectory(string source, string destination) => Directory.Move(source, destination);
    public void MoveFile(string source, string destination) => File.Move(source, destination);
    public void DeleteDirectory(string path, bool recursive = false) => Directory.Delete(path, recursive);
}
