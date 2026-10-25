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
    public static IEnumerable<object[]> MachineAdapters => EmulatorCatalog.CreateAdapters().SelectMany(adapter => adapter.Definition.MachineIds
        .Select(Enum.Parse<MachineModel>).Distinct().OrderBy(model => model).Select(model => new object[] { model, Enum.Parse<Emulator>(adapter.EmulatorKey) }));

    [Fact]
    public void SdkDefinitionsKeepCatalogIdentityCompatibilityAndSummaryValues()
    {
        var adapters = EmulatorCatalog.CreateAdapters();
        Assert.NotEmpty(adapters);
        Assert.Equal(adapters.Count, EmulatorCatalog.All.Count);
        Assert.Equal(EmulatorCatalog.All.Select(definition => definition.Id).Order(StringComparer.Ordinal), EmulatorCatalog.All.Select(definition => definition.Id));
        foreach (var adapter in adapters)
        {
            var emulator = Enum.Parse<Emulator>(adapter.EmulatorKey);
            var expected = adapter.Definition;
            var definition = EmulatorCatalog.Get(emulator);
            Assert.Same(definition, Assert.Single(EmulatorCatalog.All, item => item.Id == expected.Id));
            Assert.Equal(expected.DisplayName, definition.DisplayName);
            Assert.Equal(expected.DescriptionResourceKey, definition.DescriptionResourceKey);
            Assert.True(expected.MachineIds.SetEquals(definition.MachineIds));
            foreach (var model in Enum.GetValues<MachineModel>())
            {
                var compatible = expected.MachineIds.Select(Enum.Parse<MachineModel>).Contains(model);
                Assert.Equal(compatible, ConfigurationFunctions.IsCompatible(model, emulator));
                if (compatible)
                    Assert.Equal(expected.DisplayName, Assert.Single(ConfigurationSummaryFunctions.Create(new MachineConfiguration(model, core: emulator)).Details));
            }
        }
        Assert.True(ConfigurationFunctions.IsCompatible(MachineModel.NeoGeoAes, Emulator.Geolith));
        Assert.True(ConfigurationFunctions.IsCompatible(MachineModel.NeoGeoCd, Emulator.Geolith));
        Assert.False(ConfigurationFunctions.IsCompatible(MachineModel.NeoGeoPocket, Emulator.Geolith));
        Assert.True(ConfigurationFunctions.IsCompatible(MachineModel.NeoGeoPocketColor, Emulator.MednafenNgp));
        Assert.False(ConfigurationFunctions.IsCompatible(MachineModel.NeoGeoAes, Emulator.MednafenNgp));
        Assert.True(ConfigurationFunctions.IsCompatible(MachineModel.NeoGeoCdz, Emulator.NeoCd));
        Assert.False(ConfigurationFunctions.IsCompatible(MachineModel.NeoGeoMvs, Emulator.NeoCd));
        foreach (var model in Enum.GetValues<MachineModel>())
        {
            Assert.True(ConfigurationFunctions.IsCompatible(model, Emulator.None));
            Assert.Empty(ConfigurationSummaryFunctions.Create(new MachineConfiguration(model, core: Emulator.None)).Details);
        }
        Assert.False(ConfigurationFunctions.IsCompatible((MachineModel)int.MaxValue, Emulator.None));
        Assert.False(ConfigurationFunctions.IsCompatible(MachineModel.NeoGeoAes, (Emulator)int.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(() => EmulatorCatalog.Get(Emulator.None));
        Assert.Throws<ArgumentOutOfRangeException>(() => EmulatorCatalog.CreateAdapter(Emulator.None));
    }

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
        var localization = GWGUI.Emulation.Snk.Common.Functions.CoreHostFunctions.CreateLocalization(typeof(SnkEmulationModule).Assembly, "GWGUI.Emulation.Snk.Resources.Emulation");
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
