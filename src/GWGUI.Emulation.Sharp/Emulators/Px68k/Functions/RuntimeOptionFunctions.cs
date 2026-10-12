using GWGUI.Emulation.Sharp.Emulators.Px68k.Constants;
using GWGUI.Emulation.Sharp.Emulators.Px68k.Dictionaries;

namespace GWGUI.Emulation.Sharp.Emulators.Px68k.Functions;
internal static class RuntimeOptionFunctions
{
    internal static IReadOnlyDictionary<string, string> Defaults(MachineModel model)
    {
        var result = OptionCatalog.Options.ToDictionary(option => option.Key, option => option.DefaultValue, StringComparer.Ordinal);
        result[OptionConstants.Cpuspeed] = model == MachineModel.X68000Xvi ? RuntimeOptionConstants.XviCpuSpeed : RuntimeOptionConstants.BaseCpuSpeed;
        result[OptionConstants.Ramsize] = model == MachineModel.X68000Xvi ? RuntimeOptionConstants.XviMemory : RuntimeOptionConstants.BaseMemory;
        return result;
    }

    internal static IReadOnlyDictionary<string, string> Prepare(MachineConfiguration configuration)
    {
        var result = new Dictionary<string, string>(Defaults(configuration.Model), StringComparer.Ordinal);
        foreach (var option in configuration.Options)
            if (result.ContainsKey(option.Key))
                result[option.Key] = option.Value;
        var ports = configuration.Input.Controllers ?? [];
        var first = ports.FirstOrDefault(binding => binding.Port == ControllerConstants.FirstPort)?.Peripheral ?? PeripheralCategory.None;
        var second = ports.FirstOrDefault(binding => binding.Port == ControllerConstants.FirstPort + 1)?.Peripheral ?? PeripheralCategory.None;
        result[OptionConstants.Joytype1] = JoystickType(first);
        result[OptionConstants.Joytype2] = JoystickType(second);
        result[OptionConstants.Analog] = first == PeripheralCategory.CyberstickAnalog ? RuntimeOptionConstants.Enabled : RuntimeOptionConstants.Disabled;
        result[OptionConstants.SaveFddPath] = RuntimeOptionConstants.Enabled;
        result[OptionConstants.SaveHddPath] = RuntimeOptionConstants.Enabled;
        result[OptionConstants.MidiOutput] = RuntimeOptionConstants.Disabled;
        result[OptionConstants.RumbleOnDiskRead] = RuntimeOptionConstants.Disabled;
        return result;
    }

    private static string JoystickType(PeripheralCategory peripheral) => peripheral switch
    {
        PeripheralCategory.None or PeripheralCategory.Joystick => RuntimeOptionConstants.StandardJoystick,
        PeripheralCategory.CpsfMd => RuntimeOptionConstants.CpsfMd,
        PeripheralCategory.CpsfSfc => RuntimeOptionConstants.CpsfSfc,
        PeripheralCategory.CyberstickDigital => RuntimeOptionConstants.CyberstickDigital,
        PeripheralCategory.CyberstickAnalog => RuntimeOptionConstants.CyberstickAnalog,
        _ => throw new ArgumentOutOfRangeException(nameof(peripheral))};
}