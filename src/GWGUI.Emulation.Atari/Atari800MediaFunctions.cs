using System.Text;

namespace GWGUI.Emulation.Atari;

internal static class Atari800MediaFunctions
{
    internal static Atari800PreparedMedia Prepare(
        AtariMachineConfiguration machine,
        AtariMediaConfiguration media,
        string sessionDirectory,
        IReadOnlySet<string> coreExtensions)
    {
        var contentType = Classify(machine.Model, media);
        ValidateExtension(media, contentType);
        if (contentType is Atari800ContentType.ComputerCartridge or Atari800ContentType.ConsoleCartridge)
        {
            Cores.AtariContentFunctions.Validate(media.Path, coreExtensions);
            return new Atari800PreparedMedia(media, contentType, Path.GetFullPath(media.Path), null);
        }

        var sessionMedia = AtariSessionMediaFunctions.Prepare(media, sessionDirectory, coreExtensions);
        return new Atari800PreparedMedia(media, contentType, sessionMedia.RuntimePath, sessionMedia);
    }

    internal static Atari800ContentType Classify(AtariMachineModel model, AtariMediaConfiguration media)
    {
        if (media.CartridgeType is < Atari800MediaConstants.MinimumCartridgeType)
            throw new ArgumentOutOfRangeException(nameof(media), Atari800MediaErrors.CartridgeTypeInvalid);

        return media.Kind switch
        {
            AtariMediaKind.Floppy => RequireComputer(model, Atari800ContentType.Floppy),
            AtariMediaKind.Cassette => RequireComputer(model, Atari800ContentType.Cassette),
            AtariMediaKind.Cartridge => ClassifyCartridge(model, media),
            _ => throw new ArgumentException(Atari800MediaErrors.UnsupportedKind, nameof(media))
        };
    }

    internal static IReadOnlyDictionary<string, string> ApplyOptions(
        AtariMachineConfiguration machine,
        Atari800PreparedMedia? media)
    {
        var options = new Dictionary<string, string>(machine.Options, StringComparer.Ordinal);
        options[Atari800MediaConstants.SystemOptionKey] = AtariClassicModelCatalog.Get(machine.Model).StableModelId;
        options[Atari800MediaConstants.CassetteBootOptionKey] =
            media?.ContentType == Atari800ContentType.Cassette && media.Configuration.CassetteAutoBoot
                ? Atari800MediaConstants.EnabledOptionValue
                : Atari800MediaConstants.DisabledOptionValue;
        return options;
    }

    internal static bool HasCartridgeHeader(string path)
    {
        Span<byte> header = stackalloc byte[Atari800MediaConstants.CartridgeHeaderLength];
        using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        return stream.Read(header) == Atari800MediaConstants.CartridgeHeaderLength &&
               Encoding.ASCII.GetString(header) == Atari800MediaConstants.CartridgeHeaderText;
    }

    private static Atari800ContentType ClassifyCartridge(AtariMachineModel model, AtariMediaConfiguration media)
    {
        var isConsole = model == AtariMachineModel.Atari5200;
        var declaredConsole = media.CartridgePlatform == AtariCartridgePlatform.Atari5200;
        var declaredComputer = media.CartridgePlatform == AtariCartridgePlatform.EightBitComputer;
        if (isConsole && declaredComputer) throw new ArgumentException(Atari800MediaErrors.ComputerMediaOn5200);
        if (!isConsole && declaredConsole) throw new ArgumentException(Atari800MediaErrors.ConsoleMediaOnComputer);

        var extension = Extension(media.Path);
        if (string.Equals(extension, "a52", StringComparison.OrdinalIgnoreCase)) declaredConsole = true;
        if (string.Equals(extension, "car", StringComparison.OrdinalIgnoreCase) && HasCartridgeHeader(media.Path))
            declaredComputer = true;
        if (isConsole && declaredComputer) throw new ArgumentException(Atari800MediaErrors.ComputerMediaOn5200);
        if (!isConsole && declaredConsole) throw new ArgumentException(Atari800MediaErrors.ConsoleMediaOnComputer);
        return isConsole ? Atari800ContentType.ConsoleCartridge : Atari800ContentType.ComputerCartridge;
    }

    private static Atari800ContentType RequireComputer(AtariMachineModel model, Atari800ContentType contentType) =>
        model == AtariMachineModel.Atari5200
            ? throw new ArgumentException(Atari800MediaErrors.ComputerMediaOn5200)
            : contentType;

    private static void ValidateExtension(AtariMediaConfiguration media, Atari800ContentType contentType)
    {
        var extensions = contentType switch
        {
            Atari800ContentType.Floppy => Atari800MediaConstants.FloppyExtensions,
            Atari800ContentType.Cassette => Atari800MediaConstants.CassetteExtensions,
            _ => Atari800MediaConstants.CartridgeExtensions
        };
        if (!extensions.Contains(Extension(media.Path)))
            throw new AtariEmulationException(AtariErrorKind.Content, AtariErrorCode.ContentUnsupported,
                Atari800MediaErrors.InvalidExtension);
    }

    private static string Extension(string path) => Path.GetExtension(path).TrimStart(AtariConstants.ExtensionPrefix);
}
