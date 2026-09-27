namespace GWGUI.Emulation.Atari.Common.Machines.Common.Functions;

internal static class CartridgeFunctions
{
    internal static PreparedCartridge Prepare(
        MachineConfiguration machine,
        MediaConfiguration media,
        Emulator core,
        IReadOnlySet<string> acceptedExtensions,
        bool needsFullPath,
        IReadOnlySet<string> reportedExtensions)
    {
        if (machine.Core != core) throw new ArgumentException(ErrorMessages.IncompatibleMedia, nameof(machine));
        if (media.Category != MediaCategory.Cartridge || media.Slot != GWGUI.Emulation.Contracts.EmulationMediaSlot.Cartridge0)
            throw CartridgeExceptions.CartridgeRequired();

        var extension = Path.GetExtension(media.Path).TrimStart(MediaConstants.ExtensionPrefix);
        if (!acceptedExtensions.Contains(extension) || !reportedExtensions.Contains(extension))
            throw CartridgeExceptions.ExtensionUnsupported(
                new Dictionary<string, string>
                {
                    [ErrorContextConstants.Extension] = extension,
                    [ErrorContextConstants.SupportedExtensions] = string.Join(
                        MediaConstants.ExtensionListSeparator,
                        acceptedExtensions.Order(StringComparer.OrdinalIgnoreCase))
                });

        var path = ContentFunctions.Validate(media.Path, reportedExtensions);
        ValidateReadable(path);
        return new PreparedCartridge(media, core, path, needsFullPath);
    }

    internal static void ValidateNoUnsupportedMetadata(MediaConfiguration media)
    {
        if (media.CartridgePlatform is not null || media.CartridgeType is not null)
            throw new ArgumentException(ErrorMessages.IncompatibleMedia, nameof(media));
    }

    internal static IReadOnlyDictionary<string, string> ApplyOptions(
        IReadOnlyDictionary<string, string> configuredOptions,
        MediaConfiguration media,
        bool supportsRegion)
    {
        var options = new Dictionary<string, string>(configuredOptions, StringComparer.Ordinal);
        foreach (var option in GetMediaOptions(media, supportsRegion)) options[option.Key] = option.Value;
        return options;
    }

    internal static IReadOnlyDictionary<string, string> GetMediaOptions(
        MediaConfiguration media,
        bool supportsRegion)
    {
        var options = new Dictionary<string, string>(StringComparer.Ordinal);
        if (media.CartridgeRegion is not { } region) return options;
        if (!supportsRegion) throw CartridgeExceptions.RegionUnsupported();
        options[CartridgeConstants.RegionOptionKey] = region switch
        {
            CartridgeRegion.Automatic => CartridgeConstants.AutomaticRegionValue,
            CartridgeRegion.Ntsc => CartridgeConstants.NtscRegionValue,
            CartridgeRegion.Pal => CartridgeConstants.PalRegionValue,
            CartridgeRegion.Secam => CartridgeConstants.SecamRegionValue,
            _ => throw new ArgumentOutOfRangeException(nameof(media), media, null)
        };
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
            throw CartridgeExceptions.FileUnreadable(
                new Dictionary<string, string> { [ErrorContextConstants.Path] = path }, exception);
        }
        catch (UnauthorizedAccessException exception)
        {
            throw CartridgeExceptions.FileUnreadable(
                new Dictionary<string, string> { [ErrorContextConstants.Path] = path }, exception);
        }
    }
}
