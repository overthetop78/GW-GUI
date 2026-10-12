using GWGUI.Emulation.Acorn.Emulators.B2.Constants;
using GWGUI.Emulation.Acorn.Emulators.B2.Functions;
using GWGUI.Emulation.Acorn.Emulators.B2.Dictionaries;

namespace GWGUI.Emulation.Acorn.Common.Machines.Common.Functions;

internal static class ConfigurationFunctions
{
    internal static bool SupportsRuntime(MachineModel model) => B2ModelFunctions.GetModels(model).Count > 0;

    internal static int ControllerPortCount(MachineModel model) => SupportsRuntime(model) ? BbcMicroModelConstants.TwoPorts : ControllerPortConstants.MinimumControllerPort;

    internal static void Validate(MachineConfiguration configuration)
    {
        if (configuration.SchemaVersion != ConfigurationStoreConstants.CurrentSchemaVersion)
            throw new ArgumentOutOfRangeException(nameof(configuration), ErrorMessages.UnsupportedSchema);
        if (!Enum.IsDefined(configuration.Model) || configuration.Core != Emulator.B2)
            throw new ArgumentOutOfRangeException(nameof(configuration));
        if (configuration.Firmwares.Count > 0)
            throw new ArgumentException(ErrorMessages.IncompatibleFirmware, nameof(configuration));
        if (configuration.Options.TryGetValue(OptionConstants.Model, out var selectedModel)
            && !B2ModelFunctions.IsCompatible(configuration.Model, selectedModel))
            throw new ArgumentException(ErrorMessages.OptionValueInvalidFormat, nameof(configuration));
        if (configuration.Options.TryGetValue(OptionConstants.AutoBoot, out var autoBoot)
            && autoBoot is not (OptionConstants.Enabled or OptionConstants.Disabled))
            throw new ArgumentException(ErrorMessages.OptionValueInvalidFormat, nameof(configuration));
        foreach (var option in B2OptionCatalog.JoypadOptions)
            if (configuration.Options.TryGetValue(option, out var key)
                && !B2OptionCatalog.KeyboardValues.Contains(key, StringComparer.Ordinal))
                throw new ArgumentException(ErrorMessages.OptionValueInvalidFormat, nameof(configuration));
        var mediaSlots = new HashSet<EmulationMediaSlot>();
        foreach (var media in configuration.Media)
        {
            if (string.IsNullOrWhiteSpace(media.Path))
                throw new ArgumentException(ErrorMessages.EmptyMediaPath, nameof(configuration));
            if (!mediaSlots.Add(media.Slot))
                throw new ArgumentException(ErrorMessages.DuplicateMediaSlot, nameof(configuration));
            // b2's disk callbacks address drive 0; physical capability is broader.
            if (!SupportsRuntime(configuration.Model) || media.Category != MediaCategory.Floppy
                || media.Slot != EmulationMediaSlot.Floppy0)
                throw new ArgumentException(ErrorMessages.IncompatibleMedia, nameof(configuration));
        }
        var ports = new HashSet<int>();
        foreach (var controller in configuration.Input.Controllers ?? [])
        {
            if (controller.Port < ControllerPortConstants.MinimumControllerPort
                || controller.Port >= ControllerPortCount(configuration.Model))
                throw new ArgumentOutOfRangeException(nameof(configuration), ErrorMessages.InvalidControllerPort);
            if (!ports.Add(controller.Port))
                throw new ArgumentException(ErrorMessages.DuplicateControllerPort, nameof(configuration));
            if (controller.DeadZonePercent is < ControllerConstants.MinimumDeadZonePercent
                or > ControllerConstants.MaximumDeadZonePercent)
                throw new ArgumentOutOfRangeException(nameof(configuration), ErrorMessages.InvalidControllerDeadZone);
            if (!ControllerFunctions.Peripherals(configuration.Model).Contains(controller.Peripheral))
                throw new ArgumentException(ErrorMessages.UnsupportedControllerDevice, nameof(configuration));
        }
    }
}
