using GWGUI.Emulation;
namespace GWGUI.Emulation.Amstrad.Common.Machines.Common.Functions;

internal static class KeyboardBindingFunctions
{
    internal static IReadOnlyDictionary<string, string> Normalize(InputConfiguration input)
    {
        var result = KeyboardDictionary.DefaultHostKeys.ToDictionary(item => item.Key.ToString(), item => item.Value.ToString(), StringComparer.Ordinal);
        var source = input.KeyboardBindings ?? input.KeyboardMappings?.ToDictionary(item => item.Key, item => item.Value.ToString(), StringComparer.Ordinal);
        if (source is null) return result;
        foreach (var item in source)
        {
            if (!Enum.TryParse<EmulationKey>(item.Key, true, out var key)) { result[item.Key] = item.Value; continue; }
            var target = key switch
            {
                EmulationKey.RightAlt => EmulationKey.AmstradCopy,
                EmulationKey.Delete => EmulationKey.AmstradClear,
                EmulationKey.Numpad0 => EmulationKey.AmstradF0,
                EmulationKey.NumpadPeriod => EmulationKey.AmstradFPeriod,
                >= EmulationKey.Numpad1 and <= EmulationKey.Numpad9 => (EmulationKey)((int)EmulationKey.F1 + (int)key - (int)EmulationKey.Numpad1),
                _ => key
            };
            if (target != key && source.ContainsKey(target.ToString())) continue;
            result[target.ToString()] = item.Value;
        }
        return result;
    }

    internal static IReadOnlySet<EmulationKey> Apply(EmulationInputSnapshot snapshot, InputConfiguration input) => EmulationSdk.InputProcessing.MapKeyboard(snapshot, Normalize(input));

    internal static IReadOnlySet<EmulationKey> Apply(IReadOnlySet<EmulationKey> hostKeys, InputConfiguration input) => Apply(EmulationInputSnapshot.Empty with { Keys = hostKeys }, input);
}
