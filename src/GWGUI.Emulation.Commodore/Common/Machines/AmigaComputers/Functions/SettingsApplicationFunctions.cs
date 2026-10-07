namespace GWGUI.Emulation.Commodore.Common.Machines.AmigaComputers.Functions;

internal static class AmigaSettingsApplicationFunctions
{
    internal static MachineConfiguration Apply(MachineConfiguration current, IReadOnlyDictionary<string, string?> values)
    {
        var options = new Dictionary<string, string>(current.Options ?? new Dictionary<string, string>());
        options.Remove(SettingsConstants.CpuOriginalSpeed);
        options.Remove(SettingsConstants.CpuSpeed);
        options.Remove(SettingsConstants.ParallelJoystickAdapter);
        if (values.TryGetValue(SettingsConstants.OptionSoundVolumeCd, out var cdVolume)
            && !string.IsNullOrWhiteSpace(cdVolume))
            options[SettingsConstants.OptionSoundVolumeCd] =
                cdVolume.TrimEnd(SettingsValueConstants.PercentSuffix)
                + SettingsValueConstants.PercentSuffix;
        if (values.GetValueOrDefault(SettingsConstants.CpuSpeed)?.Split(MachineSettingsConstants.CpuSpeedSeparator) is [var throttle, var multiplier])
        {
            options[SettingsConstants.OptionCpuThrottle] = throttle;
            options[SettingsConstants.OptionCpuMultiplier] = multiplier;
        }
        var currentInput = current.Input ?? new InputConfiguration();
        var input = currentInput with
        {
            ParallelJoystickAdapterEnabled = values.TryGetValue(
                SettingsConstants.ParallelJoystickAdapter, out var parallelJoystickAdapter)
                    ? parallelJoystickAdapter == SettingsValueConstants.Enabled
                    : currentInput.ParallelJoystickAdapterEnabled
        };
        return current with { Options = options, Input = input };
    }
}
