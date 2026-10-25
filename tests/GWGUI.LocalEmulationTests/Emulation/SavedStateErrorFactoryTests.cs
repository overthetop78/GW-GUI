using System.IO;
using System.Reflection;
using GWGUI.Emulation.Enums;
using GWGUI.Emulation.Exceptions;

namespace GWGUI.Tests.Emulation;

public sealed class SavedStateErrorFactoryTests
{
    public static IEnumerable<object[]> Cases() =>
        new[] { "Panasonic", "Thomson", "Watara", "Wenquxing" }
            .SelectMany(family => new[] { false, true }
                .SelectMany(withCause => new[]
                {
                    EmulationMessageCode.SavedStateInvalid,
                    EmulationMessageCode.SavedStateIncompatible
                }.Select(code => new object[] { family, withCause, code })));

    [Theory]
    [MemberData(nameof(Cases))]
    public void SavedStateErrorsKeepFallbackMessageOrProvidedCause(
        string family, bool withCause, EmulationMessageCode code)
    {
        var assembly = Assembly.Load("gwgui.emulation." + family.ToLowerInvariant());
        var functions = assembly.GetType("GWGUI.Emulation." + family
            + ".Common.Machines.Common.Functions.SavedStateFunctions", throwOnError: true)!;
        var invalid = functions.GetMethod("Invalid", BindingFlags.Static | BindingFlags.NonPublic)!;
        const string message = "état incompatible";
        var cause = withCause ? new InvalidOperationException("provided cause") : null;
        var error = Assert.IsType<EmulationMessageException>(
            invalid.Invoke(null, [code, message, cause]));

        Assert.Equal(EmulationMessageCategory.SavedState, error.MessageData.Category);
        Assert.Equal(code, error.MessageData.MessageCode);
        Assert.Equal(EmulationMessageSeverity.Error, error.MessageData.Severity);
        Assert.Equal(EmulationMessageTarget.Dialog, error.MessageData.Target);
        Assert.Null(error.MessageData.Context);
        Assert.Null(error.MessageData.OriginalText);
        if (withCause)
            Assert.Same(cause, error.InnerException);
        else
            Assert.Equal(message, Assert.IsType<InvalidDataException>(error.InnerException).Message);
    }
}
