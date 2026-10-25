using PreparedMemoryCard = GWGUI.Emulation.Snk.Emulators.Common.Interop.Functions.MameMediaFunctions.PreparedMemoryCard;
using GWGUI.Emulation.Snk.Emulators.Common.Interop.Services;
using GWGUI.Emulation.Snk.Emulators.Common.Interop.Functions;
using System.IO;
using System.Text;
using GWGUI.Emulation.Snk.Emulators.Mame2015.Constants;

namespace GWGUI.Emulation.Snk.Emulators.Mame2015.Functions;
internal static class MediaRuntimeFunctions
{
    private const int MemoryCardBytes = 2048;
    private const int NativeCommandCapacity = 512;
    private const string NativeMemoryCardFile = "memory-card.neo";
    private const string CommandFile = "launch.cmd";
    private static readonly IReadOnlySet<string> CardExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "mcr",
        "neo"
    };
    private static readonly IReadOnlyDictionary<string, string> ParentDrivers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        ["bstarsh"] = "bstars",
        ["ridheroh"] = "ridhero",
        ["alpham2p"] = "alpham2",
        ["kotmh"] = "kotm",
        ["sengokuh"] = "sengoku",
        ["burningfh"] = "burningf",
        ["burningfp"] = "burningf",
        ["gpilotsh"] = "gpilots",
        ["quizdaisk"] = "quizdais",
        ["2020bba"] = "2020bb",
        ["2020bbh"] = "2020bb",
        ["socbrawlh"] = "socbrawl",
        ["kotm2p"] = "kotm2",
        ["samshoh"] = "samsho",
        ["tophuntrh"] = "tophuntr",
        ["aof2a"] = "aof2",
        ["fatfurspa"] = "fatfursp",
        ["samsho2k"] = "samsho2",
        ["kof95a"] = "kof95",
        ["kof95h"] = "kof95",
        ["samsho3h"] = "samsho3",
        ["fswords"] = "samsho3",
        ["rbff1a"] = "rbff1",
        ["aof3k"] = "aof3",
        ["kof96h"] = "kof96",
        ["kizuna4p"] = "kizuna",
        ["samsho4k"] = "samsho4",
        ["rbffspeck"] = "rbffspec",
        ["kof97h"] = "kof97",
        ["kof97k"] = "kof97",
        ["kof97pls"] = "kof97",
        ["kof97oro"] = "kof97",
        ["kog"] = "kof97",
        ["lastbladh"] = "lastblad",
        ["lastsold"] = "lastblad",
        ["rbff2h"] = "rbff2",
        ["rbff2k"] = "rbff2",
        ["kof98a"] = "kof98",
        ["kof98k"] = "kof98",
        ["kof98ka"] = "kof98",
        ["kof98h"] = "kof98",
        ["kof99h"] = "kof99",
        ["kof99e"] = "kof99",
        ["kof99k"] = "kof99",
        ["kof99p"] = "kof99",
        ["garouh"] = "garou",
        ["garoup"] = "garou",
        ["garoubl"] = "garou",
        ["mslug3h"] = "mslug3",
        ["mslug3b6"] = "mslug3",
        ["kof2000n"] = "kof2000",
        ["kof2001h"] = "kof2001",
        ["cthd2003"] = "kof2001",
        ["ct2k3sp"] = "kof2001",
        ["ct2k3sa"] = "kof2001",
        ["kof2002b"] = "kof2002",
        ["kf2k2pls"] = "kof2002",
        ["kf2k2pla"] = "kof2002",
        ["kf2k2mp"] = "kof2002",
        ["kf2k2mp2"] = "kof2002",
        ["kof10th"] = "kof2002",
        ["kf10thep"] = "kof2002",
        ["kf2k5uni"] = "kof2002",
        ["kof2k4se"] = "kof2002",
        ["mslug5h"] = "mslug5",
        ["ms5plus"] = "mslug5",
        ["svcpcba"] = "svcpcb",
        ["svcboot"] = "svc",
        ["svcplus"] = "svc",
        ["svcplusa"] = "svc",
        ["svcsplus"] = "svc",
        ["samsho5h"] = "samsho5",
        ["samsho5b"] = "samsho5",
        ["kof2003h"] = "kof2003",
        ["kf2k3bl"] = "kof2003",
        ["kf2k3bla"] = "kof2003",
        ["kf2k3pl"] = "kof2003",
        ["kf2k3upl"] = "kof2003",
        ["samsh5sph"] = "samsh5sp",
        ["samsh5spho"] = "samsh5sp",
        ["maglordh"] = "maglord",
        ["ncombath"] = "ncombat",
        ["wh1h"] = "wh1",
        ["wh1ha"] = "wh1",
        ["nitdbl"] = "nitd",
        ["quizkofk"] = "quizkof",
        ["ironclado"] = "ironclad",
        ["shocktroa"] = "shocktro",
        ["lans2004"] = "shocktr2",
        ["pbobblenb"] = "pbobblen",
        ["fightfeva"] = "fightfev",
        ["puzzldpr"] = "puzzledp",
        ["mslug4h"] = "mslug4",
        ["ms4plus"] = "mslug4",
        ["matrimbl"] = "matrim",
        ["jockeygpa"] = "jockeygp",
        ["vlinero"] = "vliner"
    };
    internal static IReadOnlyList<EmulationMediaDevice> Devices(MachineModel model) => model == MachineModel.NeoGeoMvs 
        ? [
        new(EmulationMediaSlot.Cartridge0, EmulationMediaType.Cartridge, RuntimeOptionConstants.CartridgeExtensions, RequiresMachineRecreation: true),
        new(EmulationMediaSlot.MemoryCard0, EmulationMediaType.MemoryCard, [".mcr", ".neo"], RequiresMachineRecreation: true)
    ]
        : [];
    internal static IReadOnlyList<string> InitialContentPaths(MachineConfiguration configuration)
    {
        var path = configuration.InitialContent is { Length: > 0 } initial ? initial : configuration.Media.SingleOrDefault(media => media.Slot == EmulationMediaSlot.Cartridge0 && media.IsInserted)?.Path;
        return path is null ? [] : [ContentFunctions.Validate(path, RuntimeOptionConstants.CartridgeExtensions.Select(extension => extension.TrimStart(MediaConstants.ExtensionPrefix)).ToHashSet(StringComparer.OrdinalIgnoreCase))];
    }

    private static MediaConfiguration? InsertedCard(MachineConfiguration configuration) => configuration.Media.SingleOrDefault(media => media.Slot == EmulationMediaSlot.MemoryCard0 && media.IsInserted);
    private static string ValidateCard(MediaConfiguration card)
    {
        var path = ContentFunctions.Validate(card.Path, CardExtensions);
        if (new FileInfo(path).Length != MemoryCardBytes)
            throw new InvalidDataException(ErrorMessages.IncompatibleMedia);
        return path;
    }

    internal static LoadedContent? Prepare(MachineConfiguration configuration, string sessionDirectory, IReadOnlySet<string> supportedExtensions)
    {
        if (configuration.Model != MachineModel.NeoGeoMvs)
            throw new ArgumentException(nameof(configuration));
        var paths = InitialContentPaths(configuration);
        if (paths.Count == 0)
            return null;
        var source = paths[0];
        if (Path.GetFileName(source).Equals(RuntimeOptionConstants.SystemArchive, StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException(ErrorMessages.IncompatibleMedia, nameof(configuration));
        var sourceDirectory = Path.GetDirectoryName(source)!;
        var driver = Path.GetFileNameWithoutExtension(source);
        if (driver.Length == 0 || driver.Any(character => !char.IsAsciiLetterOrDigit(character) && character != '_'))
            throw new ArgumentException(ErrorMessages.IncompatibleMedia, nameof(configuration));
        var directory = new SessionContentDirectory(sessionDirectory, sourceDirectory);
        LoadedContent? content = null;
        var completed = false;
        try
        {
            var stagedSource = directory.AddFile(source);
            var parentDriver = driver;
            while (ParentDrivers.TryGetValue(parentDriver, out var parent))
            {
                foreach (var extension in RuntimeOptionConstants.CartridgeExtensions)
                {
                    var parentArchive = Path.Combine(sourceDirectory, parent + extension);
                    if (File.Exists(parentArchive))
                        directory.AddFile(parentArchive);
                }

                parentDriver = parent;
            }

            var biosArchive = Path.Combine(sessionDirectory, CoreDirectoryConstants.SystemDirectoryName, RuntimeOptionConstants.NativeSystemDirectory, "bios", RuntimeOptionConstants.SystemArchive);
            directory.AddFile(biosArchive, RuntimeOptionConstants.SystemArchive);
            var command = driver + " -bios " + FirmwareRuntimeFunctions.SelectedBios(configuration);
            if (InsertedCard(configuration)is { } card)
            {
                var nativeCard = directory.AddFile(ValidateCard(card), NativeMemoryCardFile);
                File.SetAttributes(nativeCard, FileAttributes.Normal);
                command += " -memc " + MameMediaFunctions.Argument(nativeCard);
            }

            command += " -rompath " + MameMediaFunctions.Argument(Path.GetDirectoryName(stagedSource)!);
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

    internal static PreparedMemoryCard? PrepareMemoryCard(MachineConfiguration configuration, LoadedContent? content)
    => MameMediaFunctions.PrepareMemoryCard(InsertedCard(configuration), content, ValidateCard);

    internal static void CompleteMemoryCard(PreparedMemoryCard? card)
    => MameMediaFunctions.CompleteMemoryCard(card);
}