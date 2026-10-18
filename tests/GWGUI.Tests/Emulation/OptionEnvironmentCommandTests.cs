using GWGUI.Emulation.Constants;
using GWGUI.Emulation.Functions;
using GWGUI.Emulation.Interfaces;

namespace GWGUI.Tests.Emulation;

public sealed class OptionEnvironmentCommandTests
{
    [Theory]
    [InlineData(ExternalCoreApiConstants.SetVariables, "RegisterLegacyVariables")]
    [InlineData(ExternalCoreApiConstants.SetCoreOptions, "RegisterVersionOne")]
    [InlineData(ExternalCoreApiConstants.SetCoreOptionsInternational, "RegisterVersionOneInternational")]
    [InlineData(ExternalCoreApiConstants.SetCoreOptionsV2, "RegisterVersionTwo")]
    [InlineData(ExternalCoreApiConstants.SetCoreOptionsV2International, "RegisterVersionTwoInternational")]
    public void RegistrationReceivesNullDataBeforeReportingItsRefusal(uint command, string method)
    {
        foreach (var data in new nint[] { 0, 17 })
        {
            var options = new OptionCommands();
            Assert.True(ExternalCoreEnvironmentFunctions.TryHandleOptionCommand(command, data, options, out var result));
            Assert.Equal(data != nint.Zero, result);
            Assert.Equal(new[] { (method, data) }, options.Calls);
        }
    }

    [Theory]
    [InlineData(ExternalCoreApiConstants.GetVariable, "ReturnValue")]
    [InlineData(ExternalCoreApiConstants.GetVariableUpdate, "GetAndClearUpdated")]
    [InlineData(ExternalCoreApiConstants.SetCoreOptionsDisplay, "ApplyVisibility")]
    [InlineData(ExternalCoreApiConstants.SetCoreOptionsUpdateDisplayCallback, "CaptureDisplayUpdate")]
    [InlineData(ExternalCoreApiConstants.SetVariable, "SetNativeValue")]
    public void BooleanRepliesRemainRecognizedEvenWhenTheHostReturnsFalse(uint command, string method)
    {
        foreach (var hostResult in new[] { false, true })
        {
            var options = new OptionCommands { BooleanResult = hostResult };
            Assert.True(ExternalCoreEnvironmentFunctions.TryHandleOptionCommand(command, nint.Zero, options, out var result));
            Assert.Equal(hostResult, result);
            Assert.Equal(new[] { (method, nint.Zero) }, options.Calls);
        }
    }

    [Theory]
    [InlineData(0u)]
    [InlineData(ExternalCoreApiConstants.GetCanDuplicateFrames)]
    [InlineData(ExternalCoreApiConstants.GetCoreOptionsVersion)]
    [InlineData(ExternalCoreApiConstants.SetPixelFormat)]
    [InlineData(uint.MaxValue)]
    public void OtherCommandsRemainWithTheEnvironmentHost(uint command)
    {
        var options = new OptionCommands { BooleanResult = true };
        Assert.False(ExternalCoreEnvironmentFunctions.TryHandleOptionCommand(command, 17, options, out var result));
        Assert.False(result);
        Assert.Empty(options.Calls);
    }

    [Fact]
    public void RegistrationExceptionPropagatesToTheExistingHostBoundary()
    {
        var expected = new InvalidOperationException("registration failed");
        var options = new OptionCommands { Failure = expected };
        var actual = Assert.Throws<InvalidOperationException>(() =>
            ExternalCoreEnvironmentFunctions.TryHandleOptionCommand(
                ExternalCoreApiConstants.SetCoreOptionsV2, 17, options, out _));
        Assert.Same(expected, actual);
        Assert.Equal(new[] { ("RegisterVersionTwo", (nint)17) }, options.Calls);
    }

    private sealed class OptionCommands : IEmulationCoreOptionCommands
    {
        internal bool BooleanResult { get; init; }
        internal Exception? Failure { get; init; }
        internal List<(string Method, nint Data)> Calls { get; } = [];
        private void Record(string method, nint data)
        {
            Calls.Add((method, data));
            if (Failure is not null) throw Failure;
        }
        public bool ReturnValue(nint data) { Record(nameof(ReturnValue), data); return BooleanResult; }
        public void RegisterLegacyVariables(nint data) => Record(nameof(RegisterLegacyVariables), data);
        public bool GetAndClearUpdated(nint data) { Record(nameof(GetAndClearUpdated), data); return BooleanResult; }
        public void RegisterVersionOne(nint data) => Record(nameof(RegisterVersionOne), data);
        public void RegisterVersionOneInternational(nint data) => Record(nameof(RegisterVersionOneInternational), data);
        public void RegisterVersionTwo(nint data) => Record(nameof(RegisterVersionTwo), data);
        public void RegisterVersionTwoInternational(nint data) => Record(nameof(RegisterVersionTwoInternational), data);
        public bool ApplyVisibility(nint data) { Record(nameof(ApplyVisibility), data); return BooleanResult; }
        public bool CaptureDisplayUpdate(nint data) { Record(nameof(CaptureDisplayUpdate), data); return BooleanResult; }
        public bool SetNativeValue(nint data) { Record(nameof(SetNativeValue), data); return BooleanResult; }
    }
}

