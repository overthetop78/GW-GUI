using GWGUI.Emulation.Constants;
using GWGUI.Emulation.Functions;
using GWGUI.Emulation.Interfaces;

namespace GWGUI.Tests.Emulation;

public sealed class EnvironmentCallbackCommandTests
{
    [Theory]
    [InlineData(ExternalCoreApiConstants.SetRotation, "SetRotation")]
    [InlineData(ExternalCoreApiConstants.SetPerformanceLevel, "CapturePerformanceLevel")]
    [InlineData(ExternalCoreApiConstants.SetPixelFormat, "SetPixelFormat")]
    [InlineData(ExternalCoreApiConstants.SetSystemAvInfo, "SetSystemAvInfo")]
    [InlineData(ExternalCoreApiConstants.SetGeometry, "SetGeometry")]
    [InlineData(ExternalCoreApiConstants.SetMessage, "ReadMessage")]
    [InlineData(ExternalCoreApiConstants.SetMessageExtended, "ReadExtendedMessage")]
    [InlineData(ExternalCoreApiConstants.GetLogInterface, "SetLogInterface")]
    [InlineData(ExternalCoreApiConstants.GetLedInterface, "SetLedInterface")]
    [InlineData(ExternalCoreApiConstants.GetRumbleInterface, "SetRumbleInterface")]
    [InlineData(ExternalCoreApiConstants.GetSensorInterface, "SetSensorInterface")]
    [InlineData(ExternalCoreApiConstants.GetSystemDirectory, "WriteSystemDirectory")]
    [InlineData(ExternalCoreApiConstants.GetContentDirectory, "WriteContentDirectory")]
    [InlineData(ExternalCoreApiConstants.GetSaveDirectory, "WriteSaveDirectory")]
    [InlineData(ExternalCoreApiConstants.SetSupportNoGame, "SetSupportNoGame")]
    [InlineData(ExternalCoreApiConstants.SetInputDescriptors, "CaptureInputDescriptors")]
    [InlineData(ExternalCoreApiConstants.SetControllerInfo, "CaptureControllerInfo")]
    [InlineData(ExternalCoreApiConstants.SetMemoryMaps, "CaptureMemoryMaps")]
    [InlineData(ExternalCoreApiConstants.SetSupportAchievements, "SetSupportAchievements")]
    [InlineData(ExternalCoreApiConstants.GetVfsInterface, "ProvideVfs")]
    [InlineData(ExternalCoreApiConstants.SetKeyboardCallback, "SetKeyboardCallback")]
    [InlineData(ExternalCoreApiConstants.SetDiskControl, "CaptureDiskControl")]
    [InlineData(ExternalCoreApiConstants.SetDiskControlExtended, "CaptureExtendedDiskControl")]
    [InlineData(ExternalCoreApiConstants.GetDiskControlVersion, "GetDiskControlVersion")]
    public void RoutesRecognizedCommandsAndPreservesBothResults(uint command, string expected)
    {
        foreach (var result in new[] { false, true })
        {
            var callbacks = new Callbacks { Result = result };
            Assert.True(ExternalCoreEnvironmentFunctions.TryHandleCallbackCommand(command, (nint)42, callbacks, out var actual));
            Assert.Equal(result, actual);
            Assert.Equal(expected, callbacks.LastMethod);
            Assert.Equal((nint)42, callbacks.LastData);
            Assert.Equal(1, callbacks.CallCount);
        }
    }

    [Theory]
    [InlineData(uint.MaxValue)]
    [InlineData(ExternalCoreApiConstants.GetVariable)]
    [InlineData(ExternalCoreApiConstants.GetLanguage)]
    [InlineData(ExternalCoreApiConstants.SetVariables)]
    public void LeavesOtherCommandsToTheirExistingOwners(uint command)
    {
        Assert.False(ExternalCoreEnvironmentFunctions.TryHandleCallbackCommand(command, (nint)42, null!, out var result));
        Assert.False(result);
    }

    [Theory]
    [InlineData(ExternalCoreApiConstants.SetRotation)]
    [InlineData(ExternalCoreApiConstants.SetDiskControl)]
    [InlineData(ExternalCoreApiConstants.SetDiskControlExtended)]
    public void PropagatesTheOriginalCallbackError(uint command)
    {
        var error = new InvalidOperationException("callback failed");
        var callbacks = new Callbacks { Error = error };
        var observed = Assert.Throws<InvalidOperationException>(() =>
            ExternalCoreEnvironmentFunctions.TryHandleCallbackCommand(command, (nint)42, callbacks, out _));
        Assert.Same(error, observed);
        Assert.Equal(1, callbacks.CallCount);
    }

    [Theory]
    [InlineData(ExternalCoreApiConstants.SetDiskControl)]
    [InlineData(ExternalCoreApiConstants.SetDiskControlExtended)]
    public void NullDiskCaptureIsRefusedBeforeCallingTheOwner(uint command)
    {
        Assert.True(ExternalCoreEnvironmentFunctions.TryHandleCallbackCommand(command, nint.Zero, null!, out var result));
        Assert.False(result);
    }

    [Theory]
    [InlineData(ExternalCoreApiConstants.SetKeyboardCallback)]
    [InlineData(ExternalCoreApiConstants.GetDiskControlVersion)]
    public void KeyboardAndVersionKeepTheProfileDecisionForNullData(uint command)
    {
        foreach (var expected in new[] { false, true })
        {
            var callbacks = new Callbacks { Result = expected };
            Assert.True(ExternalCoreEnvironmentFunctions.TryHandleCallbackCommand(command, nint.Zero, callbacks, out var result));
            Assert.Equal(expected, result);
            Assert.Equal(nint.Zero, callbacks.LastData);
            Assert.Equal(1, callbacks.CallCount);
        }
    }

    [Theory]
    [InlineData(ExternalCoreApiConstants.GetSystemDirectory, "WriteSystemDirectory")]
    [InlineData(ExternalCoreApiConstants.SetMessage, "ReadMessage")]
    [InlineData(ExternalCoreApiConstants.GetVfsInterface, "ProvideVfs")]
    [InlineData(ExternalCoreApiConstants.SetDiskControl, "CaptureDiskControl")]
    [InlineData(ExternalCoreApiConstants.SetSystemAvInfo, "SetSystemAvInfo")]
    public void LegacySelectionPreservesTheCallbackReply(uint command, string expected)
    {
        foreach (var reply in new[] { false, true })
        {
            var callbacks = new Callbacks { Result = reply };
            var options = new OptionEnvironmentCommandTests.OptionCommands
                { Failure = new InvalidOperationException("must not call options") };
            Assert.True(ExternalCoreEnvironmentFunctions.TryHandleLegacyCallbackCommand(
                command, 42, callbacks, options, out var result));
            Assert.Equal(reply, result);
            Assert.Equal(expected, callbacks.LastMethod);
            Assert.Equal((nint)42, callbacks.LastData);
            Assert.Equal(1, callbacks.CallCount);
            Assert.Empty(options.Calls);
        }
    }

    [Theory]
    [InlineData(ExternalCoreApiConstants.SetVariables, "RegisterLegacyVariables")]
    [InlineData(ExternalCoreApiConstants.SetCoreOptionsV2International, "RegisterVersionTwoInternational")]
    public void LegacyRegistrationAcknowledgesTheOriginalNullAndNonNullPayloads(uint command, string expected)
    {
        foreach (var data in new nint[] { 0, 42 })
        {
            var options = new OptionEnvironmentCommandTests.OptionCommands();
            Assert.True(ExternalCoreEnvironmentFunctions.TryHandleLegacyCallbackCommand(
                command, data, null!, options, out var result));
            Assert.True(result);
            Assert.Equal(new[] { (expected, data) }, options.Calls);
        }
    }

    [Theory]
    [InlineData(ExternalCoreApiConstants.GetVariable, "ReturnValue")]
    [InlineData(ExternalCoreApiConstants.SetCoreOptionsUpdateDisplayCallback, "CaptureDisplayUpdate")]
    public void LegacyOptionQueriesKeepBothBooleanResults(uint command, string expected)
    {
        foreach (var reply in new[] { false, true })
        {
            var options = new OptionEnvironmentCommandTests.OptionCommands { BooleanResult = reply };
            Assert.True(ExternalCoreEnvironmentFunctions.TryHandleLegacyCallbackCommand(
                command, 42, null!, options, out var result));
            Assert.Equal(reply, result);
            Assert.Equal(new[] { (expected, (nint)42) }, options.Calls);
        }
    }

    [Theory]
    [InlineData(ExternalCoreApiConstants.SetDiskControl)]
    [InlineData(ExternalCoreApiConstants.SetDiskControlExtended)]
    public void LegacyDiskCaptureKeepsTheExistingNullRefusal(uint command)
    {
        var callbacks = new Callbacks { Error = new InvalidOperationException("must not capture") };
        Assert.True(ExternalCoreEnvironmentFunctions.TryHandleLegacyCallbackCommand(
            command, nint.Zero, callbacks, null!, out var result));
        Assert.False(result);
        Assert.Equal(0, callbacks.CallCount);
    }

    [Fact]
    public void LegacySelectionLeavesBroaderCallbacksQueriesAndCustomCommandsWithTheModule()
    {
        uint[] commands =
        [
            ExternalCoreApiConstants.SetRotation, ExternalCoreApiConstants.SetPerformanceLevel,
            ExternalCoreApiConstants.GetRumbleInterface, ExternalCoreApiConstants.GetSensorInterface,
            ExternalCoreApiConstants.GetCanDuplicateFrames, ExternalCoreApiConstants.GetInputBitmasks,
            ExternalCoreApiConstants.GetCoreOptionsVersion, ExternalCoreApiConstants.GetMessageInterfaceVersion,
            ExternalCoreApiConstants.SetVariable, ExternalCoreApiConstants.SetContentInfoOverride,
            ExternalCoreApiConstants.SetHardwareRender, ExternalCoreApiConstants.GetPreferredHardwareRender,
            ExternalCoreApiConstants.GetLanguage, ExternalCoreApiConstants.GetInputDeviceCapabilities,
            ExternalCoreApiConstants.GetPerformanceInterface, ExternalCoreApiConstants.GetMidiInterface,
            uint.MaxValue
        ];
        foreach (var command in commands)
        {
            Assert.False(ExternalCoreEnvironmentFunctions.TryHandleLegacyCallbackCommand(
                command, nint.Zero, null!, null!, out var result));
            Assert.False(result);
        }
    }

    [Fact]
    public void LegacyFastForwardOverrideIsRecognizedAsAQuietRefusal()
    {
        Assert.True(ExternalCoreEnvironmentFunctions.TryHandleLegacyCallbackCommand(
            ExternalCoreApiConstants.SetFastForwardingOverride, nint.Zero, null!, null!, out var result));
        Assert.False(result);
    }

    [Fact]
    public void LegacyCallbackFailureReachesTheOriginalModuleBoundary()
    {
        var error = new InvalidOperationException("callback failed");
        var callbacks = new Callbacks { Error = error };
        Assert.Same(error, Assert.Throws<InvalidOperationException>(() =>
            ExternalCoreEnvironmentFunctions.TryHandleLegacyCallbackCommand(
                ExternalCoreApiConstants.GetSystemDirectory, 42, callbacks, null!, out _)));
        Assert.Equal(1, callbacks.CallCount);
    }

    [Fact]
    public void LegacyOptionFailureReachesTheOriginalModuleBoundary()
    {
        var error = new InvalidOperationException("options failed");
        var options = new OptionEnvironmentCommandTests.OptionCommands { Failure = error };
        Assert.Same(error, Assert.Throws<InvalidOperationException>(() =>
            ExternalCoreEnvironmentFunctions.TryHandleLegacyCallbackCommand(
                ExternalCoreApiConstants.SetVariables, 42, null!, options, out _)));
        Assert.Equal(new[] { ("RegisterLegacyVariables", (nint)42) }, options.Calls);
    }
    private sealed class Callbacks : IEmulationCoreEnvironmentCommands
    {
        internal bool Result { get; init; }
        internal Exception? Error { get; init; }
        internal string? LastMethod { get; private set; }
        internal nint LastData { get; private set; }
        internal int CallCount { get; private set; }

        private bool Record(string method, nint data)
        {
            LastMethod = method;
            LastData = data;
            CallCount++;
            if (Error is not null) throw Error;
            return Result;
        }

        public bool SetRotation(nint data) => Record(nameof(SetRotation), data);
        public bool CapturePerformanceLevel(nint data) => Record(nameof(CapturePerformanceLevel), data);
        public bool SetPixelFormat(nint data) => Record(nameof(SetPixelFormat), data);
        public bool SetSystemAvInfo(nint data) => Record(nameof(SetSystemAvInfo), data);
        public bool SetGeometry(nint data) => Record(nameof(SetGeometry), data);
        public bool ReadMessage(nint data) => Record(nameof(ReadMessage), data);
        public bool ReadExtendedMessage(nint data) => Record(nameof(ReadExtendedMessage), data);
        public bool SetLogInterface(nint data) => Record(nameof(SetLogInterface), data);
        public bool SetLedInterface(nint data) => Record(nameof(SetLedInterface), data);
        public bool SetRumbleInterface(nint data) => Record(nameof(SetRumbleInterface), data);
        public bool SetSensorInterface(nint data) => Record(nameof(SetSensorInterface), data);
        public bool WriteSystemDirectory(nint data) => Record(nameof(WriteSystemDirectory), data);
        public bool WriteContentDirectory(nint data) => Record(nameof(WriteContentDirectory), data);
        public bool WriteSaveDirectory(nint data) => Record(nameof(WriteSaveDirectory), data);
        public bool SetSupportNoGame(nint data) => Record(nameof(SetSupportNoGame), data);
        public bool CaptureInputDescriptors(nint data) => Record(nameof(CaptureInputDescriptors), data);
        public bool CaptureControllerInfo(nint data) => Record(nameof(CaptureControllerInfo), data);
        public bool CaptureMemoryMaps(nint data) => Record(nameof(CaptureMemoryMaps), data);
        public bool SetSupportAchievements(nint data) => Record(nameof(SetSupportAchievements), data);
        public bool ProvideVfs(nint data) => Record(nameof(ProvideVfs), data);
        public bool SetKeyboardCallback(nint data) => Record(nameof(SetKeyboardCallback), data);
        public bool CaptureDiskControl(nint data) => Record(nameof(CaptureDiskControl), data);
        public bool CaptureExtendedDiskControl(nint data) => Record(nameof(CaptureExtendedDiskControl), data);
        public bool GetDiskControlVersion(nint data) => Record(nameof(GetDiskControlVersion), data);
    }
}
