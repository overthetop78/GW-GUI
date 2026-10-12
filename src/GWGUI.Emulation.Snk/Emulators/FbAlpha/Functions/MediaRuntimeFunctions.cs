using System.IO;
using GWGUI.MediaEngine.Constants;
using GWGUI.MediaEngine.Contracts;
using GWGUI.MediaEngine.Images.Formats.Optical.BinCue;
using GWGUI.Emulation.Snk.Emulators.FbAlpha.Constants;

namespace GWGUI.Emulation.Snk.Emulators.FbAlpha.Functions;

internal static class MediaRuntimeFunctions
{
    internal static IReadOnlyList<EmulationMediaDevice> Devices(MachineModel model) => model == MachineModel.NeoGeoCdz
        ? [new(EmulationMediaSlot.Cd0, EmulationMediaType.CompactDisc, RuntimeOptionConstants.CdExtensions, RequiresMachineRecreation: true)]
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
        var extensions = configuration.Model == MachineModel.NeoGeoCdz
            ? RuntimeOptionConstants.CdExtensions.Select(extension => extension.TrimStart(MediaConstants.ExtensionPrefix)).ToHashSet(StringComparer.OrdinalIgnoreCase)
            : supportedExtensions;
        if (paths.Count == 0)
            return null;
        var source = paths[0];
        var sourceDirectory = Path.GetDirectoryName(source)!;
        var dependencies = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            source
        };
        CueSheetDocument? cueSheet = null;
        foreach (var archive in Directory.EnumerateFiles(sourceDirectory, SessionContentConstants.AllFilesPattern))
        {
            if (!RuntimeOptionConstants.CartridgeExtensions.Contains(Path.GetExtension(archive), StringComparer.OrdinalIgnoreCase))
                continue;
            var name = Path.GetFileName(archive);
            if (name.Equals(RuntimeOptionConstants.SystemArchive, StringComparison.OrdinalIgnoreCase) || name.Equals(RuntimeOptionConstants.CdArchive, StringComparison.OrdinalIgnoreCase))
                continue;
            dependencies.Add(archive);
        }
    
        if (Path.GetExtension(source).Equals(RuntimeOptionConstants.CueExtension, StringComparison.OrdinalIgnoreCase))
        {
            var sheet = new CueSheetReader().Parse(File.ReadLines(source));
            CueRuntimeFunctions.Validate(sheet);
            cueSheet = sheet;
            var binary = Path.GetFullPath(sheet.Files[0].DeclaredPath, sourceDirectory);
            var length = new FileInfo(binary).Length;
            if (length == 0 || length % OpticalSectorConstants.RawSectorSize != 0)
                throw new EmulationException(ErrorCategory.Content, ErrorCode.ContentUnsupported, ErrorMessages.ContentLoadFailed);
            foreach (var file in sheet.Files)
                dependencies.Add(Path.GetFullPath(file.DeclaredPath, sourceDirectory));
        }
        else if (Path.GetExtension(source).Equals(RuntimeOptionConstants.CcdExtension, StringComparison.OrdinalIgnoreCase))
        {
            dependencies.Add(Path.ChangeExtension(source, RuntimeOptionConstants.CcdImageExtension));
            var subchannel = Path.ChangeExtension(source, RuntimeOptionConstants.CcdSubchannelExtension);
            CcdRuntimeFunctions.ValidateSizes(new FileInfo(Path.ChangeExtension(source, RuntimeOptionConstants.CcdImageExtension)).Length, new FileInfo(subchannel).Length);
            using (var subchannelStream = File.OpenRead(subchannel))
                CcdRuntimeFunctions.ValidateSubchannels(subchannelStream);
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
            if (cueSheet is not null)
            {
                var stagedBinary = directory.AddFile(Path.GetFullPath(cueSheet.Files[0].DeclaredPath, sourceDirectory));
                var prepared = CueRuntimeFunctions.Prepare(cueSheet, stagedBinary, stagedPath);
                using var cue = new FileStream(stagedPath, FileMode.Create, FileAccess.Write, FileShare.None);
                new CueSheetWriter().WriteAsync(prepared, cue).GetAwaiter().GetResult();
            }
    
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
