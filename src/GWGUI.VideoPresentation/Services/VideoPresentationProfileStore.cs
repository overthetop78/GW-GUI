using System.Text.Json;
using System.Text.Json.Serialization;

namespace GWGUI.VideoPresentation.Services;

/// <summary>Host-owned video profiles stored independently from machine configurations.</summary>
public sealed class VideoPresentationProfileStore(
    string directory,
    IVideoProfileFiles? fileSystem = null, Func<Action, Task>? schedule = null)
{
    private readonly IVideoProfileFiles _files = fileSystem ?? new VideoProfileFiles();
    private readonly Func<Action, Task> _schedule = schedule ?? Task.Run;
    private readonly object _gate = new();
    private readonly object _writeGate = new();
    private readonly HashSet<(string Module, Guid Id)> _pending = [];
    private readonly Dictionary<(string Module, Guid Id), EmulationVideoPresentationProfile> _profiles = [];
    private static readonly JsonSerializerOptions JsonOptions = CreateJsonOptions();

    public EmulationVideoPresentationProfile Get(string module, Guid id)
    {
        lock (_gate)
        {
            if (_profiles.TryGetValue((module, id), out var cached)) return cached;
            var path = ProfilePath(module, id);
            EmulationVideoPresentationProfile profile;
            if (_files.Exists(path))
                profile = (JsonSerializer.Deserialize<EmulationVideoPresentationProfile>(
                    _files.Read(path), JsonOptions)
                    ?? throw new InvalidDataException(path)).Normalize();
            else
                profile = new EmulationVideoPresentationProfile().Normalize();
            _profiles[(module, id)] = profile;
            return profile;
        }
    }

    public void Set(string module, Guid id, EmulationVideoPresentationProfile profile)
    {
        lock (_gate)
        {
            Get(module, id);
            _profiles[(module, id)] = profile.Normalize();
        }
    }

    public void Save(string module, Guid id)
    {
        lock (_writeGate)
        {
            var profile = Get(module, id);
            Write(ProfilePath(module, id), profile);
            lock (_gate)
                if (ReferenceEquals(_profiles[(module, id)], profile))
                    _pending.Remove((module, id));
        }
    }

    public Task SaveAsync(string module, Guid id)
    {
        lock (_gate) _pending.Add((module, id));
        return _schedule(() => SavePending(module, id));
    }

    private void SavePending(string module, Guid id)
    {
        lock (_writeGate)
        {
            lock (_gate)
                if (!_pending.Contains((module, id))) return;
            Save(module, id);
        }
    }

    public void FlushPending()
    {
        while (true)
        {
            (string Module, Guid Id)[] pending;
            lock (_gate) pending = _pending.ToArray();
            if (pending.Length == 0) return;
            foreach (var key in pending) SavePending(key.Module, key.Id);
        }
    }

    public void Copy(string module, Guid source, Guid destination)
    {
        lock (_writeGate)
        lock (_gate)
        {
            var profile = Get(module, source);
            Write(ProfilePath(module, destination), profile);
            _profiles[(module, destination)] = profile;
        }
    }

    public void Delete(string module, Guid id)
    {
        lock (_writeGate)
        lock (_gate)
        {
            _files.Delete(ProfilePath(module, id));
            _profiles.Remove((module, id));
            _pending.Remove((module, id));
        }
    }

    private string ProfilePath(string module, Guid id)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(module);
        // Encoding the module ID prevents paths escaping the host-owned directory.
        var key = Convert.ToHexString(System.Text.Encoding.UTF8.GetBytes(module));
        return Path.Combine(directory, key,
            id.ToString(VideoPresentationStorageConstants.IdentifierFormat)
                + VideoPresentationStorageConstants.FileExtension);
    }

    private void Write(string path, EmulationVideoPresentationProfile profile)
    {
        _files.WriteAtomically(path, stream => JsonSerializer.Serialize(stream, profile.Normalize(), JsonOptions));
    }

    private static JsonSerializerOptions CreateJsonOptions()
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web) { WriteIndented = true };
        options.Converters.Add(new JsonStringEnumConverter());
        return options;
    }
}
