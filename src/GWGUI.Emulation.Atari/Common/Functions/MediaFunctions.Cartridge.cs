namespace GWGUI.Emulation.Atari.Common.Functions;

internal static class CartridgeFunctions
{
    internal static bool Supports(Emulator core) => CartridgeConstants.CartridgeCores.Contains(core);

    internal static PreparedCartridge Prepare(
        MachineConfiguration machine,
        MediaConfiguration media,
        Emulator core,
        bool needsFullPath,
        IReadOnlySet<string> reportedExtensions)
    {
        if (!Supports(core)) throw new ArgumentException(CartridgeErrors.UnsupportedCore, nameof(core));
        if (machine.Core != core) throw new ArgumentException(ErrorMessages.IncompatibleMedia, nameof(machine));
        if (media.Category != MediaCategory.Cartridge || media.Slot != GWGUI.Emulation.Contracts.EmulationMediaSlot.Cartridge0)
            throw new ArgumentException(CartridgeErrors.CartridgeRequired, nameof(media));

        var extension = Path.GetExtension(media.Path).TrimStart(CommonConstants.ExtensionPrefix);
        var acceptedExtensions = CartridgeConstants.Extensions[core];
        if (!acceptedExtensions.Contains(extension) || !reportedExtensions.Contains(extension))
            throw new EmulationException(ErrorCategory.Content, ErrorCode.ContentUnsupported,
                CartridgeErrors.ExtensionUnsupported,
                new Dictionary<string, string>
                {
                    [CommonConstants.ExtensionContextKey] = extension,
                    [CommonConstants.SupportedExtensionsContextKey] = string.Join(
                        ContentConstants.ExtensionSeparator,
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
        Emulator core)
    {
        var options = new Dictionary<string, string>(configuredOptions, StringComparer.Ordinal);
        foreach (var option in GetMediaOptions(media, core)) options[option.Key] = option.Value;
        return options;
    }

    internal static IReadOnlyDictionary<string, string> GetMediaOptions(
        MediaConfiguration media,
        Emulator core)
    {
        var options = new Dictionary<string, string>(StringComparer.Ordinal);
        if (media.CartridgeRegion is not { } region) return options;
        switch (core)
        {
            case Emulator.Stella:
                options[CartridgeConstants.StellaRegionOptionKey] = region switch
                {
                    CartridgeRegion.Automatic => CartridgeConstants.AutomaticRegionValue,
                    CartridgeRegion.Ntsc => CartridgeConstants.NtscRegionValue,
                    CartridgeRegion.Pal => CartridgeConstants.PalRegionValue,
                    CartridgeRegion.Secam => CartridgeConstants.SecamRegionValue,
                    _ => throw new ArgumentOutOfRangeException(nameof(media), media, null)
                };
                break;
            case Emulator.VirtualJaguar:
                options[CartridgeConstants.JaguarRegionOptionKey] = region switch
                {
                    CartridgeRegion.Automatic or CartridgeRegion.Ntsc =>
                        CartridgeConstants.DisabledValue,
                    CartridgeRegion.Pal => CartridgeConstants.EnabledValue,
                    CartridgeRegion.Secam => throw new ArgumentException(
                        CartridgeErrors.SecamUnsupported, nameof(media)),
                    _ => throw new ArgumentOutOfRangeException(nameof(media), media, null)
                };
                break;
            default:
                throw new ArgumentException(CartridgeErrors.RegionUnsupported, nameof(media));
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
            throw new EmulationException(ErrorCategory.Content, ErrorCode.ContentNotFound,
                CartridgeErrors.FileUnreadable,
                new Dictionary<string, string> { [CommonConstants.PathContextKey] = path }, exception);
        }
        catch (UnauthorizedAccessException exception)
        {
            throw new EmulationException(ErrorCategory.Content, ErrorCode.ContentNotFound,
                CartridgeErrors.FileUnreadable,
                new Dictionary<string, string> { [CommonConstants.PathContextKey] = path }, exception);
        }
    }
}
