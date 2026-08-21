using System.Windows.Controls;

namespace GWGUI.App.Controls;

internal sealed record EmulationMemorySettingsContent(
    IReadOnlyList<EmulationSettingsControlField> MainMemory,
    TextBlock MainMemoryHint,
    IReadOnlyList<EmulationSettingsControlField> MemoryExtensions,
    TextBlock MemoryExtensionsHint,
    TextBlock TotalMemory);
