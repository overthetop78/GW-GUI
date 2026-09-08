namespace GWGUI.Emulation.Atari.Functions;

internal static class AtariCassetteBootFunctions
{
    internal static bool IsRequested(AtariMachineConfiguration configuration)
    {
        var configured = configuration.Options.GetValueOrDefault(
            AtariEightBitSettingsConstants.CassetteBootOptionKey);
        return string.Equals(configured, AtariEightBitSettingsConstants.Enabled,
                   StringComparison.OrdinalIgnoreCase)
               || configuration.Media.Any(media => media.Category == AtariMediaCategory.Cassette
                                                    && media.IsInserted && media.CassetteAutoBoot);
    }

    internal static bool RequiresDelayedReturn(AtariMachineConfiguration configuration) =>
        configuration.Core == AtariEmulator.Atari800
        && configuration.Model is AtariMachineModel.Atari400 or AtariMachineModel.Atari800
        && configuration.Media.Any(media => media.Category == AtariMediaCategory.Cassette && media.IsInserted)
        && IsRequested(configuration);
}
