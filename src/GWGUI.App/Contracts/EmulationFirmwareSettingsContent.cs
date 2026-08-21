using System.Windows;
using System.Windows.Controls;

namespace GWGUI.App.Controls;

internal sealed record EmulationFirmwareSettingsContent(
    UIElement ConfiguredFirmware,
    ListBox DetectedFirmware,
    Func<Button, Task> Refresh,
    Button UseSelected,
    Func<Button, Task> OpenFolder);
