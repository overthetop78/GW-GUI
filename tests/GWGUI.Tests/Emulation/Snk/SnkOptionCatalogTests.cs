using System.Globalization;
using GWGUI.Emulation.Services;
using GWGUI.Emulation.Snk.Common.Dictionaries;
using GWGUI.Emulation.Snk.Common.Machines.Common.Contracts;
using GWGUI.Emulation.Snk.Common.Machines.Common.Enums;
using GWGUI.Emulation.Snk.Common.Machines.Common.Functions;
using GWGUI.Emulation.Snk.Modules;

namespace GWGUI.Tests.Emulation.Snk;

public sealed class SnkOptionCatalogTests
{
    public static IEnumerable<object[]> MachineAdapters => EmulatorCatalog.All.SelectMany(entry => entry.Models.OrderBy(model => model).Select(model => new object[] { model, entry.Emulator }));

    [Theory]
    [MemberData(nameof(MachineAdapters))]
    public void AdvertisedAdapterOptionsBuildCompleteCommonDescriptions(MachineModel model, Emulator emulator)
    {
        var configuration = new MachineConfiguration(model, core: emulator);
        var options = CoreOptionValidationFunctions.ForConfiguration(configuration);
        Assert.Equal(options.Count, options.Select(option => option.Key).Distinct(StringComparer.Ordinal).Count());
        Assert.All(options, option => Assert.True(option.Values.Any(value => value.Value == option.DefaultValue), $"Invalid default {option.Key}: {option.DefaultValue}"));
        var defaults = options.ToDictionary(option => option.Key, option => option.DefaultValue, StringComparer.Ordinal);
        CoreOptionValidationFunctions.Validate(configuration with { Options = defaults });
        var blocks = SettingsDescriptionFunctions.Create(configuration);
        Assert.True(blocks.Count == blocks.Select(block => block.Id).Distinct(StringComparer.Ordinal).Count(), string.Join(", ", blocks.GroupBy(block => block.Id).Where(group => group.Count() > 1).Select(group => $"{group.Key}: {string.Join("/", group.Select(block => block.Tab))}")));
        var localization = new EmulationModuleLocalization(typeof(SnkEmulationModule).Assembly, "GWGUI.Emulation.Snk.Resources.Emulation");
        void Resolve(string? key)
        {
            if (key is null || !key.StartsWith("Emulation.Snk.", StringComparison.Ordinal)) return;
            Assert.True(localization.TryGetString(key, CultureInfo.InvariantCulture, out var value), $"Missing base resource {key} for {model}/{emulator}");
            Assert.False(string.IsNullOrWhiteSpace(value));
        }
        foreach (var block in blocks)
        {
            Resolve(block.TitleResourceKey);
            Assert.Equal(block.Fields.Count, block.Fields.Select(field => field.Id).Distinct(StringComparer.Ordinal).Count());
            foreach (var field in block.Fields)
            {
                Resolve(field.LabelResourceKey);
                Resolve(field.ExplanationResourceKey);
                Resolve(field.DetailedExplanationResourceKey);
                foreach (var choice in field.Choices ?? []) Resolve(choice.DisplayResourceKey);
            }
        }
    }
}
