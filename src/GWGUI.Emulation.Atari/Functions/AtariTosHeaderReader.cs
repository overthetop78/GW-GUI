namespace GWGUI.Emulation.Atari.Functions;



internal static class AtariTosHeaderReader
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

    internal static async Task<AtariTosHeader?> ReadAsync(string path, CancellationToken cancellationToken)
    {
        var file = new FileInfo(path);
        if (file.Length > int.MaxValue) return null;
        var bytes = new byte[(int)file.Length];
        await using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read,
            AtariFirmwareConstants.FileBufferSize, FileOptions.Asynchronous | FileOptions.SequentialScan);
        await stream.ReadExactlyAsync(bytes, cancellationToken).ConfigureAwait(false);
        if (bytes.Length < HeaderLength ||
            bytes[BranchOpcodeOffset] != ExpectedBranchOpcode ||
            !IsKnownBranchDisplacement(bytes[BranchDisplacementOffset]))
            return null;

        var headerVersion = $"{bytes[VersionMajorOffset]:X}.{bytes[VersionMinorOffset]:X2}";
        var text = System.Text.Encoding.ASCII.GetString(bytes);
        var variant = text.Contains(AtariTosHeaderReaderConstants.EmuTOS, StringComparison.OrdinalIgnoreCase)
            ? AtariTosVariant.EmuTos
            : text.Contains(AtariTosHeaderReaderConstants.KAOS, StringComparison.OrdinalIgnoreCase)
                ? AtariTosVariant.KaosTos
                : AtariTosVariant.Atari;
        var version = variant switch
        {
            AtariTosVariant.EmuTos => FindVersion(text, AtariTosHeaderReaderConstants.EmuTOS, headerVersion),
            AtariTosVariant.KaosTos => FindVersion(text, AtariTosHeaderReaderConstants.KAOSTOS, headerVersion),
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
            System.Text.RegularExpressions.Regex.Escape(product) + AtariTosHeaderReaderConstants.Value09016Version090913,
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        if (productVersion.Success) return productVersion.Groups[AtariTosHeaderReaderConstants.Version].Value;

        var standaloneVersion = System.Text.RegularExpressions.Regex.Match(text,
            AtariTosHeaderReaderConstants.Value09Version009090209);
        return standaloneVersion.Success ? standaloneVersion.Groups[AtariTosHeaderReaderConstants.Version].Value : fallback;
    }

    private static AtariStRegion RegionForCountryCode(int countryCode) => countryCode switch
    {
        0 => AtariStRegion.UnitedStates,
        1 => AtariStRegion.Germany,
        2 => AtariStRegion.France,
        3 => AtariStRegion.UnitedKingdom,
        4 => AtariStRegion.Spain,
        5 => AtariStRegion.Italy,
        6 => AtariStRegion.Sweden,
        7 => AtariStRegion.Switzerland,
        8 => AtariStRegion.Norway,
        12 => AtariStRegion.CzechRepublic,
        15 => AtariStRegion.Finland,
        18 => AtariStRegion.Russia,
        30 => AtariStRegion.Greece,
        _ => AtariStRegion.Multilingual
    };
}
