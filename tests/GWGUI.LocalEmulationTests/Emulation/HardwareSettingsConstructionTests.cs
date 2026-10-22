using GWGUI.Emulation;
using System.Globalization;
using GWGUI.Emulation.Contracts;
using GWGUI.Emulation.Enums;
using GWGUI.Emulation.Functions;

namespace GWGUI.Tests.Emulation;

public sealed class HardwareSettingsConstructionTests
{
    [Fact]
    public void HardwareFieldsKeepTheirIdentifiersChoicesResourcesAndEvaluationOrder()
    {
        var blocks = new List<EmulationSettingsBlock>();
        var events = new List<string>();
        string Read(string name, string value, int existingBlocks)
        {
            Assert.Equal(existingBlocks, blocks.Count);
            events.Add(name);
            return value;
        }
        EmulationSdk.HardwareSettings.AddBlocks(blocks, "bandai", "Emulation.Bandai.Cpu.V30Mz",
            () => Read("frequency", "3,072 MHz", 0), () => Read("main", "16 KiB", 1),
            () => Read("display", "80 B", 1), "Emulation.Bandai.Memory.Display",
            () => Read("resolution", "224 × 144", 2), isCompactCpu: false);
        Assert.Equal(new[] { "frequency", "main", "display", "resolution" }, events);
        Assert.Equal(new[] { EmulationMachineTab.Cpu, EmulationMachineTab.Ram, EmulationMachineTab.Video }, blocks.Select(block => block.Tab));
        Assert.All(blocks, block => Assert.Equal(block.Id, block.TitleResourceKey));
        Assert.Equal(new[] { "bandai_cpu_model", "bandai_cpu_frequency" }, blocks[0].Fields.Select(field => field.Id));
        Assert.Equal(new[] { "bandai_main_memory", "bandai_display_memory" }, blocks[1].Fields.Select(field => field.Id));
        Assert.Equal("Emulation.Bandai.Memory.Display", blocks[1].Fields[1].LabelResourceKey);
        Assert.Equal("bandai_display_resolution", Assert.Single(blocks[2].Fields).Id);
        Assert.Equal("224 × 144", blocks[2].Fields[0].Value);
        var model = blocks[0].Fields[0];
        var choice = Assert.Single(model.Choices!);
        Assert.Equal("Emulation.Bandai.Cpu.V30Mz", model.Value);
        Assert.Equal(model.Value, choice.Id);
        Assert.Equal(model.Value, choice.DisplayResourceKey);
        Assert.False(blocks[0].IsCompact);
        Assert.All(blocks.SelectMany(block => block.Fields), field => Assert.Equal(EmulationSettingsEditor.Information, field.Editor));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void AbsentOptionalValuesLeaveOnlyTheCpuModelAndKeepCompactPolicy(bool compact)
    {
        var blocks = new List<EmulationSettingsBlock>();
        EmulationSdk.HardwareSettings.AddBlocks(blocks, "sharp", "cpu", () => null, () => null, () => null,
            "display", () => null, compact);
        var block = Assert.Single(blocks);
        Assert.Equal(compact, block.IsCompact);
        Assert.Equal("sharp_cpu_model", Assert.Single(block.Fields).Id);
    }

    [Theory]
    [InlineData("frequency", 0)]
    [InlineData("main", 1)]
    [InlineData("display", 1)]
    [InlineData("resolution", 2)]
    public void DeferredFailureStopsBeforeLaterValuesAndKeepsAlreadyAddedBlocks(string failAt, int blockCount)
    {
        var blocks = new List<EmulationSettingsBlock>();
        var events = new List<string>();
        var error = new InvalidOperationException("hardware description failed");
        string Read(string name)
        {
            events.Add(name);
            if (name == failAt) throw error;
            return name;
        }
        Assert.Same(error, Assert.Throws<InvalidOperationException>(() =>
            EmulationSdk.HardwareSettings.AddBlocks(blocks, "philips", "cpu", () => Read("frequency"),
                () => Read("main"), () => Read("display"), "display", () => Read("resolution"), false)));
        Assert.Equal(blockCount, blocks.Count);
        Assert.Equal(failAt, events[^1]);
    }

    [Theory]
    [InlineData(-1, false, "-0,000001 MHz")]
    [InlineData(0, true, "0 kHz")]
    [InlineData(32768, false, "0,032768 MHz")]
    [InlineData(32768, true, "32,768 kHz")]
    [InlineData(999999, true, "999,999 kHz")]
    [InlineData(1000000, true, "1 MHz")]
    [InlineData(3072000, true, "3,072 MHz")]
    public void FrequencyFormattingKeepsTheModuleUnitPolicyAndCurrentCulture(long hertz, bool useKilohertz, string expected)
        => InFrenchCulture(() => Assert.Equal(expected, EmulationSdk.HardwareSettings.FormatFrequency(hertz, useKilohertz)));

    [Theory]
    [InlineData(1000000L, "0.00", "1,00 MHz")]
    [InlineData(1000000L, "0.##", "1 MHz")]
    [InlineData(1000000L, "0.######", "1 MHz")]
    [InlineData(32768L, "0.00", "0,03 MHz")]
    [InlineData(32768L, "0.##", "0,03 MHz")]
    [InlineData(32768L, "0.######", "0,032768 MHz")]
    public void MegahertzDisplayKeepsFixedAndOptionalDecimalPlaces(long hertz, string format, string expected)
        => InFrenchCulture(() => Assert.Equal(expected, EmulationSdk.HardwareSettings.FormatMegahertz(hertz, format)));

    [Theory]
    [InlineData(-1, "-1 B")]
    [InlineData(0, "0 B")]
    [InlineData(1023, "1023 B")]
    [InlineData(1024, "1 KiB")]
    [InlineData(1536, "1 KiB")]
    [InlineData(65536, "64 KiB")]
    public void MemoryFormattingKeepsInvariantIntegersAndExistingKibibyteTruncation(long bytes, string expected)
        => InFrenchCulture(() => Assert.Equal(expected, EmulationSdk.HardwareSettings.FormatMemory(bytes)));

    private static void InFrenchCulture(Action action)
    {
        var previous = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("fr-FR");
            action();
        }
        finally { CultureInfo.CurrentCulture = previous; }
    }
}
