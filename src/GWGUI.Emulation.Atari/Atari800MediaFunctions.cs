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

        return media.Category switch
        {
            AtariMediaCategory.Floppy => RequireComputer(model, Atari800ContentType.Floppy),
            AtariMediaCategory.Cassette => RequireComputer(model, Atari800ContentType.Cassette),
            AtariMediaCategory.Cartridge => ClassifyCartridge(model, media),
            _ => throw new ArgumentException(Atari800MediaErrors.UnsupportedMediaCategory, nameof(media))
        };
    }

    internal static IReadOnlyDictionary<string, string> ApplyOptions(
        AtariMachineConfiguration machine,
        Atari800PreparedMedia? media)
    {
        var options = new Dictionary<string, string>(AtariEightBitSettingsFunctions.Normalize(machine),
            StringComparer.Ordinal);
        options[Atari800MediaConstants.SystemOptionKey] = SystemValue(machine, options);
        options.Remove(AtariConfigurationOptionConstants.MainMemory);
        var configuredCassetteBoot = options.TryGetValue(AtariEightBitSettingsConstants.CassetteBootOptionKey,
            out var cassetteBoot) && string.Equals(cassetteBoot, AtariEightBitSettingsConstants.Enabled,
            StringComparison.OrdinalIgnoreCase);
        options[AtariEightBitSettingsConstants.CassetteBootOptionKey] =
            configuredCassetteBoot || media?.ContentType == Atari800ContentType.Cassette
                && media.Configuration.CassetteAutoBoot
                ? AtariEightBitSettingsConstants.Enabled
                : AtariEightBitSettingsConstants.Disabled;
        if (options.TryGetValue(AtariConfigurationOptionConstants.VideoStandard, out var standard))
            options[AtariEightBitSettingsConstants.VideoStandardOptionKey] =
                string.Equals(standard, AtariClassicRegion.Pal.ToString(), StringComparison.OrdinalIgnoreCase)
                    ? AtariEightBitSettingsConstants.Pal : AtariEightBitSettingsConstants.Ntsc;
        options.Remove(AtariConfigurationOptionConstants.VideoStandard);
        MoveOption(options, AtariConfigurationOptionConstants.VideoResolution,
            AtariEightBitSettingsConstants.ResolutionOptionKey);
        // Per-port dead zones are already applied by GW GUI before analog input is forwarded.
        options[AtariEightBitSettingsConstants.AnalogDeadZoneOptionKey] =
            AtariEightBitSettingsConstants.NeutralAnalogDeadZone;
        return options;
    }

    private static void MoveOption(IDictionary<string, string> options, string source, string destination)
    {
        if (options.TryGetValue(source, out var value)) options[destination] = value;
        options.Remove(source);
    }

    private static string SystemValue(AtariMachineConfiguration machine,
        IReadOnlyDictionary<string, string> options)
    {
        if (machine.Model != AtariMachineModel.XlXe)
            return AtariClassicModelCatalog.Get(machine.Model).StableModelId;
        return options.GetValueOrDefault(AtariConfigurationOptionConstants.MainMemory) switch
        {
            "589824" => AtariClassicModelConstants.XlXe576KModelId,
            "1114112" => AtariClassicModelConstants.XlXe1088KModelId,
            _ => AtariClassicModelConstants.XlXeModelId
        };
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
            throw new AtariEmulationException(AtariErrorCategory.Content, AtariErrorCode.ContentUnsupported,
                Atari800MediaErrors.InvalidExtension);
    }

    private static string Extension(string path) => Path.GetExtension(path).TrimStart(AtariConstants.ExtensionPrefix);
}
