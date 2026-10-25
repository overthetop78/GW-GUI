namespace GWGUI.Emulation.Panasonic.Emulators.Opera.Interop.Enums;

internal enum NativeDevice : uint { None = 0, Joypad = 1, Mouse = 2, Lightgun = 4, Analog = 5, Flightstick = 257, ArcadeLightgun = 260, OrbatakTrackball = 513 }
internal enum NativeButton : uint { B = 0, Y = 1, Select = 2, Start = 3, Up = 4, Down = 5, Left = 6, Right = 7, A = 8, X = 9, L = 10, R = 11, L2 = 12, R2 = 13 }
internal enum NativeMouseControl : uint { X = 0, Y = 1, Left = 2, Right = 3, Middle = 6, Button4 = 9 }
internal enum NativeLightgunControl : uint { Trigger = 2, AuxiliaryA = 3, Start = 6, Select = 7, ScreenX = 13, ScreenY = 14, Offscreen = 15, Reload = 16 }
