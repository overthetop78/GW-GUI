using System.IO;
using System.Text;
using GWGUI.Emulation.Snk.Emulators.Mame2016.Constants;

namespace GWGUI.Emulation.Snk.Emulators.Mame2016.Functions;
internal static class MediaRuntimeFunctions
{
    private const int MemoryCardBytes = 2048;
    private const int NativeArgumentCapacity = 1024;
    private const int NativeCommandCapacity = 512;
    private const string NativeMemoryCardFile = "memory-card.neo";
    private const string CommandFile = "launch.cmd";
    private static readonly IReadOnlyList<string> PocketExtensions = Array.AsReadOnly(new[] { ".bin", ".ngp", ".npc", ".ngc" });
    private static readonly IReadOnlyList<string> CdExtensions = Array.AsReadOnly(new[] { ".chd", ".cue", ".toc", ".nrg", ".gdi", ".iso", ".cdr" });
    private static readonly IReadOnlySet<string> CardExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "mcr",
        "neo"
    };
    private static readonly IReadOnlySet<long> PocketSizes = new HashSet<long>
    {
        0x8000,
        0x80000,
        0x100000,
        0x200000,
        0x400000
    };
    internal sealed record PreparedMemoryCard(string SourcePath, string RuntimePath, bool IsReadOnly);
    private static IReadOnlyList<string> Extensions(MachineModel model) => model switch
    {
        MachineModel.NeoGeoCd or MachineModel.NeoGeoCdz => CdExtensions,
        MachineModel.NeoGeoPocket or MachineModel.NeoGeoPocketColor => PocketExtensions,
        _ => RuntimeOptionConstants.CartridgeExtensions
    };
    internal static IReadOnlyList<EmulationMediaDevice> Devices(MachineModel model) => model switch
    {
        MachineModel.NeoGeoCd or MachineModel.NeoGeoCdz => [new(EmulationMediaSlot.Cd0, EmulationMediaType.CompactDisc, CdExtensions, RequiresMachineRecreation: true)],
        MachineModel.NeoGeoPocket or MachineModel.NeoGeoPocketColor => [new(EmulationMediaSlot.Cartridge0, EmulationMediaType.Cartridge, PocketExtensions, RequiresMachineRecreation: true)],
        _ => [
            new(EmulationMediaSlot.Cartridge0, EmulationMediaType.Cartridge, RuntimeOptionConstants.CartridgeExtensions, RequiresMachineRecreation: true),
            new(EmulationMediaSlot.MemoryCard0, EmulationMediaType.MemoryCard, [".mcr", ".neo"], RequiresMachineRecreation: true)
        ]
    };
    internal static IReadOnlyList<string> InitialContentPaths(MachineConfiguration configuration)
    {
        var slot = configuration.Family == MachineFamily.NeoGeoCd ? EmulationMediaSlot.Cd0 : EmulationMediaSlot.Cartridge0;
        var path = configuration.InitialContent is { Length: > 0 } initial ? initial : configuration.Media.SingleOrDefault(media => media.Slot == slot && media.IsInserted)?.Path;
        return path is null 
            ? [] 
            : [ContentFunctions.Validate(path, Extensions(configuration.Model).Select(extension => extension.TrimStart(MediaConstants.ExtensionPrefix)).ToHashSet(StringComparer.OrdinalIgnoreCase))];
    }

    private static MediaConfiguration? InsertedCard(MachineConfiguration configuration) => configuration.Media.SingleOrDefault(media => media.Slot == EmulationMediaSlot.MemoryCard0 && media.IsInserted);
    private static string ValidateCard(
        MachineConfiguration configuration,
        MediaConfiguration card
    )
    {
        if (configuration.Family != MachineFamily.NeoGeoCartridge)
            throw new ArgumentException(ErrorMessages.IncompatibleMedia, nameof(configuration));
        var path = ContentFunctions.Validate(card.Path, CardExtensions);
        if (new FileInfo(path).Length != MemoryCardBytes)
            throw new InvalidDataException(ErrorMessages.IncompatibleMedia);
        return path;
    }

    private static string Argument(string value)
    {
        if (value.IndexOfAny(['"', '\r', '\n', '\0']) >= 0 || Encoding.UTF8.GetByteCount(value) >= NativeArgumentCapacity)
            throw new ArgumentException(ErrorMessages.IncompatibleMedia, nameof(value));
        return "\"" + value + "\"";
    }

    private static string ArchiveDriver(string path)
    {
        var driver = Path.GetFileNameWithoutExtension(path);
        if (driver.Length == 0 || driver.Any(character => !char.IsAsciiLetterOrDigit(character) && character != '_'))
            throw new ArgumentException(ErrorMessages.IncompatibleMedia, nameof(path));
        return driver;
    }

    internal static LoadedContent? Prepare(
        MachineConfiguration configuration,
        string sessionDirectory,
        IReadOnlySet<string> supportedExtensions
    )
    {
        var paths = InitialContentPaths(configuration);
        if (paths.Count == 0)
            return null;
        var source = paths[0];
        if (configuration.Family == MachineFamily.NeoGeoPocket && !PocketSizes.Contains(new FileInfo(source).Length))
            throw new InvalidDataException(ErrorMessages.IncompatibleMedia);
        var sourceDirectory = Path.GetDirectoryName(source)!;
        var directory = new SessionContentDirectory(sessionDirectory, sourceDirectory);
        LoadedContent? content = null;
        var completed = false;
        try
        {
            var driver = configuration.Model == MachineModel.NeoGeoMvs ? ArchiveDriver(source) : FirmwareRuntimeFunctions.Driver(configuration.Model);
            var command = driver;
            var bios = FirmwareRuntimeFunctions.SelectedBios(configuration);
            if (bios.Length != 0)
                command += " -bios " + Argument(bios);
            if (configuration.Model == MachineModel.NeoGeoAes)
            {
                var software = ArchiveDriver(source);
                command += " -cart " + Argument("neogeo:" + software + ":cart");
            }
            else if (configuration.Family == MachineFamily.NeoGeoPocket)
                command += " -cart " + Argument(source);
            else if (configuration.Family == MachineFamily.NeoGeoCd)
                command += " -cdrm " + Argument(source);
            if (InsertedCard(configuration)is { } card)
            {
                var nativeCard = directory.AddFile(ValidateCard(configuration, card), NativeMemoryCardFile);
                File.SetAttributes(nativeCard, FileAttributes.Normal);
                command += " -memc " + Argument(nativeCard);
            }

            var biosDirectory = Path.Combine(sessionDirectory, CoreDirectoryConstants.SystemDirectoryName, RuntimeOptionConstants.NativeSystemDirectory, FirmwareRuntimeFunctions.BiosDirectory);
            command += " -rompath " + Argument(sourceDirectory + ";" + biosDirectory);
            if (Encoding.UTF8.GetByteCount(command) >= NativeCommandCapacity)
                throw new ArgumentException(ErrorMessages.IncompatibleMedia, nameof(configuration));
            var commandPath = directory.AddText(CommandFile, command);
            content = ContentFunctions.Create(commandPath, RuntimeOptionConstants.NeedsFullPath, supportedExtensions, useWindowsAnsiPath: true);
            content.OwnContentDirectory(directory);
            completed = true;
            return content;
        }
        finally
        {
            if (!completed)
            {
                try
                {
                    content?.Dispose();
                }
                finally
                {
                    directory.Dispose();
                }
            }
        }
    }

    internal static PreparedMemoryCard? PrepareMemoryCard(
        MachineConfiguration configuration,
        LoadedContent? content
    )
    {
        var card = InsertedCard(configuration);
        if (card is null)
            return null;
        var source = ValidateCard(configuration, card);
        var contentPath = content?.ContentFilePath ?? throw new InvalidOperationException(ErrorMessages.ContentRequired);
        var runtime = Path.Combine(Path.GetDirectoryName(contentPath)!, NativeMemoryCardFile);
        if (!File.Exists(runtime) || new FileInfo(runtime).Length != MemoryCardBytes)
            throw new InvalidDataException(ErrorMessages.IncompatibleMedia);
        return new(source, runtime, card.IsReadOnly);
    }

    internal static void CompleteMemoryCard(PreparedMemoryCard? card)
    {
        if (card is null || card.IsReadOnly)
            return;
        var bytes = File.ReadAllBytes(card.RuntimePath);
        if (bytes.Length != MemoryCardBytes)
            throw new InvalidDataException(ErrorMessages.IncompatibleMedia);
        File.WriteAllBytes(card.SourcePath, bytes);
    }
}