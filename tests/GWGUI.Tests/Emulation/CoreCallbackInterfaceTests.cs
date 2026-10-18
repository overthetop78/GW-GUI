using System.Runtime.InteropServices;
using GWGUI.Emulation.Functions;
using GWGUI.Emulation.Interop;

namespace GWGUI.Tests.Emulation;

public sealed class CoreCallbackInterfaceTests
{
    [Fact]
    public void RequiredDelegateThrowsTheDeferredErrorExactlyOnce()
    {
        var expected = new InvalidOperationException("incomplete interface");
        var calls = 0;
        var actual = Assert.Throws<InvalidOperationException>(() =>
            ExternalCoreEnvironmentFunctions.RequiredDelegate<ExternalCoreApi.KeyboardEvent>(nint.Zero, () =>
            {
                calls++;
                return expected;
            }));
        Assert.Same(expected, actual);
        Assert.Equal(1, calls);
    }

    [Fact]
    public void RequiredDelegatePreservesTheValidCallbackWithoutCreatingAnError()
    {
        (uint Index, nint Path, nuint Length)? observed = null;
        ExternalCoreApi.GetImagePath callback = (index, path, length) =>
        {
            observed = (index, path, length);
            return index == 7;
        };
        try
        {
            var pointer = Marshal.GetFunctionPointerForDelegate(callback);
            var converted = ExternalCoreEnvironmentFunctions.RequiredDelegate<ExternalCoreApi.GetImagePath>(pointer,
                () => throw new InvalidOperationException("must not create an error"));
            Assert.True(converted(7, (nint)123, 1024));
            Assert.Equal((7u, (nint)123, (nuint)1024), observed);
            var withoutFactory = ExternalCoreEnvironmentFunctions.RequiredDelegate<ExternalCoreApi.GetImagePath>(pointer, null!);
            Assert.False(withoutFactory(8, nint.Zero, 0));
            Assert.Equal((8u, nint.Zero, (nuint)0), observed);
        }
        finally { GC.KeepAlive(callback); }
    }

    [Fact]
    public void RequiredDelegatePropagatesAnErrorFromTheFactory()
    {
        var expected = new InvalidOperationException("factory failed");
        var actual = Assert.Throws<InvalidOperationException>(() =>
            ExternalCoreEnvironmentFunctions.RequiredDelegate<ExternalCoreApi.KeyboardEvent>(nint.Zero, () => throw expected));
        Assert.Same(expected, actual);
    }

    [Fact]
    public void OptionalDelegateReturnsNullForMissingCallbacks()
    {
        Assert.Null(ExternalCoreEnvironmentFunctions.OptionalDelegate<ExternalCoreApi.KeyboardEvent>(nint.Zero));
        Assert.Null(ExternalCoreEnvironmentFunctions.OptionalDelegate<ExternalCoreApi.GetImagePath>(nint.Zero));
    }

    [Fact]
    public void OptionalKeyboardDelegatePreservesEveryArgument()
    {
        (bool Down, uint Key, uint Character, ushort Modifiers)? observed = null;
        ExternalCoreApi.KeyboardEvent callback = (down, key, character, modifiers) => observed = (down, key, character, modifiers);
        try
        {
            var pointer = Marshal.GetFunctionPointerForDelegate(callback);
            var converted = ExternalCoreEnvironmentFunctions.OptionalDelegate<ExternalCoreApi.KeyboardEvent>(pointer);
            Assert.NotNull(converted);
            converted(true, 273, 0x1f642, ushort.MaxValue);
            Assert.Equal((true, 273u, 0x1f642u, ushort.MaxValue), observed);
            converted(false, 0, 0, 0);
            Assert.Equal((false, 0u, 0u, (ushort)0), observed);
        }
        finally { GC.KeepAlive(callback); }
    }

    [Fact]
    public void OptionalDiskDelegatePreservesArgumentsAndBooleanReturn()
    {
        (uint Index, nint Path, nuint Length)? observed = null;
        ExternalCoreApi.GetImagePath callback = (index, path, length) =>
        {
            observed = (index, path, length);
            return index == 5;
        };
        try
        {
            var pointer = Marshal.GetFunctionPointerForDelegate(callback);
            var converted = ExternalCoreEnvironmentFunctions.OptionalDelegate<ExternalCoreApi.GetImagePath>(pointer);
            Assert.NotNull(converted);
            Assert.True(converted(5, (nint)123, (nuint)1024));
            Assert.Equal((5u, (nint)123, (nuint)1024), observed);
            Assert.False(converted(6, nint.Zero, 0));
            Assert.Equal((6u, nint.Zero, (nuint)0), observed);
        }
        finally { GC.KeepAlive(callback); }
    }

    [Fact]
    public void NullDestinationsDoNotConvertOrPublishCallbacks()
    {
        Assert.False(ExternalCoreEnvironmentFunctions.SetLogInterface(nint.Zero, null!));
        Assert.False(ExternalCoreEnvironmentFunctions.SetLedInterface(nint.Zero, null!));
        Assert.False(ExternalCoreEnvironmentFunctions.SetRumbleInterface(nint.Zero, null!));
        Assert.False(ExternalCoreEnvironmentFunctions.SetSensorInterface(nint.Zero, null!, null!));
    }

    [Fact]
    public void LogInterfacePublishesTheBorrowedCallback()
    {
        (int Level, nint Format)? observed = null;
        ExternalCoreApi.LogCallback callback = (level, format) => observed = (level, format);
        WithGuardedBuffer<ExternalCoreApi.LogInterface>(
            pointer => ExternalCoreEnvironmentFunctions.SetLogInterface(pointer, callback),
            native =>
            {
                Assert.Equal(Marshal.GetFunctionPointerForDelegate(callback), native.Log);
                Marshal.GetDelegateForFunctionPointer<ExternalCoreApi.LogCallback>(native.Log)(2, (nint)123);
                Assert.Equal((2, (nint)123), observed);
            }, callback);
    }

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void FourArgumentLogCallback(int level, nint format, nint firstArgument, nint secondArgument);

    [Fact]
    public void LogInterfacePreservesTheFourArgumentCallbackSignature()
    {
        (int Level, nint Format, nint First, nint Second)? observed = null;
        FourArgumentLogCallback callback = (level, format, first, second) => observed = (level, format, first, second);
        WithGuardedBuffer<ExternalCoreApi.LogInterface>(
            pointer => ExternalCoreEnvironmentFunctions.SetLogInterface(pointer, callback),
            native =>
            {
                Assert.Equal(Marshal.GetFunctionPointerForDelegate(callback), native.Log);
                Marshal.GetDelegateForFunctionPointer<FourArgumentLogCallback>(native.Log)(3, (nint)123, (nint)456, (nint)789);
                Assert.Equal((3, (nint)123, (nint)456, (nint)789), observed);
            }, callback);
    }

    [Fact]
    public void NullDestinationDoesNotConvertAFourArgumentCallback()
        => Assert.False(ExternalCoreEnvironmentFunctions.SetLogInterface<FourArgumentLogCallback>(nint.Zero, null!));

    [Fact]
    public void LedInterfacePublishesTheBorrowedCallback()
    {
        (int Led, int State)? observed = null;
        ExternalCoreApi.SetLedState callback = (led, state) => observed = (led, state);
        WithGuardedBuffer<ExternalCoreApi.LedInterface>(
            pointer => ExternalCoreEnvironmentFunctions.SetLedInterface(pointer, callback),
            native =>
            {
                Assert.Equal(Marshal.GetFunctionPointerForDelegate(callback), native.SetLedState);
                Marshal.GetDelegateForFunctionPointer<ExternalCoreApi.SetLedState>(native.SetLedState)(7, 1);
                Assert.Equal((7, 1), observed);
            }, callback);
    }

    [Fact]
    public void RumbleInterfacePreservesPortEffectStrengthAndResult()
    {
        (uint Port, uint Effect, ushort Strength)? observed = null;
        ExternalCoreApi.SetRumbleState callback = (port, effect, strength) =>
        {
            observed = (port, effect, strength);
            return port == 3;
        };
        WithGuardedBuffer<ExternalCoreApi.RumbleInterface>(
            pointer => ExternalCoreEnvironmentFunctions.SetRumbleInterface(pointer, callback),
            native =>
            {
                Assert.Equal(Marshal.GetFunctionPointerForDelegate(callback), native.SetState);
                var published = Marshal.GetDelegateForFunctionPointer<ExternalCoreApi.SetRumbleState>(native.SetState);
                Assert.True(published(3, 1, ushort.MaxValue));
                Assert.Equal((3u, 1u, ushort.MaxValue), observed);
                Assert.False(published(4, 0, 0));
                Assert.Equal((4u, 0u, (ushort)0), observed);
            }, callback);
    }

    [Fact]
    public void SensorInterfaceKeepsBothCallbackSlotsAndTheirResults()
    {
        (uint Port, uint Action, uint Rate)? state = null;
        (uint Port, uint Id)? input = null;
        ExternalCoreApi.SetSensorState setState = (port, action, rate) =>
        {
            state = (port, action, rate);
            return true;
        };
        ExternalCoreApi.GetSensorInput getInput = (port, id) =>
        {
            input = (port, id);
            return 0.75f;
        };
        WithGuardedBuffer<ExternalCoreApi.SensorInterface>(
            pointer => ExternalCoreEnvironmentFunctions.SetSensorInterface(pointer, setState, getInput),
            native =>
            {
                Assert.Equal(Marshal.GetFunctionPointerForDelegate(setState), native.SetState);
                Assert.Equal(Marshal.GetFunctionPointerForDelegate(getInput), native.GetInput);
                Assert.Equal(nint.Zero, Marshal.OffsetOf<ExternalCoreApi.SensorInterface>(nameof(ExternalCoreApi.SensorInterface.SetState)));
                Assert.Equal((nint)IntPtr.Size, Marshal.OffsetOf<ExternalCoreApi.SensorInterface>(nameof(ExternalCoreApi.SensorInterface.GetInput)));
                Assert.True(Marshal.GetDelegateForFunctionPointer<ExternalCoreApi.SetSensorState>(native.SetState)(2, 5, 60));
                Assert.Equal((2u, 5u, 60u), state);
                Assert.Equal(0.75f, Marshal.GetDelegateForFunctionPointer<ExternalCoreApi.GetSensorInput>(native.GetInput)(2, 9));
                Assert.Equal((2u, 9u), input);
            }, setState, getInput);
    }

    [Fact]
    public void FailedSecondConversionDoesNotWriteAPartialInterface()
    {
        ExternalCoreApi.SetSensorState callback = (_, _, _) => true;
        var expected = Enumerable.Repeat((byte)0x4a, Marshal.SizeOf<ExternalCoreApi.SensorInterface>()).ToArray();
        var allocation = Marshal.AllocHGlobal(expected.Length);
        try
        {
            Marshal.Copy(expected, 0, allocation, expected.Length);
            Assert.Throws<ArgumentNullException>(() =>
                ExternalCoreEnvironmentFunctions.SetSensorInterface(allocation, callback, null!));
            var actual = new byte[expected.Length];
            Marshal.Copy(allocation, actual, 0, actual.Length);
            Assert.Equal(expected, actual);
        }
        finally
        {
            Marshal.FreeHGlobal(allocation);
            GC.KeepAlive(callback);
        }
    }

    private static void WithGuardedBuffer<T>(Func<nint, bool> install, Action<T> verify, params Delegate[] roots)
        where T : struct
    {
        var size = Marshal.SizeOf<T>();
        var bytes = Enumerable.Repeat((byte)0xa5, size + 2 * IntPtr.Size).ToArray();
        var allocation = Marshal.AllocHGlobal(bytes.Length);
        try
        {
            Marshal.Copy(bytes, 0, allocation, bytes.Length);
            var pointer = allocation + IntPtr.Size;
            Assert.True(install(pointer));
            verify(Marshal.PtrToStructure<T>(pointer));
            Marshal.Copy(allocation, bytes, 0, bytes.Length);
            Assert.All(bytes.Take(IntPtr.Size), value => Assert.Equal((byte)0xa5, value));
            Assert.All(bytes.Skip(IntPtr.Size + size), value => Assert.Equal((byte)0xa5, value));
        }
        finally
        {
            Marshal.FreeHGlobal(allocation);
            GC.KeepAlive(roots);
        }
    }
}
