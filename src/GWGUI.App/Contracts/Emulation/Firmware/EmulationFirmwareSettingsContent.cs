using System.Windows;
using System.Windows.Controls;

namespace GWGUI.App.Contracts.Emulation.Firmware;

internal sealed record EmulationFirmwareSettingsContent(
    UIElement ConfiguredFirmware,
    ListBox DetectedFirmware,
    Func<Button, Task> Refresh,
    Button UseSelected,
    Func<Button, Task> OpenFolder);
