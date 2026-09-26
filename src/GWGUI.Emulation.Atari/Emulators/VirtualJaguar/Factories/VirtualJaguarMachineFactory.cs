using GWGUI.Emulation.Atari.Emulators.VirtualJaguar.Constants;
using GWGUI.Emulation.Atari.Emulators.VirtualJaguar.Contracts;
using GWGUI.Emulation.Atari.Emulators.VirtualJaguar.Functions;

namespace GWGUI.Emulation.Atari.Emulators.VirtualJaguar.Factories;

internal sealed class VirtualJaguarMachineFactory() : MachineFactory(EmulatorConstants.Entry)
{
    internal override IReadOnlySet<string> CartridgeExtensions => EmulatorConstants.CartridgeExtensions;
    internal override bool SupportsCartridgeRegion => EmulatorConstants.SupportsCartridgeRegion;

    public override IReadOnlyDictionary<string, string> PrepareOptions(
        IReadOnlyDictionary<string, string> options) => VirtualJaguarOptionFunctions.ToNative(options);

    public override EmulatorPreparedContent? PrepareContent(MachineConfiguration configuration,
        MediaConfiguration? media, string sessionDirectory, ExternalCoreInfo coreInfo)
    {
        if (configuration.Model != MachineModel.JaguarCd || media?.Category != MediaCategory.CompactDisc)
            return base.PrepareContent(configuration, media, sessionDirectory, coreInfo);
        var prepared = JaguarCdFunctions.Prepare(configuration, media,
            coreInfo.NeedsFullPath, coreInfo.Extensions);
        return new EmulatorPreparedContent(media, prepared.RuntimePath, prepared.NeedsFullPath,
            configuration.Options, ActivityPaths: prepared.ActivityPaths, State: prepared);
    }

    public override void ValidateInsertion(MachineConfiguration configuration, MediaConfiguration media) =>
        JaguarCdFunctions.RejectForStandardJaguar(configuration.Model, media);

    public override bool SupportsEjection(EmulationMediaSlot slot) => false;

    public override Exception ContentLoadException(string message) => JaguarCdFunctions.Unsupported(message);
}
