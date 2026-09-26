using GWGUI.Emulation.Amiga.Emulators.PUAE.Constants;
using GWGUI.Emulation.Amiga.Emulators.PUAE.Contracts;
using GWGUI.Emulation.Amiga.Emulators.PUAE.Factories;
using GWGUI.Emulation.Amiga.Emulators.PUAE.Functions;
using GWGUI.Emulation.Amiga.Emulators.PUAE.Services;

namespace GWGUI.Emulation.Amiga.Emulators.PUAE.Constants;

internal static class ExternalHostCallbacksConstants
{
    internal const string OptionKickstart = "puae_kickstart";
    internal const string Extended = "-extended";
    internal const uint JoypadDevice = 1;
    internal const uint MouseDevice = 2;
    internal const uint KeyboardDevice = 3;
    internal const uint AnalogDevice = 5;
    internal const uint JoypadMask = 256;
    internal const int CoreOptionPointerFieldsBeforeValues = 6;
    internal const int CoreOptionValueFieldCount = 2;
    internal const int CoreOptionTerminatorFieldCount = 1;
    internal const int CoreOptionDescriptionPointerIndex = 3;
    internal const int CoreOptionCategoryPointerIndex = 5;
    internal const int MaximumCoreOptionDefinitions = 1024;
    internal const int MaximumCoreOptionValues = 128;
}
