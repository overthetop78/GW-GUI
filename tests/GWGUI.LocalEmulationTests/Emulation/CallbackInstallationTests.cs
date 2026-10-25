using GWGUI.Emulation;
using GWGUI.Emulation.Contracts;
using GWGUI.Emulation.Functions;
using GWGUI.Emulation.Interfaces;
using GWGUI.Emulation.Interop;

namespace GWGUI.Tests.Emulation;

public sealed class CallbackInstallationTests
{
    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    public void RegistrationReadsEachBorrowedCallbackImmediatelyBeforeItsSetterAndStopsOnFailure(int failAt)
    {
        var events = new List<string>();
        var callbacks = new Callbacks(events);
        var error = new InvalidOperationException("registration failed");
        void Set(int index, Delegate callback)
        {
            events.Add("set " + index);
            Assert.Same(callbacks.Values[index], callback);
            if (index == failAt) throw error;
        }

        var exports = new ExternalCoreExports(
            callback => Set(0, callback), callback => Set(1, callback),
            callback => Set(2, callback), callback => Set(3, callback),
            callback => Set(4, callback), callback => Set(5, callback),
            Initialize: null!, Deinitialize: null!, GetSystemInfo: null!, GetSystemAvInfo: null!,
            SetControllerPortDevice: null!, Reset: null!, Run: null!, LoadGame: null!, UnloadGame: null!,
            GetRegion: null!, GetMemoryData: null!, GetMemorySize: null!, GetSerializedSize: null!,
            Serialize: null!, Unserialize: null!);

        if (failAt < 0) EmulationSdk.EnvironmentProcessing.InstallCallbacks(exports, callbacks);
        else Assert.Same(error, Assert.Throws<InvalidOperationException>(() =>
            EmulationSdk.EnvironmentProcessing.InstallCallbacks(exports, callbacks)));
        var count = failAt < 0 ? 6 : failAt + 1;
        Assert.Equal(Enumerable.Range(0, count).SelectMany(index => new[] { "get " + index, "set " + index }), events);
    }

    private sealed class Callbacks(List<string> events) : IEmulationCoreCallbacks
    {
        public Delegate[] Values { get; } =
        [
            (ExternalCoreApi.EnvironmentCallback)((_, _) => true),
            (ExternalCoreApi.VideoCallback)((_, _, _, _) => { }),
            (ExternalCoreApi.AudioSampleCallback)((_, _) => { }),
            (ExternalCoreApi.AudioBatchCallback)((_, frames) => frames),
            (ExternalCoreApi.InputPollCallback)(() => { }),
            (ExternalCoreApi.InputStateCallback)((_, _, _, _) => 0)
        ];

        public ExternalCoreApi.EnvironmentCallback Environment => Get<ExternalCoreApi.EnvironmentCallback>(0);
        public ExternalCoreApi.VideoCallback Video => Get<ExternalCoreApi.VideoCallback>(1);
        public ExternalCoreApi.AudioSampleCallback AudioSample => Get<ExternalCoreApi.AudioSampleCallback>(2);
        public ExternalCoreApi.AudioBatchCallback AudioBatch => Get<ExternalCoreApi.AudioBatchCallback>(3);
        public ExternalCoreApi.InputPollCallback InputPoll => Get<ExternalCoreApi.InputPollCallback>(4);
        public ExternalCoreApi.InputStateCallback InputState => Get<ExternalCoreApi.InputStateCallback>(5);

        private T Get<T>(int index) where T : Delegate
        {
            events.Add("get " + index);
            return (T)Values[index];
        }
    }
}
