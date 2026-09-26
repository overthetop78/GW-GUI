using GWGUI.Emulation;

namespace GWGUI.Emulation.Atari.Common.Machines.Common.Functions;

internal static class CassetteBootFunctions
{
    internal static bool IsRequested(MachineConfiguration configuration)
    {
        var configured = configuration.Options.GetValueOrDefault(
            EightBitSettingsConstants.CassetteBootOptionKey);
        return string.Equals(configured, EightBitSettingsConstants.Enabled,
                   StringComparison.OrdinalIgnoreCase)
               || configuration.Media.Any(media => media.Category == MediaCategory.Cassette
                                                    && media.IsInserted && media.CassetteAutoBoot);
    }

    internal static bool RequiresDelayedReturn(MachineConfiguration configuration) =>
        configuration.Core == Emulator.Atari800
        && configuration.Model is MachineModel.Atari400 or MachineModel.Atari800
        && configuration.Media.Any(media => media.Category == MediaCategory.Cassette && media.IsInserted)
        && IsRequested(configuration);
}

internal static class CassetteStateFunctions
{
    internal static EmulationCassetteState From(bool mounted, bool motorActive) =>
        !mounted ? EmulationCassetteState.Empty
        : motorActive ? EmulationCassetteState.Playing
        : EmulationCassetteState.Stopped;
}
