namespace GWGUI.Emulation.Atari.Common.Machines.AtariST.Functions;



internal static class TosHeaderReader
{
    private const int HeaderLength = 30;
    private const int BranchOpcodeOffset = 0;
    private const int BranchDisplacementOffset = 1;
    private const int VersionMajorOffset = 2;
    private const int VersionMinorOffset = 3;
    private const int ConfigurationHighOffset = 28;
    private const int ConfigurationLowOffset = 29;
    private const byte ExpectedBranchOpcode = 0x60;
    private const byte OriginalTosBranchDisplacement = 0x1E;
    private const byte LaterTosBranchDisplacement = 0x2E;

    internal static async Task<TosHeader?> ReadAsync(string path, CancellationToken cancellationToken)
    {
        var file = new FileInfo(path);
        if (file.Length > int.MaxValue) return null;
        var bytes = new byte[(int)file.Length];
        await using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read,
            FirmwareConstants.FileBufferSize, FileOptions.Asynchronous | FileOptions.SequentialScan);
        await stream.ReadExactlyAsync(bytes, cancellationToken).ConfigureAwait(false);
        if (bytes.Length < HeaderLength ||
            bytes[BranchOpcodeOffset] != ExpectedBranchOpcode ||
            !IsKnownBranchDisplacement(bytes[BranchDisplacementOffset]))
            return null;

        var headerVersion = $"{bytes[VersionMajorOffset]:X}.{bytes[VersionMinorOffset]:X2}";
        var text = System.Text.Encoding.ASCII.GetString(bytes);
        var variant = text.Contains(TosHeaderReaderConstants.EmuTOS, StringComparison.OrdinalIgnoreCase)
            ? TosVariant.EmuTos
            : text.Contains(TosHeaderReaderConstants.KAOS, StringComparison.OrdinalIgnoreCase)
                ? TosVariant.KaosTos
                : TosVariant.Atari;
        var version = variant switch
        {
            TosVariant.EmuTos => FindVersion(text, TosHeaderReaderConstants.EmuTOS, headerVersion),
            TosVariant.KaosTos => FindVersion(text, TosHeaderReaderConstants.KAOSTOS, headerVersion),
            _ => headerVersion
        };
        var countryCode = ((bytes[ConfigurationHighOffset] << 8) | bytes[ConfigurationLowOffset]) >> 1;
        return new(version, RegionForCountryCode(countryCode), variant, file.Length);
    }

    private static bool IsKnownBranchDisplacement(byte displacement) =>
        displacement is OriginalTosBranchDisplacement or LaterTosBranchDisplacement;

    private static string FindVersion(string text, string product, string fallback)
    {
        var productVersion = System.Text.RegularExpressions.Regex.Match(text,
            System.Text.RegularExpressions.Regex.Escape(product) + TosHeaderReaderConstants.Value09016Version090913,
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        if (productVersion.Success) return productVersion.Groups[TosHeaderReaderConstants.Version].Value;

        var standaloneVersion = System.Text.RegularExpressions.Regex.Match(text,
            TosHeaderReaderConstants.Value09Version009090209);
        return standaloneVersion.Success ? standaloneVersion.Groups[TosHeaderReaderConstants.Version].Value : fallback;
    }

    private static StRegion RegionForCountryCode(int countryCode) => countryCode switch
    {
        0 => StRegion.UnitedStates,
        1 => StRegion.Germany,
        2 => StRegion.France,
        3 => StRegion.UnitedKingdom,
        4 => StRegion.Spain,
        5 => StRegion.Italy,
        6 => StRegion.Sweden,
        7 => StRegion.Switzerland,
        8 => StRegion.Norway,
        12 => StRegion.CzechRepublic,
        15 => StRegion.Finland,
        18 => StRegion.Russia,
        30 => StRegion.Greece,
        _ => StRegion.Multilingual
    };
}
