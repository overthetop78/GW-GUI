using System.Windows;
using System.Windows.Media;
using GWGUI.App.Localization;
using GWGUI.App.Services;

namespace GWGUI.App.Controls;

internal static class ControlErrorPresenter
{
    internal static string Describe(Exception error, string context)
    {
        var logPath = ErrorLog.Write(error, context);
        var detail = logPath is null
            ? LocExtension.Get(ControlErrorPresenterConstants.UnknownResource)
            : LocExtension.Get(ControlErrorPresenterConstants.LogSavedResource, logPath);
        return LocExtension.Get(ControlErrorPresenterConstants.UnexpectedResource, detail);
    }

    internal static void ShowUnexpected(FrameworkElement owner, Exception error, string context, string title) =>
        CommonErrorDialog.Show(owner, new CommonErrorDialogContent(title, Describe(error, context),
            CommonErrorDialog.ErrorIcon, Brushes.Firebrick));

    internal static string DescribeDetailed(Exception error, string description, string context)
    {
        var logPath = ErrorLog.Write(error, context);
        if (logPath is null) return description;
        return description + Environment.NewLine + Environment.NewLine
            + LocExtension.Get(ControlErrorPresenterConstants.LogSavedResource, logPath);
    }

    internal static void ShowDetailed(FrameworkElement owner, Exception error, string description,
        string context, string title, IReadOnlyList<CommonErrorDialogDetail>? details = null,
        IReadOnlyList<CommonErrorDialogMediaIcon>? mediaIcons = null, bool showLogPath = true) =>
        CommonErrorDialog.Show(owner, new CommonErrorDialogContent(title,
            showLogPath ? DescribeDetailed(error, description, context) : LogWithoutPath(error, description, context),
            CommonErrorDialog.ErrorIcon, Brushes.Firebrick, details, mediaIcons));

    private static string LogWithoutPath(Exception error, string description, string context)
    {
        ErrorLog.Write(error, context);
        return description;
    }
}

internal static class ControlErrorPresenterConstants
{
    internal const string UnknownResource = "Common.Unknown";
    internal const string LogSavedResource = "Error.LogSaved";
    internal const string UnexpectedResource = "Error.Unexpected";
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
