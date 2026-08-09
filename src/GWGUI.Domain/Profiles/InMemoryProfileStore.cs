namespace GWGUI.Domain.Profiles;

/// <summary>
/// Profile collection bound to one operation. A store can never receive or
/// return a profile belonging to another tab.
/// </summary>
public sealed class InMemoryProfileStore : IProfileStore<OperationProfile>
{
    private readonly OperationKind operation;
    private readonly List<OperationProfile> profiles;

    public InMemoryProfileStore(OperationKind operation, IEnumerable<OperationProfile>? userProfiles = null)
    {
        this.operation = operation;
        profiles = [OperationProfile.Default(operation)];
        if (userProfiles is null) return;
        var suppliedProfiles = userProfiles.Where(profile => !profile.IsSystem).ToArray();
        if (suppliedProfiles.Any(profile => profile.Operation != operation))
            throw new ArgumentException("A profile store can only contain profiles for its operation.", nameof(userProfiles));
        profiles.AddRange(suppliedProfiles);
    }

    public IReadOnlyList<OperationProfile> GetAll() =>
        profiles.OrderByDescending(profile => profile.IsSystem).ThenBy(profile => profile.Name).ToArray();

    public OperationProfile Save(OperationProfile profile, bool replaceExisting = false)
    {
        EnsureOperation(profile);
        if (profile.IsSystem) throw new InvalidOperationException("The system profile cannot be replaced.");
        var existing = profiles.FindIndex(item => string.Equals(item.Name, profile.Name, StringComparison.CurrentCultureIgnoreCase));
        if (existing >= 0)
        {
            if (!replaceExisting) throw new InvalidOperationException("A profile with this name already exists.");
            if (profiles[existing].IsSystem) throw new InvalidOperationException("The system profile cannot be replaced.");
            profile = profile with { Id = profiles[existing].Id };
            profiles[existing] = profile;
        }
        else profiles.Add(profile);
        return profile;
    }

    public void Rename(string id, string newName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(newName);
        var index = Find(id);
        var profile = profiles[index];
        if (profile.IsSystem) throw new InvalidOperationException("The system profile cannot be renamed.");
        if (profiles.Any(item => item.Id != id && string.Equals(item.Name, newName, StringComparison.CurrentCultureIgnoreCase)))
            throw new InvalidOperationException("A profile with this name already exists.");
        profiles[index] = profile with { Name = newName.Trim() };
    }

    public void Delete(string id)
    {
        var index = Find(id);
        if (profiles[index].IsSystem) throw new InvalidOperationException("The system profile cannot be deleted.");
        profiles.RemoveAt(index);
    }

    private void EnsureOperation(OperationProfile profile)
    {
        if (profile.Operation != operation)
            throw new ArgumentException($"Profile operation '{profile.Operation}' does not match store operation '{operation}'.", nameof(profile));
    }

    private int Find(string id)
    {
        var index = profiles.FindIndex(profile => profile.Id == id);
        return index >= 0 ? index : throw new KeyNotFoundException(id);
    }
}
