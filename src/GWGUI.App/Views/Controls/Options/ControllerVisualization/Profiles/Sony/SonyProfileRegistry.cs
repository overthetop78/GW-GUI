using GWGUI.Emulation.Constants;

namespace GWGUI.App.Views.Controls.Options.ControllerVisualization;

internal static partial class ControllerArtworkCatalog
{
    private static void AddSonyProfiles(Dictionary<string, ProfileDefinition> profiles)
    {
        profiles[EmulationControllerVisualIds.SonyPlayStationController] = CreateSonyPlayStationController();
        profiles[EmulationControllerVisualIds.SonyDualShock1] = CreateSonyDualShock1();
        profiles[EmulationControllerVisualIds.SonyDualAnalog] = CreateSonyDualAnalog();
        profiles[EmulationControllerVisualIds.SonyAnalogJoystick] = CreateSonyAnalogJoystick();
        profiles[EmulationControllerVisualIds.SonyPlayStationMouse] = CreateSonyPlayStationMouse();
        profiles[EmulationControllerVisualIds.SonyNeGcon] = CreateSonyNeGcon();
        profiles[EmulationControllerVisualIds.SonyGunCon] = CreateSonyGunCon();
        profiles[EmulationControllerVisualIds.SonyGunCon2] = CreateSonyGunCon2();
        profiles[EmulationControllerVisualIds.SonyJustifier] = CreateSonyJustifier();
        profiles[EmulationControllerVisualIds.SonyDualShock2] = CreateSonyDualShock2();
        profiles[EmulationControllerVisualIds.SonyDualShock3] = CreateSonyDualShock3();
        profiles[EmulationControllerVisualIds.SonyDualShock4] = CreateSonyDualShock4();
        profiles[EmulationControllerVisualIds.SonyDualSense] = CreateSonyDualSense();
        profiles[EmulationControllerVisualIds.SonyPsp1000] = CreateSonyPsp1000();
    }
}
