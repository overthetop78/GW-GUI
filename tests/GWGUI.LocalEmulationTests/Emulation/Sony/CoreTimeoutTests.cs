using GWGUI.Emulation.Sony.Common.Enums;
using GWGUI.Emulation.Sony.Emulators.Common.Interop.Dictionaries;
using GWGUI.Emulation.Sony.Emulators.Common.Interop.Enums;
using GWGUI.Emulation.Sony.Emulators.Common.Interop.Functions;

namespace GWGUI.Tests.Emulation.Sony;

public sealed class CoreTimeoutTests
{
    [Theory]
    [InlineData("play", (int)HostCommand.Initialize, 30)]
    [InlineData("play", (int)HostCommand.HardReset, 30)]
    [InlineData("play", (int)HostCommand.Stop, 30)]
    [InlineData("play", (int)HostCommand.Dispose, 5)]
    [InlineData("play", (int)HostCommand.RunFrame, 30)]
    [InlineData("rpcs3", (int)HostCommand.Initialize, 600)]
    [InlineData("rpcs3", (int)HostCommand.HardReset, 600)]
    [InlineData("rpcs3", (int)HostCommand.Stop, 300)]
    [InlineData("rpcs3", (int)HostCommand.Dispose, 300)]
    [InlineData("rpcs3", (int)HostCommand.RunFrame, 30)]
    [InlineData("mednafen_psx", (int)HostCommand.Dispose, 30)]
    public void CommandDeadlinesPreserveStartupAndPersistencePolicies(string emulator, int command, int seconds) =>
        Assert.Equal(TimeSpan.FromSeconds(seconds), CoreTimeoutFunctions.For(CoreCatalog.Get(emulator), (HostCommand)command));

    [Fact]
    public void AnUnconfiguredHostUsesTheExistingDefault() => Assert.Equal(TimeSpan.FromSeconds(30), CoreTimeoutFunctions.For(null, HostCommand.Dispose));
}
