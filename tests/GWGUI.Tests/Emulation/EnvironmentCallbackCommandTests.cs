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
    [InlineData(ExternalCoreApiConstants.SetKeyboardCallback)]
    [InlineData(ExternalCoreApiConstants.SetDiskControl)]
    public void LeavesOtherCommandsToTheirExistingOwners(uint command)
    {
        Assert.False(ExternalCoreEnvironmentFunctions.TryHandleCallbackCommand(command, (nint)42, null!, out var result));
        Assert.False(result);
    }

    [Fact]
    public void PropagatesTheOriginalCallbackError()
    {
        var error = new InvalidOperationException("callback failed");
        var callbacks = new Callbacks { Error = error };
        var observed = Assert.Throws<InvalidOperationException>(() =>
            ExternalCoreEnvironmentFunctions.TryHandleCallbackCommand(ExternalCoreApiConstants.SetRotation, (nint)42, callbacks, out _));
        Assert.Same(error, observed);
        Assert.Equal(1, callbacks.CallCount);
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
    }
}
