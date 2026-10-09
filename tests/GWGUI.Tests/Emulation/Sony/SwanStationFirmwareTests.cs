using GWGUI.Emulation.Sony.Common.Machines.Common.Contracts;
using GWGUI.Emulation.Sony.Emulators.SwanStation.Constants;
using GWGUI.Emulation.Sony.Emulators.SwanStation.Dictionaries;
using GWGUI.Emulation.Sony.Emulators.SwanStation.Functions;

namespace GWGUI.Tests.Emulation.Sony;

public sealed class SwanStationFirmwareTests
{
    [Theory]
    [InlineData(FirmwareConstants.JapanField, FirmwareConstants.JapanPathKey, FirmwareConstants.JapanFile)]
    [InlineData(FirmwareConstants.NorthAmericaField, FirmwareConstants.NorthAmericaPathKey, FirmwareConstants.NorthAmericaFile)]
    [InlineData(FirmwareConstants.EuropeField, FirmwareConstants.EuropePathKey, FirmwareConstants.EuropeFile)]
    public void RegionalBiosUsesTheSelectedRomRegardlessOfItsOriginalName(
        string field, string nativeKey, string nativeFile)
    {
        var configuration = Configuration(field);
        var native = SwanStationOptionFunctions.ToNative(configuration);
        Assert.Equal(nativeFile, native.Options![nativeKey]);
        Assert.Equal(configuration.FirmwarePaths, native.FirmwarePaths);
    }

    [Theory]
    [InlineData(FirmwareConstants.PspField, FirmwareConstants.PspFile)]
    [InlineData(FirmwareConstants.Ps3Field, FirmwareConstants.Ps3File)]
    public void AlternativeBiosIsUsedForAllRegions(string field, string nativeFile)
    {
        var native = SwanStationOptionFunctions.ToNative(Configuration(field));
        Assert.All(FirmwareConstants.Regions.Values,
            region => Assert.Equal(nativeFile, native.Options![region.PathKey]));
    }

    [Fact]
    public void BiosPathsAreProvidedByRomSelectionRatherThanOptionChoices()
    {
        Assert.DoesNotContain(OptionCatalog.All,
            option => FirmwareConstants.Regions.Values.Any(region => region.PathKey == option.Key));
    }

    private static MachineConfiguration Configuration(string field) =>
        new(GWGUI.Emulation.Sony.Common.Machines.Common.Constants.ModelConstants.PlayStation,
            CoreConstants.Id, FirmwarePaths: new Dictionary<string, string>
            {
                [field] = @"C:\mes-roms\mon-bios-personnel.bin",
            });
}
