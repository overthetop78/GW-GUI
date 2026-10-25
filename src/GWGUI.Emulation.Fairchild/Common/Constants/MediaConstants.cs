using GWGUI.Emulation.Contracts;

namespace GWGUI.Emulation.Fairchild.Common.Constants;

internal static class MediaConstants
{
    internal const char ExtensionPrefix = '.';
    internal const char SupportedExtensionSeparator = '|';
    internal const string ExtensionListSeparator = ", ";

    /// <summary>Emplacements qui imposent une recréation dans la politique de base du module.</summary>
    /// <remarks>Cette liste appartient au module et reste protégée contre les modifications. Une liste vide autorise les échanges à chaud de base.</remarks>
    internal static IReadOnlyList<EmulationMediaSlot> RestartSlots { get; } = Array.AsReadOnly<EmulationMediaSlot>([EmulationMediaSlot.Cartridge0]);
}
