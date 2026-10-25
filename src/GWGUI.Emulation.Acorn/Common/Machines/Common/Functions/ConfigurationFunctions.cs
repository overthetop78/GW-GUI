using CommonControllerFunctions = GWGUI.Emulation.Acorn.Common.Functions.ControllerFunctions;

namespace GWGUI.Emulation.Acorn.Common.Machines.Common.Functions;

internal static class ConfigurationFunctions
{
    internal static bool SupportsRuntime(MachineModel model) => EmulatorCatalog.GetAll(model.ToString()).Count > 0;

    internal static int ControllerPortCount(MachineModel model) => HardwareModelCatalog.Get(model).Ports.Where(port => port.Capability == HardwarePortCapability.AnalogJoystick).Sum(port => port.Count);

    internal static Emulator DefaultEmulator(MachineModel model)
    {
        if (!Enum.IsDefined(model)) throw new ArgumentOutOfRangeException(nameof(model));
        var definition = EmulatorCatalog.GetAll(model.ToString()).FirstOrDefault();
        return definition is null ? Emulator.None : Enum.Parse<Emulator>(new Engine().Adapter(definition.Id).EmulatorKey);
    }

    internal static void Validate(MachineConfiguration configuration)
    {
        if (configuration.SchemaVersion != ConfigurationStoreConstants.CurrentSchemaVersion)
            throw new ArgumentOutOfRangeException(nameof(configuration), ErrorMessages.UnsupportedSchema);
        if (!Enum.IsDefined(configuration.Model) || !Enum.IsDefined(configuration.Core)
            || configuration.Core != Emulator.None && !EmulatorCatalog.Get(configuration.Core).MachineIds.Contains(configuration.MachineId))
            throw new ArgumentOutOfRangeException(nameof(configuration));
        if (configuration.Firmwares.Count > 0)
            throw new ArgumentException(ErrorMessages.IncompatibleFirmware, nameof(configuration));
        if (configuration.Core != Emulator.None && new Engine().Adapter(configuration) is IEmulatorConfigurationOptions options)
            options.ValidateOptions(configuration);
        var devices = configuration.Core != Emulator.None && new Engine().Adapter(configuration) is { } storage ? storage.DescribeMediaDevices(configuration) : [];
        var mediaSlots = new HashSet<EmulationMediaSlot>();
        foreach (var media in configuration.Media)
        {
            if (string.IsNullOrWhiteSpace(media.Path))
                throw new ArgumentException(ErrorMessages.EmptyMediaPath, nameof(configuration));
            if (!mediaSlots.Add(media.Slot))
                throw new ArgumentException(ErrorMessages.DuplicateMediaSlot, nameof(configuration));
            var mounted = EmulationMediaConversionFunctions.ToCommon(media);
            if (!devices.Any(device => device.Slot == media.Slot && device.MediaType == mounted.Type))
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
            if (!CommonControllerFunctions.Peripherals(configuration.Model).Contains(controller.Peripheral))
                throw new ArgumentException(ErrorMessages.UnsupportedControllerDevice, nameof(configuration));
        }
    }
}
