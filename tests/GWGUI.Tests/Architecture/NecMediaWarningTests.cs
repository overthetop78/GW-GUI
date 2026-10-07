using GWGUI.Emulation.Contracts;
using GWGUI.Emulation.Enums;
using GWGUI.Emulation.Nec.Common.Machines.Common.Contracts;
using GWGUI.Emulation.Nec.Common.Machines.Common.Constants;
using GWGUI.Emulation.Nec.Common.Machines.PcEngine.Constants;
using GWGUI.Emulation.Nec.Common.Services;
using GWGUI.Emulation.Nec.Emulators.BeetlePceFast.Constants;
using GWGUI.Emulation.Nec.Emulators.Common.Interop.Constants;
using GWGUI.Emulation.Nec.Emulators.Geargrafx.Constants;
using GWGUI.Emulation.Nec.Emulators.Geargrafx.Functions;

namespace GWGUI.Tests.Architecture;

public sealed class NecMediaWarningTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void MissingMediaListsOnlyConfiguredDrives(bool cdDriveEnabled)
    {
        var slots = cdDriveEnabled
            ? new[] { EmulationMediaSlot.Cartridge0, EmulationMediaSlot.Cd0 }
            : new[] { EmulationMediaSlot.Cartridge0 };
        var options = new Dictionary<string, string>();
        GeargrafxStorageFunctions.ApplyOptions(options, slots);
        var configuration = new MachineConfiguration(PcEngineMachineConstants.Id,
            GeargrafxConstants.Id, options);

        var warning = Machine.RequiredMediaWarning(configuration, new Exception());

        Assert.Equal(EmulationMessageCode.RequiredMediaMissing, warning.MessageData.MessageCode);
        Assert.Equal(EmulationMessageSeverity.Warning, warning.MessageData.Severity);
        Assert.Equal(EmulationMessageTarget.Dialog, warning.MessageData.Target);
        var required = Assert.IsType<EmulationRequiredMachineMediaMessageContext>(
            warning.MessageData.Context);
        Assert.Equal(cdDriveEnabled
                ? new[] { EmulationMediaCategory.CartridgeSlot, EmulationMediaCategory.CompactDiscDrive }
                : new[] { EmulationMediaCategory.CartridgeSlot },
            required.RequiredMedia);
        Assert.Equal(cdDriveEnabled
                ? new[] { BeetlePceFastStorageConstants.CartridgeSlotLabel,
                    StorageSettingsFunctionsConstants.CdDriveLabel }
                : new[] { BeetlePceFastStorageConstants.CartridgeSlotLabel },
            required.RequiredMediaNames);
    }
}
