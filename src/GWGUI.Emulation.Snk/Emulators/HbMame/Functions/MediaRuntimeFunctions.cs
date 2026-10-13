using GWGUI.Emulation.Snk.Emulators.Common.Interop.Services;
using GWGUI.Emulation.Snk.Emulators.Common.Interop.Functions;
using GWGUI.Emulation.Snk.Emulators.Common.Interop.Constants;
using System.IO;
using System.Text;
using GWGUI.Emulation.Snk.Emulators.HbMame.Constants;

namespace GWGUI.Emulation.Snk.Emulators.HbMame.Functions;
internal static class MediaRuntimeFunctions
{
    private const int MemoryCardBytes = 2048;
    private const int NativeArgumentCapacity = 1024;
    private const int NativeCommandCapacity = 4096;
    private const string NativeMemoryCardFile = "memory-card.neo";
    private const string CommandFile = "launch.cmd";
    private static readonly IReadOnlySet<string> CardExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "mcr",
        "neo"
    };
    internal sealed record PreparedMemoryCard(string SourcePath, string RuntimePath, bool IsReadOnly);
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

    private static string Argument(string value)
    {
        if (value.IndexOfAny(['"', '\r', '\n', '\0']) >= 0 || Encoding.UTF8.GetByteCount(value) >= NativeArgumentCapacity)
            throw new ArgumentException(ErrorMessages.IncompatibleMedia, nameof(value));
        return "\"" + value + "\"";
    }

    internal static LoadedContent? Prepare(MachineConfiguration configuration, string sessionDirectory, IReadOnlySet<string> supportedExtensions)
    {
        if (configuration.Model != MachineModel.NeoGeoMvs)
            throw new ArgumentException(nameof(configuration));
        var paths = InitialContentPaths(configuration);
        if (paths.Count == 0)
            return null;
        var source = paths[0];
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
            foreach (var archive in Directory.EnumerateFiles(sourceDirectory, SessionContentConstants.AllFilesPattern))
            {
                if (!RuntimeOptionConstants.CartridgeExtensions.Contains(Path.GetExtension(archive), StringComparer.OrdinalIgnoreCase) || Path.GetFileName(archive).Equals(RuntimeOptionConstants.SystemArchive, StringComparison.OrdinalIgnoreCase))
                    continue;
                directory.AddFile(archive);
            }

            var command = driver + " -bios " + FirmwareRuntimeFunctions.SelectedBios(configuration);
            if (InsertedCard(configuration)is { } card)
            {
                var nativeCard = directory.AddFile(ValidateCard(card), NativeMemoryCardFile);
                File.SetAttributes(nativeCard, FileAttributes.Normal);
                command += " -memc " + Argument(nativeCard);
            }

            var biosDirectory = Path.Combine(sessionDirectory, CoreDirectoryConstants.SystemDirectoryName, RuntimeOptionConstants.NativeSystemDirectory, "bios");
            command += " -rompath " + Argument(Path.GetDirectoryName(stagedSource)! + ";" + biosDirectory);
            if (Encoding.UTF8.GetByteCount(command) >= NativeCommandCapacity)
                throw new ArgumentException(ErrorMessages.IncompatibleMedia, nameof(configuration));
            var commandPath = directory.AddText(CommandFile, command);
            content = ContentFunctions.Create(commandPath, RuntimeOptionConstants.NeedsFullPath, supportedExtensions);
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
    {
        var card = InsertedCard(configuration);
        if (card is null)
            return null;
        var source = ValidateCard(card);
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