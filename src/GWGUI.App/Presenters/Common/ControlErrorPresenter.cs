using GWGUI.App.Constants.Presenters.Common;
using GWGUI.App.Contracts.Dialogs;
using GWGUI.App.Enums.Dialogs;
using GWGUI.App.Functions.Localization;
using GWGUI.App.Localization.Extensions;
using GWGUI.App.Services.Logging;
using GWGUI.App.Views.Dialogs.Common;
using System.Windows;
using System.Windows.Media;
using GWGUI.Emulation;


namespace GWGUI.App.Presenters.Common;

internal static class ControlErrorPresenter
{
    internal static string Describe(Exception error, string context)
    {
        ErrorLog.Write(error, context);
        return LocExtension.Get(ControlErrorPresenterConstants.UnexpectedResource,
            ExceptionDescriptionFunctions.Describe(error));
    }

    internal static void ShowUnexpected(FrameworkElement owner, Exception error, string context, string title) =>
        CommonErrorDialog.Show(owner, new CommonErrorDialogContent(title, Describe(error, context),
            CommonErrorDialog.ErrorIcon, Brushes.Firebrick));

    internal static void ShowEmulation(FrameworkElement owner, Exception error, string context, string machineName)
    {
        if (error is not EmulationMessageException messageError)
        {
            ShowUnexpected(owner, error, context, machineName);
            return;
        }
        ErrorLog.Write(messageError.InnerException ?? messageError, context);
        var message = messageError.MessageData;
        if (message.Target == EmulationMessageTarget.Silent) return;
        var description = MessageText(message);
        if (message.Target != EmulationMessageTarget.Dialog) return;
        var details = MessageDetails(message, machineName);
        var media = MessageMedia(message);
        CommonErrorDialog.Show(owner, new CommonErrorDialogContent(
            MessageHeading(message), description, CommonErrorDialog.ErrorIcon,
            MessageBrush(message.Severity), details, media));
    }

    private static string MessageHeading(EmulationMessage message) => message.MessageCode switch
    {
        EmulationMessageCode.RequiredMediaMissing =>
            LocExtension.Get(ControlErrorPresenterConstants.PowerFailureTitleResource),
        _ => LocExtension.Get(ControlErrorPresenterConstants.UnexpectedResource)
    };

    private static string MessageText(EmulationMessage message)
    {
        if (message.MessageCode == EmulationMessageCode.UntranslatedEmulatorMessage
            && !string.IsNullOrWhiteSpace(message.OriginalText)) return message.OriginalText;
        return message.MessageCode switch
        {
            EmulationMessageCode.RequiredMediaMissing =>
                LocExtension.Get(ControlErrorPresenterConstants.RequiredMediaMissingResource),
            EmulationMessageCode.EmulatorNotInstalled =>
                LocExtension.Get(ControlErrorPresenterConstants.EmulatorNotInstalledResource),
            EmulationMessageCode.EmulatorRejected =>
                LocExtension.Get(ControlErrorPresenterConstants.EmulatorRejectedResource),
            EmulationMessageCode.EmulatorUpdateBlocked =>
                LocExtension.Get(ControlErrorPresenterConstants.EmulatorUpdateBlockedResource),
            EmulationMessageCode.FirmwareMissing =>
                LocExtension.Get(ControlErrorPresenterConstants.FirmwareMissingResource),
            EmulationMessageCode.FirmwareIncompatible =>
                LocExtension.Get(ControlErrorPresenterConstants.FirmwareIncompatibleResource),
            EmulationMessageCode.MediaNotFound =>
                LocExtension.Get(ControlErrorPresenterConstants.MediaNotFoundResource),
            EmulationMessageCode.MediaUnsupported =>
                LocExtension.Get(ControlErrorPresenterConstants.MediaUnsupportedResource),
            EmulationMessageCode.OptionInvalid =>
                LocExtension.Get(ControlErrorPresenterConstants.OptionInvalidResource),
            EmulationMessageCode.HostCommunicationFailed =>
                LocExtension.Get(ControlErrorPresenterConstants.HostCommunicationFailedResource),
            EmulationMessageCode.SavedStateInvalid =>
                LocExtension.Get(ControlErrorPresenterConstants.SavedStateInvalidResource),
            EmulationMessageCode.SavedStateIncompatible =>
                LocExtension.Get(ControlErrorPresenterConstants.SavedStateIncompatibleResource),
            EmulationMessageCode.MachineStartFailed =>
                LocExtension.Get(ControlErrorPresenterConstants.MachineStartFailedResource),
            EmulationMessageCode.MediaOperationFailed =>
                LocExtension.Get(ControlErrorPresenterConstants.MediaOperationFailedResource),
            EmulationMessageCode.SavedStateOperationFailed =>
                LocExtension.Get(ControlErrorPresenterConstants.SavedStateOperationFailedResource),
            _ => LocExtension.Get(ControlErrorPresenterConstants.UnknownResource)
        };
    }

    private static IReadOnlyList<CommonErrorDialogDetail>? MessageDetails(EmulationMessage message,
        string machineName) => message.Context is IEmulationRequiredMediaMessageContext required
        ?
        [
            new CommonErrorDialogDetail(LocExtension.Get(ControlErrorPresenterConstants.MachineResource), machineName),
            new CommonErrorDialogDetail(LocExtension.Get(ControlErrorPresenterConstants.RequiredMediaResource),
                string.Join(" / ", required.RequiredMedia.Select(MediaName)))
        ]
        : null;

    private static IReadOnlyList<CommonErrorDialogMediaIcon>? MessageMedia(EmulationMessage message) =>
        message.Context is IEmulationRequiredMediaMessageContext required
            ? required.RequiredMedia.Select(MediaIcon).Distinct().ToArray()
            : null;

    private static string MediaName(EmulationMediaCategory category) => LocExtension.Get(category switch
    {
        EmulationMediaCategory.FloppyDrive => "Emulation.Storage.Floppy.Device",
        EmulationMediaCategory.HardDisk => "Emulation.Storage.HardDisk.Device",
        EmulationMediaCategory.CompactDiscDrive => "Emulation.Storage.CompactDiscs",
        EmulationMediaCategory.CartridgeSlot => "Emulation.Storage.Cartridges",
        EmulationMediaCategory.CassetteDrive => "Emulation.Storage.Cassettes",
        _ => "Emulation.Storage.Media.Associated"
    });

    private static CommonErrorDialogMediaIcon MediaIcon(EmulationMediaCategory category) => category switch
    {
        EmulationMediaCategory.FloppyDrive => CommonErrorDialogMediaIcon.Floppy,
        EmulationMediaCategory.HardDisk => CommonErrorDialogMediaIcon.HardDisk,
        EmulationMediaCategory.CompactDiscDrive => CommonErrorDialogMediaIcon.CompactDisc,
        EmulationMediaCategory.CartridgeSlot => CommonErrorDialogMediaIcon.Cartridge,
        EmulationMediaCategory.CassetteDrive => CommonErrorDialogMediaIcon.Cassette,
        _ => CommonErrorDialogMediaIcon.Floppy
    };

    private static Brush MessageBrush(EmulationMessageSeverity severity) => severity switch
    {
        EmulationMessageSeverity.Information => Brushes.DodgerBlue,
        EmulationMessageSeverity.Warning => Brushes.DarkOrange,
        _ => Brushes.Firebrick
    };

    internal static string DescribeDetailed(Exception error, string description, string context)
    {
        ErrorLog.Write(error, context);
        return description;
    }

    internal static void ShowDetailed(FrameworkElement owner, Exception error, string description,
        string context, string title, IReadOnlyList<CommonErrorDialogDetail>? details = null,
        IReadOnlyList<CommonErrorDialogMediaIcon>? mediaIcons = null) =>
        CommonErrorDialog.Show(owner, new CommonErrorDialogContent(title,
            DescribeDetailed(error, description, context),
            CommonErrorDialog.ErrorIcon, Brushes.Firebrick, details, mediaIcons));
}
