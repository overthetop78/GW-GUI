using GWGUI.Emulation.Interfaces;
using GWGUI.Emulation.Sony.Common.Exceptions;
using CoreHostFunctions = GWGUI.Emulation.Sony.Common.Functions.CoreHostFunctions;
using Pcee2Core = GWGUI.Emulation.Sony.Emulators.Pcee2.Constants.CoreConstants;

namespace GWGUI.Tests.Emulation.Sony;

public sealed class NativeStartupDiagnosticsTests
{
    [Fact]
    public void StartupFailureKeepsTheOriginalCauseAndTheNativeMessagesInOrder()
    {
        var cause = new InvalidOperationException("Load failed");
        string[] diagnostics = ["Opening GS device", "Native graphics error", Pcee2Core.InitializationFailureDiagnostic];

        var error = CoreHostFunctions.Messages.WithNativeDiagnostics(cause, diagnostics);

        Assert.Same(cause, error.InnerException);
        Assert.Equal(cause.Message + Environment.NewLine + string.Join(Environment.NewLine, diagnostics), error.Message);
        Assert.Contains("Native graphics error", error.ToString());
        Assert.Equal(3, diagnostics.Length);
    }

    [Fact]
    public void StartupFailureWithoutNativeMessagesKeepsTheOriginalException()
    {
        var cause = new ArgumentException("Invalid configuration");
        Assert.Same(cause, CoreHostFunctions.Messages.WithNativeDiagnostics(cause, []));
    }

    [Fact]
    public void FatalCaptureReturnsNormallyAndTheNextManagedCheckReportsTheNativeFailure()
    {
        var monitor = CreateMonitor(Pcee2Core.Definition.StartupFailureDiagnostics);
        var first = "[1] [PCSX2] " + Pcee2Core.InitializationFailureDiagnostic;
        monitor.Capture(first);
        monitor.Capture("Repeated: " + Pcee2Core.InitializationFailureDiagnostic);
        Assert.Equal(first, monitor.Failure);
        var error = Assert.Throws<InvalidOperationException>(monitor.ThrowIfFailed);
        Assert.Equal(first, error.InnerException!.Message);
        Assert.NotEqual(first, error.Message);
    }

    [Theory]
    [InlineData("[3] Failed to open or read NVRAM, creating default.")]
    [InlineData("[3] BIOS rom1 module not found, skipping...")]
    [InlineData("[1] VM subsystems initialized in 1870.00 ms")]
    [InlineData("[1] Opening GS device: renderer Software, API Vulkan")]
    public void RecoverableDiagnosticsDoNotBecomeFatal(string diagnostic)
    {
        var monitor = CreateMonitor(Pcee2Core.Definition.StartupFailureDiagnostics);
        monitor.Capture(diagnostic);
        Assert.Null(monitor.Failure);
        monitor.ThrowIfFailed();
    }

    [Fact]
    public void OtherBackendsKeepTheirOwnDiagnosticPolicy()
    {
        var definition = GWGUI.Emulation.Sony.Emulators.SwanStation.Constants.CoreConstants.Definition;
        var monitor = CreateMonitor(definition.StartupFailureDiagnostics);
        monitor.Capture(Pcee2Core.InitializationFailureDiagnostic);
        Assert.Null(monitor.Failure);
        monitor.ThrowIfFailed();
    }
    [Fact]
    public void AlreadyAttachedMessagesAreNotAddedAgain()
    {
        var cause = new InvalidOperationException("Load failed");
        string[] diagnostics = ["native-first", "native-second"];
        var first = CoreHostFunctions.Messages.WithNativeDiagnostics(cause, diagnostics);
        Assert.Same(first, CoreHostFunctions.Messages.WithNativeDiagnostics(first, diagnostics));
        Assert.Same(cause, first.InnerException);
    }

    [Fact]
    public void CaptureNeverInvokesTheFailureFactoryAndUnknownProfilesCanSupplyTheirOwnException()
    {
        var created = 0;
        var monitor = CoreHostFunctions.Messages.CreateCoreDiagnosticMonitor(["custom-failure"], diagnostic =>
        {
            created++;
            return new ArgumentException(diagnostic);
        });
        monitor.Capture("normal");
        monitor.Capture("custom-failure:first");
        monitor.Capture("custom-failure:second");
        Assert.Equal(0, created);
        Assert.Equal("custom-failure:first", monitor.Failure);
        Assert.Equal("custom-failure:first", Assert.Throws<ArgumentException>(monitor.ThrowIfFailed).Message);
        Assert.Equal(1, created);
    }

    private static IEmulationCoreDiagnosticMonitor CreateMonitor(IReadOnlyList<string>? signatures)
        => CoreHostFunctions.Messages.CreateCoreDiagnosticMonitor(signatures, static diagnostic => new InvalidOperationException(CoreExceptions.ContentRefused(), new InvalidOperationException(diagnostic)));
}
