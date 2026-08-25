using GWGUI.Emulation;
using System.Globalization;

namespace GWGUI.Emulation.Atari.Functions;

internal static class AtariCompatibilityFunctions
{
    internal static IReadOnlyList<T> Values<T>(params T[] values) => Array.AsReadOnly(values);

    internal static IReadOnlyDictionary<AtariMachineModel, AtariCompatibilityDefinition> Index(
        IReadOnlyList<AtariCompatibilityDefinition> definitions)
    {
        var index = definitions.ToDictionary(definition => definition.Model);
        if (index.Count != definitions.Count)
            throw new InvalidOperationException(AtariErrorMessages.DuplicateCompatibilityDefinition);
        return index;
    }

    internal static AtariOptionRule Editable(AtariSettingOption option) =>
        new(option, AtariOptionAvailability.Editable);

    internal static AtariOptionRule Forced(AtariSettingOption option, string value) =>
        new(option, AtariOptionAvailability.Forced, value,
            AtariCompatibilityConstants.ForcedByModelResource);

    internal static AtariOptionRule Unavailable(AtariSettingOption option, string explanationResourceKey) =>
        new(option, AtariOptionAvailability.Unavailable, ExplanationResourceKey: explanationResourceKey);

    internal static AtariOptionRule Hidden(AtariSettingOption option) =>
        new(option, AtariOptionAvailability.Hidden);

    internal static AtariMediaCompatibilityRule Media(AtariMediaCategory category, params EmulationMediaSlot[] slots) =>
        new(category, Array.AsReadOnly(slots));

    internal static AtariMediaCompatibilityRule UnavailableMedia(
        AtariMediaCategory category,
        string explanationResourceKey,
        params EmulationMediaSlot[] slots) =>
        new(category, Array.AsReadOnly(slots), AtariMediaAvailability.Unavailable, explanationResourceKey);

    internal static string JoinValues<T>(IEnumerable<T> values) =>
        string.Join(AtariCompatibilityConstants.ForcedValueSeparator, values);

    internal static bool IsFirmwareCompatible(AtariCompatibilityDefinition definition, AtariFirmwareCategory category) =>
        definition.Firmware.Contains(category);

    internal static bool IsMediaCompatible(AtariCompatibilityDefinition definition, AtariMediaCategory category,
        EmulationMediaSlot slot) => definition.Media.Any(rule => rule.Category == category
            && rule.Availability == AtariMediaAvailability.Available && rule.Slots.Contains(slot));

    internal static void Validate(AtariCompatibilityDefinition definition)
    {
        if (definition.Options.Select(rule => rule.Option).Distinct().Count() !=
            Enum.GetValues<AtariSettingOption>().Length)
            throw new InvalidOperationException(AtariErrorMessages.IncompleteCompatibilityOptions);
        if (definition.Options.Any(rule => rule.Availability is AtariOptionAvailability.Forced
                                               or AtariOptionAvailability.Unavailable
                                           && string.IsNullOrWhiteSpace(rule.ExplanationResourceKey)))
            throw new InvalidOperationException(AtariErrorMessages.MissingUnavailableExplanation);
        if (definition.Options.Any(rule => rule.Availability == AtariOptionAvailability.Forced
                                           && string.IsNullOrWhiteSpace(rule.ForcedValue)))
            throw new InvalidOperationException(AtariErrorMessages.MissingForcedOptionValue);
        if (definition.Media.Any(rule => rule.Availability == AtariMediaAvailability.Unavailable
                                         && string.IsNullOrWhiteSpace(rule.ExplanationResourceKey)))
            throw new InvalidOperationException(AtariErrorMessages.MissingUnavailableExplanation);
        if (definition.ControllerPortCount < AtariCompatibilityConstants.NoControllerPort)
            throw new InvalidOperationException(AtariErrorMessages.InvalidCompatibilityControllerCount);
    }

    internal static AtariCompatibilityDefinition Create(AtariMachineModel model)
    {
        var family = AtariConfigurationFunctions.GetFamily(model);
        return family == AtariMachineFamily.St ? CreateSt(model) : CreateClassic(model);
    }

    private static AtariCompatibilityDefinition CreateSt(AtariMachineModel model)
    {
        var hardware = AtariStModelCatalog.Get(model);
        var options = Values(
            Forced(AtariSettingOption.CpuModel, hardware.DefaultCpu.ToString()),
            Editable(AtariSettingOption.CpuPrecision),
            hardware.CpuFrequenciesMhz.Count > AtariCompatibilityConstants.SingleChoiceCount
                ? Editable(AtariSettingOption.CpuSpeed)
                : Forced(AtariSettingOption.CpuSpeed,
                    hardware.DefaultCpuFrequencyMhz.ToString(CultureInfo.InvariantCulture)),
            hardware.Fpus.Count > AtariCompatibilityConstants.SingleChoiceCount
                ? Editable(AtariSettingOption.Fpu)
                : Unavailable(AtariSettingOption.Fpu, AtariCompatibilityConstants.NoFpuResource),
            Editable(AtariSettingOption.MainMemory),
            hardware.AlternateMemoryMib.Any(value => value > AtariStModelConstants.NoAlternateMemoryMib)
                ? Editable(AtariSettingOption.AlternateMemory)
                : Unavailable(AtariSettingOption.AlternateMemory,
                    AtariCompatibilityConstants.NoAlternateMemoryResource),
            Hidden(AtariSettingOption.MosaicMemory),
            Hidden(AtariSettingOption.AxlonMemory),
            Hidden(AtariSettingOption.AxlonShadow),
            Hidden(AtariSettingOption.MapRam),
            Editable(AtariSettingOption.Firmware),
            Editable(AtariSettingOption.Region),
            Editable(AtariSettingOption.VideoStandard),
            Editable(AtariSettingOption.Renderer),
            Editable(AtariSettingOption.AudioEnabled),
            Editable(AtariSettingOption.Storage),
            Editable(AtariSettingOption.KeyboardMappings),
            Editable(AtariSettingOption.MouseSpeed),
            Editable(AtariSettingOption.MouseMappings),
            Editable(AtariSettingOption.ControllerMappings));
        return NewDefinition(model, AtariEmulator.Hatari, options, Values(AtariFirmwareCategory.Tos),
            Values(
                Media(AtariMediaCategory.Floppy, EmulationMediaSlot.Floppy0, EmulationMediaSlot.Floppy1,
                    EmulationMediaSlot.Floppy2, EmulationMediaSlot.Floppy3),
                Media(AtariMediaCategory.HardDisk, EmulationMediaSlot.HardDisk0),
                Media(AtariMediaCategory.Directory, EmulationMediaSlot.HardDisk0)),
            AtariCompatibilityConstants.TwoControllerPorts);
    }

    private static AtariCompatibilityDefinition CreateClassic(AtariMachineModel model)
    {
        var hardware = AtariClassicModelCatalog.Get(model);
        var family = AtariConfigurationFunctions.GetFamily(model);
        var hasKeyboard = family == AtariMachineFamily.EightBit;
        var region = hardware.Regions.Count > AtariCompatibilityConstants.SingleChoiceCount
            ? Editable(AtariSettingOption.Region)
            : Forced(AtariSettingOption.Region, hardware.DefaultRegion.ToString());
        var firmware = hardware.Firmware.Count == AtariCompatibilityConstants.EmptyCollectionCount
            ? Unavailable(AtariSettingOption.Firmware, AtariCompatibilityConstants.NoFirmwareResource)
            : Editable(AtariSettingOption.Firmware);
        var originalComputer = AtariEightBitSettingsCatalog.SupportsOriginalComputerOptions(model);
        var options = Values(
            Forced(AtariSettingOption.CpuModel, JoinValues(hardware.Cpus)),
            Hidden(AtariSettingOption.CpuPrecision),
            Forced(AtariSettingOption.CpuSpeed,
                hardware.DefaultCpuFrequencyHz.ToString(CultureInfo.InvariantCulture)),
            Hidden(AtariSettingOption.Fpu),
            model == AtariMachineModel.XlXe
                ? Editable(AtariSettingOption.MainMemory)
                : Forced(AtariSettingOption.MainMemory,
                    hardware.MainMemoryBytes.ToString(CultureInfo.InvariantCulture)),
            Hidden(AtariSettingOption.AlternateMemory),
            originalComputer ? Editable(AtariSettingOption.MosaicMemory) : Hidden(AtariSettingOption.MosaicMemory),
            originalComputer ? Editable(AtariSettingOption.AxlonMemory) : Hidden(AtariSettingOption.AxlonMemory),
            originalComputer ? Editable(AtariSettingOption.AxlonShadow) : Hidden(AtariSettingOption.AxlonShadow),
            AtariEightBitSettingsCatalog.SupportsMapRam(model)
                ? Editable(AtariSettingOption.MapRam) : Hidden(AtariSettingOption.MapRam),
            firmware,
            originalComputer ? Hidden(AtariSettingOption.Region) : region,
            region with { Option = AtariSettingOption.VideoStandard },
            Editable(AtariSettingOption.Renderer),
            Editable(AtariSettingOption.AudioEnabled),
            hardware.Media.Count == AtariCompatibilityConstants.EmptyCollectionCount
                ? Unavailable(AtariSettingOption.Storage, AtariCompatibilityConstants.NoStorageResource)
                : Editable(AtariSettingOption.Storage),
            hasKeyboard
                ? Editable(AtariSettingOption.KeyboardMappings)
                : Unavailable(AtariSettingOption.KeyboardMappings, AtariCompatibilityConstants.NoKeyboardResource),
            Hidden(AtariSettingOption.MouseSpeed),
            Hidden(AtariSettingOption.MouseMappings),
            Editable(AtariSettingOption.ControllerMappings));
        var media = hardware.Media.Select(CreateMediaRule).ToList();
        if (model == AtariMachineModel.Jaguar)
            media.Add(UnavailableMedia(AtariMediaCategory.CompactDisc,
                AtariCompatibilityConstants.JaguarStandardNoCdResource, EmulationMediaSlot.Cd0));
        var portCount = hardware.Ports.Max(port => port.Count);
        var visibleTabs = EnumValues<AtariSettingsTab>()
            .Where(tab => hasKeyboard || tab != AtariSettingsTab.Keyboard)
            .Where(tab => tab != AtariSettingsTab.Mouse)
            .ToArray();
        return NewDefinition(model, hardware.Core, options, hardware.Firmware, media, portCount, visibleTabs);
    }

    private static AtariMediaCompatibilityRule CreateMediaRule(AtariMediaCategory category) => category switch
    {
        AtariMediaCategory.Floppy => Media(category, EmulationMediaSlot.Floppy0, EmulationMediaSlot.Floppy1,
            EmulationMediaSlot.Floppy2, EmulationMediaSlot.Floppy3),
        AtariMediaCategory.Cassette => Media(category, EmulationMediaSlot.Cassette0),
        AtariMediaCategory.Cartridge => Media(category, EmulationMediaSlot.Cartridge0),
        AtariMediaCategory.CompactDisc => Media(category, EmulationMediaSlot.Cd0),
        _ => throw new ArgumentOutOfRangeException(nameof(category), category, AtariErrorMessages.IncompatibleMedia)
    };

    private static AtariCompatibilityDefinition NewDefinition(AtariMachineModel model, AtariEmulator core,
        IReadOnlyList<AtariOptionRule> options, IReadOnlyList<AtariFirmwareCategory> firmware,
        IReadOnlyList<AtariMediaCompatibilityRule> media, int controllerPortCount,
        IReadOnlyList<AtariSettingsTab>? visibleTabs = null)
    {
        var definition = new AtariCompatibilityDefinition(model, core, visibleTabs ?? EnumValues<AtariSettingsTab>(),
            EnumValues<AtariSettingsGroup>(), options, firmware, media, controllerPortCount);
        Validate(definition);
        return definition;
    }

    private static IReadOnlyList<T> EnumValues<T>() where T : struct, Enum =>
        Array.AsReadOnly(Enum.GetValues<T>());
}
