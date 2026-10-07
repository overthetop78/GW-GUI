using System.Globalization;
using GWGUI.Emulation.Atari.Emulators.HatariB.Constants;

namespace GWGUI.Emulation.Atari.Emulators.HatariB.Functions;

internal static class OptionFunctions
{
    internal static IReadOnlyDictionary<string, string> Apply(MachineConfiguration configuration)
    {
        var model = StModelCatalog.Get(configuration.Model);
        var result = new Dictionary<string, string>(ToNative(configuration.Options), StringComparer.Ordinal)
        {
            [OptionConstants.MachineType] = MachineType(configuration.Model),
            [OptionConstants.MemorySize] = MemorySize(configuration.Options.GetValueOrDefault(
                SettingsConstants.MainMemory, ((long)model.DefaultMainMemoryKib *
                    OptionConstants.BytesPerKibibyte).ToString(CultureInfo.InvariantCulture))),
            [OptionConstants.CpuClock] = configuration.Options.GetValueOrDefault(
                SettingsConstants.CpuFrequency, model.DefaultCpuFrequencyMhz.ToString(CultureInfo.InvariantCulture)),
            [OptionConstants.Cpu] = Cpu(configuration.Options.GetValueOrDefault(
                SettingsConstants.Cpu, model.DefaultCpu.ToString())),
            [OptionConstants.Fpu] = Fpu(configuration.Options.GetValueOrDefault(
                SettingsConstants.Fpu, model.DefaultFpu.ToString()))
        };
        result.TryAdd(OptionConstants.Tos, configuration.Firmwares.Any(item => item.Category == FirmwareCategory.Tos)
            ? OptionConstants.UserTos : OptionConstants.InternalTos);
        result.TryAdd(OptionConstants.HostKeyboard, OptionConstants.Enabled);
        result.TryAdd(OptionConstants.HostMouse, OptionConstants.Enabled);
        if (configuration.Media.FirstOrDefault(item => item.IsInserted && item.Category == MediaCategory.Floppy)
                is { } floppy)
        {
            result[OptionConstants.ReadOnlyFloppy] = floppy.IsReadOnly ? OptionConstants.Enabled : OptionConstants.Disabled;
            if (configuration.Options.TryGetValue(MachineOptionConstants.FloppySpeedPrefix + floppy.Slot, out var speed))
                result[OptionConstants.FastFloppy] = int.Parse(speed, CultureInfo.InvariantCulture)
                    == OptionConstants.NormalFloppySpeedPercent ? OptionConstants.Disabled : OptionConstants.Enabled;
        }
        if (configuration.Media.FirstOrDefault(item => item.IsInserted &&
                item.Category is MediaCategory.HardDisk or MediaCategory.Directory) is { } storage)
            result[OptionConstants.ReadOnlyHardDisk] = storage.IsReadOnly ? OptionConstants.Enabled : OptionConstants.Disabled;
        return result;
    }

    internal static IReadOnlyDictionary<string, string> ToNative(IReadOnlyDictionary<string, string> options)
    {
        var result = new Dictionary<string, string>(options, StringComparer.Ordinal);
        Convert(result, SettingsConstants.MainMemory, OptionConstants.MemorySize, MemorySize);
        Convert(result, SettingsConstants.CpuFrequency, OptionConstants.CpuClock, value => value);
        Convert(result, SettingsConstants.Cpu, OptionConstants.Cpu, Cpu);
        Convert(result, SettingsConstants.Fpu, OptionConstants.Fpu, Fpu);
        Convert(result, SettingsConstants.CpuPrecision, OptionConstants.CpuCycleExact, value =>
            value == nameof(StCpuPrecision.CycleExact) ? OptionConstants.Enabled : OptionConstants.Disabled);
        Convert(result, MachineOptionConstants.DriveActivity, OptionConstants.StatusBar, value =>
            bool.Parse(value) ? OptionConstants.DriveLights : OptionConstants.Disabled);
        Convert(result, SettingsDescriptionFunctionsConstants.FastBoot, OptionConstants.FastBoot, Boolean);
        Convert(result, MachineOptionConstants.Crop, OptionConstants.Borders, value =>
            value == VideoAudioSettingsConstants.Enabled ? OptionConstants.Disabled : OptionConstants.DefaultBorders);
        Convert(result, MachineOptionConstants.PointerSpeed, OptionConstants.MouseSpeed, MouseSpeed);
        Convert(result, MachineValues.ResetType, OptionConstants.SoftReset, value =>
            value == MachineValues.Value0 ? OptionConstants.Enabled : OptionConstants.Disabled);
        if (result.Remove(VideoAudioSettingsConstants.StandardOption, out var standard))
        {
            result[OptionConstants.Monitor] = standard == MachineOptionFunctionsConstants.Monochrome
                ? OptionConstants.MonochromeMonitor : OptionConstants.ColourMonitor;
            if (standard == MachineOptionFunctionsConstants.NTSC)
                result[OptionConstants.EmutosFramerate] = OptionConstants.NtscFramerate;
            else if (standard == MachineOptionFunctionsConstants.PAL)
                result[OptionConstants.EmutosFramerate] = OptionConstants.PalFramerate;
        }
        return result;
    }

    private static void Convert(IDictionary<string, string> options, string source, string target,
        Func<string, string> conversion)
    {
        if (options.Remove(source, out var value)) options[target] = conversion(value);
    }

    private static string Boolean(string value) => bool.Parse(value) ? OptionConstants.Enabled : OptionConstants.Disabled;
    private static string MemorySize(string value) =>
        (long.Parse(value, CultureInfo.InvariantCulture) / OptionConstants.BytesPerKibibyte).ToString(CultureInfo.InvariantCulture);
    private static string Cpu(string value) => Enum.Parse<StCpu>(value) switch
    {
        StCpu.Motorola68000 => OptionConstants.Cpu68000,
        StCpu.Motorola68030 => OptionConstants.Cpu68030,
        _ => throw new ArgumentOutOfRangeException(nameof(value))
    };
    private static string Fpu(string value) => Enum.Parse<StFpu>(value) switch
    {
        StFpu.None => OptionConstants.NoFpu,
        StFpu.Motorola68881 => OptionConstants.Fpu68881,
        StFpu.Motorola68882 => OptionConstants.Fpu68882,
        _ => throw new ArgumentOutOfRangeException(nameof(value))
    };
    private static string MouseSpeed(string value) => int.Parse(value, CultureInfo.InvariantCulture) switch
    {
        <= OptionConstants.SlowMouseLimitPercent => OptionConstants.SlowMouseSpeed,
        <= OptionConstants.NormalMouseLimitPercent => OptionConstants.NormalMouseSpeed,
        <= OptionConstants.FastMouseLimitPercent => OptionConstants.FastMouseSpeed,
        <= OptionConstants.FasterMouseLimitPercent => OptionConstants.FasterMouseSpeed,
        _ => OptionConstants.FastestMouseSpeed
    };
    private static string MachineType(MachineModel model) => model switch
    {
        MachineModel.St or MachineModel.Stf or MachineModel.Stfm => OptionConstants.StMachine,
        MachineModel.MegaSt => OptionConstants.MegaStMachine,
        MachineModel.Ste => OptionConstants.SteMachine,
        MachineModel.MegaSte => OptionConstants.MegaSteMachine,
        MachineModel.Tt => OptionConstants.TtMachine,
        MachineModel.Falcon => OptionConstants.FalconMachine,
        _ => throw new ArgumentOutOfRangeException(nameof(model))
    };
}
