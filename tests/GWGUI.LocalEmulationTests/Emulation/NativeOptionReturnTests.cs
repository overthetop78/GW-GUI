using GWGUI.Emulation;
using System.Runtime.InteropServices;
using GWGUI.Emulation.Functions;
using GWGUI.Emulation.Interop;

namespace GWGUI.Tests.Emulation;

public sealed class NativeOptionReturnTests
{
    [Theory]
    [InlineData(true, "known", "known", true)]
    [InlineData(false, "known", "known", true)]
    [InlineData(true, "missing", "known", false)]
    [InlineData(false, "missing", "known", false)]
    [InlineData(true, null, "known", false)]
    [InlineData(false, null, "known", false)]
    [InlineData(true, "", "", true)]
    [InlineData(false, "", "", true)]
    [InlineData(true, "KNOWN", "known", false)]
    [InlineData(false, "KNOWN", "known", false)]
    [InlineData(true, "clé_日本", "clé_日本", true)]
    [InlineData(false, "clé_日本", "clé_日本", true)]
    public void NativeRepliesKeepAbsencePolicyAndPreserveTheKeyPointer(
        bool allowUnavailable, string? key, string storedKey, bool known)
    {
        WithVariable(key, (data, keyPointer) =>
        {
            var values = new Dictionary<string, string>(StringComparer.Ordinal) { [storedKey] = "configured" };
            var calls = 0;
            nint CreateValue(string actualKey, string value)
            {
                calls++;
                Assert.Equal(storedKey, actualKey);
                Assert.Equal("configured", value);
                return 1234;
            }
            var result = allowUnavailable
                ? EmulationSdk.OptionProcessing.ReturnNativeValue(data, values, CreateValue)
                : EmulationSdk.OptionProcessing.ReturnNativeValue(data, values, CreateValue, unavailableResult: false);
            Assert.Equal(known || allowUnavailable, result);
            Assert.Equal(known ? 1 : 0, calls);
            var reply = Marshal.PtrToStructure<ExternalCoreApi.Variable>(data);
            Assert.Equal(keyPointer, reply.Key);
            Assert.Equal(known ? (nint)1234 : nint.Zero, reply.Value);
        });
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void NullDataReturnsTheProfileResultWithoutCallingTheValueFactory(bool allowUnavailable)
    {
        var values = new Dictionary<string, string>();
        nint UnexpectedValue(string key, string value) => throw new InvalidOperationException("factory must not run");
        var result = allowUnavailable
            ? EmulationSdk.OptionProcessing.ReturnNativeValue(nint.Zero, values, UnexpectedValue)
            : EmulationSdk.OptionProcessing.ReturnNativeValue(nint.Zero, values, UnexpectedValue, unavailableResult: false);
        Assert.Equal(allowUnavailable, result);
    }

    [Fact]
    public void ValueFactoryExceptionLeavesTheNativeReplyUnwritten()
    {
        WithVariable("known", (data, keyPointer) =>
        {
            var expected = new InvalidOperationException("native value unavailable");
            var values = new Dictionary<string, string> { ["known"] = "configured" };
            var actual = Assert.Throws<InvalidOperationException>(() =>
                EmulationSdk.OptionProcessing.ReturnNativeValue(data, values, (_, _) => throw expected));
            Assert.Same(expected, actual);
            var reply = Marshal.PtrToStructure<ExternalCoreApi.Variable>(data);
            Assert.Equal(keyPointer, reply.Key);
            Assert.Equal((nint)9876, reply.Value);
        });
    }

    private static void WithVariable(string? key, Action<nint, nint> assert)
    {
        nint keyPointer = nint.Zero;
        nint data = nint.Zero;
        try
        {
            if (key is not null) keyPointer = Marshal.StringToCoTaskMemUTF8(key);
            data = Marshal.AllocHGlobal(Marshal.SizeOf<ExternalCoreApi.Variable>());
            Marshal.StructureToPtr(new ExternalCoreApi.Variable { Key = keyPointer, Value = 9876 }, data, false);
            assert(data, keyPointer);
        }
        finally
        {
            if (data != nint.Zero) Marshal.FreeHGlobal(data);
            if (keyPointer != nint.Zero) Marshal.FreeCoTaskMem(keyPointer);
        }
    }
}

