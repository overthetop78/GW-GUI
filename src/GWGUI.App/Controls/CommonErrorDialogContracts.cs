using System.Windows.Media;

namespace GWGUI.App.Controls;

internal sealed record CommonErrorDialogDetail(string Label, string Value);

internal enum CommonErrorDialogMediaIcon { Floppy, HardDisk, CompactDisc, Cartridge, Cassette }

internal sealed record CommonErrorDialogContent(
    string Heading, string Message, string Icon, Brush IconBrush,
    IReadOnlyList<CommonErrorDialogDetail>? Details = null,
    IReadOnlyList<CommonErrorDialogMediaIcon>? MediaIcons = null);
