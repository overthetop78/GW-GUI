namespace GWGUI.Emulation.Atari.Common.Machines.Common.Functions;

internal static class CompatibilityFunctions
{
    internal static IReadOnlyList<T> Values<T>(params T[] values) => Array.AsReadOnly(values);

    internal static IReadOnlyDictionary<MachineModel, CompatibilityDefinition> Index(
        IReadOnlyList<CompatibilityDefinition> definitions)
    {
        var index = definitions.ToDictionary(definition => definition.Model);
        if (index.Count != definitions.Count)
            throw new InvalidOperationException(ErrorMessages.DuplicateCompatibilityDefinition);
        return index;
    }

    internal static MediaCompatibilityRule Media(MediaCategory category, params EmulationMediaSlot[] slots) =>
        new(category, Array.AsReadOnly(slots));

    internal static MediaCompatibilityRule UnavailableMedia(
        MediaCategory category,
        string explanationResourceKey,
        params EmulationMediaSlot[] slots) =>
        new(category, Array.AsReadOnly(slots), MediaAvailability.Unavailable, explanationResourceKey);

    internal static bool IsFirmwareCompatible(CompatibilityDefinition definition, FirmwareCategory category) =>
        definition.Firmware.Contains(category);

    internal static bool IsMediaCompatible(CompatibilityDefinition definition, MediaCategory category,
        EmulationMediaSlot slot) => definition.Media.Any(rule => rule.Category == category
            && rule.Availability == MediaAvailability.Available && rule.Slots.Contains(slot));

    internal static void Validate(CompatibilityDefinition definition)
    {
        if (definition.Media.Any(rule => rule.Availability == MediaAvailability.Unavailable
                                         && string.IsNullOrWhiteSpace(rule.ExplanationResourceKey)))
            throw new InvalidOperationException(ErrorMessages.MissingUnavailableExplanation);
        if (definition.ControllerPortCount < ControllerPortConstants.MinimumControllerPort)
            throw new InvalidOperationException(ErrorMessages.InvalidCompatibilityControllerCount);
    }

    internal static CompatibilityDefinition Create(MachineModel model)
    {
        var family = ConfigurationFunctions.GetFamily(model);
        return family == MachineFamily.St ? CreateSt(model) : CreateHardware(model);
    }

    private static CompatibilityDefinition CreateSt(MachineModel model)
    {
        return NewDefinition(model, Emulator.Hatari, Values(FirmwareCategory.Tos),
            Values(
                Media(MediaCategory.Floppy, EmulationMediaSlot.Floppy0, EmulationMediaSlot.Floppy1,
                    EmulationMediaSlot.Floppy2, EmulationMediaSlot.Floppy3),
                Media(MediaCategory.HardDisk, EmulationMediaSlot.HardDisk0),
                Media(MediaCategory.Directory, EmulationMediaSlot.HardDisk0)),
            ControllerPortConstants.TwoControllerPorts);
    }

    private static CompatibilityDefinition CreateHardware(MachineModel model)
    {
        var hardware = HardwareModelCatalog.Get(model);
        var family = ConfigurationFunctions.GetFamily(model);
        var hasKeyboard = family == MachineFamily.EightBit;
        var media = hardware.Media.Select(CreateMediaRule).ToList();
        if (model == MachineModel.Jaguar)
            media.Add(UnavailableMedia(MediaCategory.CompactDisc,
                SettingsDescriptionFunctionsConstants.ResourceUnavailableJaguarStandardNoCd,
                EmulationMediaSlot.Cd0));
        var portCount = hardware.Ports.Max(port => port.Count);
        var visibleTabs = EnumValues<SettingsTab>()
            .Where(tab => hasKeyboard || tab != SettingsTab.Keyboard)
            .Where(tab => hardware.Firmware.Count > 0 || tab != SettingsTab.Firmware)
            .Where(tab => tab != SettingsTab.Mouse)
            .ToArray();
        return NewDefinition(model, hardware.Emulator, hardware.Firmware, media, portCount, visibleTabs);
    }

    private static MediaCompatibilityRule CreateMediaRule(MediaCategory category) => category switch
    {
        MediaCategory.Floppy => Media(category, EmulationMediaSlot.Floppy0, EmulationMediaSlot.Floppy1,
            EmulationMediaSlot.Floppy2, EmulationMediaSlot.Floppy3),
        MediaCategory.Cassette => Media(category, EmulationMediaSlot.Cassette0),
        MediaCategory.Cartridge => Media(category, EmulationMediaSlot.Cartridge0),
        MediaCategory.CompactDisc => Media(category, EmulationMediaSlot.Cd0),
        _ => throw new ArgumentOutOfRangeException(nameof(category), category, ErrorMessages.IncompatibleMedia)
    };

    private static CompatibilityDefinition NewDefinition(MachineModel model, Emulator core,
        IReadOnlyList<FirmwareCategory> firmware,
        IReadOnlyList<MediaCompatibilityRule> media, int controllerPortCount,
        IReadOnlyList<SettingsTab>? visibleTabs = null)
    {
        var definition = new CompatibilityDefinition(model, core, visibleTabs ?? EnumValues<SettingsTab>(),
            firmware, media, controllerPortCount);
        Validate(definition);
        return definition;
    }

    private static IReadOnlyList<T> EnumValues<T>() where T : struct, Enum =>
        Array.AsReadOnly(Enum.GetValues<T>());
}
