using System.Collections.Concurrent;
using System.IO;
using GWGUI.MediaEngine.Contracts;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using C = GWGUI.MediaEngine.Constants.RecordedMechanicalSoundConstants;

namespace GWGUI.MediaEngine.Functions;

/// <summary>Lecture des échantillons réels : gain constant, rééchantillonnage commun et horloge fournie par l’appelant.</summary>
internal sealed class RecordedMechanicalSoundFunctions
{
    private readonly IReadOnlyDictionary<string, RecordedMechanicalSoundProfile> _profiles;
    private readonly Func<RecordedMechanicalSoundSample, Stream> _openSample;
    private readonly IReadOnlyDictionary<string, RecordedMechanicalSoundSample> _sampleDefinitions;
    private readonly ConcurrentDictionary<(string Sample, int Rate), Lazy<float[]>> _samples = new();
    private readonly ConcurrentDictionary<string, Lazy<double>> _durations = new(StringComparer.Ordinal);
    private static readonly Lazy<RecordedMechanicalSoundFunctions> External = new(LoadAssets);
    internal static RecordedMechanicalSoundFunctions Shared => External.Value;

    internal RecordedMechanicalSoundFunctions(RecordedMechanicalSoundCatalog document, Func<RecordedMechanicalSoundSample, Stream> openSample)
    {
        ArgumentNullException.ThrowIfNull(document);
        _profiles = document.Profiles.ToDictionary(profile => profile.Id, StringComparer.Ordinal);
        _openSample = openSample;
        _sampleDefinitions = document.Profiles.SelectMany(profile => profile.Samples).GroupBy(sample => sample.Id, StringComparer.Ordinal).ToDictionary(group => group.Key, group => group.First(), StringComparer.Ordinal);
    }

    internal IReadOnlyList<RecordedMechanicalSoundProfile> Profiles => _profiles.Values.ToArray();
    internal bool Contains(string id) => _profiles.ContainsKey(id);
    internal static string? BaseProfile(string device) => device switch
    {
        Constants.MechanicalSoundConstants.Floppy => C.DefaultFloppyProfile,
        Constants.MechanicalSoundConstants.HardDisk => C.DefaultHardDiskProfile,
        Constants.MechanicalSoundConstants.Optical => C.DefaultOpticalProfile,
        _ => null
    };
    internal RecordedMechanicalSoundProfile Profile(string id) => _profiles.TryGetValue(id, out var profile) ? profile : throw new ArgumentOutOfRangeException(nameof(id));

    private static RecordedMechanicalSoundFunctions LoadAssets()
    {
        var directory = Path.Combine(AppContext.BaseDirectory, C.AssetsDirectory, C.BankDirectory);
        return new(Constants.RecordedMechanicalSoundFiles.All, sample => File.OpenRead(Path.Combine(directory, sample.FilePath)));
    }

    internal double Duration(string profileId, string operation, int distance = 1, int direction = 1, int cylinder = -1)
    {
        var sample = SelectWithFallback(Profile(profileId), operation, distance, direction, cylinder);
        return sample is null ? 0 : _durations.GetOrAdd(sample.Id, id => new Lazy<double>(() => ReadDuration(id))).Value;
    }

    internal short[] Render(string device, string operation, int sampleRate, int count, long offset, string profileId,
        double volumePercent = C.DefaultVolumePercent, int distance = 1, int direction = 1, int cylinder = -1)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(sampleRate);
        ArgumentOutOfRangeException.ThrowIfNegative(count);
        ArgumentOutOfRangeException.ThrowIfNegative(offset);
        if (!double.IsFinite(volumePercent) || volumePercent is < 0 or > C.MaximumVolumePercent) throw new ArgumentOutOfRangeException(nameof(volumePercent));
        var profile = Profile(profileId);
        if (profile.Device != device) throw new ArgumentException(nameof(device));
        var result = new short[count];
        var sample = SelectWithFallback(profile, operation, distance, direction, cylinder);
        if (sample is null || volumePercent == 0) return result;
        var source = Samples(sample.Id, sampleRate);
        if (source.Length == 0) return result;
        var looping = sample.Operations.Any(item => item.Operation == operation && item.Loop);
        var startSample = looping ? SelectWithFallback(profile, C.Start, distance, direction, cylinder) : null;
        var start = startSample is null ? [] : Samples(startSample.Id, sampleRate);
        var gain = volumePercent / C.MaximumVolumePercent;
        for (var index = 0; index < count; index++)
        {
            var position = offset + index;
            double value;
            if (position < start.Length) value = start[(int)position];
            else
            {
                position -= start.Length;
                if (!looping && position >= source.Length) break;
                value = source[(int)(looping ? position % source.Length : position)];
            }
            result[index] = (short)Math.Clamp(Math.Round(value * gain * C.PcmScale), short.MinValue, short.MaxValue);
        }
        return result;
    }

    private RecordedMechanicalSoundSample? SelectWithFallback(RecordedMechanicalSoundProfile profile, string operation, int distance, int direction, int cylinder)
    {
        var sample = Select(profile, operation, distance, direction, cylinder);
        if (sample is not null) return sample;
        var fallback = BaseProfile(profile.Device);
        return fallback is not null && fallback != profile.Id
            ? Select(Profile(fallback), operation, distance, direction, cylinder)
            : null;
    }

    private static RecordedMechanicalSoundSample? Select(RecordedMechanicalSoundProfile profile, string operation, int distance, int direction, int cylinder)
    {
        var candidates = profile.Samples.SelectMany(sample => sample.Operations.Where(item => item.Operation == operation).Select(item => (Sample: sample, Operation: item)));
        return candidates.OrderBy(candidate => candidate.Operation.Direction != 0 && candidate.Operation.Direction != Math.Sign(direction))
            .ThenBy(candidate => candidate.Operation.Distance == 0 ? int.MaxValue : Math.Abs(candidate.Operation.Distance - Math.Abs(distance)))
            .ThenBy(candidate => cylinder < 0 || candidate.Operation.Cylinder < 0 ? 0 : Math.Abs(candidate.Operation.Cylinder - cylinder))
            .Select(candidate => candidate.Sample).FirstOrDefault();
    }

    private float[] Samples(string id, int rate) => _samples.GetOrAdd((id, rate), key => new Lazy<float[]>(() => Read(key.Sample, key.Rate))).Value;

    /// <summary>Lit la durée du signal décodé dans le fichier, sans métadonnée de catalogue.</summary>
    private double ReadDuration(string id)
    {
        var definition = _sampleDefinitions[id];
        using var stream = _openSample(definition);
        using var reader = OpenReader(stream, definition.FilePath);
        var source = reader.ToSampleProvider();
        var buffer = new float[C.ReadBufferLength];
        long sampleCount = 0;
        int read;
        while ((read = source.Read(buffer, 0, buffer.Length)) > 0) sampleCount += read;
        return (double)sampleCount / source.WaveFormat.Channels / source.WaveFormat.SampleRate;
    }

    /// <summary>Ouvre le décodeur du fichier ; l'appelant possède le lecteur retourné et son flux.</summary>
    private static WaveStream OpenReader(Stream stream, string path) =>
        path.EndsWith(Constants.DiskImageFileExtensions.Flac, StringComparison.OrdinalIgnoreCase)
            ? new StreamMediaFoundationReader(stream, new MediaFoundationReader.MediaFoundationReaderSettings { RequestFloatOutput = true })
            : new WaveFileReader(stream);

    private float[] Read(string id, int sampleRate)
    {
        var definition = _sampleDefinitions[id];
        using var stream = _openSample(definition);
        using var reader = OpenReader(stream, definition.FilePath);
        ISampleProvider source = reader.ToSampleProvider();
        if (source.WaveFormat.Channels == 2) source = new StereoToMonoSampleProvider(source) { LeftVolume = (float)C.StereoChannelGain, RightVolume = (float)C.StereoChannelGain };
        if (source.WaveFormat.Channels != 1) throw new InvalidDataException(id);
        if (source.WaveFormat.SampleRate != sampleRate) source = new WdlResamplingSampleProvider(source, sampleRate);
        var samples = new List<float>();
        var buffer = new float[C.ReadBufferLength];
        int read;
        while ((read = source.Read(buffer, 0, buffer.Length)) > 0) samples.AddRange(buffer.AsSpan(0, read).ToArray());
        return samples.ToArray();
    }
}
