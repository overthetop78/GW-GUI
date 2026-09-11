namespace GWGUI.MediaEngine.Conversion.Migration;

public interface IMigrationNamePolicy
{
    bool IsValid(string name);
}
