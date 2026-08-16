using System.Windows.Input;

namespace GWGUI.App.Controls;

internal static class AtariAccessibilityConstants
{
    internal const int ConfigurationListTabIndex = 0;
    internal const int ModelTabIndex = 1;
    internal const int FirstEditorTabIndex = 2;
    internal const string ConfigurationListResource = "Emulation.Configuration";
    internal const string ConfigurationTabsResource = "Emulation.Configuration";
    internal const string FirmwareStatusResource = "Menu.Firmware";
    internal const string ControllerStatusResource = "Emulation.ControllersTab";
    internal const string MediaStatusResource = "Emulation.StorageDevices";
    internal const ModifierKeys CommandModifier = ModifierKeys.Control;
    internal const Key NewConfigurationKey = Key.N;
    internal const Key SaveConfigurationKey = Key.S;
    internal const Key RefreshConfigurationKey = Key.F5;
    internal const Key DeleteConfigurationKey = Key.Delete;
    internal const string NewConfigurationAccelerator = "Ctrl+N";
    internal const string SaveConfigurationAccelerator = "Ctrl+S";
    internal const string RefreshConfigurationAccelerator = "F5";
    internal const string DeleteConfigurationAccelerator = "Delete";
}
