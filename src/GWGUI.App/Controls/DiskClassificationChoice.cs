using GWGUI.Domain.Formats;

namespace GWGUI.App.Controls;

/// <summary>Machine présentée par le sélecteur avec son état de détection pour l'image courante.</summary>
public sealed record DiskMachineChoice(string DisplayName, bool IsDetected);

/// <summary>Format présenté par le sélecteur avec son état de détection pour l'image courante.</summary>
public sealed record DiskFormatChoice(DiskFormat Format, bool IsDetected)
{
    public string DisplayName => Format.DisplayName;
}
