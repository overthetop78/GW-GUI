namespace GWGUI.MediaEngine.Definitions;

/// <summary>Regroupe les préfixes qui identifient les familles de formats Apple prises en charge.</summary>
public static class AppleDiskImageFormatFamilies
{
    /// <summary>Indique si l'identifiant appartient à une famille Apple II, Apple III, Lisa ou Macintosh.</summary>
    /// <param name="formatId">Identifiant à examiner, ou <see langword="null"/>.</param>
    /// <returns><see langword="true"/> lorsque l'identifiant appartient à une famille Apple connue.</returns>
    public static bool Contains(string? formatId) => formatId?.StartsWith(DiskImageFormatIds.AppleIIPrefix, StringComparison.OrdinalIgnoreCase) == true || formatId?.StartsWith(DiskImageFormatIds.AppleIIIPrefix, StringComparison.OrdinalIgnoreCase) == true || formatId?.StartsWith(DiskImageFormatIds.AppleLisaPrefix, StringComparison.OrdinalIgnoreCase) == true || formatId?.StartsWith(DiskImageFormatIds.AppleMacPrefix, StringComparison.OrdinalIgnoreCase) == true || formatId?.StartsWith(DiskImageFormatIds.MacPrefix, StringComparison.OrdinalIgnoreCase) == true;
}
