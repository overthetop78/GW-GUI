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

    [Theory]
    [InlineData(ExternalCoreApiConstants.GetMidiInterface)]
    [InlineData(ExternalCoreApiConstants.SetFastForwardingOverride)]
    [InlineData(ExternalCoreApiConstants.SetContentInfoOverride)]
    [InlineData(ExternalCoreApiConstants.SetNetworkPacketInterface)]
    [InlineData(ExternalCoreApiConstants.SetSerializationQuirks)]
    public void RefusedCommandsDoNotReachOptionsOrUnknownDiagnostics(uint command)
    {
        var options = new OptionCommands { Failure = new InvalidOperationException("must not call") };
        var unknown = new HashSet<uint>();
        var diagnostics = new List<string>();
        Assert.False(ExternalCoreEnvironmentFunctions.HandleRemainingCommand(command, 17, options,
            unknown, diagnostics, _ => throw new InvalidOperationException("must not format")));
        Assert.Empty(options.Calls);
        Assert.Empty(unknown);
        Assert.Empty(diagnostics);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void RemainingOptionRepliesDoNotBecomeUnknownCommands(bool reply)
    {
        var options = new OptionCommands { BooleanResult = reply };
        var unknown = new HashSet<uint>();
        var diagnostics = new List<string>();
        Assert.Equal(reply, ExternalCoreEnvironmentFunctions.HandleRemainingCommand(
            ExternalCoreApiConstants.GetVariable, 17, options, unknown, diagnostics,
            _ => throw new InvalidOperationException("must not format")));
        Assert.Equal(new[] { ("ReturnValue", (nint)17) }, options.Calls);
        Assert.Empty(unknown);
        Assert.Empty(diagnostics);
    }

    [Fact]
    public void UnknownCommandIsRecordedBeforeFormattingAndPublishedOnce()
    {
        var options = new OptionCommands();
        var unknown = new HashSet<uint>();
        var diagnostics = new List<string>();
        var formatted = 0;
        string Format(uint command)
        {
            Assert.Contains(command, unknown);
            formatted++;
            return $"module diagnostic {command}";
        }
        for (var attempt = 0; attempt < 2; attempt++)
            Assert.False(ExternalCoreEnvironmentFunctions.HandleRemainingCommand(uint.MaxValue,
                17, options, unknown, diagnostics, Format));
        Assert.Equal(1, formatted);
        Assert.Equal(new[] { uint.MaxValue }, unknown);
        Assert.Equal(new[] { $"module diagnostic {uint.MaxValue}" }, diagnostics);
        Assert.Empty(options.Calls);
    }

    [Fact]
    public void FailedDiagnosticFormattingKeepsTheOriginalDeduplicationOrder()
    {
        var options = new OptionCommands();
        var unknown = new HashSet<uint>();
        var diagnostics = new List<string>();
        var failure = new InvalidOperationException("format failed");
        var formatted = 0;
        string Format(uint command) { formatted++; throw failure; }
        Assert.Same(failure, Assert.Throws<InvalidOperationException>(() =>
            ExternalCoreEnvironmentFunctions.HandleRemainingCommand(uint.MaxValue, 17,
                options, unknown, diagnostics, Format)));
        Assert.Contains(uint.MaxValue, unknown);
        Assert.Empty(diagnostics);
        Assert.False(ExternalCoreEnvironmentFunctions.HandleRemainingCommand(uint.MaxValue, 17,
            options, unknown, diagnostics, Format));
        Assert.Equal(1, formatted);
        Assert.Empty(diagnostics);
    }

    [Fact]
    public void RemainingOptionFailurePropagatesWithoutCreatingUnknownDiagnostic()
    {
        var failure = new InvalidOperationException("option failed");
        var options = new OptionCommands { Failure = failure };
        var unknown = new HashSet<uint>();
        var diagnostics = new List<string>();
        Assert.Same(failure, Assert.Throws<InvalidOperationException>(() =>
            ExternalCoreEnvironmentFunctions.HandleRemainingCommand(ExternalCoreApiConstants.SetCoreOptionsV2,
                17, options, unknown, diagnostics,
                _ => throw new InvalidOperationException("must not format"))));
        Assert.Equal(new[] { ("RegisterVersionTwo", (nint)17) }, options.Calls);
        Assert.Empty(unknown);
        Assert.Empty(diagnostics);
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

