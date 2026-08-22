using GWGUI.App.Enums.Dialogs;
using System.Windows.Media;

namespace GWGUI.App.Contracts.Dialogs;

internal sealed record CommonErrorDialogContent(
    string Heading, string Message, string Icon, Brush IconBrush,
    IReadOnlyList<CommonErrorDialogDetail>? Details = null,
    IReadOnlyList<CommonErrorDialogMediaIcon>? MediaIcons = null);
