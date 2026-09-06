using GWGUI.Infrastructure.Settings;
using System.Text;
namespace GWGUI.Tests.Application.TestInfrastructure;

internal sealed class MemoryLogFiles : GWGUI.Infrastructure.Processes.ILogFileSystem
{
    public Dictionary<string,string> Files = [];
    public List<string> Calls = [];
    public bool Fail;
    private void Call(string action) { Calls.Add(action); if(Fail) throw new IOException("synthetic log failure"); }
    public void CreateDirectory(string path) => Call("directory");
    public bool Exists(string path) { Call("exists"); return Files.ContainsKey(path); }
    public long Length(string path) { Call("length"); return Encoding.UTF8.GetByteCount(Files[path]); }
    public void Delete(string path) { Call("delete"); Files.Remove(path); }
    public void Move(string source,string destination) { Call("move"); Files.Add(destination,Files[source]); Files.Remove(source); }
    public Task AppendAsync(string path,string text,CancellationToken cancellationToken = default)
    { cancellationToken.ThrowIfCancellationRequested(); Call("append"); Files[path] = Files.GetValueOrDefault(path,"") + text; return Task.CompletedTask; }
    public Task<string[]> ReadLinesAsync(string path)
    { Call("read"); using var reader = new StringReader(Files[path]); var lines = new List<string>(); while(reader.ReadLine() is { } line) lines.Add(line); return Task.FromResult(lines.ToArray()); }
    public Task WriteLinesAsync(string path,IEnumerable<string> lines)
    { Call("write"); Files[path] = string.Concat(lines.Select(x => x + Environment.NewLine)); return Task.CompletedTask; }
}

internal sealed class MemoryVideoProfileFiles : GWGUI.VideoPresentation.Services.IVideoProfileFiles
{
    public Dictionary<string,string> Files = [];
    public List<string> Writes = [];
    public bool FailWrite;
    public bool Exists(string path) => Files.ContainsKey(path);
    public string Read(string path) => Files[path];
    public void Delete(string path) => Files.Remove(path);
    public void WriteAtomically(string path, Action<Stream> write)
    {
        using var stream = new MemoryStream(); write(stream);
        if (FailWrite) throw new IOException("synthetic video profile failure");
        Files[path] = Encoding.UTF8.GetString(stream.ToArray()); Writes.Add(path);
    }
}

internal sealed class MemoryImageFiles : GWGUI.MediaEngine.Containers.Storage.IAtomicImageFileWriter
{
    public Dictionary<string, byte[]> Files { get; } = new(StringComparer.OrdinalIgnoreCase);
    public List<string> Calls { get; } = [];
    public bool FailCommit { get; set; }
    public async Task WriteAsync(string path, Func<Stream, CancellationToken, Task> write, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested(); Calls.Add(path);
        using var output = new MemoryStream();
        await write(output, cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();
        if (FailCommit) throw new IOException("synthetic storage failure");
        Files[path] = output.ToArray();
    }
}

internal sealed class SyntheticData : ISettingsFileSystem
{
    public Dictionary<string,byte[]> Files { get; } = new(StringComparer.OrdinalIgnoreCase);
    public List<string> Calls { get; } = [];
    public string? FailOperation { get; set; }
    public void Seed(string path,string text) => Files[path]=Encoding.UTF8.GetBytes(text);
    public string Text(string path)=>Encoding.UTF8.GetString(Files[path]);
    private void Call(string operation,string path)
    {
        Calls.Add(operation+":"+path);
        if(FailOperation==operation) throw new IOException("simulated "+operation+" failure");
    }
    public bool Exists(string path) { Call("exists",path);return Files.ContainsKey(path); }
    public Stream OpenRead(string path) { Call("read",path);return new MemoryStream(Files[path],false); }
    public Stream Create(string path)
    {
        Call("create",path);
        return new CommitStream(bytes=>Files[path]=bytes);
    }
    public void CreateDirectory(string path)=>Call("directory",path);
    public void Copy(string source,string destination,bool overwrite)
    {
        Call("copy",destination);
        if(!overwrite && Files.ContainsKey(destination))throw new IOException("destination exists");
        Files[destination]=(byte[])Files[source].Clone();
    }
    public void Move(string source,string destination,bool overwrite)
    {
        Call("move",destination);
        if(!overwrite && Files.ContainsKey(destination))throw new IOException("destination exists");
        Files[destination]=Files[source];
        Files.Remove(source);
    }
    private sealed class CommitStream(Action<byte[]> commit):MemoryStream
    {
        protected override void Dispose(bool disposing)
        {
            if(disposing)commit(ToArray());
            base.Dispose(disposing);
        }
    }
}
