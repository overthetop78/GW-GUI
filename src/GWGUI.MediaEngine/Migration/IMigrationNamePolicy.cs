namespace GWGUI.MediaEngine.Migration;

public interface IMigrationNamePolicy
{
    bool IsValid(string name);
}
