namespace GWGUI.VideoPresentation.Services;

public interface IVideoProfileFiles
{
    bool Exists(string path);
    string Read(string path);
    void WriteAtomically(string path, Action<Stream> write);
    void Delete(string path);
}

internal sealed class VideoProfileFiles : IVideoProfileFiles
{
    public bool Exists(string path) => File.Exists(path);
    public string Read(string path) => File.ReadAllText(path);
    public void Delete(string path) => File.Delete(path);
    public void WriteAtomically(string path, Action<Stream> write)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        var temporary = path + Guid.NewGuid().ToString(VideoPresentationStorageConstants.IdentifierFormat)
            + VideoPresentationStorageConstants.TemporaryExtension;
        try
        {
            using (var stream = new FileStream(temporary, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            {
                write(stream);
                stream.Flush(flushToDisk: true);
            }
            File.Move(temporary, path, overwrite: true);
        }
        finally { if (File.Exists(temporary)) File.Delete(temporary); }
    }
}
