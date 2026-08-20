using System.Globalization;
using GWGUI.App.Localization;
using GWGUI.Emulation.Atari;

namespace GWGUI.App.Controls;

internal static class AtariHardwareSettingsFunctions
{
    internal static string FirmwareKindName(AtariFirmwareKind kind) => kind switch
    {
        AtariFirmwareKind.Tos => AtariHardwareSettingsConstants.TosFirmwareContext,
        AtariFirmwareKind.AtariSystemOs => LocExtension.Get(AtariHardwareSettingsConstants.SystemRomResource),
        AtariFirmwareKind.AtariOsA => AtariHardwareSettingsConstants.AtariOsAFirmwareContext,
        AtariFirmwareKind.AtariOsB => AtariHardwareSettingsConstants.AtariOsBFirmwareContext,
        AtariFirmwareKind.AtariXlOs => AtariHardwareSettingsConstants.AtariXlOsFirmwareContext,
        AtariFirmwareKind.AtariBasic => AtariHardwareSettingsConstants.AtariBasicFirmwareContext,
        AtariFirmwareKind.Atari5200Bios => AtariHardwareSettingsConstants.Atari5200BiosFirmwareContext,
        AtariFirmwareKind.AtariXegsBios => AtariHardwareSettingsConstants.AtariXegsBiosFirmwareContext,
        AtariFirmwareKind.Atari7800Bios => AtariHardwareSettingsConstants.Atari7800BiosFirmwareContext,
        AtariFirmwareKind.LynxBootRom => AtariHardwareSettingsConstants.LynxBootRomFirmwareContext,
        AtariFirmwareKind.JaguarBootRom => AtariHardwareSettingsConstants.JaguarBootRomFirmwareContext,
        AtariFirmwareKind.JaguarCdBios => AtariHardwareSettingsConstants.JaguarCdBiosFirmwareContext,
        _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, null)
    };

    internal static AtariHardwareView Create(AtariMachineModel model,
        IReadOnlyDictionary<string, string> options)
    {
        var compatibility = AtariCompatibilityCatalog.Get(model);
        return compatibility.Core == AtariCoreKind.Hatari
            ? CreateSt(model, compatibility, options)
            : CreateClassic(model, compatibility, options);
    }

    internal static AtariMachineConfiguration ReplaceOptions(AtariMachineConfiguration source,
        IEnumerable<KeyValuePair<string, string>> displayed) =>
        AtariGeneralSettingsFunctions.ReplaceGeneral(source, source.Model, source.Folders, source.Firmwares,
            AtariGeneralSettingsFunctions.MergeOptions(source.Options, displayed));

    internal static AtariClassicRegion ClassicRegion(AtariMachineConfiguration configuration)
    {
        var hardware = AtariClassicModelCatalog.Get(configuration.Model);
        return configuration.Options.TryGetValue(AtariVideoAudioSettingsConstants.StandardOptionKey,
                   out var configured)
               && Enum.TryParse<AtariClassicRegion>(configured, true, out var region)
               && hardware.Regions.Contains(region)
            ? region
            : hardware.DefaultRegion;
    }

    internal static long TotalMemoryBytes(IReadOnlyDictionary<string, string> options,
        AtariHardwareView view)
    {
        return view.Memory.Where(field => field.Availability != AtariOptionAvailability.Hidden)
            .Where(field => field.Option is AtariSettingOption.MainMemory or AtariSettingOption.AlternateMemory
                or AtariSettingOption.MosaicMemory or AtariSettingOption.AxlonMemory)
            .Sum(field =>
            {
                var key = OptionKey(field.Option);
                var selected = options.TryGetValue(key, out var current) ? current : field.SelectedValue;
                return field.Choices.FirstOrDefault(choice => choice.Value == selected)?.Bytes ?? 0;
            });
    }

    internal static (string Value, string Unit) FormatMemoryTotal(long bytes)
    {
        if (bytes % AtariHardwareSettingsConstants.BytesPerMebibyte == AtariHardwareSettingsConstants.NoBytes)
            return ((bytes / AtariHardwareSettingsConstants.BytesPerMebibyte)
                .ToString(CultureInfo.CurrentCulture), AtariHardwareSettingsConstants.MebibyteSuffix.Trim());
        if (bytes % AtariHardwareSettingsConstants.BytesPerKibibyte == AtariHardwareSettingsConstants.NoBytes)
            return ((bytes / AtariHardwareSettingsConstants.BytesPerKibibyte)
                .ToString(CultureInfo.CurrentCulture), AtariHardwareSettingsConstants.KibibyteSuffix.Trim());
        return (bytes.ToString(CultureInfo.CurrentCulture), AtariHardwareSettingsConstants.ByteSuffix.Trim());
    }

    internal static string OptionKey(AtariSettingOption option) => option switch
    {
        AtariSettingOption.CpuModel => AtariHardwareSettingsConstants.CpuOptionKey,
        AtariSettingOption.CpuSpeed => AtariHardwareSettingsConstants.FrequencyOptionKey,
        AtariSettingOption.CpuPrecision => AtariHardwareSettingsConstants.PrecisionOptionKey,
        AtariSettingOption.Fpu => AtariHardwareSettingsConstants.FpuOptionKey,
        AtariSettingOption.MainMemory => AtariHardwareSettingsConstants.MainMemoryOptionKey,
        AtariSettingOption.AlternateMemory => AtariHardwareSettingsConstants.AlternateMemoryOptionKey,
        AtariSettingOption.MosaicMemory => AtariEightBitSettingsConstants.MosaicMemoryOptionKey,
        AtariSettingOption.AxlonMemory => AtariEightBitSettingsConstants.AxlonMemoryOptionKey,
        AtariSettingOption.AxlonShadow => AtariEightBitSettingsConstants.AxlonShadowOptionKey,
        AtariSettingOption.MapRam => AtariEightBitSettingsConstants.MapRamOptionKey,
        _ => throw new ArgumentOutOfRangeException(nameof(option), option, null)
    };

    internal static string Explanation(AtariHardwareField field) => field.ExplanationResourceKey is not null
        ? LocExtension.Get(field.ExplanationResourceKey)
        : field.Availability == AtariOptionAvailability.Forced
            ? LocExtension.Get(AtariHardwareSettingsConstants.ForcedResource)
            : string.Empty;

    private static AtariHardwareView CreateSt(AtariMachineModel model,
        AtariCompatibilityDefinition compatibility, IReadOnlyDictionary<string, string> options)
    {
        var hardware = AtariStModelCatalog.Get(model);
        var cpu = new[]
        {
            Field(compatibility, AtariSettingOption.CpuModel, AtariHardwareSettingsConstants.CpuResource,
                Choices(hardware.Cpus), hardware.DefaultCpu.ToString(), options,
                AtariHardwareSettingsConstants.CpuOptionKey),
            Field(compatibility, AtariSettingOption.CpuSpeed, AtariHardwareSettingsConstants.FrequencyResource,
                hardware.CpuFrequenciesMhz.Select(value => Choice(value,
                    value + AtariHardwareSettingsConstants.FrequencyMhzSuffix)).ToArray(),
                hardware.DefaultCpuFrequencyMhz.ToString(CultureInfo.InvariantCulture), options,
                AtariHardwareSettingsConstants.FrequencyOptionKey),
            Field(compatibility, AtariSettingOption.CpuPrecision, AtariHardwareSettingsConstants.PrecisionResource,
                PrecisionChoices(hardware.CpuPrecisions), hardware.DefaultCpuPrecision.ToString(), options,
                AtariHardwareSettingsConstants.PrecisionOptionKey),
            Field(compatibility, AtariSettingOption.Fpu, AtariHardwareSettingsConstants.FpuResource,
                FpuChoices(hardware.Fpus), hardware.DefaultFpu.ToString(), options,
                AtariHardwareSettingsConstants.FpuOptionKey)
        };
        var memory = new[]
        {
            Field(compatibility, AtariSettingOption.MainMemory, AtariHardwareSettingsConstants.MainMemoryResource,
                hardware.MainMemoryKib.Select(value => Choice(value * AtariHardwareSettingsConstants.BytesPerKibibyte,
                    value + AtariHardwareSettingsConstants.KibibyteSuffix)).ToArray(),
                (hardware.DefaultMainMemoryKib * AtariHardwareSettingsConstants.BytesPerKibibyte).ToString(CultureInfo.InvariantCulture),
                options, AtariHardwareSettingsConstants.MainMemoryOptionKey),
            Field(compatibility, AtariSettingOption.AlternateMemory, AtariHardwareSettingsConstants.AlternateMemoryResource,
                hardware.AlternateMemoryMib.Select(value => Choice((long)value * AtariHardwareSettingsConstants.BytesPerMebibyte,
                    value + AtariHardwareSettingsConstants.MebibyteSuffix)).ToArray(),
                ((long)hardware.DefaultAlternateMemoryMib * AtariHardwareSettingsConstants.BytesPerMebibyte).ToString(CultureInfo.InvariantCulture),
                options, AtariHardwareSettingsConstants.AlternateMemoryOptionKey)
        };
        return new AtariHardwareView(cpu, memory, AtariFirmwareCatalog.ForModel(model),
            hardware.Regions.Select(value => new AtariHardwareChoice(value.ToString(), RegionName(value))).ToArray());
    }

    private static AtariHardwareView CreateClassic(AtariMachineModel model,
        AtariCompatibilityDefinition compatibility, IReadOnlyDictionary<string, string> options)
    {
        var hardware = AtariClassicModelCatalog.Get(model);
        var standard = options.TryGetValue(AtariVideoAudioSettingsConstants.StandardOptionKey, out var configuredStandard)
            && Enum.TryParse<AtariClassicRegion>(configuredStandard, true, out var configuredRegion)
            && hardware.Regions.Contains(configuredRegion)
            ? configuredRegion : hardware.DefaultRegion;
        var frequency = AtariEightBitSettingsCatalog.SupportsOriginalComputerOptions(model)
            ? AtariEightBitSettingsCatalog.CpuFrequency(standard) : hardware.DefaultCpuFrequencyHz;
        var cpuValue = string.Join(AtariHardwareSettingsConstants.ValueSeparator, hardware.Cpus);
        var cpu = new[]
        {
            Field(compatibility, AtariSettingOption.CpuModel, AtariHardwareSettingsConstants.CpuResource,
                [new AtariHardwareChoice(cpuValue, cpuValue)], cpuValue, options,
                AtariHardwareSettingsConstants.CpuOptionKey),
            Field(compatibility, AtariSettingOption.CpuSpeed, AtariHardwareSettingsConstants.FrequencyResource,
                [Choice(frequency, FormatFrequency(frequency, standard))],
                frequency.ToString(CultureInfo.InvariantCulture), options,
                AtariHardwareSettingsConstants.FrequencyOptionKey),
            Field(compatibility, AtariSettingOption.CpuPrecision, AtariHardwareSettingsConstants.PrecisionResource,
                [new AtariHardwareChoice(AtariHardwareSettingsConstants.CoreManagedValue,
                    LocExtension.Get(AtariHardwareSettingsConstants.CoreResource))],
                AtariHardwareSettingsConstants.CoreManagedValue, options,
                AtariHardwareSettingsConstants.PrecisionOptionKey),
            Field(compatibility, AtariSettingOption.Fpu, AtariHardwareSettingsConstants.FpuResource,
                [new AtariHardwareChoice(AtariStFpu.None.ToString(),
                    LocExtension.Get(AtariHardwareSettingsConstants.NoneResource))],
                AtariStFpu.None.ToString(), options, AtariHardwareSettingsConstants.FpuOptionKey)
        };
        var memory = new List<AtariHardwareField>
        {
            Field(compatibility, AtariSettingOption.MainMemory, AtariHardwareSettingsConstants.MainMemoryResource,
                model == AtariMachineModel.XlXe
                    ? XlXeMemoryChoices()
                    : [Choice(hardware.MainMemoryBytes, FormatBytes(hardware.MainMemoryBytes))],
                hardware.MainMemoryBytes.ToString(CultureInfo.InvariantCulture), options,
                AtariHardwareSettingsConstants.MainMemoryOptionKey),
            Field(compatibility, AtariSettingOption.AlternateMemory, AtariHardwareSettingsConstants.AlternateMemoryResource,
                [Choice(AtariHardwareSettingsConstants.NoBytes, AtariHardwareSettingsConstants.NoBytes + AtariHardwareSettingsConstants.ByteSuffix)],
                AtariHardwareSettingsConstants.NoBytes.ToString(CultureInfo.InvariantCulture), options,
                AtariHardwareSettingsConstants.AlternateMemoryOptionKey)
        };
        if (AtariEightBitSettingsCatalog.SupportsOriginalComputerOptions(model))
        {
            memory.Add(Field(compatibility, AtariSettingOption.MosaicMemory,
                AtariHardwareSettingsConstants.MosaicMemoryResource,
                ExpansionChoices(AtariEightBitSettingsCatalog.Mosaic(model)),
                AtariEightBitSettingsConstants.Disabled, options,
                AtariEightBitSettingsConstants.MosaicMemoryOptionKey));
            memory.Add(Field(compatibility, AtariSettingOption.AxlonMemory,
                AtariHardwareSettingsConstants.AxlonMemoryResource,
                ExpansionChoices(AtariEightBitSettingsCatalog.Axlon(model)),
                AtariEightBitSettingsConstants.Disabled, options,
                AtariEightBitSettingsConstants.AxlonMemoryOptionKey));
            memory.Add(Field(compatibility, AtariSettingOption.AxlonShadow,
                AtariHardwareSettingsConstants.AxlonShadowResource, ToggleChoices(),
                AtariEightBitSettingsConstants.Disabled, options,
                AtariEightBitSettingsConstants.AxlonShadowOptionKey));
        }
        if (AtariEightBitSettingsCatalog.SupportsMapRam(model))
            memory.Add(Field(compatibility, AtariSettingOption.MapRam,
                AtariHardwareSettingsConstants.MapRamResource, ToggleChoices(),
                AtariEightBitSettingsConstants.Disabled, options,
                AtariEightBitSettingsConstants.MapRamOptionKey));
        return new AtariHardwareView(cpu, memory, AtariFirmwareCatalog.ForModel(model),
            hardware.Regions.Select(value => new AtariHardwareChoice(value.ToString(),
                AtariRegionDisplayFunctions.DisplayName(value))).ToArray());
    }

    private static AtariHardwareField Field(AtariCompatibilityDefinition compatibility,
        AtariSettingOption option, string resource, IReadOnlyList<AtariHardwareChoice> choices,
        string defaultValue, IReadOnlyDictionary<string, string> options, string key)
    {
        var rule = compatibility.Options.Single(value => value.Option == option);
        var selected = options.TryGetValue(key, out var configured)
            && choices.Any(choice => choice.Value == configured) ? configured : defaultValue;
        return new AtariHardwareField(option, resource, choices, selected,
            rule.Availability, rule.ExplanationResourceKey);
    }

    private static IReadOnlyList<AtariHardwareChoice> Choices<T>(IEnumerable<T> values) => values
        .Select(value => new AtariHardwareChoice(value!.ToString()!, value.ToString()!)).ToArray();
    private static IReadOnlyList<AtariHardwareChoice> PrecisionChoices(IEnumerable<AtariStCpuPrecision> values) =>
        values.Select(value => new AtariHardwareChoice(value.ToString(), LocExtension.Get(value switch
        {
            AtariStCpuPrecision.Compatible => AtariHardwareSettingsConstants.CompatibleResource,
            AtariStCpuPrecision.CycleExact => AtariHardwareSettingsConstants.CycleExactResource,
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, null)
        }))).ToArray();
    private static IReadOnlyList<AtariHardwareChoice> FpuChoices(IEnumerable<AtariStFpu> values) => values
        .Select(value => new AtariHardwareChoice(value.ToString(), value == AtariStFpu.None
            ? LocExtension.Get(AtariHardwareSettingsConstants.NoneResource) : value.ToString())).ToArray();
    private static string RegionName(AtariStRegion region) => region == AtariStRegion.Multilingual
        ? LocExtension.Get(AtariHardwareSettingsConstants.MultilingualResource)
        : CultureInfo.GetCultureInfo(region switch
        {
            AtariStRegion.UnitedStates => AtariHardwareSettingsConstants.UnitedStatesCulture,
            AtariStRegion.Germany => AtariHardwareSettingsConstants.GermanyCulture,
            AtariStRegion.France => AtariHardwareSettingsConstants.FranceCulture,
            AtariStRegion.UnitedKingdom => AtariHardwareSettingsConstants.UnitedKingdomCulture,
            AtariStRegion.Spain => AtariHardwareSettingsConstants.SpainCulture,
            AtariStRegion.Italy => AtariHardwareSettingsConstants.ItalyCulture,
            AtariStRegion.Sweden => AtariHardwareSettingsConstants.SwedenCulture,
            AtariStRegion.Switzerland => AtariHardwareSettingsConstants.SwitzerlandCulture,
            AtariStRegion.Finland => AtariHardwareSettingsConstants.FinlandCulture,
            AtariStRegion.Norway => AtariHardwareSettingsConstants.NorwayCulture,
            AtariStRegion.CzechRepublic => AtariHardwareSettingsConstants.CzechRepublicCulture,
            AtariStRegion.Russia => AtariHardwareSettingsConstants.RussiaCulture,
            AtariStRegion.Greece => AtariHardwareSettingsConstants.GreeceCulture,
            _ => throw new ArgumentOutOfRangeException(nameof(region), region, null)
        }).DisplayName;
    private static AtariHardwareChoice Choice(long value, string display) =>
        new(value.ToString(CultureInfo.InvariantCulture), display, value);
    private static IReadOnlyList<AtariHardwareChoice> ExpansionChoices(
        IReadOnlyList<AtariMemoryExpansionChoice> choices) => choices.Select(choice =>
            new AtariHardwareChoice(choice.Value,
                choice.AdditionalBytes == 0 ? LocExtension.Get(AtariHardwareSettingsConstants.NoneResource)
                    : FormatBytes(choice.AdditionalBytes), choice.AdditionalBytes)).ToArray();
    private static IReadOnlyList<AtariHardwareChoice> XlXeMemoryChoices() =>
    [
        Choice(320 * AtariHardwareSettingsConstants.BytesPerKibibyte, "320 KiB (Compy Shop)"),
        Choice(576 * AtariHardwareSettingsConstants.BytesPerKibibyte, "576 KiB"),
        Choice(1088 * AtariHardwareSettingsConstants.BytesPerKibibyte, "1088 KiB")
    ];
    private static IReadOnlyList<AtariHardwareChoice> ToggleChoices() =>
    [
        new(AtariEightBitSettingsConstants.Disabled,
            LocExtension.Get(AtariVideoAudioSettingsConstants.DisabledResource)),
        new(AtariEightBitSettingsConstants.Enabled,
            LocExtension.Get(AtariVideoAudioSettingsConstants.EnabledResource))
    ];
    private static string FormatFrequency(long hertz, AtariClassicRegion region) =>
        $"{hertz / 1_000_000d:0.00} MHz ({AtariRegionDisplayFunctions.DisplayName(region)})";
    private static long SelectedNumber(IReadOnlyDictionary<string, string> values, string key, string fallback) =>
        long.Parse(values.TryGetValue(key, out var value) ? value : fallback, CultureInfo.InvariantCulture);
    private static string FormatBytes(long value) => value % AtariHardwareSettingsConstants.BytesPerKibibyte == AtariHardwareSettingsConstants.NoBytes
        ? value / AtariHardwareSettingsConstants.BytesPerKibibyte + AtariHardwareSettingsConstants.KibibyteSuffix
        : value + AtariHardwareSettingsConstants.ByteSuffix;
}
