using GWGUI.Emulation.Enums;
using GWGUI.Emulation.Thomson.Common.Constants;
using GWGUI.Emulation.Thomson.Common.Machines.Common.Constants;
using GWGUI.Emulation.Thomson.Common.Machines.Common.Contracts;
using GWGUI.Emulation.Thomson.Common.Machines.Common.Enums;

namespace GWGUI.Emulation.Thomson.Common.Machines.Common.Functions;
internal static partial class ConfigurationFunctions
{
    internal static IReadOnlyList<EmulationMediaSlot> IntegratedStorageSlots(MachineModel model) => model switch
    {
        MachineModel.Mo6 => [EmulationMediaSlot.Cartridge0, EmulationMediaSlot.Cassette0],
        MachineModel.To8D or MachineModel.To9 or MachineModel.To9Plus => [EmulationMediaSlot.Cartridge0, EmulationMediaSlot.Floppy0],
        MachineModel.Mo5 or MachineModel.To7 or MachineModel.To770 or MachineModel.To8 => [EmulationMediaSlot.Cartridge0],
        _ => throw new ArgumentOutOfRangeException(nameof(model))
    };

    internal static MachineFamily Family(MachineModel model) => model switch
    {
        MachineModel.Mo5 or MachineModel.Mo6 => MachineFamily.Mo,
        MachineModel.To7 or MachineModel.To770 or MachineModel.To8 or MachineModel.To8D or MachineModel.To9 or MachineModel.To9Plus => MachineFamily.To,
        _ => throw new ArgumentOutOfRangeException(nameof(model))
    };
    internal static int ControllerPortCount(MachineModel model)
    {
        _ = Family(model);
        return ControllerConstants.PortCount;
    }

    internal static void Validate(MachineConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        if (configuration.Id == Guid.Empty || configuration.SchemaVersion != ConfigurationStoreConstants.CurrentSchemaVersion || !Enum.IsDefined(configuration.Model) || !Enum.IsDefined(configuration.Core))
            throw new ArgumentException(nameof(configuration));
        if (configuration.ConfiguredSlots.Distinct().Count() != configuration.ConfiguredSlots.Count)
            throw new ArgumentException(nameof(configuration.ConfiguredSlots));
        if (IntegratedStorageSlots(configuration.Model).Any(slot => !configuration.ConfiguredSlots.Contains(slot)))
            throw new ArgumentException(nameof(configuration.ConfiguredSlots));
        foreach (var slot in configuration.ConfiguredSlots)
            if (slot.Index < 0 || slot.Category is not (EmulationMediaCategory.CassetteDrive or EmulationMediaCategory.FloppyDrive or EmulationMediaCategory.CartridgeSlot))
                throw new ArgumentException(nameof(configuration.ConfiguredSlots));
        if (configuration.Media.Select(item => item.Slot).Distinct().Count() != configuration.Media.Count)
            throw new ArgumentException(nameof(configuration.Media));
        foreach (var media in configuration.Media)
        {
            if (!configuration.ConfiguredSlots.Contains(media.Slot) || string.IsNullOrWhiteSpace(media.Path))
                throw new ArgumentException(nameof(configuration.Media));
            var expected = media.Slot.Category switch
            {
                EmulationMediaCategory.CassetteDrive => MediaCategory.Tape,
                EmulationMediaCategory.FloppyDrive => MediaCategory.Floppy,
                EmulationMediaCategory.CartridgeSlot => MediaCategory.Cartridge,
                _ => throw new ArgumentException(nameof(configuration.Media))
            };
            if (media.Category != expected)
                throw new ArgumentException(nameof(configuration.Media));
        }

        if (configuration.Options.Any(option => string.IsNullOrWhiteSpace(option.Key) || option.Value is null))
            throw new ArgumentException(nameof(configuration.Options));
        if (configuration.Input.MouseSpeedPercent is < InputMappingConstants.MinimumMouseSpeedPercent or > InputMappingConstants.MaximumMouseSpeedPercent)
            throw new ArgumentException(nameof(configuration.Input));
        InputSettingsFunctions.Validate(configuration);
        var ports = new HashSet<int>();
        foreach (var controller in configuration.Input.Controllers ?? [])
            if (controller.Port < ControllerConstants.FirstPort || !ports.Add(controller.Port) || !Enum.IsDefined(controller.Peripheral) || controller.DeadZonePercent is < ControllerConstants.MinimumDeadZonePercent or > ControllerConstants.MaximumDeadZonePercent)
                throw new ArgumentException(nameof(configuration.Input));
    }
}
