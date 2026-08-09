namespace GWGUI.Domain.Profiles;

public interface IProfileStore<TProfile>
{
    IReadOnlyList<TProfile> GetAll();
    TProfile Save(TProfile profile, bool replaceExisting = false);
    void Rename(string id, string newName);
    void Delete(string id);
}
