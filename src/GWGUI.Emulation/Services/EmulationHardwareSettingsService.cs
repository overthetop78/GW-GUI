using GWGUI.Emulation.Contracts;
using GWGUI.Emulation.Enums;
using GWGUI.Emulation.Functions;
using GWGUI.Emulation.Interfaces;

namespace GWGUI.Emulation.Services;

/// <summary>Relie les réglages matériels aux constructeurs et formats existants sous leur contrat public.</summary>
internal sealed class EmulationHardwareSettingsService : IEmulationHardwareSettings
{
    /// <inheritdoc/>
    public void AddBlocks(List<EmulationSettingsBlock> blocks, string fieldPrefix, string cpuResourceKey,
        Func<string?> cpuFrequency, Func<string?> mainMemory, Func<string?> displayMemory,
        string displayMemoryResourceKey, Func<string?> resolution, bool isCompactCpu)
        => EmulationHardwareSettingsFunctions.AddBlocks(blocks, fieldPrefix, cpuResourceKey, cpuFrequency, mainMemory, displayMemory, displayMemoryResourceKey, resolution, isCompactCpu);

    /// <inheritdoc/>
    public EmulationSettingsField AudioOutput(string id, string block, string label, string value, IReadOnlyList<EmulationSettingsChoice> choices, string? shortHelp, string? detailedHelp)
        => EmulationHardwareSettingsFunctions.AudioOutput(id, block, label, value, choices, shortHelp, detailedHelp);

    /// <inheritdoc/>
    public EmulationSettingsField AudioSelection(IReadOnlyDictionary<string, string> options, string id, string block, string resource, int fallback, IReadOnlyList<int> values, string unit, IFormatProvider? labelFormatProvider)
        => EmulationHardwareSettingsFunctions.AudioSelection(options, id, block, resource, fallback, values, unit, labelFormatProvider);

    /// <inheritdoc/>
    public EmulationSettingsBlock Block(string id, EmulationMachineTab tab, string title, string icon, int columns, EmulationSettingsField[] fields, bool isCompact = false)
        => EmulationHardwareSettingsFunctions.Block(id, tab, title, icon, columns, fields, isCompact);

    /// <inheritdoc/>
    public string FormatFrequency(long hertz, bool useKilohertz = false)
        => EmulationHardwareSettingsFunctions.FormatFrequency(hertz, useKilohertz);

    /// <inheritdoc/>
    public string FormatMegahertz(long hertz, string format)
        => EmulationHardwareSettingsFunctions.FormatMegahertz(hertz, format);

    /// <inheritdoc/>
    public string FormatMemory(long bytes)
        => EmulationHardwareSettingsFunctions.FormatMemory(bytes);

    /// <inheritdoc/>
    public EmulationSettingsField Information(string id, EmulationMachineTab tab, string block, string label, string value, string? shortHelp, string? detailedHelp)
        => EmulationHardwareSettingsFunctions.Information(id, tab, block, label, value, shortHelp, detailedHelp);

    /// <inheritdoc/>
    public EmulationSettingsChoice InvariantChoice(string id, string text, long? numericValue = null)
        => EmulationHardwareSettingsFunctions.InvariantChoice(id, text, numericValue);

    /// <inheritdoc/>
    public EmulationSettingsField Number(string id, EmulationMachineTab tab, string block, string label, string value, string? shortHelp, string? detailedHelp)
        => EmulationHardwareSettingsFunctions.Number(id, tab, block, label, value, shortHelp, detailedHelp);

    /// <inheritdoc/>
    public EmulationSettingsField Select(string id, EmulationMachineTab tab, string block, string label, string value,
        IReadOnlyList<EmulationSettingsChoice> choices, string? shortHelp, string? detailedHelp,
        bool isEnabled = true, bool requiresRestart = false)
        => EmulationHardwareSettingsFunctions.Select(id, tab, block, label, value, choices, shortHelp, detailedHelp, isEnabled, requiresRestart);

    /// <inheritdoc/>
    public EmulationSettingsField Toggle(string id, EmulationMachineTab tab, string block, string label, bool value, string? shortHelp, string? detailedHelp, bool refreshSettingsOnChange = false)
        => EmulationHardwareSettingsFunctions.Toggle(id, tab, block, label, value, shortHelp, detailedHelp, refreshSettingsOnChange);
}
