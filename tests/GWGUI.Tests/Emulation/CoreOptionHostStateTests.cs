using System.Runtime.InteropServices;
using GWGUI.Emulation.Functions;
using GWGUI.Emulation.Interop;
using GWGUI.Emulation.Services;
using AcornHost = GWGUI.Emulation.Acorn.Emulators.Common.Interop.Services.CoreOptionHost;
using AcornOption = GWGUI.Emulation.Contracts.EmulationResourceRestartOption;
using MattelHost = GWGUI.Emulation.Mattel.Emulators.Common.Interop.Services.CoreOptionHost;
using MattelOption = GWGUI.Emulation.Contracts.EmulationTimingOption;

namespace GWGUI.Tests.Emulation;

public sealed class CoreOptionHostStateTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(-1)]
    [InlineData(int.MaxValue)]
    public void NullUpdateQueryPreservesTheFlagUntilARealDestinationConsumesIt(int initial)
    {
        var updated = initial;
        Assert.False(EmulationCoreOptionFunctions.GetAndClearUpdated(nint.Zero, ref updated));
        Assert.Equal(initial, updated);

        WithByte(pointer =>
        {
            Marshal.WriteByte(pointer, 0xa5);
            Assert.True(EmulationCoreOptionFunctions.GetAndClearUpdated(pointer, ref updated));
            Assert.Equal(initial == 0 ? 0 : 1, Marshal.ReadByte(pointer));
            Assert.Equal(0, updated);

            Marshal.WriteByte(pointer, 0xa5);
            Assert.True(EmulationCoreOptionFunctions.GetAndClearUpdated(pointer, ref updated));
            Assert.Equal(0, Marshal.ReadByte(pointer));
            Assert.Equal(0, updated);
        });
    }

    [Fact]
    public void ConfiguredValuesRemainAvailableUntilExplicitValidation()
    {
        using var host = CreateHost(new Dictionary<string, string> { ["mode"] = "unsupported", ["other"] = "kept" });
        host.ReplaceCatalog([Mode()]);

        Assert.Equal("unsupported", host.Catalog[0].CurrentValue);
        Assert.Equal("kept", host.DocumentValues["other"]);
        Assert.Throws<ArgumentException>(() => host.ValidateConfiguredValues(
            (key, value) => throw new ArgumentException(key + ":" + value)));
        Assert.True(host.TrySetValue("mode", "fast"));
        host.ValidateConfiguredValues((_, _) => throw new InvalidOperationException());
    }

    [Fact]
    public void RejectedWritesDoNotChangeValuesAndEveryAcceptedWriteMarksUpdated()
    {
        using var host = CreateHost();
        Assert.False(host.TrySetValue("mode", "normal"));
        host.ReplaceCatalog([Mode(), new Option("free", "", "", [])]);
        Assert.False(host.TrySetValue("mode", "unsupported"));
        Assert.False(host.TrySetValue("MODE", "fast"));
        Assert.Equal("normal", host.DocumentValues["mode"]);

        WithByte(pointer =>
        {
            Assert.True(host.GetAndClearUpdated(pointer));
            Assert.Equal(0, Marshal.ReadByte(pointer));
            Assert.True(host.TrySetValue("mode", "normal"));
            Assert.True(host.GetAndClearUpdated(pointer));
            Assert.Equal(1, Marshal.ReadByte(pointer));
            Assert.True(host.GetAndClearUpdated(pointer));
            Assert.Equal(0, Marshal.ReadByte(pointer));
            Assert.True(host.TrySetValue("free", "arbitrary"));
        });
        Assert.Equal("arbitrary", host.DocumentValues["free"]);
    }

    [Fact]
    public void NativeVariablesReturnTheCurrentUtf8ValueAndAnUnknownKeyReturnsNull()
    {
        using var host = CreateHost();
        host.ReplaceCatalog([Mode()]);
        using var key = new ExternalCoreUtf8String("mode");
        using var value = new ExternalCoreUtf8String("fast");
        using var unknown = new ExternalCoreUtf8String("unknown");

        WithNative(new ExternalCoreApi.Variable { Key = key.Pointer, Value = value.Pointer }, pointer =>
        {
            Assert.True(host.SetNativeValue(pointer));
            Assert.True(host.ReturnValue(pointer));
            var variable = Marshal.PtrToStructure<ExternalCoreApi.Variable>(pointer);
            Assert.Equal("fast", Marshal.PtrToStringUTF8(variable.Value));
        });
        WithNative(new ExternalCoreApi.Variable { Key = unknown.Pointer, Value = value.Pointer }, pointer =>
        {
            Assert.True(host.ReturnValue(pointer));
            Assert.Equal(nint.Zero, Marshal.PtrToStructure<ExternalCoreApi.Variable>(pointer).Value);
        });
        Assert.True(host.ReturnValue(nint.Zero));
        Assert.False(host.SetNativeValue(nint.Zero));
        Assert.False(host.GetAndClearUpdated(nint.Zero));
        Assert.False(host.ApplyVisibility(nint.Zero));
        Assert.False(host.CaptureDisplayUpdate(nint.Zero));
    }

    [Theory]
    [InlineData(false, true)]
    [InlineData(true, false)]
    public void InitialVisibilityUsesTheModulePolicy(bool preserveDeclaredVisibility, bool expected)
    {
        using var host = CreateHost(preserveDeclaredVisibility: preserveDeclaredVisibility);
        host.ReplaceCatalog([Mode() with { IsVisible = false }]);
        Assert.Equal(expected, host.Catalog[0].IsVisible);
    }

    [Fact]
    public void PresentationSeesTheSelectedValueAndNativeVisibilityKeepsHiddenOptionsHidden()
    {
        string? presentedValue = null;
        using var host = CreateHost(new Dictionary<string, string> { ["mode"] = "fast" },
            exposed: _ => false, present: option =>
            {
                presentedValue = option.CurrentValue;
                return option;
            });
        host.ReplaceCatalog([Mode()]);
        Assert.Equal("fast", presentedValue);
        Assert.False(host.Catalog[0].IsVisible);
        using var key = new ExternalCoreUtf8String("mode");
        WithNative(new ExternalCoreApi.CoreOptionDisplay { Key = key.Pointer, Visible = true }, pointer =>
            Assert.True(host.ApplyVisibility(pointer)));
        Assert.False(host.Catalog[0].IsVisible);
    }

    [Fact]
    public void DisplayCallbackRunsAfterTheValueCatalogAndUpdatedFlagAreChanged()
    {
        using var host = CreateHost();
        host.ReplaceCatalog([Mode()]);
        var called = 0;
        WithByte(updated =>
        {
            ExternalCoreApi.UpdateCoreOptionsDisplay callback = () =>
            {
                called++;
                Assert.Equal("fast", host.DocumentValues["mode"]);
                Assert.Equal("fast", host.Catalog[0].CurrentValue);
                Assert.True(host.GetAndClearUpdated(updated));
                Assert.Equal(1, Marshal.ReadByte(updated));
                return true;
            };
            try
            {
                WithNative(new ExternalCoreApi.CoreOptionsUpdateDisplayCallback
                {
                    Callback = Marshal.GetFunctionPointerForDelegate(callback)
                }, pointer => Assert.True(host.CaptureDisplayUpdate(pointer)));
                Assert.True(host.TrySetValue("mode", "fast"));
            }
            finally { GC.KeepAlive(callback); }
        });
        Assert.Equal(1, called);
    }

    [Fact]
    public void AcornAndMattelRetainTheirDifferentConstructorValidationOrder()
    {
        var acorn = new AcornOption("same", "Same", null, null, "normal", "normal", []);
        var mattel = new MattelOption("same", "Same", null, null, "normal", "normal", []);
        Assert.Throws<ArgumentException>(() =>
        {
            using var host = new AcornHost(null!, [acorn, acorn]);
        });
        Assert.Throws<ArgumentNullException>(() =>
        {
            using var host = new MattelHost(null!, [mattel, mattel]);
        });
    }

    private static Option Mode() => new("mode", "normal", "normal", ["normal", "fast"]);

    private static EmulationCoreOptionHost<Option, string> CreateHost(
        IReadOnlyDictionary<string, string>? configured = null, bool preserveDeclaredVisibility = true,
        Func<string, bool>? exposed = null, Func<Option, Option>? present = null) =>
        new(configured ?? new Dictionary<string, string>(), static option => option.Key,
            static option => option.DefaultValue, static option => option.Values, static value => value,
            static (option, current) => option with { CurrentValue = current },
            (option, visible) => option with { IsVisible = visible && (exposed?.Invoke(option.Key) ?? true) },
            (option, current, visible) => option with
            {
                CurrentValue = current,
                IsVisible = (!preserveDeclaredVisibility || option.IsVisible)
                    && (exposed?.Invoke(option.Key) ?? true) && visible
            }, present);

    private static void WithNative<T>(T value, Action<nint> action) where T : unmanaged
    {
        var pointer = Marshal.AllocCoTaskMem(Marshal.SizeOf<T>());
        try
        {
            Marshal.StructureToPtr(value, pointer, false);
            action(pointer);
        }
        finally { Marshal.FreeCoTaskMem(pointer); }
    }

    private static void WithByte(Action<nint> action)
    {
        var pointer = Marshal.AllocCoTaskMem(1);
        try
        {
            Marshal.WriteByte(pointer, 0);
            action(pointer);
        }
        finally { Marshal.FreeCoTaskMem(pointer); }
    }

    private sealed record Option(string Key, string DefaultValue, string CurrentValue,
        IReadOnlyList<string> Values, bool IsVisible = true);
}
