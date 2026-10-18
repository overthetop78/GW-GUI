using System.Runtime.InteropServices;
using GWGUI.Emulation.Functions;
using GWGUI.Emulation.Interop;

namespace GWGUI.Tests.Emulation;

public sealed class CoreAvUpdateTests
{
    [Theory]
    [InlineData(1.5f, 4u, 3u, 1.5f)]
    [InlineData(float.Epsilon, 4u, 3u, float.Epsilon)]
    [InlineData(0f, 3u, 2u, 1.5f)]
    [InlineData(-1f, 3u, 2u, 1.5f)]
    [InlineData(float.NaN, 3u, 2u, 1.5f)]
    [InlineData(float.PositiveInfinity, 3u, 2u, 1.5f)]
    [InlineData(float.NegativeInfinity, 3u, 2u, 1.5f)]
    [InlineData(0f, 0u, 2u, 0f)]
    [InlineData(0f, 3u, 0u, 1.75f)]
    [InlineData(float.NaN, 3u, 0u, 1.75f)]
    public void GeometryUsesTheExplicitRatioOrDimensionsAndOtherwiseKeepsTheOldValue(float ratio, uint width, uint height, float expected)
    {
        var current = 1.75f;
        EmulationVideoFunctions.ApplyGeometry(new ExternalCoreApi.Geometry { AspectRatio = ratio, BaseWidth = width, BaseHeight = height }, ref current);
        Assert.Equal(expected, current);
    }

    [Theory]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    [InlineData(0d)]
    [InlineData(-1d)]
    public void InvalidTimingPreservesBothExistingValues(double value)
    {
        var fps = 50d;
        var sampleRate = 48000;
        var geometryCalls = 0;
        ExternalCoreEnvironmentFunctions.ApplySystemAvInfo(Info(value, value), _ => geometryCalls++, value => fps = value, value => sampleRate = value);
        Assert.Equal(1, geometryCalls);
        Assert.Equal(50d, fps);
        Assert.Equal(48000, sampleRate);
    }

    [Theory]
    [InlineData(0.1d, 0)]
    [InlineData(0.5d, 0)]
    [InlineData(1.5d, 2)]
    [InlineData(44000.5d, 44000)]
    [InlineData(44001.5d, 44002)]
    [InlineData(2147483647d, int.MaxValue)]
    public void AudioRoundingKeepsTheOriginalPositiveInputGuardAndBankersRounding(double input, int expected)
    {
        var sampleRate = -1;
        var fps = 0d;
        ExternalCoreEnvironmentFunctions.ApplySystemAvInfo(Info(double.Epsilon, input), _ => { }, value => fps = value, value => sampleRate = value);
        Assert.Equal(double.Epsilon, fps);
        Assert.Equal(expected, sampleRate);
    }

    [Fact]
    public void OutOfRangeAudioIsIgnoredWhileTheVideoFrequencyStillUpdates()
    {
        var sampleRate = 48000;
        var fps = 0d;
        ExternalCoreEnvironmentFunctions.ApplySystemAvInfo(Info(59.94d, (double)int.MaxValue + 0.1d), _ => { }, value => fps = value, value => sampleRate = value);
        Assert.Equal(59.94d, fps);
        Assert.Equal(48000, sampleRate);
    }

    [Fact]
    public void CallbacksRunInGeometryVideoAudioOrderWithTheOriginalValues()
    {
        var calls = new List<string>();
        var info = Info(59.94d, 44100d);
        info.Geometry.BaseWidth = 320;
        ExternalCoreEnvironmentFunctions.ApplySystemAvInfo(info,
            geometry => { Assert.Equal(320u, geometry.BaseWidth); calls.Add("geometry"); },
            fps => { Assert.Equal(59.94d, fps); calls.Add("video"); },
            rate => { Assert.Equal(44100, rate); calls.Add("audio"); });
        Assert.Equal(new[] { "geometry", "video", "audio" }, calls);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void CallbackFailureStopsLaterUpdatesAndKeepsTheSameError(int failure)
    {
        var error = new InvalidOperationException("update failure");
        var calls = new List<int>();
        void Update(int index) { calls.Add(index); if (index == failure) throw error; }
        var actual = Assert.Throws<InvalidOperationException>(() => ExternalCoreEnvironmentFunctions.ApplySystemAvInfo(
            Info(60, 48000), _ => Update(0), _ => Update(1), _ => Update(2)));
        Assert.Same(error, actual);
        Assert.Equal(Enumerable.Range(0, failure + 1), calls);
    }

    [Fact]
    public void NullStructurePointerReturnsFalseWithoutCallingTheBorrowedConsumer()
    {
        Assert.False(ExternalCoreEnvironmentFunctions.ApplyStructure<ExternalCoreApi.Geometry>(0, _ => throw new InvalidOperationException()));
        Assert.False(ExternalCoreEnvironmentFunctions.ApplyStructure<ExternalCoreApi.Geometry>(0, null!));
    }

    [Fact]
    public void StructureIsCopiedBeforeTheConsumerAndTheBufferRemainsUnchanged()
    {
        var pointer = Marshal.AllocHGlobal(Marshal.SizeOf<ExternalCoreApi.SystemAvInfo>());
        try
        {
            var info = Info(60, 48000);
            info.Geometry.BaseWidth = 640;
            Marshal.StructureToPtr(info, pointer, false);
            var calls = 0;
            Assert.True(ExternalCoreEnvironmentFunctions.ApplyStructure<ExternalCoreApi.SystemAvInfo>(pointer, value =>
            {
                calls++;
                Assert.Equal(640u, value.Geometry.BaseWidth);
                Assert.Equal(60d, value.Timing.FramesPerSecond);
                Assert.Equal(48000d, value.Timing.SampleRate);
                value.Geometry.BaseWidth = 1;
            }));
            Assert.Equal(1, calls);
            Assert.Equal(640u, Marshal.PtrToStructure<ExternalCoreApi.SystemAvInfo>(pointer).Geometry.BaseWidth);
        }
        finally { Marshal.FreeHGlobal(pointer); }
    }

    [Fact]
    public void StructureConsumerFailurePropagatesWithoutTakingOwnershipOfTheBuffer()
    {
        var pointer = Marshal.AllocHGlobal(Marshal.SizeOf<ExternalCoreApi.Geometry>());
        try
        {
            Marshal.StructureToPtr(new ExternalCoreApi.Geometry { BaseHeight = 240 }, pointer, false);
            var error = new InvalidOperationException("consumer failure");
            Assert.Same(error, Assert.Throws<InvalidOperationException>(() =>
                ExternalCoreEnvironmentFunctions.ApplyStructure<ExternalCoreApi.Geometry>(pointer, _ => throw error)));
            Assert.Equal(240u, Marshal.PtrToStructure<ExternalCoreApi.Geometry>(pointer).BaseHeight);
        }
        finally { Marshal.FreeHGlobal(pointer); }
    }

    private static ExternalCoreApi.SystemAvInfo Info(double fps, double sampleRate)
    {
        var info = new ExternalCoreApi.SystemAvInfo();
        info.Timing.FramesPerSecond = fps;
        info.Timing.SampleRate = sampleRate;
        return info;
    }
}
