using GWGUI.App.Contracts.Emulation.Configurations;
using GWGUI.App.Constants.Emulation;
using GWGUI.App.Constants.Machine;
using GWGUI.App.Functions.Views.Emulation.Settings;
using GWGUI.App.Localization.Extensions;
using GWGUI.Emulation;

namespace GWGUI.App.Presenters.Emulation.Configurations;

internal static class EmulationConfigurationTablePresenter
{
    internal static IReadOnlyList<EmulationConfigurationTableRow> CreateRows(
        IEnumerable<(IEmulationModule Module, IEmulationConfiguration Configuration)> configurations) =>
        configurations.Select(item => CreateRow(item.Module, item.Configuration))
            .OrderBy(row => row.MachineName, StringComparer.CurrentCulture)
            .ToArray();

    private static EmulationConfigurationTableRow CreateRow(
        IEmulationModule module, IEmulationConfiguration configuration)
    {
        var machine = module.Machines.First(item => item.Id == configuration.MachineId);
        var settings = module.Describe(configuration.MachineId, configuration);
        var cpuField = settings.Blocks
            .Where(block => block.Tab == EmulationMachineTab.Cpu && block.IsVisible)
            .SelectMany(block => block.Fields)
            .FirstOrDefault(field => field.IsVisible
                && field.LabelResourceKey == EmulationHardwareSettingsConstants.CpuModelResourceKey);
        var cpu = cpuField is null ? string.Empty
            : EmulationSettingsValuePresentationFunctions.DisplayValue(cpuField);
        var ramFields = settings.Blocks
            .Where(block => block.Tab == EmulationMachineTab.Ram && block.IsVisible)
            .SelectMany(block => block.Fields)
            .Where(field => field.IsVisible)
            .ToArray();
        var totalRam = string.Empty;
        if (ramFields.Length != 0)
        {
            var bytes = ramFields.Sum(EmulationSettingsValuePresentationFunctions.DefaultNumericValue);
            var formatted = EmulationSettingsValuePresentationFunctions.FormatMemorySize(bytes);
            totalRam = string.Concat(formatted.Value,
                EmulationMemorySettingsConstants.ValueUnitSeparator, formatted.Unit);
        }


        return new EmulationConfigurationTableRow(
            module,
            configuration,
            LocExtension.Get(machine.DisplayResourceKey),
            cpu,
            totalRam,
            ReaderGlyphs(module, configuration),
            PeripheralGlyphs(module, configuration));
    }
    private static IReadOnlyList<string> ReaderGlyphs(
        IEmulationModule module, IEmulationConfiguration configuration)
    {
        if (module is not IEmulationStorageSettingsManager storageManager) return [];
        var storage = storageManager.DescribeStorageSettings(configuration);
        return storage.ConfiguredSlots
            .Select(slot => storage.AvailableDevices.First(device => device.Slot == slot).MediaType)
            .Select(ReaderGlyph)
            .ToArray();
    }

    private static string ReaderGlyph(EmulationMediaType mediaType) => mediaType switch
    {
        EmulationMediaType.Floppy => MachinePresentationConstants.FloppyGlyph,
        EmulationMediaType.HardDisk => MachinePresentationConstants.HardDiskGlyph,
        EmulationMediaType.CompactDisc => MachinePresentationConstants.CompactDiscGlyph,
        EmulationMediaType.Cassette => MachinePresentationConstants.CassetteGlyph,
        EmulationMediaType.Cartridge => MachinePresentationConstants.CartridgeGlyph,
        _ => throw new ArgumentOutOfRangeException(nameof(mediaType), mediaType, null)
    };

    private static IReadOnlyList<string> PeripheralGlyphs(
        IEmulationModule module, IEmulationConfiguration configuration)
    {
        if (module is not IEmulationInputSettingsManager inputManager) return [];
        var input = inputManager.DescribeInputSettings(configuration);
        var glyphs = new List<string>();
        if (input.Keyboard is not null) glyphs.Add(EmulationInputSettingsConstants.KeyboardIcon);
        if (input.Mouse is not null) glyphs.Add(EmulationInputSettingsConstants.MouseIcon);
        foreach (var port in input.ControllerPorts)
        {
            var choice = port.ControllerChoices.FirstOrDefault(item => item.Id == port.SelectedControllerId);
            if (choice is null
                || choice.DisplayResourceKey == EmulationInputSettingsConstants.NoneControllerResourceKey)
                continue;
            glyphs.Add(choice.Id == EmulationInputSettingsConstants.KeyboardControllerId
                ? EmulationInputSettingsConstants.KeyboardIcon
                : choice.Id == EmulationInputSettingsConstants.MouseControllerId
                    ? EmulationInputSettingsConstants.MouseIcon
                    : EmulationInputSettingsConstants.ControllerIcon);
        }
        return glyphs;
    }

}
