using GWGUI.MediaFileSystems.Conversion.Migration;

namespace GWGUI.MediaFileSystems.Fat12;

public sealed class Fat12VolumeNamePolicy : IMigrationNamePolicy
{
    public bool IsValid(string name) => name.Length <= FatBootSectorLayout.VolumeLabelLength && name.All(value => value is >= 'A' and <= 'Z' or >= '0' and <= '9' || value == ' ');
}
