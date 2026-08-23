using GWGUI.Emulation;

namespace GWGUI.Emulation.Atari;

internal static class AtariInputSnapshotFunctions
{
    private static readonly IReadOnlyDictionary<string, int> CommonButtons =
        new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            ["Fire1"] = 0, ["Fire2"] = 8, ["Turbo"] = 9,
            ["Up"] = 4, ["Down"] = 5, ["Left"] = 6, ["Right"] = 7,
            ["A"] = 8, ["B"] = 0, ["C"] = 1,
            ["Pause"] = 2, ["Option"] = 3, ["Option1"] = 10, ["Option2"] = 11,
            ["Start"] = 3, ["Reset"] = 9,
            ["Key0"] = 9, ["Key1"] = 10, ["Key2"] = 11, ["Key3"] = 12,
            ["Key4"] = 13, ["Key5"] = 14, ["Key6"] = 15,
            ["Star"] = 1, ["Hash"] = 8
        };

    internal static EmulationInputSnapshot Apply(EmulationInputSnapshot snapshot,
        AtariInputConfiguration? configuration, AtariMachineModel model)
    {
        var mappedKeyboard = snapshot with
        {
            Keys = EmulationInputMappingFunctions.MapKeyboard(snapshot.Keys, configuration?.KeyboardMappings)
        };
        var withDeadZones = AtariControllerFunctions.ApplyDeadZones(mappedKeyboard, configuration?.Controllers);
        if (configuration?.Controllers is not { Count: > 0 }) return withDeadZones;
        var count = Math.Max(withDeadZones.Controllers.Count,
            configuration.Controllers.Max(binding => binding.Port) + 1);
        var controllers = Enumerable.Repeat(EmulationControllerState.Empty, count).ToArray();
        foreach (var binding in configuration.Controllers)
        {
            if (binding.Port < 0 || binding.Port >= controllers.Length) continue;
            var source = EmulationInputMappingFunctions.ResolveController(binding.DeviceId,
                withDeadZones.Controllers, binding.Port);
            if (binding.Mappings is not { Count: > 0 })
            {
                controllers[binding.Port] = source;
                continue;
            }
            uint buttons = 0;
            foreach (var mapping in binding.Mappings)
            {
                var target = ResolveButton(model, mapping.Key);
                if (target >= 0 && IsPressed(mapping.Value, source, withDeadZones.Keys))
                    buttons |= 1u << target;
            }
            controllers[binding.Port] = source with { Buttons = buttons };
        }
        return withDeadZones with { Controllers = controllers };
    }

    private static bool IsPressed(string sourceName, EmulationControllerState controller,
        IReadOnlySet<EmulationKey> keys)
    {
        if (sourceName.StartsWith("Keyboard:", StringComparison.OrdinalIgnoreCase)
            && Enum.TryParse<EmulationKey>(sourceName[9..], true, out var key)) return keys.Contains(key);
        return EmulationInputMappingFunctions.IsControllerSourcePressed(sourceName, controller);
    }

    private static int ResolveButton(AtariMachineModel model, string action)
    {
        if (model == AtariMachineModel.Lynx)
            return action switch
            {
                "Option1" => 10, "Option2" => 11, "Pause" => 3,
                _ => CommonButtons.GetValueOrDefault(action, -1)
            };
        if (model == AtariMachineModel.Atari5200)
            return action switch
            {
                "Pause" => 2, "Start" => 3, "Key0" => 10, "Key1" => 11, "Key2" => 12,
                "Key3" => 13, "Key7" => 14, "Star" => 9, "Hash" => 1,
                _ => CommonButtons.GetValueOrDefault(action, -1)
            };
        return CommonButtons.GetValueOrDefault(action, -1);
    }
}
