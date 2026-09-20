using GWGUI.MediaEngine.Conversion.Migration;

namespace GWGUI.MediaEngine.Conversion.Migration.Apple;

/// <summary>Valide le nom technique DOS-nnn qui représente le numéro de volume Apple DOS.</summary>
public sealed class AppleDosVolumeNamePolicy : IMigrationNamePolicy
{
    /// <inheritdoc />
    public bool IsValid(string name) => TryParse(name, out _);

    /// <summary>Extrait le numéro de volume d'un nom technique DOS-nnn.</summary>
    public static bool TryParse(string name, out byte volumeNumber)
        => GWGUI.MediaFileSystems.FileSystems.Apple.Dos.AppleDosVolumeNamePolicy.TryParse(name, out volumeNumber);
}
