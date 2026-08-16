using System.Globalization;
using GWGUI.App.Localization;
using GWGUI.Emulation.Atari;

namespace GWGUI.Tests;

public sealed class AtariStModelCatalogTests
{
    public static TheoryData<AtariMachineModel, string, AtariStCpu, int, int, string> Models => new()
    {
        { AtariMachineModel.St, AtariStModelConstants.StMachineId, AtariStCpu.Motorola68000,
            AtariStModelConstants.BaseCpuFrequencyMhz, AtariStModelConstants.FourMibibytesKib,
            AtariStModelConstants.Tos100 },
        { AtariMachineModel.Stf, AtariStModelConstants.StMachineId, AtariStCpu.Motorola68000,
            AtariStModelConstants.BaseCpuFrequencyMhz, AtariStModelConstants.FourMibibytesKib,
            AtariStModelConstants.Tos102 },
        { AtariMachineModel.Stfm, AtariStModelConstants.StMachineId, AtariStCpu.Motorola68000,
            AtariStModelConstants.BaseCpuFrequencyMhz, AtariStModelConstants.FourMibibytesKib,
            AtariStModelConstants.Tos104 },
        { AtariMachineModel.MegaSt, AtariStModelConstants.StMachineId, AtariStCpu.Motorola68000,
            AtariStModelConstants.BaseCpuFrequencyMhz, AtariStModelConstants.FourMibibytesKib,
            AtariStModelConstants.Tos104 },
        { AtariMachineModel.Ste, AtariStModelConstants.SteMachineId, AtariStCpu.Motorola68000,
            AtariStModelConstants.BaseCpuFrequencyMhz, AtariStModelConstants.FourMibibytesKib,
            AtariStModelConstants.Tos162 },
        { AtariMachineModel.MegaSte, AtariStModelConstants.SteMachineId, AtariStCpu.Motorola68000,
            AtariStModelConstants.EnhancedCpuFrequencyMhz, AtariStModelConstants.EightMibibytesKib,
            AtariStModelConstants.Tos206 },
        { AtariMachineModel.Tt, AtariStModelConstants.TtMachineId, AtariStCpu.Motorola68030,
            AtariStModelConstants.TtCpuFrequencyMhz, AtariStModelConstants.EightMibibytesKib,
            AtariStModelConstants.Tos306 },
        { AtariMachineModel.Falcon, AtariStModelConstants.FalconMachineId, AtariStCpu.Motorola68030,
            AtariStModelConstants.EnhancedCpuFrequencyMhz, AtariStModelConstants.FourteenMibibytesKib,
            AtariStModelConstants.Tos404 }
    };

    [Theory]
    [MemberData(nameof(Models))]
    public void EveryModelHasACompleteCompatibleHardwareDefinition(AtariMachineModel model,
        string technicalMachineId, AtariStCpu defaultCpu, int expectedFrequency, int maximumMemoryKib,
        string compatibleTos)
    {
        var definition = AtariStModelCatalog.Get(model);

        Assert.Equal(model, definition.Model);
        Assert.Equal(technicalMachineId, definition.TechnicalMachineId);
        Assert.Equal(defaultCpu, definition.DefaultCpu);
        Assert.Equal(AtariStFpu.None, definition.DefaultFpu);
        Assert.Contains(defaultCpu, definition.Cpus);
        Assert.Contains(expectedFrequency, definition.CpuFrequenciesMhz);
        Assert.Equal(expectedFrequency, definition.DefaultCpuFrequencyMhz);
        Assert.Equal(AtariStCpuPrecision.Compatible, definition.DefaultCpuPrecision);
        Assert.Contains(AtariStCpuPrecision.Compatible, definition.CpuPrecisions);
        Assert.Contains(AtariStCpuPrecision.CycleExact, definition.CpuPrecisions);
        Assert.Equal(maximumMemoryKib, definition.MainMemoryKib.Max());
        Assert.Contains(compatibleTos, definition.TosVersions);
        Assert.Equal(definition.TosVersions.Last(), definition.RecommendedTosVersion);
        Assert.Equal(AtariStRegion.UnitedStates, definition.DefaultRegion);
        Assert.Equal(Enum.GetValues<AtariStRegion>().Order(), definition.Regions.Order());
        Assert.NotEmpty(definition.Video);
        Assert.NotEmpty(definition.Audio);
        Assert.NotEmpty(definition.Storage);
        Assert.NotEmpty(definition.Ports);
        Assert.Equal(definition.Cpus.Count, definition.Cpus.Distinct().Count());
        Assert.Equal(definition.Fpus.Count, definition.Fpus.Distinct().Count());
        Assert.Equal(definition.MainMemoryKib.Count, definition.MainMemoryKib.Distinct().Count());
        Assert.Equal(definition.TosVersions.Count, definition.TosVersions.Distinct(StringComparer.Ordinal).Count());
    }

    [Fact]
    public void LaterMachinesExposeTheirDistinctHardware()
    {
        var megaSte = AtariStModelCatalog.Get(AtariMachineModel.MegaSte);
        var tt = AtariStModelCatalog.Get(AtariMachineModel.Tt);
        var falcon = AtariStModelCatalog.Get(AtariMachineModel.Falcon);

        Assert.Contains(AtariStStorageCapability.FloppyHighDensity, megaSte.Storage);
        Assert.Contains(AtariStPortCapability.LocalAreaNetwork, megaSte.Ports);
        Assert.Contains(AtariStPortCapability.Vme, tt.Ports);
        Assert.Contains(AtariStStorageCapability.Scsi, tt.Storage);
        Assert.Contains(AtariStVideoCapability.TtShifter, tt.Video);
        Assert.Contains(AtariStVideoCapability.Videl, falcon.Video);
        Assert.Contains(AtariStAudioCapability.DigitalSignalProcessor, falcon.Audio);
        Assert.Contains(AtariStAudioCapability.Microphone, falcon.Audio);
        Assert.Contains(AtariStFpu.Motorola68882, tt.Fpus);
        Assert.Contains(AtariStFpu.Motorola68882, falcon.Fpus);
        Assert.Equal(AtariStModelConstants.OneThousandTwentyFourMibibytes,
            tt.AlternateMemoryMib.Max());
        Assert.Equal(AtariStModelConstants.AlternateMemoryStepMib,
            tt.AlternateMemoryMib[1] - tt.AlternateMemoryMib[0]);
    }

    [Fact]
    public void NonStModelsAreRejected()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            AtariStModelCatalog.Get(AtariMachineModel.Atari2600));
    }

    [Fact]
    public void EveryDisplayNameIsAvailableInEverySupportedLanguage()
    {
        var previousCulture = CultureInfo.CurrentCulture;
        var previousUiCulture = CultureInfo.CurrentUICulture;
        try
        {
            foreach (var language in UiLanguageCatalog.Available)
            {
                var culture = UiLanguageResolver.GetUiCulture(language.Code);
                CultureInfo.CurrentCulture = culture;
                CultureInfo.CurrentUICulture = culture;
                foreach (var definition in AtariStModelCatalog.All)
                    Assert.DoesNotContain('[', LocExtension.Get(definition.DisplayNameResourceKey));
            }
        }
        finally
        {
            CultureInfo.CurrentCulture = previousCulture;
            CultureInfo.CurrentUICulture = previousUiCulture;
        }
    }
}
