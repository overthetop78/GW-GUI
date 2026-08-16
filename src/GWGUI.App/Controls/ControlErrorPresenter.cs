using System.Windows;
using GWGUI.App.Localization;
using GWGUI.App.Services;

namespace GWGUI.App.Controls;

internal static class ControlErrorPresenter
{
    internal static string Describe(Exception error, string context)
    {
        var logPath = ErrorLog.Write(error, context);
        var detail = logPath is null
            ? LocExtension.Get("Common.Unknown")
            : LocExtension.Get("Error.LogSaved", logPath);
        return LocExtension.Get("Error.Unexpected", detail);
    }

    internal static void ShowUnexpected(FrameworkElement owner, Exception error, string context, string title) =>
        MessageBox.Show(Window.GetWindow(owner), Describe(error, context), title,
            MessageBoxButton.OK, MessageBoxImage.Error);
}

internal static class ControlErrorContexts
{
    internal const string AmigaConfigurationOpening = "Opening an Amiga configuration";
    internal const string AmigaConfiguration = "Amiga configuration";
    internal const string AtariConfiguration = "Atari configuration";
    internal const string AmigaCoreManagement = "Managing the external Amiga core";
    internal const string AtariCoreManagement = "Managing an external Atari core";
    internal const string AtariCoreOptions = "Reading Atari core options";
    internal const string AmigaEmulatorCommand = "Amiga emulator command";
}
