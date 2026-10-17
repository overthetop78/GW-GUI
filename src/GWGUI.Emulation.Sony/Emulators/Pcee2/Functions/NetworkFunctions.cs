using System.IO;

namespace GWGUI.Emulation.Sony.Emulators.Pcee2.Functions;

internal static class NetworkFunctions
{
    internal const string EnabledKey = "network.ethernetEnabled";

    internal static IReadOnlyList<EmulationSettingsBlock> Describe(MachineConfiguration configuration, IReadOnlyList<EmulationSettingsBlock> blocks) =>
    [
        .. blocks,
        new("ethernet", EmulationMachineTab.Ethernet, "Emulation.Tab.Ethernet",
        [
            new(EnabledKey, EmulationMachineTab.Ethernet, "ethernet", "Emulation.Sony.Ethernet.Enabled",
                EmulationSettingsEditor.Toggle, configuration.Options?.GetValueOrDefault(EnabledKey) ?? EmulationSettingConstants.Disabled,
                ExplanationResourceKey: "Emulation.Sony.Ethernet.Help", RequiresRestart: true),
            new("network.mode", EmulationMachineTab.Ethernet, "ethernet", "Emulation.Sony.Ethernet.Mode",
                EmulationSettingsEditor.Information, "automatic",
                [new("automatic", "Emulation.Sony.Ethernet.Automatic")])
        ])
    ];

    internal static void Prepare(MachineConfiguration configuration, string systemDirectory)
    {
        CheatFunctions.Prepare(configuration, systemDirectory);
        var directory = Path.Combine(systemDirectory, "pcsx2", "inis");
        Directory.CreateDirectory(directory);
        var path = Path.Combine(directory, "PCSX2.ini");
        var lines = BuildSettings(File.Exists(path) ? File.ReadAllLines(path) : [],
            configuration.Options?.GetValueOrDefault(EnabledKey) == EmulationSettingConstants.Enabled);
        var temporary = path + "." + Guid.NewGuid().ToString("N") + ".tmp";
        try
        {
            File.WriteAllLines(temporary, lines);
            File.Move(temporary, path, true);
        }
        finally
        {
            if (File.Exists(temporary)) File.Delete(temporary);
        }
    }

    internal static IReadOnlyList<string> BuildSettings(IEnumerable<string> existing, bool enabled)
    {
        var lines = existing.ToList();
        Set("EthEnable", enabled ? "true" : "false");
        Set("EthApi", "Sockets");
        Set("EthDevice", "Auto");
        Set("InterceptDHCP", "true");
        return lines;

        void Set(string key, string value)
        {
            var start = lines.FindIndex(line => line.Trim().Equals("[DEV9/Eth]", StringComparison.OrdinalIgnoreCase));
            if (start < 0)
            {
                lines.Add("[DEV9/Eth]");
                start = lines.Count - 1;
            }
            var end = lines.FindIndex(start + 1, line => line.TrimStart().StartsWith('['));
            if (end < 0) end = lines.Count;
            for (var index = start + 1; index < end; index++)
            {
                var separator = lines[index].IndexOf('=');
                if (separator < 0 || !lines[index][..separator].Trim().Equals(key, StringComparison.OrdinalIgnoreCase)) continue;
                lines[index] = key + " = " + value;
                return;
            }
            lines.Insert(end, key + " = " + value);
        }
    }
}
