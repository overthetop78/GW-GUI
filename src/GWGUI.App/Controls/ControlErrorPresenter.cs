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
