using System.Text;
namespace GWGUI.Infrastructure.Processes;

public interface ILogFileSystem
{
    void CreateDirectory(string path);
    bool Exists(string path);
    long Length(string path);
    void Delete(string path);
    void Move(string source, string destination);
    Task AppendAsync(string path, string text, CancellationToken cancellationToken = default);
    Task<string[]> ReadLinesAsync(string path);
    Task WriteLinesAsync(string path, IEnumerable<string> lines);
}

internal sealed class LogFileSystem : ILogFileSystem
{
    public void CreateDirectory(string path) => Directory.CreateDirectory(path);
    public bool Exists(string path) => File.Exists(path);
    public long Length(string path) => new FileInfo(path).Length;
    public void Delete(string path) => File.Delete(path);
    public void Move(string source, string destination) => File.Move(source, destination);
    public Task AppendAsync(string path, string text, CancellationToken cancellationToken = default) => File.AppendAllTextAsync(path,text,new UTF8Encoding(false),cancellationToken);
    public Task<string[]> ReadLinesAsync(string path) => File.ReadAllLinesAsync(path);
    public Task WriteLinesAsync(string path, IEnumerable<string> lines) => File.WriteAllLinesAsync(path,lines,new UTF8Encoding(false));
}
