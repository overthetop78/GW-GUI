namespace GWGUI.Domain.Profiles;

public interface IProfileStore
{
    IReadOnlyList<OperationProfile> Get(OperationKind operation);
    OperationProfile Save(OperationProfile profile, bool replaceExisting = false);
    void Rename(OperationKind operation, string id, string newName);
    void Delete(OperationKind operation, string id);
}

public sealed class InMemoryProfileStore : IProfileStore
{
    private readonly List<OperationProfile> _profiles;

    public InMemoryProfileStore(IEnumerable<OperationProfile>? userProfiles = null)
    {
        _profiles = [OperationProfile.Default(OperationKind.Read), OperationProfile.Default(OperationKind.Write), OperationProfile.Default(OperationKind.Convert)];
        if (userProfiles is not null) _profiles.AddRange(userProfiles.Where(x => !x.IsSystem));
    }

    public IReadOnlyList<OperationProfile> Get(OperationKind operation) =>
        _profiles.Where(x => x.Operation == operation).OrderByDescending(x => x.IsSystem).ThenBy(x => x.Name).ToArray();

    public OperationProfile Save(OperationProfile profile, bool replaceExisting = false)
    {
        if (profile.IsSystem) throw new InvalidOperationException("The system profile cannot be replaced.");
        var existing = _profiles.FindIndex(x => x.Operation == profile.Operation && string.Equals(x.Name, profile.Name, StringComparison.CurrentCultureIgnoreCase));
        if (existing >= 0)
        {
            if (!replaceExisting) throw new InvalidOperationException("A profile with this name already exists.");
            if (_profiles[existing].IsSystem) throw new InvalidOperationException("The system profile cannot be replaced.");
            profile = profile with { Id = _profiles[existing].Id };
            _profiles[existing] = profile;
        }
        else _profiles.Add(profile);
        return profile;
    }

    public void Rename(OperationKind operation, string id, string newName)
    {
        var index = Find(operation, id);
        var profile = _profiles[index];
        if (profile.IsSystem) throw new InvalidOperationException("The system profile cannot be renamed.");
        if (_profiles.Any(x => x.Operation == operation && x.Id != id && string.Equals(x.Name, newName, StringComparison.CurrentCultureIgnoreCase)))
            throw new InvalidOperationException("A profile with this name already exists.");
        _profiles[index] = profile with { Name = newName.Trim() };
    }

    public void Delete(OperationKind operation, string id)
    {
        var index = Find(operation, id);
        if (_profiles[index].IsSystem) throw new InvalidOperationException("The system profile cannot be deleted.");
        _profiles.RemoveAt(index);
    }

    private int Find(OperationKind operation, string id)
    {
        var index = _profiles.FindIndex(x => x.Operation == operation && x.Id == id);
        return index >= 0 ? index : throw new KeyNotFoundException(id);
    }
}
