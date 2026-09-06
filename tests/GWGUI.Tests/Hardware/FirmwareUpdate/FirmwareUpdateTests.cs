using GWGUI.Tests.Application.TestInfrastructure;
namespace GWGUI.Tests.Hardware.FirmwareUpdate;
[Collection("WPF")]
public sealed class FirmwareUpdateTests(StaExecutionScenarios sta)
{
    [Theory] [InlineData(false, false)] [InlineData(true, false)] [InlineData(true, true)]
    public Task UpdateConfirmation(bool bootloader, bool accepted) => sta.Run(() => FirmwareUpdateScenarios.Confirmation(bootloader, accepted));
    [Theory] [InlineData(0, false)] [InlineData(3, false)] [InlineData(0, true)]
    public Task UpdateOutcome(int exit, bool cancelled) => sta.Run(() => FirmwareUpdateScenarios.Outcome(exit, cancelled));
    [Fact] public Task UpdateErrorRestoresCommand() => sta.Run(FirmwareUpdateScenarios.Error);
}
