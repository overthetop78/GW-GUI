using GWGUI.Emulation;

namespace GWGUI.Emulation.Amiga.Common.Machines.Common.Functions;

internal static partial class SettingsDescriptionFunctions
{
private static string Value(IReadOnlyDictionary<string, string> values, string key, string fallback) =>
        values.GetValueOrDefault(key) ?? fallback;

    private static string DefaultFpu(string cpu) => cpu is SettingsDescriptionFunctionsConstants.Value68040 or SettingsDescriptionFunctionsConstants.Value68060 ? SettingsDescriptionFunctionsConstants.Cpu : SettingsDescriptionFunctionsConstants.Value0;
    private static IReadOnlyList<string> FpuValues(string cpu) => cpu switch
    {
        SettingsDescriptionFunctionsConstants.Value68000 or SettingsDescriptionFunctionsConstants.Value68010 => [SettingsDescriptionFunctionsConstants.Value0],
        SettingsDescriptionFunctionsConstants.Value68020 or SettingsDescriptionFunctionsConstants.Value68030 => [SettingsDescriptionFunctionsConstants.Value0, SettingsDescriptionFunctionsConstants.Value68881, SettingsDescriptionFunctionsConstants.Value68882],
        _ => [SettingsDescriptionFunctionsConstants.Cpu, SettingsDescriptionFunctionsConstants.Value0, SettingsDescriptionFunctionsConstants.Value68881, SettingsDescriptionFunctionsConstants.Value68882]
    };
    private static IReadOnlyList<string> ChipMemoryValues(Model model) => model.Id switch
    {
        SettingsDescriptionFunctionsConstants.A1000 => [SettingsDescriptionFunctionsConstants.Value1], SettingsDescriptionFunctionsConstants.A500 => [SettingsDescriptionFunctionsConstants.Value1, SettingsDescriptionFunctionsConstants.Value22, SettingsDescriptionFunctionsConstants.Value33, SettingsDescriptionFunctionsConstants.Value4],
        SettingsDescriptionFunctionsConstants.A500PLUS or SettingsDescriptionFunctionsConstants.A600 => [SettingsDescriptionFunctionsConstants.Value22, SettingsDescriptionFunctionsConstants.Value4], SettingsDescriptionFunctionsConstants.A2000 => [SettingsDescriptionFunctionsConstants.Value1, SettingsDescriptionFunctionsConstants.Value22, SettingsDescriptionFunctionsConstants.Value4], _ => [SettingsDescriptionFunctionsConstants.Value4]
    };
    private static IReadOnlyList<string> SlowMemoryValues(Model model) => model.Id switch
    {
        SettingsDescriptionFunctionsConstants.A1000 or SettingsDescriptionFunctionsConstants.A500 or SettingsDescriptionFunctionsConstants.A500PLUS or SettingsDescriptionFunctionsConstants.A2000 => [SettingsDescriptionFunctionsConstants.Value0, SettingsDescriptionFunctionsConstants.Value22, SettingsDescriptionFunctionsConstants.Value4, SettingsDescriptionFunctionsConstants.Value62, SettingsDescriptionFunctionsConstants.Value72], _ => [SettingsDescriptionFunctionsConstants.Value0]
    };
    private static string ChipMemoryValue(int kib) => Math.Clamp(kib / 512, 1, 4).ToString();
    private static string SlowMemoryValue(int kib) => kib switch
    {
        512 => SettingsDescriptionFunctionsConstants.Value22, 1024 => SettingsDescriptionFunctionsConstants.Value4, 1536 => SettingsDescriptionFunctionsConstants.Value62, 1792 => SettingsDescriptionFunctionsConstants.Value72, _ => SettingsDescriptionFunctionsConstants.Value0
    };

    private static EmulationSettingsChoice CpuChoice(string value) =>
        new(value, string.Empty, value == SettingsDescriptionFunctionsConstants.Value68020 ? SettingsDescriptionFunctionsConstants.Motorola68EC020 : $"Motorola {value}");

    private static EmulationSettingsChoice FpuChoice(string value) => value switch
    {
        SettingsDescriptionFunctionsConstants.Value0 => new(value, SettingsDescriptionFunctionsConstants.ResourceMemoryNone),
        SettingsDescriptionFunctionsConstants.Cpu => new(value, string.Empty, SettingsDescriptionFunctionsConstants.CPU),
        _ => new(value, string.Empty, value)
    };

    private static IReadOnlyList<EmulationSettingsChoice> CompatibilityChoices() =>
    [
        new(SettingsDescriptionFunctionsConstants.Normal, SettingsDescriptionFunctionsConstants.ResourceCpuCompatibilityNormal),
        new(SettingsDescriptionFunctionsConstants.Compatible, SettingsDescriptionFunctionsConstants.ResourceCpuCompatibilityCompatible),
        new(SettingsDescriptionFunctionsConstants.Memory, SettingsDescriptionFunctionsConstants.ResourceCpuCompatibilityMemory),
        new(SettingsDescriptionFunctionsConstants.Exact, SettingsDescriptionFunctionsConstants.ResourceCpuCompatibilityExact)
    ];

    private static EmulationSettingsChoice ChipMemoryChoice(string value)
    {
        var kib = int.TryParse(value, out var units) ? units * 512 : 0;
        return MemoryChoice(value, kib * 1024L);
    }

    private static EmulationSettingsChoice SlowMemoryChoice(string value)
    {
        var kib = value switch { SettingsDescriptionFunctionsConstants.Value22 => 512, SettingsDescriptionFunctionsConstants.Value4 => 1024, SettingsDescriptionFunctionsConstants.Value62 => 1536, SettingsDescriptionFunctionsConstants.Value72 => 1792, _ => 0 };
        return MemoryChoice(value, kib * 1024L);
    }

    private static EmulationSettingsChoice MemoryMibChoice(string value)
    {
        var mib = int.TryParse(value, out var parsed) ? parsed : 0;
        return MemoryChoice(value, mib * 1024L * 1024L);
    }

    private static EmulationSettingsChoice MemoryChoice(string id, long bytes) => bytes == 0
        ? new EmulationSettingsChoice(id, SettingsDescriptionFunctionsConstants.ResourceMemoryNone, NumericValue: 0)
        : bytes < 1024L * 1024L
            ? new EmulationSettingsChoice(id, string.Empty, $"{bytes / 1024L} KiB", bytes)
            : new EmulationSettingsChoice(id, string.Empty, $"{bytes / (1024L * 1024L)} MiB", bytes);

    private static IReadOnlyList<EmulationSettingsChoice> CpuFrequencyChoices(Model model,
        string compatibility, bool ntsc)
    {
        var nominal = NominalCpuFrequencyMhz(model, ntsc);
        if (compatibility is SettingsDescriptionFunctionsConstants.Memory or SettingsDescriptionFunctionsConstants.Exact)
        {
            var halfA500Clock = ntsc ? 3.579545d : 3.546895d;
            var choices = new[] { 1, 2, 4, 8, 16 }.Select(multiplier =>
            {
                var frequency = halfA500Clock * multiplier;
                var ratio = frequency / nominal;
                return FrequencyChoice(ratio, SettingsDescriptionFunctionsConstants.Value00, multiplier.ToString(), frequency);
            }).Where(choice => !Approximately(choice.NumericValue.GetValueOrDefault() / 1_000_000d, nominal))
                .ToList();
            choices.Add(FrequencyChoice(1d, SettingsDescriptionFunctionsConstants.Value00, SettingsDescriptionFunctionsConstants.Value0, nominal));
            return choices.OrderBy(choice => choice.NumericValue).ToArray();
        }

        return new[]
        {
            (Ratio: 0.5d, Throttle: SettingsDescriptionFunctionsConstants.Value5000), (Ratio: 1d, Throttle: SettingsDescriptionFunctionsConstants.Value00),
            (Ratio: 2d, Throttle: SettingsDescriptionFunctionsConstants.Value10000), (Ratio: 4d, Throttle: SettingsDescriptionFunctionsConstants.Value30000),
            (Ratio: 8d, Throttle: SettingsDescriptionFunctionsConstants.Value70000)
        }.Select(item => FrequencyChoice(item.Ratio, item.Throttle, SettingsDescriptionFunctionsConstants.Value0, nominal * item.Ratio)).ToArray();
    }

    private static EmulationSettingsChoice FrequencyChoice(double ratio, string throttle, string multiplier,
        double frequency)
    {
        var prefix = Approximately(ratio, 1d) ? SettingsDescriptionFunctionsConstants.Value1003 : $"{Math.Round(ratio * 100d):0} %";
        return new EmulationSettingsChoice($"{throttle}|{multiplier}", string.Empty,
            $"{prefix} — {FormatMhz(frequency)}", (long)Math.Round(frequency * 1_000_000d));
    }

    private static string CpuFrequencyValue(IReadOnlyDictionary<string, string> options, string compatibility,
        IReadOnlyList<EmulationSettingsChoice> choices)
    {
        var throttle = Value(options, SettingsConstants.OptionCpuThrottle, SettingsDescriptionFunctionsConstants.Value00);
        var multiplier = Value(options, SettingsConstants.OptionCpuMultiplier, SettingsDescriptionFunctionsConstants.Value0);
        var expected = compatibility is SettingsDescriptionFunctionsConstants.Memory or SettingsDescriptionFunctionsConstants.Exact ? $"0.0|{multiplier}" : $"{throttle}|0";
        return choices.Any(choice => choice.Id == expected)
            ? expected : choices.FirstOrDefault(choice => choice.Id == SettingsDescriptionFunctionsConstants.Value000)?.Id ?? choices[0].Id;
    }

    private static double NominalCpuFrequencyMhz(Model model, bool ntsc) => model.Id switch
    {
        SettingsDescriptionFunctionsConstants.A1200 or SettingsDescriptionFunctionsConstants.CD32 => ntsc ? 14.31818d : 14.18758d,
        SettingsDescriptionFunctionsConstants.A3000 or SettingsDescriptionFunctionsConstants.A4000 => 25d,
        _ => ntsc ? 7.15909d : 7.09379d
    };

    private static string FormatMhz(double frequency) => $"{frequency:0.00} MHz";
    private static bool Approximately(double left, double right) => Math.Abs(left - right) < 0.0001d;

    private static EmulationSettingsChoice Invariant(string id, string text, long? numericValue = null) =>
        new(id, string.Empty, text, numericValue);

    private static IEnumerable<EmulationSettingsChoice> InvariantChoices(params string[] values) =>
        values.Select(value => Invariant(value, value));

    private static IReadOnlyList<EmulationSettingsChoice> VideoStandardChoices() =>
    [Invariant(SettingsDescriptionFunctionsConstants.PALAuto, SettingsDescriptionFunctionsConstants.PALAuto), Invariant(SettingsDescriptionFunctionsConstants.NTSCAuto, SettingsDescriptionFunctionsConstants.NTSCAuto),
        Invariant(SettingsDescriptionFunctionsConstants.PAL, SettingsDescriptionFunctionsConstants.PAL), Invariant(SettingsDescriptionFunctionsConstants.NTSC, SettingsDescriptionFunctionsConstants.NTSC)];

    private static IReadOnlyList<EmulationSettingsChoice> VideoResolutionChoices() =>
    [new(SettingsDescriptionFunctionsConstants.Auto, SettingsDescriptionFunctionsConstants.VisualAutomatic), new(SettingsDescriptionFunctionsConstants.AutoLores, SettingsDescriptionFunctionsConstants.ResourceVideoResolutionAutoLow),
        new(SettingsDescriptionFunctionsConstants.AutoSuperhires, SettingsDescriptionFunctionsConstants.ResourceVideoResolutionAutoSuperHigh),
        new(SettingsDescriptionFunctionsConstants.Lores, SettingsDescriptionFunctionsConstants.ResourceVideoResolutionLow), new(SettingsDescriptionFunctionsConstants.Hires, SettingsDescriptionFunctionsConstants.ResourceVideoResolutionHigh),
        new(SettingsDescriptionFunctionsConstants.Superhires, SettingsDescriptionFunctionsConstants.ResourceVideoResolutionSuperHigh)];

    private static IReadOnlyList<EmulationSettingsChoice> VideoAspectChoices() =>
    [new(SettingsDescriptionFunctionsConstants.Auto, SettingsDescriptionFunctionsConstants.VisualAutomatic), Invariant(SettingsDescriptionFunctionsConstants.PAL, SettingsDescriptionFunctionsConstants.PAL), Invariant(SettingsDescriptionFunctionsConstants.NTSC, SettingsDescriptionFunctionsConstants.NTSC),
        Invariant(SettingsDescriptionFunctionsConstants.Value11, SettingsDescriptionFunctionsConstants.Value11)];

    private static IReadOnlyList<EmulationSettingsChoice> CropChoices() =>
    [new(SettingsDescriptionFunctionsConstants.Disabled, SettingsDescriptionFunctionsConstants.ResourceValueDisabled), new(SettingsDescriptionFunctionsConstants.Minimum, SettingsDescriptionFunctionsConstants.ResourceValueMinimum),
        new(SettingsDescriptionFunctionsConstants.Smaller, SettingsDescriptionFunctionsConstants.ResourceValueVerySmall), new(SettingsDescriptionFunctionsConstants.Small, SettingsDescriptionFunctionsConstants.ResourceValueSmall),
        new(SettingsDescriptionFunctionsConstants.Medium, SettingsDescriptionFunctionsConstants.ResourceValueMedium), new(SettingsDescriptionFunctionsConstants.Large, SettingsDescriptionFunctionsConstants.ResourceValueLarge),
        new(SettingsDescriptionFunctionsConstants.Larger, SettingsDescriptionFunctionsConstants.ResourceValueVeryLarge), new(SettingsDescriptionFunctionsConstants.Maximum, SettingsDescriptionFunctionsConstants.ResourceValueMaximum),
        new(SettingsDescriptionFunctionsConstants.Auto, SettingsDescriptionFunctionsConstants.VisualAutomatic)];

    private static IReadOnlyList<EmulationSettingsChoice> LineModeChoices() =>
    [new(SettingsDescriptionFunctionsConstants.Auto, SettingsDescriptionFunctionsConstants.VisualAutomatic), new(SettingsDescriptionFunctionsConstants.Single, SettingsDescriptionFunctionsConstants.ResourceVideoLineModeSingle),
        new(SettingsDescriptionFunctionsConstants.Double, SettingsDescriptionFunctionsConstants.ResourceVideoLineModeDouble)];

    private static IReadOnlyList<EmulationSettingsChoice> HzChangeChoices() =>
    [new(SettingsDescriptionFunctionsConstants.Disabled, SettingsDescriptionFunctionsConstants.ResourceValueDisabled), new(SettingsDescriptionFunctionsConstants.Enabled, SettingsDescriptionFunctionsConstants.ResourceValueEnabled),
        new(SettingsDescriptionFunctionsConstants.Locked, SettingsDescriptionFunctionsConstants.ResourceStateLocked)];

    private static IReadOnlyList<EmulationSettingsChoice> FrameSkipChoices() =>
    [new(SettingsDescriptionFunctionsConstants.Disabled, SettingsDescriptionFunctionsConstants.ResourceValueDisabled), Invariant(SettingsDescriptionFunctionsConstants.Value1, SettingsDescriptionFunctionsConstants.Value1), Invariant(SettingsDescriptionFunctionsConstants.Value22, SettingsDescriptionFunctionsConstants.Value22)];

    private static IReadOnlyList<EmulationSettingsChoice> ImmediateBlitChoices() =>
    [new(SettingsDescriptionFunctionsConstants.False, SettingsDescriptionFunctionsConstants.ResourceValueDisabled), new(SettingsDescriptionFunctionsConstants.Immediate, SettingsDescriptionFunctionsConstants.ResourceStateImmediate),
        new(SettingsDescriptionFunctionsConstants.Waiting, SettingsDescriptionFunctionsConstants.ResourceStateWaiting)];

    private static IReadOnlyList<EmulationSettingsChoice> CollisionChoices() =>
    [new(SettingsDescriptionFunctionsConstants.None, SettingsDescriptionFunctionsConstants.HostToolsNone), new(SettingsDescriptionFunctionsConstants.Sprites, SettingsDescriptionFunctionsConstants.ResourceVideoCollisionSprites),
        new(SettingsDescriptionFunctionsConstants.Playfields, SettingsDescriptionFunctionsConstants.ResourceVideoCollisionPlayfields),
        new(SettingsDescriptionFunctionsConstants.Full, SettingsDescriptionFunctionsConstants.ResourceVideoCollisionFull)];

    private static IReadOnlyList<EmulationSettingsChoice> AudioInterpolationChoices() =>
    [new(SettingsDescriptionFunctionsConstants.None, SettingsDescriptionFunctionsConstants.HostToolsNone), new(SettingsDescriptionFunctionsConstants.Anti, SettingsDescriptionFunctionsConstants.ResourceAudioInterpolationAnti),
        Invariant(SettingsDescriptionFunctionsConstants.Sinc, SettingsDescriptionFunctionsConstants.Sinc2), Invariant(SettingsDescriptionFunctionsConstants.Rh, SettingsDescriptionFunctionsConstants.RH), Invariant(SettingsDescriptionFunctionsConstants.Crux, SettingsDescriptionFunctionsConstants.Crux2)];

    private static IReadOnlyList<EmulationSettingsChoice> AudioFilterChoices() =>
    [new(SettingsDescriptionFunctionsConstants.Emulated, SettingsDescriptionFunctionsConstants.ResourceAudioFilterEmulated), new(SettingsDescriptionFunctionsConstants.Off, SettingsDescriptionFunctionsConstants.ResourceValueDisabled),
        new(SettingsDescriptionFunctionsConstants.On, SettingsDescriptionFunctionsConstants.ResourceValueEnabled)];

    private static IReadOnlyList<EmulationSettingsChoice> FilterTypeChoices() =>
    [new(SettingsDescriptionFunctionsConstants.Auto, SettingsDescriptionFunctionsConstants.VisualAutomatic), new(SettingsDescriptionFunctionsConstants.Standard, SettingsDescriptionFunctionsConstants.ResourceValueStandard),
        new(SettingsDescriptionFunctionsConstants.Enhanced, SettingsDescriptionFunctionsConstants.ResourceValueEnhanced)];

    private static IEnumerable<EmulationSettingsChoice> PercentageChoices(int minimum, int maximum, int step) =>
        Enumerable.Range(0, (maximum - minimum) / step + 1)
            .Select(index => minimum + index * step).Select(value => Invariant(value.ToString(), $"{value} %", value));

    private static IReadOnlyList<EmulationSettingsChoice> AnalogMouseChoices() =>
    [new(SettingsDescriptionFunctionsConstants.Disabled, SettingsDescriptionFunctionsConstants.ResourceValueDisabled), new(SettingsDescriptionFunctionsConstants.Left, SettingsDescriptionFunctionsConstants.ResourceControllerStickLeft),
        new(SettingsDescriptionFunctionsConstants.Right, SettingsDescriptionFunctionsConstants.ResourceControllerStickRight), new(SettingsDescriptionFunctionsConstants.Both, SettingsDescriptionFunctionsConstants.ResourceControllerStickBoth)];

    private static IEnumerable<EmulationSettingsChoice> RatioChoices() => Enumerable.Range(1, 30)
        .Select(value => value / 10d).Select(value => Invariant(value.ToString(SettingsDescriptionFunctionsConstants.Value00,
            System.Globalization.CultureInfo.InvariantCulture), $"{value:0.0}×"));
}
