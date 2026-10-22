using GWGUI.Emulation.Nec.Emulators.Common.Interop.Factories;
using System.IO;
using GWGUI.Emulation.Nec.Common.Machines.CoreGrafx.Constants;
using GWGUI.Emulation.Nec.Common.Machines.LaserActive.Constants;
using GWGUI.Emulation.Nec.Common.Machines.PcEngine.Constants;
using GWGUI.Emulation.Nec.Common.Machines.PcEngineDuo.Constants;
using GWGUI.Emulation.Nec.Common.Machines.PcEngineLt.Constants;
using GWGUI.Emulation.Nec.Common.Machines.SuperGrafx.Constants;
using GWGUI.Emulation.Nec.Common.Machines.TurboExpress.Constants;
using GWGUI.Emulation.Nec.Emulators.Common.Interop.Contracts;
using GWGUI.Emulation.Nec.Emulators.BeetlePceFast.Functions;
using GWGUI.Emulation.Nec.Emulators.Common.Interop.Functions;
using GWGUI.Emulation.Nec.Emulators.Common.Interop.Services;
using GWGUI.Emulation.Nec.Emulators.Geargrafx.Constants;
using GWGUI.Emulation.Nec.Emulators.Geargrafx.Functions;

namespace GWGUI.Emulation.Nec.Emulators.Geargrafx.Factories;

internal sealed class GeargrafxMachineFactory : CoreMachineFactory, IEmulatorMachineDefaults, IEmulatorFirmwareInspection
{
    public ScannedFirmware? IdentifyFirmware(ScannedFirmware firmware) => GeargrafxFirmwareFunctions.Identify(firmware);

    public bool IsDefaultFor(string machineId) => machineId == LaserActiveMachineConstants.Id;

    public override EmulationEmulatorDefinition Definition { get; } = new(
        GeargrafxConstants.Id, GeargrafxConstants.DisplayName,
        GeargrafxConstants.DescriptionResourceKey,
        new[] { PcEngineMachineConstants.Id, CoreGrafxMachineConstants.Id,
            PcEngineDuoMachineConstants.Id, PcEngineLtMachineConstants.Id,
            TurboExpressMachineConstants.Id,
            GWGUI.Emulation.Nec.Common.Machines.PcEngineCd.Constants.MachineConstants.Id,
            GWGUI.Emulation.Nec.Common.Machines.PcEngineSuperCd.Constants.MachineConstants.Id, SuperGrafxMachineConstants.Id,
            LaserActiveMachineConstants.Id }.ToHashSet(StringComparer.Ordinal));

    public override string EmulatorId => GeargrafxConstants.Id;
    protected override CoreReleaseSettings ReleaseSettings => GeargrafxConstants.ReleaseSettings;
    protected override MachineConfiguration Configure(MachineConfiguration configuration) => BeetlePceFastOptionFunctions.ToNative(configuration);

    protected override void PrepareSession(MachineConfiguration configuration, EmulatorCreationContext context, string sessionDirectory) =>
        GeargrafxFirmwareFunctions.Prepare(context.FirmwareDirectory!, sessionDirectory, configuration.FirmwarePath);
}
