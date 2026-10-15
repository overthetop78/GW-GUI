using System.Globalization;
using GWGUI.Emulation.Sony.Emulators.Pcee2.Constants;
using GWGUI.Emulation.Sony.Emulators.Pcee2.Enums;

namespace GWGUI.Emulation.Sony.Emulators.Pcee2.Functions;

internal static class AudioConfigurationFunctions
{
    internal static IReadOnlyList<int> Latencies { get; } = Enum.GetValues<AudioBufferMs>().Select(value => (int)value).ToArray();

    internal static AudioConfiguration Resolve(MachineConfiguration configuration)
    {
        var audio = configuration.Audio ?? new AudioConfiguration();
        var latency = audio.LatencyMilliseconds;
        if (configuration.Audio is null && int.TryParse(configuration.Options?.GetValueOrDefault(OptionConstants.AudioBufferMsKey),
                NumberStyles.Integer, CultureInfo.InvariantCulture, out var previous) && Latencies.Contains(previous))
            latency = previous;
        return audio with { LatencyMilliseconds = Latencies.OrderBy(value => Math.Abs((long)value - latency)).ThenBy(value => value).First() };
    }

    internal static MachineConfiguration Configure(MachineConfiguration configuration)
    {
        var audio = Resolve(configuration);
        var options = new Dictionary<string, string>(configuration.Options ?? new Dictionary<string, string>(), StringComparer.Ordinal)
        {
            [OptionConstants.AudioBufferMsKey] = OptionValueFunctions.ToNative((AudioBufferMs)audio.LatencyMilliseconds)
        };
        return configuration with { Audio = audio, Options = options };
    }
}
