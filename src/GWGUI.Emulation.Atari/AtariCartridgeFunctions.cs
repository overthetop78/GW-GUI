namespace GWGUI.Emulation.Atari;

internal static class AtariCartridgeFunctions
{
    internal static bool Supports(AtariCoreKind core) => AtariCartridgeConstants.CartridgeCores.Contains(core);

    internal static AtariPreparedCartridge Prepare(
        AtariMachineConfiguration machine,
        AtariMediaConfiguration media,
        AtariCoreKind core,
        bool needsFullPath,
        IReadOnlySet<string> reportedExtensions)
    {
        if (!Supports(core)) throw new ArgumentException(AtariCartridgeErrors.UnsupportedCore, nameof(core));
        if (machine.Core != core) throw new ArgumentException(AtariErrorMessages.IncompatibleMedia, nameof(machine));
        if (media.Kind != AtariMediaKind.Cartridge || media.Slot != GWGUI.Emulation.EmulationMediaSlot.Cartridge0)
            throw new ArgumentException(AtariCartridgeErrors.CartridgeRequired, nameof(media));

        var extension = Path.GetExtension(media.Path).TrimStart(AtariConstants.ExtensionPrefix);
        var acceptedExtensions = AtariCartridgeConstants.Extensions[core];
        if (!acceptedExtensions.Contains(extension) || !reportedExtensions.Contains(extension))
            throw new AtariEmulationException(AtariErrorKind.Content, AtariErrorCode.ContentUnsupported,
                AtariCartridgeErrors.ExtensionUnsupported,
                new Dictionary<string, string>
                {
                    [AtariConstants.ExtensionContextKey] = extension,
                    [AtariConstants.SupportedExtensionsContextKey] = string.Join(
                        Cores.AtariContentConstants.ExtensionSeparator,
                        acceptedExtensions.Order(StringComparer.OrdinalIgnoreCase))
                });

        var path = Cores.AtariContentFunctions.Validate(media.Path, reportedExtensions);
        ValidateReadable(path);
        return new AtariPreparedCartridge(media, core, path, needsFullPath);
    }

    internal static void ValidateNoUnsupportedMetadata(AtariMediaConfiguration media)
    {
        if (media.CartridgePlatform is not null || media.CartridgeType is not null)
            throw new ArgumentException(AtariErrorMessages.IncompatibleMedia, nameof(media));
    }

    internal static IReadOnlyDictionary<string, string> ApplyOptions(
        IReadOnlyDictionary<string, string> configuredOptions,
        AtariMediaConfiguration media,
        AtariCoreKind core)
    {
        var options = new Dictionary<string, string>(configuredOptions, StringComparer.Ordinal);
        foreach (var option in GetMediaOptions(media, core)) options[option.Key] = option.Value;
        return options;
    }

    internal static IReadOnlyDictionary<string, string> GetMediaOptions(
        AtariMediaConfiguration media,
        AtariCoreKind core)
    {
        var options = new Dictionary<string, string>(StringComparer.Ordinal);
        if (media.CartridgeRegion is not { } region) return options;
        switch (core)
        {
            case AtariCoreKind.Stella:
                options[AtariCartridgeConstants.StellaRegionOptionKey] = region switch
                {
                    AtariCartridgeRegion.Automatic => AtariCartridgeConstants.AutomaticRegionValue,
                    AtariCartridgeRegion.Ntsc => AtariCartridgeConstants.NtscRegionValue,
                    AtariCartridgeRegion.Pal => AtariCartridgeConstants.PalRegionValue,
                    AtariCartridgeRegion.Secam => AtariCartridgeConstants.SecamRegionValue,
                    _ => throw new ArgumentOutOfRangeException(nameof(media), media, null)
                };
                break;
            case AtariCoreKind.VirtualJaguar:
                options[AtariCartridgeConstants.JaguarRegionOptionKey] = region switch
                {
                    AtariCartridgeRegion.Automatic or AtariCartridgeRegion.Ntsc =>
                        AtariCartridgeConstants.DisabledValue,
                    AtariCartridgeRegion.Pal => AtariCartridgeConstants.EnabledValue,
                    AtariCartridgeRegion.Secam => throw new ArgumentException(
                        AtariCartridgeErrors.SecamUnsupported, nameof(media)),
                    _ => throw new ArgumentOutOfRangeException(nameof(media), media, null)
                };
                break;
            default:
                throw new ArgumentException(AtariCartridgeErrors.RegionUnsupported, nameof(media));
        }
        return options;
    }

    private static void ValidateReadable(string path)
    {
        try
        {
            using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        }
        catch (IOException exception)
        {
            throw new AtariEmulationException(AtariErrorKind.Content, AtariErrorCode.ContentNotFound,
                AtariCartridgeErrors.FileUnreadable,
                new Dictionary<string, string> { [AtariConstants.PathContextKey] = path }, exception);
        }
        catch (UnauthorizedAccessException exception)
        {
            throw new AtariEmulationException(AtariErrorKind.Content, AtariErrorCode.ContentNotFound,
                AtariCartridgeErrors.FileUnreadable,
                new Dictionary<string, string> { [AtariConstants.PathContextKey] = path }, exception);
        }
    }
}
