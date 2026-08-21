using System.Globalization;
using GWGUI.App.Localization;
using GWGUI.Emulation.Atari;

namespace GWGUI.Tests;

public sealed class AtariCompatibilityCatalogTests
{
    public static TheoryData<AtariMachineModel> Models => new()
    {
        AtariMachineModel.St,
        AtariMachineModel.Stf,
        AtariMachineModel.Stfm,
        AtariMachineModel.MegaSt,
        AtariMachineModel.Ste,
        AtariMachineModel.MegaSte,
        AtariMachineModel.Tt,
        AtariMachineModel.Falcon,
        AtariMachineModel.Atari400,
        AtariMachineModel.Atari800,
        AtariMachineModel.Atari800Xl,
        AtariMachineModel.Atari130Xe,
        AtariMachineModel.XlXe,
        AtariMachineModel.Xegs,
        AtariMachineModel.Atari5200,
        AtariMachineModel.Atari2600,
        AtariMachineModel.Atari7800,
        AtariMachineModel.Lynx,
        AtariMachineModel.Jaguar,
        AtariMachineModel.JaguarCd
    };

    [Theory]
    [MemberData(nameof(Models))]
    public void EveryModelDeclaresAllVisibleSectionsAndOptionStates(AtariMachineModel model)
    {
        var definition = AtariCompatibilityCatalog.Get(model);

        Assert.Equal(model, definition.Model);
        Assert.Equal(AtariConfigurationFunctions.GetCore(model), definition.Core);
        Assert.Equal(definition.VisibleTabs.Distinct().Count(), definition.VisibleTabs.Count);
        Assert.All(definition.VisibleTabs, tab => Assert.Contains(tab, Enum.GetValues<AtariSettingsTab>()));
        Assert.Contains(AtariSettingsTab.General, definition.VisibleTabs);
        Assert.Equal(Enum.GetValues<AtariSettingsGroup>().Order(), definition.VisibleGroups.Order());
        Assert.Equal(Enum.GetValues<AtariSettingOption>().Order(),
            definition.Options.Select(rule => rule.Option).Order());
        Assert.All(definition.Options.Where(rule => rule.Availability is AtariOptionAvailability.Forced
                or AtariOptionAvailability.Unavailable),
            rule => Assert.False(string.IsNullOrWhiteSpace(rule.ExplanationResourceKey)));
        Assert.All(definition.Options.Where(rule => rule.Availability == AtariOptionAvailability.Forced),
            rule => Assert.False(string.IsNullOrWhiteSpace(rule.ForcedValue)));
        Assert.InRange(definition.ControllerPortCount, AtariCompatibilityConstants.OneControllerPort,
            AtariCompatibilityConstants.FourControllerPorts);
        Assert.Equal(definition.Media.Select(rule => rule.Category).Distinct().Count(), definition.Media.Count);
        Assert.All(definition.Media, rule => Assert.NotEmpty(rule.Slots));
        Assert.All(definition.Media.Where(rule => rule.Availability == AtariMediaAvailability.Unavailable),
            rule => Assert.False(string.IsNullOrWhiteSpace(rule.ExplanationResourceKey)));
    }

    [Theory]
    [MemberData(nameof(Models))]
    public void EveryUnavailableExplanationResolvesInEveryLanguage(AtariMachineModel model)
    {
        var previousCulture = CultureInfo.CurrentCulture;
        var previousUiCulture = CultureInfo.CurrentUICulture;
        try
        {
            var definition = AtariCompatibilityCatalog.Get(model);
            var keys = definition.Options
                .Where(rule => rule.Availability is AtariOptionAvailability.Forced
                    or AtariOptionAvailability.Unavailable)
                .Select(rule => rule.ExplanationResourceKey!)
                .Concat(definition.Media
                    .Where(rule => rule.Availability == AtariMediaAvailability.Unavailable)
                    .Select(rule => rule.ExplanationResourceKey!))
                .Distinct(StringComparer.Ordinal)
                .ToArray();
            foreach (var language in UiLanguageCatalog.Available)
            {
                var culture = UiLanguageResolver.GetUiCulture(language.Code);
                CultureInfo.CurrentCulture = culture;
                CultureInfo.CurrentUICulture = culture;
                Assert.All(keys, key => Assert.DoesNotContain('[', LocExtension.Get(key)));
            }
        }
        finally
        {
            CultureInfo.CurrentCulture = previousCulture;
            CultureInfo.CurrentUICulture = previousUiCulture;
        }
    }

    [Theory]
    [MemberData(nameof(Models))]
    public void EngineUsesTheCatalogControllerPortCount(AtariMachineModel model)
    {
        var portCount = AtariCompatibilityCatalog.Get(model).ControllerPortCount;
        _ = new AtariMachineConfiguration(model, input: new AtariInputConfiguration(Controllers:
        [
            new AtariControllerBinding(portCount - AtariCompatibilityConstants.OneControllerPort,
                AtariPeripheralCategory.Automatic)
        ]));

        Assert.Throws<ArgumentOutOfRangeException>(() => new AtariMachineConfiguration(model,
            input: new AtariInputConfiguration(Controllers:
            [
                new AtariControllerBinding(portCount, AtariPeripheralCategory.Automatic)
            ])));
    }

    [Fact]
    public void ModelCatalogIsExhaustiveAndUnique()
    {
        Assert.Equal(Enum.GetValues<AtariMachineModel>().Order(),
            AtariCompatibilityCatalog.All.Select(definition => definition.Model).Order());
        Assert.Equal(AtariCompatibilityCatalog.All.Count,
            AtariCompatibilityCatalog.All.Select(definition => definition.Model).Distinct().Count());
    }

    [Fact]
    public void ComputerAndConsoleInputRulesRemainDistinct()
    {
        var computer = AtariCompatibilityCatalog.Get(AtariMachineModel.Atari800Xl);
        var console = AtariCompatibilityCatalog.Get(AtariMachineModel.Atari2600);

        Assert.Equal(AtariOptionAvailability.Editable,
            computer.Options.Single(rule => rule.Option == AtariSettingOption.KeyboardMappings).Availability);
        Assert.Equal(AtariOptionAvailability.Unavailable,
            console.Options.Single(rule => rule.Option == AtariSettingOption.KeyboardMappings).Availability);
        Assert.Equal(AtariOptionAvailability.Hidden,
            console.Options.Single(rule => rule.Option == AtariSettingOption.MouseMappings).Availability);
        Assert.Equal(AtariOptionAvailability.Hidden,
            computer.Options.Single(rule => rule.Option == AtariSettingOption.MouseSpeed).Availability);
        var st = AtariCompatibilityCatalog.Get(AtariMachineModel.St);
        Assert.Equal(AtariOptionAvailability.Editable,
            st.Options.Single(rule => rule.Option == AtariSettingOption.MouseSpeed).Availability);
        Assert.Equal(AtariOptionAvailability.Editable,
            st.Options.Single(rule => rule.Option == AtariSettingOption.MouseMappings).Availability);
    }
}
