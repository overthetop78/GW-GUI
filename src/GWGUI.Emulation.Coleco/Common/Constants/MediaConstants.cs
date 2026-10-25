using GWGUI.Emulation.Contracts;

namespace GWGUI.Emulation.Coleco.Common.Constants;

internal static class MediaConstants
{
    internal const int MaximumBatterySaveBytes = 16 * 1024 * 1024;
    internal const char ExtensionPrefix = '.';
    internal const char SupportedExtensionSeparator = '|';
    internal const string ExtensionListSeparator = ", ";
    internal const string BatterySaveExtension = ".sav";
    internal const uint BatteryMemoryId = 0;

    /// <summary>Emplacements qui imposent une recréation dans la politique de base du module.</summary>
    /// <remarks>Cette liste appartient au module et reste protégée contre les modifications. Une liste vide autorise les échanges à chaud de base.</remarks>
    internal static IReadOnlyList<EmulationMediaSlot> RestartSlots { get; } = Array.AsReadOnly<EmulationMediaSlot>([EmulationMediaSlot.Cartridge0]);
}
