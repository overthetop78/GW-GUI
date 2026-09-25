using System.Globalization;

namespace GWGUI.Emulation.Atari.Common.Functions;

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

    internal static OptionRule Editable(SettingOption option) =>
        new(option, OptionAvailability.Editable);

    internal static OptionRule Forced(SettingOption option, string value) =>
        new(option, OptionAvailability.Forced, value,
            CompatibilityConstants.ForcedByModelResource);

    internal static OptionRule Unavailable(SettingOption option, string explanationResourceKey) =>
        new(option, OptionAvailability.Unavailable, ExplanationResourceKey: explanationResourceKey);

    internal static OptionRule Hidden(SettingOption option) =>
        new(option, OptionAvailability.Hidden);

    internal static MediaCompatibilityRule Media(MediaCategory category, params EmulationMediaSlot[] slots) =>
        new(category, Array.AsReadOnly(slots));

    internal static MediaCompatibilityRule UnavailableMedia(
        MediaCategory category,
        string explanationResourceKey,
        params EmulationMediaSlot[] slots) =>
        new(category, Array.AsReadOnly(slots), MediaAvailability.Unavailable, explanationResourceKey);

    internal static string JoinValues<T>(IEnumerable<T> values) =>
        string.Join(CompatibilityConstants.ForcedValueSeparator, values);

    internal static bool IsFirmwareCompatible(CompatibilityDefinition definition, FirmwareCategory category) =>
        definition.Firmware.Contains(category);

    internal static bool IsMediaCompatible(CompatibilityDefinition definition, MediaCategory category,
        EmulationMediaSlot slot) => definition.Media.Any(rule => rule.Category == category
            && rule.Availability == MediaAvailability.Available && rule.Slots.Contains(slot));

    internal static void Validate(CompatibilityDefinition definition)
    {
        if (definition.Options.Select(rule => rule.Option).Distinct().Count() !=
            Enum.GetValues<SettingOption>().Length)
            throw new InvalidOperationException(ErrorMessages.IncompleteCompatibilityOptions);
        if (definition.Options.Any(rule => rule.Availability is OptionAvailability.Forced
                                               or OptionAvailability.Unavailable
                                           && string.IsNullOrWhiteSpace(rule.ExplanationResourceKey)))
            throw new InvalidOperationException(ErrorMessages.MissingUnavailableExplanation);
        if (definition.Options.Any(rule => rule.Availability == OptionAvailability.Forced
                                           && string.IsNullOrWhiteSpace(rule.ForcedValue)))
            throw new InvalidOperationException(ErrorMessages.MissingForcedOptionValue);
        if (definition.Media.Any(rule => rule.Availability == MediaAvailability.Unavailable
                                         && string.IsNullOrWhiteSpace(rule.ExplanationResourceKey)))
            throw new InvalidOperationException(ErrorMessages.MissingUnavailableExplanation);
        if (definition.ControllerPortCount < CompatibilityConstants.NoControllerPort)
            throw new InvalidOperationException(ErrorMessages.InvalidCompatibilityControllerCount);
    }

    internal static CompatibilityDefinition Create(MachineModel model)
    {
        var family = ConfigurationFunctions.GetFamily(model);
        return family == MachineFamily.St ? CreateSt(model) : CreateClassic(model);
    }

    private static CompatibilityDefinition CreateSt(MachineModel model)
    {
        var hardware = StModelCatalog.Get(model);
        var options = Values(
            Forced(SettingOption.CpuModel, hardware.DefaultCpu.ToString()),
            Editable(SettingOption.CpuPrecision),
            hardware.CpuFrequenciesMhz.Count > CompatibilityConstants.SingleChoiceCount
                ? Editable(SettingOption.CpuSpeed)
                : Forced(SettingOption.CpuSpeed,
                    hardware.DefaultCpuFrequencyMhz.ToString(CultureInfo.InvariantCulture)),
            hardware.Fpus.Count > CompatibilityConstants.SingleChoiceCount
                ? Editable(SettingOption.Fpu)
                : Unavailable(SettingOption.Fpu, CompatibilityConstants.NoFpuResource),
            Editable(SettingOption.MainMemory),
            hardware.AlternateMemoryMib.Any(value => value > StModelConstants.NoAlternateMemoryMib)
                ? Editable(SettingOption.AlternateMemory)
                : Unavailable(SettingOption.AlternateMemory,
                    CompatibilityConstants.NoAlternateMemoryResource),
            Hidden(SettingOption.MosaicMemory),
            Hidden(SettingOption.AxlonMemory),
            Hidden(SettingOption.AxlonShadow),
            Hidden(SettingOption.MapRam),
            Editable(SettingOption.Firmware),
            Editable(SettingOption.Region),
            Editable(SettingOption.VideoStandard),
            Editable(SettingOption.Renderer),
            Editable(SettingOption.AudioEnabled),
            Editable(SettingOption.Storage),
            Editable(SettingOption.KeyboardMappings),
            Editable(SettingOption.MouseSpeed),
            Editable(SettingOption.MouseMappings),
            Editable(SettingOption.ControllerMappings));
        return NewDefinition(model, Emulator.Hatari, options, Values(FirmwareCategory.Tos),
            Values(
                Media(MediaCategory.Floppy, EmulationMediaSlot.Floppy0, EmulationMediaSlot.Floppy1,
                    EmulationMediaSlot.Floppy2, EmulationMediaSlot.Floppy3),
                Media(MediaCategory.HardDisk, EmulationMediaSlot.HardDisk0),
                Media(MediaCategory.Directory, EmulationMediaSlot.HardDisk0)),
            CompatibilityConstants.TwoControllerPorts);
    }

    private static CompatibilityDefinition CreateClassic(MachineModel model)
    {
        var hardware = ClassicModelCatalog.Get(model);
        var family = ConfigurationFunctions.GetFamily(model);
        var hasKeyboard = family == MachineFamily.EightBit;
        var region = hardware.Regions.Count > CompatibilityConstants.SingleChoiceCount
            ? Editable(SettingOption.Region)
            : Forced(SettingOption.Region, hardware.DefaultRegion.ToString());
        var firmware = hardware.Firmware.Count == CompatibilityConstants.EmptyCollectionCount
            ? Unavailable(SettingOption.Firmware, CompatibilityConstants.NoFirmwareResource)
            : Editable(SettingOption.Firmware);
        var originalComputer = EightBitSettingsCatalog.SupportsOriginalComputerOptions(model);
        var options = Values(
            Forced(SettingOption.CpuModel, JoinValues(hardware.Cpus)),
            Hidden(SettingOption.CpuPrecision),
            Forced(SettingOption.CpuSpeed,
                hardware.DefaultCpuFrequencyHz.ToString(CultureInfo.InvariantCulture)),
            Hidden(SettingOption.Fpu),
            model == MachineModel.XlXe
                ? Editable(SettingOption.MainMemory)
                : Forced(SettingOption.MainMemory,
                    hardware.MainMemoryBytes.ToString(CultureInfo.InvariantCulture)),
            Hidden(SettingOption.AlternateMemory),
            originalComputer ? Editable(SettingOption.MosaicMemory) : Hidden(SettingOption.MosaicMemory),
            originalComputer ? Editable(SettingOption.AxlonMemory) : Hidden(SettingOption.AxlonMemory),
            originalComputer ? Editable(SettingOption.AxlonShadow) : Hidden(SettingOption.AxlonShadow),
            EightBitSettingsCatalog.SupportsMapRam(model)
                ? Editable(SettingOption.MapRam) : Hidden(SettingOption.MapRam),
            firmware,
            originalComputer ? Hidden(SettingOption.Region) : region,
            region with { Option = SettingOption.VideoStandard },
            Editable(SettingOption.Renderer),
            Editable(SettingOption.AudioEnabled),
            hardware.Media.Count == CompatibilityConstants.EmptyCollectionCount
                ? Unavailable(SettingOption.Storage, CompatibilityConstants.NoStorageResource)
                : Editable(SettingOption.Storage),
            hasKeyboard
                ? Editable(SettingOption.KeyboardMappings)
                : Unavailable(SettingOption.KeyboardMappings, CompatibilityConstants.NoKeyboardResource),
            Hidden(SettingOption.MouseSpeed),
            Hidden(SettingOption.MouseMappings),
            Editable(SettingOption.ControllerMappings));
        var media = hardware.Media.Select(CreateMediaRule).ToList();
        if (model == MachineModel.Jaguar)
            media.Add(UnavailableMedia(MediaCategory.CompactDisc,
                CompatibilityConstants.JaguarStandardNoCdResource, EmulationMediaSlot.Cd0));
        var portCount = hardware.Ports.Max(port => port.Count);
        var visibleTabs = EnumValues<SettingsTab>()
            .Where(tab => hasKeyboard || tab != SettingsTab.Keyboard)
            .Where(tab => hardware.Firmware.Count > 0 || tab != SettingsTab.Firmware)
            .Where(tab => tab != SettingsTab.Mouse)
            .ToArray();
        return NewDefinition(model, hardware.Core, options, hardware.Firmware, media, portCount, visibleTabs);
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
        IReadOnlyList<OptionRule> options, IReadOnlyList<FirmwareCategory> firmware,
        IReadOnlyList<MediaCompatibilityRule> media, int controllerPortCount,
        IReadOnlyList<SettingsTab>? visibleTabs = null)
    {
        var definition = new CompatibilityDefinition(model, core, visibleTabs ?? EnumValues<SettingsTab>(),
            EnumValues<SettingsGroup>(), options, firmware, media, controllerPortCount);
        Validate(definition);
        return definition;
    }

    private static IReadOnlyList<T> EnumValues<T>() where T : struct, Enum =>
        Array.AsReadOnly(Enum.GetValues<T>());
}
