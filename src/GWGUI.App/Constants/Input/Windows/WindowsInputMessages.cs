namespace GWGUI.App.Constants.Input.Windows;

internal static class WindowsInputMessages
{
    internal const int KeyDown = 0x0100, KeyUp = 0x0101, SystemKeyDown = 0x0104, SystemKeyUp = 0x0105;
    internal const int SetCursor = 0x0020, MouseMove = 0x0200;
    internal const int LeftButtonDown = 0x0201, LeftButtonUp = 0x0202;
    internal const int RightButtonDown = 0x0204, RightButtonUp = 0x0205;
    internal const int MiddleButtonDown = 0x0207, MiddleButtonUp = 0x0208;
    internal const int MouseWheel = 0x020A, XButtonDown = 0x020B, XButtonUp = 0x020C;
    internal const int MouseHorizontalWheel = 0x020E;
    internal const int LeftMouseVirtualKey = 0x01;
    internal const int RightMouseVirtualKey = 0x02;
    internal const int MiddleMouseVirtualKey = 0x04;
    internal const int FirstExtendedMouseVirtualKey = 0x05;
    internal const int SecondExtendedMouseVirtualKey = 0x06;
    internal const int NeutralWheelDelta = 0;
    internal const int WheelHighWordShift = 16;
    internal const long UnsignedWordMask = 0xffff;
}
