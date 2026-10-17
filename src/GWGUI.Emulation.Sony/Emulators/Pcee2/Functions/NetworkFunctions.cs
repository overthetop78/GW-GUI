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
        var lines = MemoryCardFunctions.BuildSettings(configuration, BuildSettings(File.Exists(path) ? File.ReadAllLines(path) : [],
            configuration.Options?.GetValueOrDefault(EnabledKey) == EmulationSettingConstants.Enabled));
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
        IniSettingsFunctions.Set(lines, "DEV9/Eth", "EthEnable", enabled ? "true" : "false");
        IniSettingsFunctions.Set(lines, "DEV9/Eth", "EthApi", "Sockets");
        IniSettingsFunctions.Set(lines, "DEV9/Eth", "EthDevice", "Auto");
        IniSettingsFunctions.Set(lines, "DEV9/Eth", "InterceptDHCP", "true");
        return lines;

    }
}
