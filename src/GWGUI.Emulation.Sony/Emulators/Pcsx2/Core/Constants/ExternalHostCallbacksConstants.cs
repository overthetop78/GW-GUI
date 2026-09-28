using GWGUI.Emulation.Sony.Emulators.Pcsx2.Constants;
using GWGUI.Emulation.Sony.Emulators.Pcsx2.Contracts;
using GWGUI.Emulation.Sony.Emulators.Pcsx2.Factories;
using GWGUI.Emulation.Sony.Emulators.Pcsx2.Functions;
using GWGUI.Emulation.Sony.Emulators.Pcsx2.Services;

namespace GWGUI.Emulation.Sony.Emulators.Pcsx2.Constants;

internal static class ExternalHostCallbacksConstants
{
    internal const string Extended = "-extended";
    internal const uint JoypadDevice = 1;
    internal const uint MouseDevice = 2;
    internal const uint KeyboardDevice = 3;
    internal const uint AnalogDevice = 5;
    internal const uint PointerDevice = 6;
    internal const uint JoypadMask = 256;
    internal const uint PointerX = 0;
    internal const uint PointerY = 1;
    internal const uint PointerPressed = 2;
    internal const int PointerCoordinateScale = 128;
    internal const int PointerCoordinateCenter = 1;
    internal const int PointerCoordinateMinimum = -32767;
    internal const int PointerCoordinateMaximum = 32767;
    internal const int CoreOptionPointerFieldsBeforeValues = 6;
    internal const int CoreOptionValueFieldCount = 2;
    internal const int CoreOptionTerminatorFieldCount = 1;
    internal const int CoreOptionDescriptionPointerIndex = 3;
    internal const int CoreOptionCategoryPointerIndex = 5;
    internal const int MaximumCoreOptionDefinitions = 1024;
    internal const int MaximumCoreOptionValues = 128;
}


