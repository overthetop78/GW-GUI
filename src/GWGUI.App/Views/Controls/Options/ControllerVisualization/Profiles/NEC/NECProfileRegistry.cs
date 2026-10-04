using GWGUI.Emulation.Constants;

namespace GWGUI.App.Views.Controls.Options.ControllerVisualization;

internal static partial class ControllerArtworkCatalog
{
    private static void AddNECProfiles(Dictionary<string, ProfileDefinition> profiles)
    {
        profiles[EmulationControllerVisualIds.NecPcEnginePad] = CreateNecPcEnginePad();
        profiles[EmulationControllerVisualIds.NecPcEngineTurboPad] = CreateNecPcEngineTurboPad();
        profiles[EmulationControllerVisualIds.NecTurboGrafxTurboPad] = CreateNecTurboGrafxTurboPad();
        profiles[EmulationControllerVisualIds.NecCoreGrafxTurboPad] = CreateNecCoreGrafxTurboPad();
        profiles[EmulationControllerVisualIds.NecCoreGrafxIiTurboPad] = CreateNecCoreGrafxIiTurboPad();
        profiles[EmulationControllerVisualIds.NecDuoRTurboPad] = CreateNecDuoRTurboPad();
        profiles[EmulationControllerVisualIds.NecPcEngineTurboPadIi] = CreateNecPcEngineTurboPadIi();
        profiles[EmulationControllerVisualIds.NecPcEngineTurboStick] = CreateNecPcEngineTurboStick();
        profiles[EmulationControllerVisualIds.NecAvenuePad3] = CreateNecAvenuePad3();
        profiles[EmulationControllerVisualIds.NecAvenuePad6] = CreateNecAvenuePad6();
        profiles[EmulationControllerVisualIds.NecArcadePad6] = CreateNecArcadePad6();
        profiles[EmulationControllerVisualIds.NecTurboGrafxTurboStick] = CreateNecTurboGrafxTurboStick();
        profiles[EmulationControllerVisualIds.NecDuoPad] = CreateNecDuoPad();
        profiles[EmulationControllerVisualIds.NecCordlessPad] = CreateNecCordlessPad();
        profiles[EmulationControllerVisualIds.NecTurboExpressControls] = CreateNecTurboExpressControls();
        profiles[EmulationControllerVisualIds.NecPcEngineLtControls] = CreateNecPcEngineLtControls();
        profiles[EmulationControllerVisualIds.NecPcFxPad] = CreateNecPcFxPad();
        profiles[EmulationControllerVisualIds.NecPcEngineMouse] = CreateNecPcEngineMouse();
        profiles[EmulationControllerVisualIds.NecPcFxMouse] = CreateNecPcFxMouse();
    }
}
