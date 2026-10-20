using System.Reflection;
using GWGUI.Emulation.Contracts;
using GWGUI.Emulation.Enums;
using GWGUI.Emulation.Exceptions;
using GWGUI.Emulation.Services;

namespace GWGUI.Tests.Emulation;

public sealed class MachineErrorFactoryTests
{
    [Theory]
    [InlineData("")]
    [InlineData(" adapter-with-spaces ")]
    [InlineData("Émulateur-日本語")]
    public void MissingEmulatorKeepsOpaqueIdentityAndIndependentMessageContexts(string emulatorId)
    {
        var first = EmulationErrorService.EmulatorNotInstalled(emulatorId);
        var second = EmulationErrorService.EmulatorNotInstalled(emulatorId);
        Assert.NotSame(first, second);
        Assert.NotSame(first.MessageData, second.MessageData);
        Assert.NotSame(first.MessageData.Context, second.MessageData.Context);
        Assert.Equal(first.MessageData, second.MessageData);
        Assert.Same(emulatorId, Assert.IsType<EmulationEmulatorMessageContext>(first.MessageData.Context).EmulatorId);
        Assert.Equal(EmulationMessageCategory.Emulator, first.MessageData.Category);
        Assert.Equal(EmulationMessageCode.EmulatorNotInstalled, first.MessageData.MessageCode);
        Assert.Equal(EmulationMessageSeverity.Error, first.MessageData.Severity);
        Assert.Equal(EmulationMessageTarget.Dialog, first.MessageData.Target);
        Assert.Null(first.MessageData.OriginalText);
        Assert.Null(first.InnerException);
    }

    public static IEnumerable<object[]> Modules() =>
        new[] { "Amstrad", "Commodore", "Microsoft", "Nec", "Nintendo", "Sega", "Sony" }
            .SelectMany(family => new[] { false, true }.Select(withCause => new object[] { family, withCause }));

    [Theory]
    [MemberData(nameof(Modules))]
    public void FourMachineErrorsKeepTheirOriginalMessagesAndCause(string family, bool withCause)
    {
        var assembly = Assembly.Load("gwgui.emulation." + family.ToLowerInvariant());
        var type = assembly.GetType("GWGUI.Emulation." + family
            + ".Common.Machines.Common.Exceptions.MachineExceptions", throwOnError: true)!;
        var cause = withCause ? new InvalidOperationException("original cause") : null;
        var cases = new[]
        {
            ("SavedStateInvalid", EmulationMessageCategory.SavedState, EmulationMessageCode.SavedStateInvalid),
            ("SavedStateIncompatible", EmulationMessageCategory.SavedState, EmulationMessageCode.SavedStateIncompatible),
            ("MediaNotFound", EmulationMessageCategory.Media, EmulationMessageCode.MediaNotFound),
            ("MachineNotRunning", EmulationMessageCategory.Machine, EmulationMessageCode.MachineStartFailed)
        };
        foreach (var (name, category, code) in cases)
        {
            var method = type.GetMethod(name, BindingFlags.Static | BindingFlags.NonPublic)!;
            var error = Assert.IsType<EmulationMessageException>(method.Invoke(null, [cause]));
            Assert.Same(cause, error.InnerException);
            Assert.Equal(category, error.MessageData.Category);
            Assert.Equal(code, error.MessageData.MessageCode);
            Assert.Equal(EmulationMessageSeverity.Error, error.MessageData.Severity);
            Assert.Equal(EmulationMessageTarget.Dialog, error.MessageData.Target);
            Assert.Null(error.MessageData.Context);
            Assert.Null(error.MessageData.OriginalText);
        }
    }

    [Fact]
    public void SharedFactoryCreatesIndependentExceptionAndMessageInstances()
    {
        var cause = new InvalidOperationException("cause");
        var first = EmulationErrorService.CreateError(
            EmulationMessageCategory.Media, EmulationMessageCode.MediaNotFound, cause);
        var second = EmulationErrorService.CreateError(
            EmulationMessageCategory.Media, EmulationMessageCode.MediaNotFound, cause);
        Assert.NotSame(first, second);
        Assert.NotSame(first.MessageData, second.MessageData);
        Assert.Equal(first.MessageData, second.MessageData);
        Assert.Same(cause, first.InnerException);
        Assert.Same(cause, second.InnerException);
    }
}
