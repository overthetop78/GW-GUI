using GWGUI.Emulation.Snk.Emulators.Common.Interop.Services;
using GWGUI.Emulation.Snk.Emulators.Common.Interop.Functions;
using GWGUI.Emulation.Snk.Emulators.Common.Interop.Constants;
using System.IO;
using GWGUI.MediaEngine.Images.Formats.Optical.BinCue;
using GWGUI.Emulation.Snk.Emulators.FbNeo.Constants;

namespace GWGUI.Emulation.Snk.Emulators.FbNeo.Functions;
internal static class MediaRuntimeFunctions
{
    private const string PocketArchive = "ngp.zip";
    private const string MemoryCardExtension = ".mcr";
    private const string NativeMemoryCardExtension = ".memcard";
    private const string NativeSaveDirectory = "fbneo";
    private const string MemoryCardHeader = "FB1 FC1 ";
    private const int RawMemoryCardBytes = 2048;
    private const int NativeMemoryCardBytes = 131072;
    private const int MemoryCardHeaderBytes = 32;
    private const int MinimumMemoryCardVersion = 0x029713;
    private const int VersionFieldCount = 2;
    private const int ReservedFieldCount = 3;
    private const int MemoryCardStride = 2;
    private const int MemoryCardDataByteOffset = 1;
    internal sealed record PreparedMemoryCard(string SourcePath, string RuntimePath, bool IsReadOnly);
    internal static PreparedMemoryCard? PrepareMemoryCard(MachineConfiguration configuration, string saveDirectory)
    {
        var card = configuration.Media.SingleOrDefault(media => media.Slot == EmulationMediaSlot.MemoryCard0 && media.IsInserted);
        if (card is null)
            return null;
        var source = ContentFunctions.Validate(card.Path, new HashSet<string>(StringComparer.OrdinalIgnoreCase) { MemoryCardExtension.TrimStart(MediaConstants.ExtensionPrefix) });
        var raw = File.ReadAllBytes(source);
        if (raw.Length != RawMemoryCardBytes || configuration.Family != MachineFamily.NeoGeoCartridge)
            throw new ArgumentException(ErrorMessages.IncompatibleMedia, nameof(configuration));
        var content = InitialContentPaths(configuration).SingleOrDefault() ?? throw new EmulationException(ErrorCategory.Content, ErrorCode.ContentRequired, ErrorMessages.ContentRequired);
        var directory = Path.Combine(saveDirectory, NativeSaveDirectory);
        Directory.CreateDirectory(directory);
        var runtime = Path.Combine(directory, Path.GetFileNameWithoutExtension(content) + NativeMemoryCardExtension);
        var data = new byte[NativeMemoryCardBytes];
        for (var index = 0; index < raw.Length; index++)
            data[index * MemoryCardStride + MemoryCardDataByteOffset] = raw[index];
        using var stream = File.Create(runtime);
        using var writer = new BinaryWriter(stream);
        writer.Write(System.Text.Encoding.ASCII.GetBytes(MemoryCardHeader));
        writer.Write(NativeMemoryCardBytes + MemoryCardHeaderBytes);
        for (var index = 0; index < VersionFieldCount; index++)
            writer.Write(MinimumMemoryCardVersion);
        for (var index = 0; index < ReservedFieldCount; index++)
            writer.Write(0);
        writer.Write(data);
        return new(source, runtime, card.IsReadOnly);
    }

    internal static void CompleteMemoryCard(PreparedMemoryCard? card)
    {
        if (card is null || card.IsReadOnly || !File.Exists(card.RuntimePath))
            return;
        var data = File.ReadAllBytes(card.RuntimePath);
        if (data.Length < MemoryCardHeaderBytes + RawMemoryCardBytes * MemoryCardStride || !data.AsSpan(0, MemoryCardHeader.Length).SequenceEqual(System.Text.Encoding.ASCII.GetBytes(MemoryCardHeader)))
            throw new InvalidDataException(ErrorMessages.IncompatibleMedia);
        var raw = new byte[RawMemoryCardBytes];
        for (var index = 0; index < raw.Length; index++)
            raw[index] = data[MemoryCardHeaderBytes + index * MemoryCardStride + MemoryCardDataByteOffset];
        File.WriteAllBytes(card.SourcePath, raw);
    }

    internal static IReadOnlyList<EmulationMediaDevice> Devices(MachineModel model) => model == MachineModel.NeoGeoCdz 
        ? [new(EmulationMediaSlot.Cd0, EmulationMediaType.CompactDisc, RuntimeOptionConstants.CdExtensions, RequiresMachineRecreation: true)] 
        : model is MachineModel.NeoGeoAes or MachineModel.NeoGeoMvs 
        ? [
        new(EmulationMediaSlot.Cartridge0, EmulationMediaType.Cartridge, RuntimeOptionConstants.CartridgeExtensions, RequiresMachineRecreation: true),
        new(EmulationMediaSlot.MemoryCard0, EmulationMediaType.MemoryCard, [MemoryCardExtension], RequiresMachineRecreation: true)
    ]
        : [new(EmulationMediaSlot.Cartridge0, EmulationMediaType.Cartridge, RuntimeOptionConstants.CartridgeExtensions, RequiresMachineRecreation: true)];
    internal static IReadOnlyList<string> InitialContentPaths(MachineConfiguration configuration)
    {
        var slot = configuration.Model == MachineModel.NeoGeoCdz ? EmulationMediaSlot.Cd0 : EmulationMediaSlot.Cartridge0;
        var path = configuration.InitialContent is { Length: > 0 } initial ? initial : configuration.Media.SingleOrDefault(media => media.Slot == slot && media.IsInserted)?.Path;
        var extensions = configuration.Model == MachineModel.NeoGeoCdz ? RuntimeOptionConstants.CdExtensions : RuntimeOptionConstants.CartridgeExtensions;
        return path is null ? [] : [ContentFunctions.Validate(path, extensions.Select(extension => extension.TrimStart(MediaConstants.ExtensionPrefix)).ToHashSet(StringComparer.OrdinalIgnoreCase))];
    }

    internal static LoadedContent? Prepare(MachineConfiguration configuration, string sessionDirectory, IReadOnlySet<string> supportedExtensions)
    {
        var paths = InitialContentPaths(configuration);
        var extensions = configuration.Model == MachineModel.NeoGeoCdz ? RuntimeOptionConstants.CdExtensions.Select(extension => extension.TrimStart(MediaConstants.ExtensionPrefix)).ToHashSet(StringComparer.OrdinalIgnoreCase) : supportedExtensions;
        if (paths.Count == 0)
            return null;
        var source = paths[0];
        var sourceDirectory = Path.GetDirectoryName(source)!;
        var dependencies = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            source
        };
        foreach (var archive in Directory.EnumerateFiles(sourceDirectory, SessionContentConstants.AllFilesPattern))
        {
            if (!RuntimeOptionConstants.CartridgeExtensions.Contains(Path.GetExtension(archive), StringComparer.OrdinalIgnoreCase))
                continue;
            var name = Path.GetFileName(archive);
            if (name.Equals(RuntimeOptionConstants.SystemArchive, StringComparison.OrdinalIgnoreCase) || name.Equals(RuntimeOptionConstants.CdArchive, StringComparison.OrdinalIgnoreCase) || name.Equals(PocketArchive, StringComparison.OrdinalIgnoreCase))
                continue;
            dependencies.Add(archive);
        }

        if (Path.GetExtension(source).Equals(RuntimeOptionConstants.CueExtension, StringComparison.OrdinalIgnoreCase))
        {
            var sheet = new CueSheetReader().Parse(File.ReadLines(source));
            foreach (var file in sheet.Files)
                dependencies.Add(Path.GetFullPath(file.DeclaredPath, sourceDirectory));
        }
        else if (Path.GetExtension(source).Equals(RuntimeOptionConstants.CcdExtension, StringComparison.OrdinalIgnoreCase))
        {
            dependencies.Add(Path.ChangeExtension(source, RuntimeOptionConstants.CcdImageExtension));
            var subchannel = Path.ChangeExtension(source, RuntimeOptionConstants.CcdSubchannelExtension);
            if (File.Exists(subchannel))
                dependencies.Add(subchannel);
        }

        var sourceRoot = sourceDirectory;
        while (dependencies.Any(path => !path.StartsWith((Path.EndsInDirectorySeparator(sourceRoot) ? sourceRoot : sourceRoot + Path.DirectorySeparatorChar), StringComparison.OrdinalIgnoreCase)))
            sourceRoot = Path.GetDirectoryName(Path.TrimEndingDirectorySeparator(sourceRoot)) ?? throw new ArgumentException(nameof(configuration));
        var directory = new SessionContentDirectory(sessionDirectory, sourceRoot);
        LoadedContent? content = null;
        var completed = false;
        try
        {
            var stagedPath = directory.AddFile(source);
            foreach (var dependency in dependencies)
                directory.AddFile(dependency);
            if (Path.GetExtension(source).Equals(RuntimeOptionConstants.CueExtension, StringComparison.OrdinalIgnoreCase))
                CueRuntimeFunctions.Prepare(source, stagedPath, directory);
            content = ContentFunctions.Create(stagedPath, RuntimeOptionConstants.NeedsFullPath, extensions);
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
}