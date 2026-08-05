namespace GWGUI.Domain.Profiles;

public interface IProfileStore<TProfile>
{
    IReadOnlyList<TProfile> GetAll();
    TProfile Save(TProfile profile, bool replaceExisting = false);
    void Rename(string id, string newName);
    void Delete(string id);
}

/// <summary>
/// Profile collection bound to one operation. A store can never receive or
/// return a profile belonging to another tab.
/// </summary>
public sealed class InMemoryProfileStore : IProfileStore<OperationProfile>
{
    private readonly OperationKind _operation;
    private readonly List<OperationProfile> _profiles;

    public InMemoryProfileStore(OperationKind operation, IEnumerable<OperationProfile>? userProfiles = null)
    {
        _operation = operation;
        _profiles = [OperationProfile.Default(operation)];
        if (userProfiles is not null)
        {
            var profiles = userProfiles.Where(profile => !profile.IsSystem).ToArray();
            if (profiles.Any(profile => profile.Operation != operation))
                throw new ArgumentException("A profile store can only contain profiles for its operation.", nameof(userProfiles));
            _profiles.AddRange(profiles);
        }
    }

    public IReadOnlyList<OperationProfile> GetAll() =>
        _profiles.OrderByDescending(profile => profile.IsSystem).ThenBy(profile => profile.Name).ToArray();

    public OperationProfile Save(OperationProfile profile, bool replaceExisting = false)
    {
        EnsureOperation(profile);
        if (profile.IsSystem) throw new InvalidOperationException("The system profile cannot be replaced.");
        var existing = _profiles.FindIndex(item => string.Equals(item.Name, profile.Name, StringComparison.CurrentCultureIgnoreCase));
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

    public void Rename(string id, string newName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(newName);
        var index = Find(id);
        var profile = _profiles[index];
        if (profile.IsSystem) throw new InvalidOperationException("The system profile cannot be renamed.");
        if (_profiles.Any(item => item.Id != id && string.Equals(item.Name, newName, StringComparison.CurrentCultureIgnoreCase)))
            throw new InvalidOperationException("A profile with this name already exists.");
        _profiles[index] = profile with { Name = newName.Trim() };
    }

    public void Delete(string id)
    {
        var index = Find(id);
        if (_profiles[index].IsSystem) throw new InvalidOperationException("The system profile cannot be deleted.");
        _profiles.RemoveAt(index);
    }

    private void EnsureOperation(OperationProfile profile)
    {
        if (profile.Operation != _operation)
            throw new ArgumentException($"Profile operation '{profile.Operation}' does not match store operation '{_operation}'.", nameof(profile));
    }

    private int Find(string id)
    {
        var index = _profiles.FindIndex(profile => profile.Id == id);
        return index >= 0 ? index : throw new KeyNotFoundException(id);
    }
}
