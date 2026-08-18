using System.Globalization;
using System.Windows;
using System.Windows.Threading;
using GWGUI.App.Controls;
using GWGUI.Emulation;
using GWGUI.Emulation.Atari;

namespace GWGUI.Tests;

[Collection(AtariNativeCoreTestConstants.CollectionName)]
public sealed class AtariVideoAudioSettingsTests
{
    public static TheoryData<AtariMachineModel> EveryModel => new(Enum.GetValues<AtariMachineModel>());

    [Theory]
    [MemberData(nameof(EveryModel))]
    public void EveryModelBuildsVideoAndAudioChoicesFromItsCatalog(AtariMachineModel model)
    {
        var configuration = new AtariMachineConfiguration(model);
        var view = AtariVideoAudioSettingsFunctions.Create(configuration);

        Assert.NotEmpty(view.Standards);
        Assert.NotEmpty(view.Regions);
        Assert.NotEmpty(view.Resolutions);
        Assert.NotEmpty(view.AspectRatios);
        Assert.NotEmpty(view.Cropping);
        Assert.NotEmpty(view.FrameSkips);
        Assert.NotEmpty(view.Renderers);
        Assert.NotEmpty(view.Outputs);
        Assert.NotEmpty(view.Latencies);
        Assert.NotEmpty(view.Volumes);
        Assert.NotEmpty(view.Qualities);
        Assert.Equal(configuration.VideoRenderer, view.Renderer);
        Assert.Equal(Enum.GetValues<EmulationVideoRenderer>().Order(),
            view.Renderers.Select(value => Enum.Parse<EmulationVideoRenderer>(value.Value)).Order());
    }

    [Fact]
    public void ApplyPersistsFrontendValuesAndPreservesUnknownOptions()
    {
        var source = new AtariMachineConfiguration(AtariMachineModel.St,
            options: new Dictionary<string, string>
            {
                [AtariVideoAudioSettingsTestConstants.UnknownKey] = AtariVideoAudioSettingsTestConstants.UnknownValue
            });
        var displayed = new Dictionary<string, string>
        {
            [AtariVideoAudioSettingsConstants.FrameSkipOptionKey] = AtariVideoAudioSettingsTestConstants.FrameSkip
        };

        var result = AtariVideoAudioSettingsFunctions.Apply(source, displayed, false, EmulationVideoRenderer.Wpf);

        Assert.False(result.AudioEnabled);
        Assert.Equal(EmulationVideoRenderer.Wpf, result.VideoRenderer);
        Assert.Equal(AtariVideoAudioSettingsTestConstants.FrameSkip,
            result.Options[AtariVideoAudioSettingsConstants.FrameSkipOptionKey]);
        Assert.Equal(AtariVideoAudioSettingsTestConstants.UnknownValue,
            result.Options[AtariVideoAudioSettingsTestConstants.UnknownKey]);
    }

    [Fact]
    public void InvalidPersistedValueFallsBackWithoutDeletingStoredValue()
    {
        var choices = new[] { new AtariVideoAudioChoice(AtariVideoAudioSettingsTestConstants.ValidValue,
            AtariVideoAudioSettingsTestConstants.ValidValue) };
        var options = new Dictionary<string, string>
        {
            [AtariVideoAudioSettingsTestConstants.OptionKey] = AtariVideoAudioSettingsTestConstants.UnknownValue
        };

        var selected = AtariVideoAudioSettingsFunctions.Select(options,
            AtariVideoAudioSettingsTestConstants.OptionKey, choices, AtariVideoAudioSettingsTestConstants.ValidValue);

        Assert.Equal(AtariVideoAudioSettingsTestConstants.ValidValue, selected);
        Assert.Equal(AtariVideoAudioSettingsTestConstants.UnknownValue,
            options[AtariVideoAudioSettingsTestConstants.OptionKey]);
    }

    [Theory]
    [InlineData("en-US", AtariStRegion.UnitedStates)]
    [InlineData("en-GB", AtariStRegion.UnitedKingdom)]
    [InlineData("fr-FR", AtariStRegion.France)]
    [InlineData("de-CH", AtariStRegion.Switzerland)]
    [InlineData("ja-JP", AtariStRegion.Multilingual)]
    public void StRegionFollowsTheApplicationCulture(string cultureName, AtariStRegion expected)
    {
        var previous = CultureInfo.CurrentUICulture;
        try
        {
            CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo(cultureName);
            var view = AtariVideoAudioSettingsFunctions.Create(
                new AtariMachineConfiguration(AtariMachineModel.St));

            Assert.Equal(expected.ToString(),
                AtariVideoAudioSettingsFunctions.PreferredRegion(AtariMachineModel.St, view.Regions));
        }
        finally
        {
            CultureInfo.CurrentUICulture = previous;
        }
    }

    [Theory]
    [InlineData(AtariMachineModel.Atari400, "fr-FR", AtariClassicRegion.Pal)]
    [InlineData(AtariMachineModel.Atari800, "de-DE", AtariClassicRegion.Pal)]
    [InlineData(AtariMachineModel.Atari800Xl, "pt-PT", AtariClassicRegion.Pal)]
    [InlineData(AtariMachineModel.Atari130Xe, "en-US", AtariClassicRegion.Ntsc)]
    [InlineData(AtariMachineModel.Xegs, "ja-JP", AtariClassicRegion.Ntsc)]
    [InlineData(AtariMachineModel.Atari5200, "fr-FR", AtariClassicRegion.Pal)]
    [InlineData(AtariMachineModel.Atari2600, "en-US", AtariClassicRegion.Ntsc)]
    [InlineData(AtariMachineModel.Atari7800, "de-DE", AtariClassicRegion.Pal)]
    [InlineData(AtariMachineModel.Jaguar, "fr-FR", AtariClassicRegion.Pal)]
    [InlineData(AtariMachineModel.Jaguar, "zh-Hans", AtariClassicRegion.Ntsc)]
    [InlineData(AtariMachineModel.JaguarCd, "pt-BR", AtariClassicRegion.Ntsc)]
    public void CultureSelectedVideoStandardFollowsTheApplicationCulture(AtariMachineModel model,
        string cultureName, AtariClassicRegion expected)
    {
        var previous = CultureInfo.CurrentUICulture;
        try
        {
            CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo(cultureName);
            var view = AtariVideoAudioSettingsFunctions.Create(
                new AtariMachineConfiguration(model));

            Assert.Equal(expected.ToString(),
                AtariVideoAudioSettingsFunctions.PreferredVideoStandard(model, view.Standards));
            Assert.Equal(expected.ToString(),
                AtariVideoAudioSettingsFunctions.PreferredRegion(model, view.Regions));
        }
        finally
        {
            CultureInfo.CurrentUICulture = previous;
        }
    }

    [Fact]
    public void EveryClassicMachineWithPalAndNtscFollowsTheApplicationCulture()
    {
        var previous = CultureInfo.CurrentUICulture;
        try
        {
            var models = AtariClassicModelCatalog.All
                .Where(definition => definition.Regions.Contains(AtariClassicRegion.Pal)
                    && definition.Regions.Contains(AtariClassicRegion.Ntsc))
                .Select(definition => definition.Model)
                .ToArray();

            foreach (var model in models)
            {
                var view = AtariVideoAudioSettingsFunctions.Create(new AtariMachineConfiguration(model));
                CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("fr-FR");
                Assert.Equal("PAL", view.Standards.Single(choice =>
                    choice.Value == AtariClassicRegion.Pal.ToString()).DisplayName);
                Assert.Equal("NTSC", view.Standards.Single(choice =>
                    choice.Value == AtariClassicRegion.Ntsc.ToString()).DisplayName);
                Assert.Equal("PAL", view.Regions.Single(choice =>
                    choice.Value == AtariClassicRegion.Pal.ToString()).DisplayName);
                Assert.Equal("NTSC", view.Regions.Single(choice =>
                    choice.Value == AtariClassicRegion.Ntsc.ToString()).DisplayName);
                Assert.Equal(AtariClassicRegion.Pal.ToString(),
                    AtariVideoAudioSettingsFunctions.PreferredVideoStandard(model, view.Standards));
                Assert.Equal(AtariClassicRegion.Pal.ToString(),
                    AtariVideoAudioSettingsFunctions.PreferredRegion(model, view.Regions));

                CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("en-US");
                Assert.Equal(AtariClassicRegion.Ntsc.ToString(),
                    AtariVideoAudioSettingsFunctions.PreferredVideoStandard(model, view.Standards));
                Assert.Equal(AtariClassicRegion.Ntsc.ToString(),
                    AtariVideoAudioSettingsFunctions.PreferredRegion(model, view.Regions));
            }
        }
        finally
        {
            CultureInfo.CurrentUICulture = previous;
        }
    }

    [Fact]
    public void CultureBasedVideoStandardDoesNotReplaceTheStSpecificDefault()
    {
        var previous = CultureInfo.CurrentUICulture;
        try
        {
            CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("fr-FR");
            var view = AtariVideoAudioSettingsFunctions.Create(
                new AtariMachineConfiguration(AtariMachineModel.St));

            Assert.Equal(view.Standards.First().Value,
                AtariVideoAudioSettingsFunctions.PreferredVideoStandard(AtariMachineModel.St,
                    view.Standards));
        }
        finally
        {
            CultureInfo.CurrentUICulture = previous;
        }
    }

    [Fact]
    public void LynxRemainsRegionFreeForEveryApplicationCulture()
    {
        var previous = CultureInfo.CurrentUICulture;
        try
        {
            CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("fr-FR");
            var view = AtariVideoAudioSettingsFunctions.Create(
                new AtariMachineConfiguration(AtariMachineModel.Lynx));

            Assert.Single(view.Standards);
            Assert.Equal(AtariClassicRegion.RegionFree.ToString(), view.Standards[0].Value);
            Assert.Equal(view.Standards[0].Value,
                AtariVideoAudioSettingsFunctions.PreferredVideoStandard(AtariMachineModel.Lynx,
                    view.Standards));
        }
        finally
        {
            CultureInfo.CurrentUICulture = previous;
        }
    }

    [Fact]
    public void ControlsCanLoadTheSameConfigurationRepeatedly()
    {
        WpfTestHost.Run(() =>
        {
            var section = new AtariVideoAudioSettingsSection();
            var configuration = new AtariMachineConfiguration(AtariMachineModel.St);
            section.Load(configuration);
            section.Load(configuration);
        });
    }
}

internal static class AtariVideoAudioSettingsTestConstants
{
    internal const string UnknownKey = "future_video_option";
    internal const string UnknownValue = "preserved";
    internal const string FrameSkip = "2";
    internal const string OptionKey = "option";
    internal const string ValidValue = "valid";
}
