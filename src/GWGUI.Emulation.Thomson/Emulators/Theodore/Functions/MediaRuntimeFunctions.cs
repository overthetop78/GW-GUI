using System.IO;
using GWGUI.Emulation.Thomson.Emulators.Theodore.Constants;

namespace GWGUI.Emulation.Thomson.Emulators.Theodore.Functions;
internal static class MediaRuntimeFunctions
{
    internal static MediaCategory Category(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        return Path.GetExtension(path).ToLowerInvariant() switch
        {
            MediaRuntimeConstants.Floppy or MediaRuntimeConstants.SapFloppy => MediaCategory.Floppy,
            MediaRuntimeConstants.Tape => MediaCategory.Tape,
            MediaRuntimeConstants.MoCartridge or MediaRuntimeConstants.ToCartridge or MediaRuntimeConstants.RomCartridge => MediaCategory.Cartridge,
            _ => throw new EmulationMessageException(new EmulationMessage(EmulationMessageCategory.Media, EmulationMessageCode.MediaUnsupported, EmulationMessageSeverity.Error, EmulationMessageTarget.Dialog))
        };
    }

    internal static IReadOnlyList<string> InitialContentPaths(MachineConfiguration configuration) => configuration.InitialContent is { Length: > 0 } path ? [Path.GetFullPath(path)] : configuration.Media.Where(media => media.IsInserted).Select(media => Path.GetFullPath(media.Path)).ToArray();
    internal static IReadOnlyList<EmulationMediaDevice> Devices(MachineConfiguration configuration)
    {
        var cartridge = configuration.Family == MachineFamily.Mo ? MediaRuntimeConstants.MoCartridge : MediaRuntimeConstants.ToCartridge;
        return[
            new(EmulationMediaSlot.Cassette0, EmulationMediaType.Cassette, [MediaRuntimeConstants.Tape], RequiresMachineRecreation: true),
            new(EmulationMediaSlot.Floppy0, EmulationMediaType.Floppy, [MediaRuntimeConstants.Floppy, MediaRuntimeConstants.SapFloppy], RequiresMachineRecreation: true),
            new(EmulationMediaSlot.Cartridge0, EmulationMediaType.Cartridge, [cartridge, MediaRuntimeConstants.RomCartridge], RequiresMachineRecreation: true)
        ];
    }
}