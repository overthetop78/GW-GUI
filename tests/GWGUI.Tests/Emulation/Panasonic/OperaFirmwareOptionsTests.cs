using System.IO;
using GWGUI.Emulation.Enums;
using GWGUI.Emulation.Panasonic.Common.Machines.Common.Contracts;
using GWGUI.Emulation.Panasonic.Common.Machines.Common.Enums;
using GWGUI.Emulation.Panasonic.Common.Machines.Common.Functions;
using GWGUI.Emulation.Panasonic.Emulators.Opera.Constants;
using GWGUI.Emulation.Panasonic.Emulators.Opera.Functions;

namespace GWGUI.Tests.Emulation.Panasonic;

public sealed class OperaFirmwareOptionsTests
{
    [Fact]
    public void MissingExternalRomSelectsEmbeddedBootInsteadOfAFileName()
    {
        var configuration = new MachineConfiguration(MachineModel.ThreeDO, core: Emulator.Opera,
            options: new Dictionary<string, string> { [RuntimeOptionConstants.Bios] = "panafz1.bin" });

        var options = RuntimeOptionFunctions.Prepare(configuration);

        Assert.Equal("opera-boot-internal", options[RuntimeOptionConstants.Bios]);
        Assert.Equal("disabled", options[RuntimeOptionConstants.Font]);
        Assert.Empty(configuration.Firmware.Images);
    }

    [Theory]
    [InlineData("panafz10.bin", "panafz10.bin")]
    [InlineData("custom.rom", "panafz1.bin")]
    public void ExternalRomKeepsItsNativeFileSelection(string sourceName, string expectedName)
    {
        var configuration = new MachineConfiguration(MachineModel.ThreeDO, core: Emulator.Opera,
            firmware: new FirmwareConfiguration(new Dictionary<FirmwareRole, string>
            {
                [FirmwareRole.SystemRom] = Path.Combine("firmware", sourceName)
            }));

        var options = RuntimeOptionFunctions.Prepare(configuration);

        Assert.Equal(expectedName, options[RuntimeOptionConstants.Bios]);
        Assert.Equal(Path.Combine("firmware", sourceName), configuration.Firmware.Images[FirmwareRole.SystemRom]);
    }

    [Fact]
    public void BothExternalRomFieldsRemainAvailableAndOptional()
    {
        var configuration = new MachineConfiguration(MachineModel.ThreeDO, core: Emulator.Opera);
        var requirements = FirmwareFunctions.Requirements(configuration);
        var fields = FirmwareFunctions.Describe(configuration).Fields;

        Assert.Equal(2, requirements.Count);
        Assert.All(requirements, requirement => Assert.False(requirement.IsRequired));
        Assert.Contains(requirements, requirement => requirement.Role == FirmwareRole.SystemRom);
        Assert.Contains(requirements, requirement => requirement.Role == FirmwareRole.FontRom);
        Assert.All(requirements, requirement => Assert.Contains(fields, field => field.Id == requirement.FieldId && field.Editor == EmulationSettingsEditor.Path));
    }
}
