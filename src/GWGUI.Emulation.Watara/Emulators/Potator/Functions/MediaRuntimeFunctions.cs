using System.IO;

namespace GWGUI.Emulation.Watara.Emulators.Potator.Functions;
internal static class MediaRuntimeFunctions
{
    private const string BinExtension = ".bin";
    private const string SupervisionExtension = ".sv";
    internal static MediaCategory Category(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        return Path.GetExtension(path).ToLowerInvariant() switch
        {
            BinExtension or SupervisionExtension => MediaCategory.Cartridge,
            _ => throw GWGUI.Emulation.Services.EmulationErrorService.CreateError(EmulationMessageCategory.Media, EmulationMessageCode.MediaUnsupported, null)
        };
    }

    internal static IReadOnlyList<string> InitialContentPaths(MachineConfiguration configuration) => configuration.Media.Where(media => media.IsInserted).Select(media => Path.GetFullPath(media.Path)).ToArray();
    internal static IReadOnlyList<EmulationMediaDevice> Devices(MachineConfiguration configuration) => [new(EmulationMediaSlot.Cartridge0, EmulationMediaType.Cartridge, [BinExtension, SupervisionExtension], RequiresMachineRecreation: true, IsPermanent: true)];
}
