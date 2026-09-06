namespace GWGUI.Infrastructure.Settings;

public interface ISettingsFileSystem
{
    bool Exists(string path);
    Stream OpenRead(string path);
    Stream Create(string path);
    void CreateDirectory(string path);
    void Copy(string source, string destination, bool overwrite);
    void Move(string source, string destination, bool overwrite);
}

internal sealed class PhysicalSettingsFileSystem : ISettingsFileSystem
{
    public bool Exists(string path) => File.Exists(path);
    public Stream OpenRead(string path) => File.OpenRead(path);
    public Stream Create(string path) => File.Create(path);
    public void CreateDirectory(string path) => Directory.CreateDirectory(path);
    public void Copy(string source, string destination, bool overwrite) => File.Copy(source, destination, overwrite);
    public void Move(string source, string destination, bool overwrite) => File.Move(source, destination, overwrite);
}
