using System.Windows;

namespace GWGUI.App.Constants.Emulation;

public static class EmulationControllerSettingsConstants
{
    internal const string BehaviorResourceKey = "Emulation.Input.Behavior";
    internal const double MappingHeight = 520;
    public static GridLength StateIconColumnWidth { get; } = new(104);
    internal const double BehaviorWidth = 150;
}
