namespace GWGUI.MediaFileSystems.Migration;

public interface IMigrationNamePolicy
{
    bool IsValid(string name);
}
