namespace GWGUI.MediaFileSystems.Conversion.Migration;

public interface IMigrationNamePolicy
{
    bool IsValid(string name);
}
