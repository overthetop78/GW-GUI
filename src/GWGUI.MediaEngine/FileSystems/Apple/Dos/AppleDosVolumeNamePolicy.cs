using GWGUI.MediaEngine.Migration;

namespace GWGUI.MediaEngine.FileSystems.Apple.Dos;

/// <summary>Valide le nom technique DOS-nnn qui représente le numéro de volume Apple DOS.</summary>
public sealed class AppleDosVolumeNamePolicy : IMigrationNamePolicy
{
    /// <inheritdoc />
    public bool IsValid(string name) => TryParse(name, out _);

    /// <summary>Extrait le numéro de volume d'un nom technique DOS-nnn.</summary>
    public static bool TryParse(string name, out byte volumeNumber)
    {
        volumeNumber = 0;
        return name.Length == AppleDosFileSystemLayout.VolumeNamePrefix.Length + 3 && name.StartsWith(AppleDosFileSystemLayout.VolumeNamePrefix, StringComparison.OrdinalIgnoreCase) && byte.TryParse(name.AsSpan(AppleDosFileSystemLayout.VolumeNamePrefix.Length), out volumeNumber) && volumeNumber < byte.MaxValue;
    }
}
