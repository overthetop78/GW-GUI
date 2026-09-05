using System.Text.Json;
using System.Text.Json.Serialization;

namespace GWGUI.VideoPresentation.Services;

/// <summary>Host-owned profiles. Legacy data is read before any module may replace its JSON.</summary>
public sealed class VideoPresentationProfileStore(
    string directory, Func<string, Guid, IEnumerable<string>>? legacyPaths = null)
{
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
            if (File.Exists(path))
                profile = (JsonSerializer.Deserialize<EmulationVideoPresentationProfile>(
                    File.ReadAllText(path), JsonOptions)
                    ?? throw new InvalidDataException(path)).Normalize();
            else
            {
                var legacy = ReadLegacy(module, id);
                profile = legacy ?? new EmulationVideoPresentationProfile().Normalize();
                // Only an actual legacy profile requires a migration write.
                // Keep the source untouched, and never cache a migration whose write failed.
                if (legacy is not null) Write(path, profile);
            }
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
        return Task.Run(() => SavePending(module, id));
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
            File.Delete(ProfilePath(module, id));
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

    private EmulationVideoPresentationProfile? ReadLegacy(string module, Guid id)
    {
        foreach (var path in legacyPaths?.Invoke(module, id) ?? [])
        {
            if (!File.Exists(path)) continue;
            using var document = JsonDocument.Parse(File.ReadAllText(path));
            var renderer = EmulationVideoRenderer.Direct3D11;
            EmulationVideoProcessingConfiguration? processing = null;
            var found = false;
            foreach (var property in document.RootElement.EnumerateObject())
            {
                if (property.Name.Equals(VideoPresentationStorageConstants.LegacyRenderer,
                    StringComparison.OrdinalIgnoreCase))
                {
                    renderer = property.Value.Deserialize<EmulationVideoRenderer>(JsonOptions);
                    found = true;
                }
                if (property.Name.Equals(VideoPresentationStorageConstants.LegacyProcessing,
                    StringComparison.OrdinalIgnoreCase))
                {
                    processing = property.Value.Deserialize<EmulationVideoProcessingConfiguration>(JsonOptions);
                    found = true;
                }
            }
            if (found) return new EmulationVideoPresentationProfile(renderer, processing).Normalize();
        }
        return null;
    }

    private static void Write(string path, EmulationVideoPresentationProfile profile)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        var temporary = path + Guid.NewGuid().ToString(VideoPresentationStorageConstants.IdentifierFormat)
            + VideoPresentationStorageConstants.TemporaryExtension;
        try
        {
            using (var stream = new FileStream(temporary, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            {
                JsonSerializer.Serialize(stream, profile.Normalize(), JsonOptions);
                stream.Flush(flushToDisk: true);
            }
            File.Move(temporary, path, overwrite: true);
        }
        finally { if (File.Exists(temporary)) File.Delete(temporary); }
    }

    private static JsonSerializerOptions CreateJsonOptions()
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web) { WriteIndented = true };
        options.Converters.Add(new JsonStringEnumConverter());
        options.Converters.Add(new LegacyBooleanJsonConverter());
        return options;
    }
}
