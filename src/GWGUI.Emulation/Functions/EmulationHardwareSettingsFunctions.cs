using System.Globalization;

namespace GWGUI.Emulation.Functions;

internal static class EmulationHardwareSettingsFunctions
{
    private const string CpuResource = "Emulation.Cpu.Processor";
    private const string CpuModelResource = "Emulation.Cpu.Model";
    private const string CpuFrequencyResource = "Emulation.Cpu.SpeedOriginal";
    private const string MemoryResource = "Emulation.Memory.Main";
    private const string ResolutionResource = "Emulation.Video.Resolution";

    internal static void AddBlocks(List<EmulationSettingsBlock> blocks, string fieldPrefix, string cpuResourceKey,
        Func<string?> cpuFrequency, Func<string?> mainMemory, Func<string?> displayMemory,
        string displayMemoryResourceKey, Func<string?> resolution, bool isCompactCpu)
    {
        var cpu = new List<EmulationSettingsField>
        {
            new(fieldPrefix + "_cpu_model", EmulationMachineTab.Cpu, CpuResource, CpuModelResource,
                EmulationSettingsEditor.Information, cpuResourceKey, [new(cpuResourceKey, cpuResourceKey)])
        };
        if (cpuFrequency() is { } frequency)
            cpu.Add(Information(fieldPrefix + "_cpu_frequency", EmulationMachineTab.Cpu, CpuResource, CpuFrequencyResource, frequency));
        blocks.Add(new(CpuResource, EmulationMachineTab.Cpu, CpuResource, cpu.ToArray(), IsCompact: isCompactCpu));
        var memory = new List<EmulationSettingsField>();
        if (mainMemory() is { } main)
            memory.Add(Information(fieldPrefix + "_main_memory", EmulationMachineTab.Ram, MemoryResource, MemoryResource, main));
        if (displayMemory() is { } display)
            memory.Add(Information(fieldPrefix + "_display_memory", EmulationMachineTab.Ram, MemoryResource, displayMemoryResourceKey, display));
        if (memory.Count > 0)
            blocks.Add(new(MemoryResource, EmulationMachineTab.Ram, MemoryResource, memory.ToArray()));
        if (resolution() is { } dimensions)
            blocks.Add(new(ResolutionResource, EmulationMachineTab.Video, ResolutionResource,
                [Information(fieldPrefix + "_display_resolution", EmulationMachineTab.Video, ResolutionResource, ResolutionResource, dimensions)]));
    }

    internal static string FormatFrequency(long hertz, bool useKilohertz = false) =>
        !useKilohertz || hertz >= 1_000_000
            ? FormatMegahertz(hertz, "0.######")
            : $"{(hertz / 1_000d).ToString("0.###", CultureInfo.CurrentCulture)} kHz";

    internal static string FormatMegahertz(long hertz, string format) =>
        $"{(hertz / 1_000_000d).ToString(format, CultureInfo.CurrentCulture)} MHz";

    internal static string FormatMemory(long bytes) => bytes >= 1024
        ? $"{(bytes / 1024).ToString(CultureInfo.InvariantCulture)} KiB"
        : $"{bytes.ToString(CultureInfo.InvariantCulture)} B";

    private static EmulationSettingsField Information(string id, EmulationMachineTab tab, string block, string label, string value) =>
        new(id, tab, block, label, EmulationSettingsEditor.Information, value);
}
