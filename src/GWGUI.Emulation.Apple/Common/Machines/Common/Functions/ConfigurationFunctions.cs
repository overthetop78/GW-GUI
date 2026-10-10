using GWGUI.Emulation.Apple.Common.Constants;
using GWGUI.Emulation.Apple.Common.Machines.Common.Contracts;
using GWGUI.Emulation.Apple.Common.Machines.Common.Enums;
using GWGUI.Emulation.Apple.Common.Machines.Common.Dictionaries;
using GWGUI.Emulation.Apple.Common.Contracts;
using GWGUI.Emulation.Apple.Emulators.AppleWin.Dictionaries;
using GWGUI.Emulation.Apple.Emulators.MiniVmac.Dictionaries;
using AppleWinOptionConstants = GWGUI.Emulation.Apple.Emulators.AppleWin.Constants.OptionConstants;

namespace GWGUI.Emulation.Apple.Common.Machines.Common.Functions;

internal static class ConfigurationFunctions
{
    internal static bool SupportsRuntime(MachineModel model) => model is MachineModel.AppleII
        or MachineModel.AppleIIPlus or MachineModel.AppleIIJPlus or MachineModel.AppleIIe
        or MachineModel.AppleIIeEnhanced or MachineModel.MacintoshII;

    internal static int ControllerPortCount(MachineModel model) => model is MachineModel.AppleII
        or MachineModel.AppleIIPlus or MachineModel.AppleIIJPlus or MachineModel.AppleIIe
        or MachineModel.AppleIIeEnhanced ? 1 : 0;

    internal static void Validate(MachineConfiguration configuration)
    {
        if (configuration.SchemaVersion != ConfigurationStoreConstants.CurrentSchemaVersion)
            throw new ArgumentOutOfRangeException(nameof(configuration.SchemaVersion));
        if (configuration.Id == Guid.Empty || !Enum.IsDefined(configuration.Model))
            throw new ArgumentException(nameof(configuration));
        var hardware = HardwareModelCatalog.Get(configuration.Model);
        if (configuration.Core != hardware.Emulator) throw new ArgumentException(nameof(configuration.Core));
        var catalog = configuration.Core == Emulator.AppleWin ? AppleWinOptionCatalog.All : MiniVmacOptionCatalog.All;
        foreach (var option in catalog)
            if (configuration.Options.TryGetValue(option.Key, out var value)
                && !option.Values.Any(choice => string.Equals(choice.Value, value, StringComparison.Ordinal)))
                throw new ArgumentException(nameof(configuration.Options));
        if (configuration.Core == Emulator.AppleWin
            && configuration.Options.TryGetValue(AppleWinOptionConstants.Machine, out var nativeModel)
            && !string.Equals(nativeModel, AppleWinModelCatalog.GetNativeValue(configuration.Model), StringComparison.Ordinal))
            throw new ArgumentException(nameof(configuration.Model));
        var firmwareRoles = new HashSet<FirmwareCategory>();
        foreach (var firmware in configuration.Firmwares)
            if (configuration.Core != Emulator.MiniVmac || !firmwareRoles.Add(firmware.Category)
                || !HardwareModelFunctions.IsFirmwareCompatible(hardware, firmware.Category)
                || string.IsNullOrWhiteSpace(firmware.Path))
                throw new ArgumentException(nameof(configuration.Firmwares));
        var slots = new HashSet<EmulationMediaSlot>();
        foreach (var media in configuration.Media)
        {
            if (!SupportsRuntime(configuration.Model) || string.IsNullOrWhiteSpace(media.Path)
                || !slots.Add(media.Slot) || !HardwareModelFunctions.IsMediaCompatible(hardware, media.Category, media.Slot)
                || media.Category == MediaCategory.Cassette
                || configuration.Core == Emulator.MiniVmac && media.Slot == EmulationMediaSlot.Floppy1)
                throw new ArgumentException(nameof(configuration.Media));
        }
        // The Mini vMac frontend accepts a single initial disk path and exposes no disk-control callbacks.
        if (configuration.Core == Emulator.MiniVmac && configuration.Media.Count > 1)
            throw new ArgumentException(nameof(configuration.Media));
        var ports = new HashSet<int>();
        foreach (var controller in configuration.Input.Controllers ?? [])
            if (controller.Port < 0 || controller.Port >= ControllerPortCount(configuration.Model)
                || !ports.Add(controller.Port) || controller.DeadZonePercent is < ControllerConstants.MinimumDeadZonePercent
                    or > ControllerConstants.MaximumDeadZonePercent
                || controller.Peripheral is not (PeripheralCategory.None or PeripheralCategory.Automatic
                    or PeripheralCategory.AnalogJoystick))
                throw new ArgumentException(nameof(configuration.Input));
    }
}
