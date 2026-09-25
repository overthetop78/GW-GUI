using GWGUI.Emulation;
using System.Text.Json;

namespace GWGUI.Emulation.Atari.Common.Functions;

internal static class ConfigurationFunctions
{
    internal static Emulator GetCore(MachineModel model) => model switch
    {
        MachineModel.St or MachineModel.Stf or MachineModel.Stfm or MachineModel.MegaSt
            or MachineModel.Ste or MachineModel.MegaSte or MachineModel.Tt or MachineModel.Falcon
            => Emulator.Hatari,
        MachineModel.Atari400 or MachineModel.Atari800 or MachineModel.Atari800Xl
            or MachineModel.Atari130Xe or MachineModel.Xegs or MachineModel.XlXe
            or MachineModel.Atari5200
            => Emulator.Atari800,
        MachineModel.Atari2600 => Emulator.Stella,
        MachineModel.Atari7800 => Emulator.ProSystem,
        MachineModel.Lynx => Emulator.BeetleLynx,
        MachineModel.Jaguar or MachineModel.JaguarCd => Emulator.VirtualJaguar,
        _ => throw new ArgumentOutOfRangeException(nameof(model), model, null)
    };

    internal static MachineFamily GetFamily(MachineModel model) => model switch
    {
        MachineModel.St or MachineModel.Stf or MachineModel.Stfm or MachineModel.MegaSt
            or MachineModel.Ste or MachineModel.MegaSte or MachineModel.Tt or MachineModel.Falcon
            => MachineFamily.St,
        MachineModel.Atari400 or MachineModel.Atari800 or MachineModel.Atari800Xl
            or MachineModel.Atari130Xe or MachineModel.Xegs or MachineModel.XlXe
            => MachineFamily.EightBit,
        MachineModel.Atari5200 => MachineFamily.Atari5200,
        MachineModel.Atari2600 => MachineFamily.Atari2600,
        MachineModel.Atari7800 => MachineFamily.Atari7800,
        MachineModel.Lynx => MachineFamily.Lynx,
        MachineModel.Jaguar or MachineModel.JaguarCd => MachineFamily.Jaguar,
        _ => throw new ArgumentOutOfRangeException(nameof(model), model, null)
    };

    internal static void Validate(int schemaVersion, MachineModel model,
        IReadOnlyList<FirmwareConfiguration> firmwares, IReadOnlyList<MediaConfiguration> media,
        InputConfiguration input)
    {
        if (schemaVersion != CommonConstants.CurrentConfigurationSchemaVersion)
            throw new ArgumentOutOfRangeException(nameof(schemaVersion), ErrorMessages.UnsupportedSchema);

        ValidateFirmware(model, firmwares);
        ValidateMedia(model, media);
        ValidateInput(model, input);
    }

    private static void ValidateFirmware(MachineModel model, IReadOnlyList<FirmwareConfiguration> firmwares)
    {
        var categories = new HashSet<FirmwareCategory>();
        foreach (var firmware in firmwares)
        {
            if (string.IsNullOrWhiteSpace(firmware.Path))
                throw new ArgumentException(ErrorMessages.EmptyFirmwarePath, nameof(firmwares));
            if (!categories.Add(firmware.Category))
                throw new ArgumentException(ErrorMessages.DuplicateFirmware, nameof(firmwares));
            if (!IsFirmwareCompatible(model, firmware.Category))
                throw new ArgumentException(ErrorMessages.IncompatibleFirmware, nameof(firmwares));
        }
    }

    private static bool IsFirmwareCompatible(MachineModel model, FirmwareCategory category)
        => CompatibilityFunctions.IsFirmwareCompatible(CompatibilityCatalog.Get(model), category);

    private static void ValidateMedia(MachineModel model, IReadOnlyList<MediaConfiguration> media)
    {
        var slots = new HashSet<EmulationMediaSlot>();
        foreach (var item in media)
        {
            if (string.IsNullOrWhiteSpace(item.Path))
                throw new ArgumentException(ErrorMessages.EmptyMediaPath, nameof(media));
            if (!slots.Add(item.Slot))
                throw new ArgumentException(ErrorMessages.DuplicateMediaSlot, nameof(media));
            if (!IsMediaCompatible(model, item.Category, item.Slot))
                throw new ArgumentException(ErrorMessages.IncompatibleMedia, nameof(media));
        }
    }

    private static bool IsMediaCompatible(MachineModel model, MediaCategory category, EmulationMediaSlot slot)
        => CompatibilityFunctions.IsMediaCompatible(CompatibilityCatalog.Get(model), category, slot);

    private static void ValidateInput(MachineModel model, InputConfiguration input)
    {
        var compatiblePortCount = CompatibilityCatalog.Get(model).ControllerPortCount;
        var ports = new HashSet<int>();
        foreach (var controller in input.Controllers ?? [])
        {
            if (controller.Port < CommonConstants.MinimumControllerPort
                || controller.Port >= compatiblePortCount)
                throw new ArgumentOutOfRangeException(nameof(input), ErrorMessages.InvalidControllerPort);
            if (!ports.Add(controller.Port))
                throw new ArgumentException(ErrorMessages.DuplicateControllerPort, nameof(input));
            if (controller.DeadZonePercent is < ControllerConstants.MinimumDeadZonePercent
                or > ControllerConstants.MaximumDeadZonePercent)
                throw new ArgumentOutOfRangeException(nameof(input), ErrorMessages.InvalidControllerDeadZone);
            if (!ControllerFunctions.Peripherals(model).Contains(controller.Peripheral))
                throw new ArgumentException(ErrorMessages.UnsupportedControllerDevice, nameof(input));
        }
    }
}

internal static class ConfigurationMigrationFunctions
{
    internal static ConfigurationDocument MigrateToCurrent(JsonElement root)
    {
        if (!root.TryGetProperty(ConfigurationMigrationConstants.SchemaVersionPropertyName,
                out var schemaProperty)
            || !schemaProperty.TryGetInt32(out var schemaVersion))
            throw new InvalidDataException(ConfigurationStoreConstants.UnsupportedSchemaError);
        return schemaVersion switch
        {
            CommonConstants.CurrentConfigurationSchemaVersion =>
                root.Deserialize<ConfigurationDocument>(ConfigurationStoreConstants.JsonOptions)
                ?? throw new InvalidDataException(ConfigurationStoreConstants.EmptyDocumentError),
            _ => throw new InvalidDataException(ConfigurationStoreConstants.UnsupportedSchemaError)
        };
    }
}
