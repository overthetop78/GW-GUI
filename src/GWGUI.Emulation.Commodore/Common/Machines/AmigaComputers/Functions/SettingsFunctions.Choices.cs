using GWGUI.Emulation.Commodore.Common.Machines.AmigaComputers.Enums;
using GWGUI.Emulation;

namespace GWGUI.Emulation.Commodore.Common.Machines.AmigaComputers.Functions;

internal static partial class AmigaSettingsDescriptionFunctions
{
private static string Value(IReadOnlyDictionary<string, string> values, string key, string fallback) =>
        values.GetValueOrDefault(key) ?? fallback;

    private static string CpuValue(CpuModel cpu) =>
        ((int)cpu).ToString(System.Globalization.CultureInfo.InvariantCulture);

    private static FpuModel DefaultFpu(CpuModel cpu) =>
        cpu is CpuModel.Motorola68040 or CpuModel.Motorola68060 ? FpuModel.Integrated : FpuModel.None;

    private static IReadOnlyList<FpuModel> FpuValues(CpuModel cpu) => cpu switch
    {
        CpuModel.Motorola68000 or CpuModel.Motorola68010 => [FpuModel.None],
        CpuModel.Motorola68020 or CpuModel.Motorola68030 =>
            [FpuModel.None, FpuModel.Motorola68881, FpuModel.Motorola68882],
        _ => [FpuModel.Integrated, FpuModel.None, FpuModel.Motorola68881, FpuModel.Motorola68882]
    };

    private static string FpuValue(FpuModel fpu) => fpu switch
    {
        FpuModel.None => SettingsValueConstants.NoFpu,
        FpuModel.Integrated => SettingsValueConstants.IntegratedFpu,
        FpuModel.Motorola68881 => SettingsValueConstants.Motorola68881Option,
        FpuModel.Motorola68882 => SettingsValueConstants.Motorola68882Option,
        _ => throw new ArgumentOutOfRangeException(nameof(fpu), fpu, null)
    };

    private static IReadOnlyList<RamCapacity> ChipMemoryValues(AmigaModel model) => model.Id switch
    {
        ModelConstants.A1000 => [RamCapacity._512KB],
        ModelConstants.A500 => [RamCapacity._512KB, RamCapacity._1MB, RamCapacity._1536KB, RamCapacity._2MB],
        ModelConstants.A500PLUS or ModelConstants.A600 => [RamCapacity._1MB, RamCapacity._2MB],
        ModelConstants.A2000 => [RamCapacity._512KB, RamCapacity._1MB, RamCapacity._2MB],
        _ => [RamCapacity._2MB]
    };

    private static IReadOnlyList<RamCapacity> SlowMemoryValues(AmigaModel model) => model.Id switch
    {
        ModelConstants.A1000 or ModelConstants.A500 or ModelConstants.A500PLUS or ModelConstants.A2000 =>
            [RamCapacity.None, RamCapacity._512KB, RamCapacity._1MB, RamCapacity._1536KB, RamCapacity._1792KB],
        _ => [RamCapacity.None]
    };

    private static string ChipMemoryValue(RamCapacity capacity) =>
        ((int)capacity / (int)RamCapacity._512KB).ToString(System.Globalization.CultureInfo.InvariantCulture);

    private static string SlowMemoryValue(RamCapacity capacity) =>
        ((int)capacity / (int)RamCapacity._256KB).ToString(System.Globalization.CultureInfo.InvariantCulture);

    private static EmulationSettingsChoice CpuChoice(CpuModel cpu) =>
        new(CpuValue(cpu), string.Empty, cpu == CpuModel.Motorola68020
            ? SettingsValueConstants.Motorola68EC020DisplayName
            : string.Concat(SettingsValueConstants.MotorolaDisplayPrefix, CpuValue(cpu)));

    private static EmulationSettingsChoice FpuChoice(FpuModel fpu) => fpu switch
    {
        FpuModel.None => new(FpuValue(fpu), SettingsResourceKeys.MemoryNone),
        FpuModel.Integrated => new(FpuValue(fpu), string.Empty, SettingsValueConstants.CpuDisplayName),
        _ => new(FpuValue(fpu), string.Empty, FpuValue(fpu))
    };

    private static IReadOnlyList<EmulationSettingsChoice> CompatibilityChoices() =>
        Enum.GetValues<CpuCompatibility>().Select(value => SettingsChoiceConstants.CpuCompatibilityChoices[value]).ToArray();

    private static EmulationSettingsChoice ChipMemoryChoice(RamCapacity capacity) =>
        MemoryChoice(ChipMemoryValue(capacity), (long)capacity * MemoryConstants.BytesPerKib);

    private static EmulationSettingsChoice SlowMemoryChoice(RamCapacity capacity) =>
        MemoryChoice(SlowMemoryValue(capacity), (long)capacity * MemoryConstants.BytesPerKib);

    private static EmulationSettingsChoice MemoryMibChoice(RamCapacity capacity) =>
        MemoryChoice(((int)capacity / MemoryConstants.KibPerMib)
            .ToString(System.Globalization.CultureInfo.InvariantCulture),
            (long)capacity * MemoryConstants.BytesPerKib);

    private static EmulationSettingsChoice MemoryChoice(string id, long bytes) => bytes == (long)RamCapacity.None
        ? new EmulationSettingsChoice(id, SettingsResourceKeys.MemoryNone, NumericValue: (long)RamCapacity.None)
        : bytes < MemoryConstants.BytesPerMib
            ? new EmulationSettingsChoice(id, string.Empty, string.Format(MemoryConstants.KibLabelFormat, bytes / MemoryConstants.BytesPerKib), bytes)
            : new EmulationSettingsChoice(id, string.Empty, string.Format(MemoryConstants.MibLabelFormat, bytes / MemoryConstants.BytesPerMib), bytes);

    private static IReadOnlyList<EmulationSettingsChoice> CpuFrequencyChoices(AmigaModel model,
        CpuCompatibility compatibility, bool ntsc)
    {
        var nominal = NominalCpuFrequencyMhz(model, ntsc);
        if (compatibility is CpuCompatibility.MemoryExact or CpuCompatibility.Exact)
        {
            var halfA500Clock = ntsc ? ToMhz(ClockFrequency.NtscHalfBase) : ToMhz(ClockFrequency.PalHalfBase);
            var choices = ClockConstants.CycleExactMultipliers.Select(multiplier =>
            {
                var frequency = halfA500Clock * (int)multiplier;
                var ratio = frequency / nominal;
                return FrequencyChoice(ratio, SettingsValueConstants.NominalCpuThrottle, ((int)multiplier).ToString(), frequency);
            }).Where(choice => !Approximately(choice.NumericValue.GetValueOrDefault() / ClockConstants.HertzPerMegahertz, nominal))
                .ToList();
            choices.Add(FrequencyChoice(ClockConstants.NominalSpeedRatio, SettingsValueConstants.NominalCpuThrottle, SettingsValueConstants.AutomaticCpuMultiplier, nominal));
            return choices.OrderBy(choice => choice.NumericValue).ToArray();
        }

        return new[]
        {
            (Ratio: ClockConstants.HalfSpeedRatio, Throttle: SettingsValueConstants.HalfSpeedCpuThrottle), (Ratio: ClockConstants.NominalSpeedRatio, Throttle: SettingsValueConstants.NominalCpuThrottle),
            (Ratio: ClockConstants.DoubleSpeedRatio, Throttle: SettingsValueConstants.DoubleSpeedCpuThrottle), (Ratio: ClockConstants.QuadrupleSpeedRatio, Throttle: SettingsValueConstants.QuadrupleSpeedCpuThrottle),
            (Ratio: ClockConstants.EightfoldSpeedRatio, Throttle: SettingsValueConstants.EightfoldSpeedCpuThrottle)
        }.Select(item => FrequencyChoice(item.Ratio, item.Throttle, SettingsValueConstants.AutomaticCpuMultiplier, nominal * item.Ratio)).ToArray();
    }

    private static EmulationSettingsChoice FrequencyChoice(double ratio, string throttle, string multiplier,
        double frequency)
    {
        var prefix = Approximately(ratio, ClockConstants.NominalSpeedRatio) ? SettingsValueConstants.NominalCpuSpeedLabel : string.Format(SettingsValueConstants.PercentageLabelFormat, Math.Round(ratio * SettingsValueConstants.MaximumPercentage));
        return new EmulationSettingsChoice(string.Join(SettingsValueConstants.CpuSpeedSelectionSeparator, throttle, multiplier), string.Empty,
            string.Format(SettingsValueConstants.FrequencyLabelFormat, prefix, FormatMhz(frequency)), (long)Math.Round(frequency * ClockConstants.HertzPerMegahertz));
    }

    private static string CpuFrequencyValue(IReadOnlyDictionary<string, string> options, CpuCompatibility compatibility,
        IReadOnlyList<EmulationSettingsChoice> choices)
    {
        var throttle = Value(options, SettingsConstants.OptionCpuThrottle, SettingsValueConstants.NominalCpuThrottle);
        var multiplier = Value(options, SettingsConstants.OptionCpuMultiplier, SettingsValueConstants.AutomaticCpuMultiplier);
        var expected = compatibility is CpuCompatibility.MemoryExact or CpuCompatibility.Exact ? string.Join(SettingsValueConstants.CpuSpeedSelectionSeparator, SettingsValueConstants.NominalCpuThrottle, multiplier) : string.Join(SettingsValueConstants.CpuSpeedSelectionSeparator, throttle, SettingsValueConstants.AutomaticCpuMultiplier);
        return choices.Any(choice => choice.Id == expected)
            ? expected : choices.FirstOrDefault(choice => choice.Id == SettingsValueConstants.NominalCpuSpeedSelection)?.Id ?? choices[BufferConstants.FirstCollectionIndex].Id;
    }

    private static double NominalCpuFrequencyMhz(AmigaModel model, bool ntsc) => model.Id switch
    {
        GWGUI.Emulation.Commodore.Common.Machines.AmigaComputers.Constants.ModelConstants.A1200 or GWGUI.Emulation.Commodore.Common.Machines.AmigaCD32.Constants.ModelConstants.CD32 => ntsc ? ToMhz(ClockFrequency.Ntsc68EC020) : ToMhz(ClockFrequency.Pal68EC020),
        GWGUI.Emulation.Commodore.Common.Machines.AmigaComputers.Constants.ModelConstants.A3000 or GWGUI.Emulation.Commodore.Common.Machines.AmigaComputers.Constants.ModelConstants.A4000 => ToMhz(ClockFrequency.Workstation),
        _ => ntsc ? ToMhz(ClockFrequency.NtscBase) : ToMhz(ClockFrequency.PalBase)
    };

    private static double ToMhz(ClockFrequency frequency) => (double)frequency / ClockConstants.HertzPerMegahertz;

    private static string FormatMhz(double frequency) => string.Format(SettingsValueConstants.MegahertzLabelFormat, frequency);
    private static bool Approximately(double left, double right) => Math.Abs(left - right) < ClockConstants.ComparisonToleranceMhz;

    private static EmulationSettingsChoice Invariant(string id, string text, long? numericValue = null) =>
        new(id, string.Empty, text, numericValue);

    private static IEnumerable<EmulationSettingsChoice> InvariantChoices(params string[] values) =>
        values.Select(value => Invariant(value, value));

    private static IReadOnlyList<EmulationSettingsChoice> VideoStandardChoices() =>
        Enum.GetValues<VideoStandard>().Select(value => SettingsChoiceConstants.VideoStandardChoices[value]).ToArray();

    private static IReadOnlyList<EmulationSettingsChoice> VideoResolutionChoices() =>
        Enum.GetValues<VideoResolution>().Select(value => SettingsChoiceConstants.VideoResolutionChoices[value]).ToArray();

    private static IReadOnlyList<EmulationSettingsChoice> VideoAspectChoices() =>
        Enum.GetValues<VideoAspect>().Select(value => SettingsChoiceConstants.VideoAspectChoices[value]).ToArray();

    private static IReadOnlyList<EmulationSettingsChoice> CropChoices() =>
        Enum.GetValues<CropMode>().Select(value => SettingsChoiceConstants.CropModeChoices[value]).ToArray();

    private static IReadOnlyList<EmulationSettingsChoice> LineModeChoices() =>
        Enum.GetValues<VideoLineMode>().Select(value => SettingsChoiceConstants.VideoLineModeChoices[value]).ToArray();

    private static IReadOnlyList<EmulationSettingsChoice> HzChangeChoices() =>
        Enum.GetValues<RefreshRateChange>().Select(value => SettingsChoiceConstants.RefreshRateChangeChoices[value]).ToArray();

    private static IReadOnlyList<EmulationSettingsChoice> FrameSkipChoices() =>
    [new(SettingsValueConstants.Disabled, SettingsResourceKeys.ValueDisabled), Invariant(SettingsValueConstants.FrameSkipOne, SettingsValueConstants.FrameSkipOne), Invariant(SettingsValueConstants.FrameSkipTwo, SettingsValueConstants.FrameSkipTwo)];

    private static IReadOnlyList<EmulationSettingsChoice> ImmediateBlitChoices() =>
        Enum.GetValues<BlitterMode>().Select(value => SettingsChoiceConstants.BlitterModeChoices[value]).ToArray();

    private static IReadOnlyList<EmulationSettingsChoice> CollisionChoices() =>
        Enum.GetValues<CollisionMode>().Select(value => SettingsChoiceConstants.CollisionModeChoices[value]).ToArray();

    private static IReadOnlyList<EmulationSettingsChoice> AudioInterpolationChoices() =>
        Enum.GetValues<AudioInterpolation>().Select(value => SettingsChoiceConstants.AudioInterpolationChoices[value]).ToArray();

    private static IReadOnlyList<EmulationSettingsChoice> AudioFilterChoices() =>
        Enum.GetValues<AudioFilter>().Select(value => SettingsChoiceConstants.AudioFilterChoices[value]).ToArray();

    private static IReadOnlyList<EmulationSettingsChoice> FilterTypeChoices() =>
        Enum.GetValues<AudioFilterType>().Select(value => SettingsChoiceConstants.AudioFilterTypeChoices[value]).ToArray();

    private static IEnumerable<EmulationSettingsChoice> PercentageChoices(int minimum, int maximum, int step) =>
        Enumerable.Range(BufferConstants.FirstCollectionIndex, (maximum - minimum) / step + SettingsValueConstants.InclusiveRangeEndpointCount)
            .Select(index => minimum + index * step).Select(value => Invariant(value.ToString(), string.Format(SettingsValueConstants.PercentageLabelFormat, value), value));

    private static IReadOnlyList<EmulationSettingsChoice> AnalogMouseChoices() =>
        Enum.GetValues<AnalogMouseMode>().Select(value => SettingsChoiceConstants.AnalogMouseModeChoices[value]).ToArray();

    private static IEnumerable<EmulationSettingsChoice> RatioChoices() => Enumerable.Range(SettingsValueConstants.FirstRatioStep, SettingsValueConstants.RatioStepCount)
        .Select(value => value / SettingsValueConstants.RatioStepsPerUnit).Select(value => Invariant(value.ToString(SettingsValueConstants.RatioFormat,
            System.Globalization.CultureInfo.InvariantCulture), string.Format(SettingsValueConstants.RatioLabelFormat, value)));
}
