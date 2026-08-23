using GWGUI.App.Services.Input.GameInput;
using System.Reflection;
using System.Runtime.InteropServices;

namespace GWGUI.Tests;

public sealed class GameInputInteropSignatureTests
{
    [Fact]
    public void EveryGameInputComMethodPreservesTheOfficialNativeSignature()
    {
        var interfaces = typeof(IGameInput).Assembly.GetTypes()
            .Where(type => type.IsInterface && type.IsImport &&
                type.Namespace == typeof(IGameInput).Namespace &&
                type.Name.StartsWith("IGameInput", StringComparison.Ordinal));

        foreach (var type in interfaces)
        {
            foreach (var method in type.GetMethods())
            {
                Assert.True(
                    method.MethodImplementationFlags.HasFlag(MethodImplAttributes.PreserveSig),
                    $"{type.Name}.{method.Name} must preserve the GameInput.h ABI.");
            }
        }
    }

    [Fact]
    public void GetRawReportReturnsTheOfficialNativeBoolean()
    {
        var method = typeof(IGameInputReading).GetMethod(nameof(IGameInputReading.GetRawReport))!;

        Assert.Equal(typeof(bool), method.ReturnType);
        Assert.Equal(
            UnmanagedType.I1,
            method.ReturnParameter.GetCustomAttribute<MarshalAsAttribute>()!.Value);
    }

    [Fact]
    public void RegisteredRuntimeBeatsSystemFallbackEvenWhenSystemVersionIsHigher()
    {
        var selected = GameInputNative.SelectRuntime(
        [
            new("system", new Version(99, 0), GameInputRuntimeSource.SystemFallback),
            new("registered", new Version(3, 5, 268), GameInputRuntimeSource.Registered)
        ]);

        Assert.Equal("registered", selected);
    }

    [Fact]
    public void NewerAppLocalRuntimeBeatsOlderRegisteredRuntime()
    {
        var selected = GameInputNative.SelectRuntime(
        [
            new("registered", new Version(3, 5, 268), GameInputRuntimeSource.Registered),
            new("app-local", new Version(4, 0), GameInputRuntimeSource.AppLocal)
        ]);

        Assert.Equal("app-local", selected);
    }

    [Fact]
    public void RegisteredRuntimeWinsAnEqualVersionTieAndSystemRemainsFallback()
    {
        var version = new Version(3, 5, 268);
        Assert.Equal("registered", GameInputNative.SelectRuntime(
        [
            new("app-local", version, GameInputRuntimeSource.AppLocal),
            new("registered", version, GameInputRuntimeSource.Registered)
        ]));
        Assert.Equal("system", GameInputNative.SelectRuntime(
        [
            new("system", version, GameInputRuntimeSource.SystemFallback)
        ]));
    }
}
