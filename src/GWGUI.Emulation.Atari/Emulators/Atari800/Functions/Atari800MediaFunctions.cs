using GWGUI.Emulation.Atari.Emulators.Atari800.Constants;
using GWGUI.Emulation.Atari.Emulators.Atari800.Exceptions;
using GWGUI.Emulation.Atari.Emulators.Atari800.Contracts;
using GWGUI.Emulation.Atari.Emulators.Atari800.Enums;
using GWGUI.Emulation.Atari.Emulators.Atari800.Functions;

using System.Text;

namespace GWGUI.Emulation.Atari.Emulators.Atari800.Functions;

internal static class Atari800MediaFunctions
{
    internal static MediaConfiguration? Primary(IEnumerable<MediaConfiguration> media) => media
        .Where(item => item.IsInserted)
        .OrderBy(item => item.Category switch
        {
            MediaCategory.Cartridge => 0,
            MediaCategory.Cassette => 1,
            MediaCategory.Floppy => 2,
            _ => 3
        })
        .ThenBy(item => item.MountOrder)
        .ThenBy(item => item.Path, StringComparer.OrdinalIgnoreCase)
        .FirstOrDefault();

    internal static Atari800PreparedMedia Prepare(
        MachineConfiguration machine,
        MediaConfiguration media,
        string sessionDirectory,
        IReadOnlySet<string> coreExtensions)
    {
        var contentType = Classify(machine.Model, media);
        if (contentType != Atari800ContentType.Floppy || !ScpMediaFunctions.IsScp(media.Path))
            ValidateExtension(media, contentType);
        if (contentType is Atari800ContentType.ComputerCartridge or Atari800ContentType.ConsoleCartridge)
        {
            ContentFunctions.Validate(media.Path, coreExtensions);
            return new Atari800PreparedMedia(media, contentType, Path.GetFullPath(media.Path), null);
        }

        var sessionMedia = ScpMediaFunctions.Prepare(machine, media, sessionDirectory, coreExtensions);
        return new Atari800PreparedMedia(media, contentType, sessionMedia.RuntimePath, sessionMedia);
    }

    internal static Atari800ContentType Classify(MachineModel model, MediaConfiguration media)
    {
        if (media.CartridgeType is < Atari800MediaConstants.MinimumCartridgeType)
            throw new ArgumentOutOfRangeException(nameof(media), Atari800MediaErrors.CartridgeTypeInvalid);

        return media.Category switch
        {
            MediaCategory.Floppy => RequireComputer(model, Atari800ContentType.Floppy),
            MediaCategory.Cassette => RequireComputer(model, Atari800ContentType.Cassette),
            MediaCategory.Cartridge => ClassifyCartridge(model, media),
            _ => throw new ArgumentException(Atari800MediaErrors.UnsupportedMediaCategory, nameof(media))
        };
    }

    internal static IReadOnlyDictionary<string, string> ApplyOptions(
        MachineConfiguration machine,
        Atari800PreparedMedia? media)
    {
        var options = new Dictionary<string, string>(EightBitSettingsFunctions.Normalize(machine),
            StringComparer.Ordinal);
        options[EightBitSettingsConstants.SystemOptionKey] = SystemValue(machine, options);
        options.Remove(SettingsConstants.MainMemory);
        var configuredCassetteBoot = options.TryGetValue(EightBitSettingsConstants.CassetteBootOptionKey,
            out var cassetteBoot) && string.Equals(cassetteBoot, EightBitSettingsConstants.Enabled,
            StringComparison.OrdinalIgnoreCase);
        options[EightBitSettingsConstants.CassetteBootOptionKey] =
            media?.ContentType == Atari800ContentType.Cassette
                && (configuredCassetteBoot || media.Configuration.CassetteAutoBoot)
                ? EightBitSettingsConstants.Enabled
                : EightBitSettingsConstants.Disabled;
        options[EightBitSettingsConstants.SioAccelerationOptionKey] =
            EightBitSettingsConstants.Enabled;
        if (options.TryGetValue(VideoAudioSettingsConstants.StandardOption, out var standard))
            options[EightBitSettingsConstants.VideoStandardOptionKey] =
                string.Equals(standard, HardwareRegion.Pal.ToString(), StringComparison.OrdinalIgnoreCase)
                    ? EightBitSettingsConstants.Pal : EightBitSettingsConstants.Ntsc;
        options.Remove(VideoAudioSettingsConstants.StandardOption);
        MoveOption(options, VideoAudioSettingsConstants.ResolutionOption,
            EightBitSettingsConstants.ResolutionOptionKey);
        if (RequiresFullOverlayWidth(options))
            options[EightBitSettingsConstants.ResolutionOptionKey] = "384x240";
        // Per-port dead zones are already applied by GW GUI before analog input is forwarded.
        options[EightBitSettingsConstants.AnalogDeadZoneOptionKey] =
            EightBitSettingsConstants.NeutralAnalogDeadZone;
        return options;
    }

    private static bool RequiresFullOverlayWidth(IReadOnlyDictionary<string, string> options) =>
        IsEnabled(options, EightBitSettingsConstants.ShowActivityOptionKey) ||
        IsEnabled(options, EightBitSettingsConstants.ShowSectorOptionKey) ||
        IsEnabled(options, EightBitSettingsConstants.ShowSpeedOptionKey);

    private static bool IsEnabled(IReadOnlyDictionary<string, string> options, string key) =>
        string.Equals(options.GetValueOrDefault(key), EightBitSettingsConstants.Enabled,
            StringComparison.OrdinalIgnoreCase);

    private static void MoveOption(IDictionary<string, string> options, string source, string destination)
    {
        if (options.TryGetValue(source, out var value)) options[destination] = value;
        options.Remove(source);
    }

    private static string SystemValue(MachineConfiguration machine,
        IReadOnlyDictionary<string, string> options)
    {
        if (machine.Model != MachineModel.XlXe)
            return HardwareModelCatalog.Get(machine.Model).StableModelId;
        return options.GetValueOrDefault(SettingsConstants.MainMemory) switch
        {
            Atari800MediaFunctionsConstants.Value589824 => Atari8BitModelConstants.XlXe576KModelId,
            Atari800MediaFunctionsConstants.Value1114112 => Atari8BitModelConstants.XlXe1088KModelId,
            _ => Atari8BitModelConstants.XlXeModelId
        };
    }

    internal static bool HasCartridgeHeader(string path)
    {
        Span<byte> header = stackalloc byte[Atari800MediaConstants.CartridgeHeaderLength];
        using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        return stream.Read(header) == Atari800MediaConstants.CartridgeHeaderLength &&
               Encoding.ASCII.GetString(header) == Atari800MediaConstants.CartridgeHeaderText;
    }

    private static Atari800ContentType ClassifyCartridge(MachineModel model, MediaConfiguration media)
    {
        var isConsole = model == MachineModel.Atari5200;
        var declaredConsole = media.CartridgePlatform == CartridgePlatform.Atari5200;
        var declaredComputer = media.CartridgePlatform == CartridgePlatform.EightBitComputer;
        if (isConsole && declaredComputer) throw new ArgumentException(Atari800MediaErrors.ComputerMediaOn5200);
        if (!isConsole && declaredConsole) throw new ArgumentException(Atari800MediaErrors.ConsoleMediaOnComputer);

        var extension = Extension(media.Path);
        if (string.Equals(extension, Atari800MediaFunctionsConstants.A52, StringComparison.OrdinalIgnoreCase)) declaredConsole = true;
        if (string.Equals(extension, Atari800MediaFunctionsConstants.Car, StringComparison.OrdinalIgnoreCase) && HasCartridgeHeader(media.Path))
            declaredComputer = true;
        if (isConsole && declaredComputer) throw new ArgumentException(Atari800MediaErrors.ComputerMediaOn5200);
        if (!isConsole && declaredConsole) throw new ArgumentException(Atari800MediaErrors.ConsoleMediaOnComputer);
        return isConsole ? Atari800ContentType.ConsoleCartridge : Atari800ContentType.ComputerCartridge;
    }

    private static Atari800ContentType RequireComputer(MachineModel model, Atari800ContentType contentType) =>
        model == MachineModel.Atari5200
            ? throw new ArgumentException(Atari800MediaErrors.ComputerMediaOn5200)
            : contentType;

    private static void ValidateExtension(MediaConfiguration media, Atari800ContentType contentType)
    {
        var extensions = contentType switch
        {
            Atari800ContentType.Floppy => Atari800MediaConstants.FloppyExtensions,
            Atari800ContentType.Cassette => Atari800MediaConstants.CassetteExtensions,
            _ => Atari800MediaConstants.CartridgeExtensions
        };
        if (!extensions.Contains(Extension(media.Path)))
            throw new EmulationException(ErrorCategory.Content, ErrorCode.ContentUnsupported,
                Atari800MediaErrors.InvalidExtension);
    }

    private static string Extension(string path) =>
        Path.GetExtension(path).TrimStart(MediaConstants.ExtensionPrefix);
}
