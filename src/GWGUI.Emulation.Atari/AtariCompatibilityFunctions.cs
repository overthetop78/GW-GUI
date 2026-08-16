using GWGUI.Emulation;
using System.Globalization;

namespace GWGUI.Emulation.Atari;

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

    internal static AtariMediaCompatibilityRule Media(AtariMediaKind kind, params EmulationMediaSlot[] slots) =>
        new(kind, Array.AsReadOnly(slots));

    internal static string JoinValues<T>(IEnumerable<T> values) =>
        string.Join(AtariCompatibilityConstants.ForcedValueSeparator, values);

    internal static bool IsFirmwareCompatible(AtariCompatibilityDefinition definition, AtariFirmwareKind kind) =>
        definition.Firmware.Contains(kind);

    internal static bool IsMediaCompatible(AtariCompatibilityDefinition definition, AtariMediaKind kind,
        EmulationMediaSlot slot) => definition.Media.Any(rule => rule.Kind == kind && rule.Slots.Contains(slot));

    internal static void Validate(AtariCompatibilityDefinition definition)
    {
        if (definition.Options.Select(rule => rule.Option).Distinct().Count() !=
            Enum.GetValues<AtariSettingOption>().Length)
            throw new InvalidOperationException(AtariErrorMessages.IncompleteCompatibilityOptions);
        if (definition.Options.Any(rule => rule.Availability != AtariOptionAvailability.Editable
                                           && string.IsNullOrWhiteSpace(rule.ExplanationResourceKey)))
            throw new InvalidOperationException(AtariErrorMessages.MissingUnavailableExplanation);
        if (definition.Options.Any(rule => rule.Availability == AtariOptionAvailability.Forced
                                           && string.IsNullOrWhiteSpace(rule.ForcedValue)))
            throw new InvalidOperationException(AtariErrorMessages.MissingForcedOptionValue);
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
            Editable(AtariSettingOption.Firmware),
            Editable(AtariSettingOption.Region),
            Editable(AtariSettingOption.VideoStandard),
            Editable(AtariSettingOption.Renderer),
            Editable(AtariSettingOption.AudioEnabled),
            Editable(AtariSettingOption.Storage),
            Editable(AtariSettingOption.KeyboardMappings),
            Editable(AtariSettingOption.MouseMappings),
            Editable(AtariSettingOption.ControllerMappings));
        return NewDefinition(model, AtariCoreKind.Hatari, options, Values(AtariFirmwareKind.Tos),
            Values(
                Media(AtariMediaKind.Floppy, EmulationMediaSlot.Floppy0, EmulationMediaSlot.Floppy1,
                    EmulationMediaSlot.Floppy2, EmulationMediaSlot.Floppy3),
                Media(AtariMediaKind.HardDisk, EmulationMediaSlot.HardDisk0),
                Media(AtariMediaKind.Directory, EmulationMediaSlot.HardDisk0)),
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
        var options = Values(
            Forced(AtariSettingOption.CpuModel, JoinValues(hardware.Cpus)),
            Forced(AtariSettingOption.CpuPrecision, AtariCompatibilityConstants.CoreManagedValue),
            Forced(AtariSettingOption.CpuSpeed,
                hardware.DefaultCpuFrequencyHz.ToString(CultureInfo.InvariantCulture)),
            Unavailable(AtariSettingOption.Fpu, AtariCompatibilityConstants.NoFpuResource),
            Forced(AtariSettingOption.MainMemory,
                hardware.MainMemoryBytes.ToString(CultureInfo.InvariantCulture)),
            Unavailable(AtariSettingOption.AlternateMemory,
                AtariCompatibilityConstants.NoAlternateMemoryResource),
            firmware,
            region,
            region with { Option = AtariSettingOption.VideoStandard },
            Editable(AtariSettingOption.Renderer),
            Editable(AtariSettingOption.AudioEnabled),
            hardware.Media.Count == AtariCompatibilityConstants.EmptyCollectionCount
                ? Unavailable(AtariSettingOption.Storage, AtariCompatibilityConstants.NoStorageResource)
                : Editable(AtariSettingOption.Storage),
            hasKeyboard
                ? Editable(AtariSettingOption.KeyboardMappings)
                : Unavailable(AtariSettingOption.KeyboardMappings, AtariCompatibilityConstants.NoKeyboardResource),
            Unavailable(AtariSettingOption.MouseMappings, AtariCompatibilityConstants.NoMouseResource),
            Editable(AtariSettingOption.ControllerMappings));
        var media = hardware.Media.Select(CreateMediaRule).ToArray();
        var portCount = hardware.Ports.Max(port => port.Count);
        return NewDefinition(model, hardware.Core, options, hardware.Firmware, media, portCount);
    }

    private static AtariMediaCompatibilityRule CreateMediaRule(AtariMediaKind kind) => kind switch
    {
        AtariMediaKind.Floppy => Media(kind, EmulationMediaSlot.Floppy0, EmulationMediaSlot.Floppy1,
            EmulationMediaSlot.Floppy2, EmulationMediaSlot.Floppy3),
        AtariMediaKind.Cassette => Media(kind, EmulationMediaSlot.Cassette0),
        AtariMediaKind.Cartridge => Media(kind, EmulationMediaSlot.Cartridge0),
        AtariMediaKind.CompactDisc => Media(kind, EmulationMediaSlot.Cd0),
        _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, AtariErrorMessages.IncompatibleMedia)
    };

    private static AtariCompatibilityDefinition NewDefinition(AtariMachineModel model, AtariCoreKind core,
        IReadOnlyList<AtariOptionRule> options, IReadOnlyList<AtariFirmwareKind> firmware,
        IReadOnlyList<AtariMediaCompatibilityRule> media, int controllerPortCount)
    {
        var definition = new AtariCompatibilityDefinition(model, core, EnumValues<AtariSettingsTab>(),
            EnumValues<AtariSettingsGroup>(), options, firmware, media, controllerPortCount);
        Validate(definition);
        return definition;
    }

    private static IReadOnlyList<T> EnumValues<T>() where T : struct, Enum =>
        Array.AsReadOnly(Enum.GetValues<T>());
}
