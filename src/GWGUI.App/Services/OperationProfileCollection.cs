using GWGUI.Domain.Profiles;
using GWGUI.Domain.Settings;

namespace GWGUI.App.Services;

public sealed class OperationProfileCollection
{
    private readonly Dictionary<OperationKind, IProfileStore<OperationProfile>> _stores = [];

    public OperationProfileCollection()
    {
        Reset([]);
    }

    public IProfileStore<OperationProfile> For(OperationKind operation) =>
        _stores.TryGetValue(operation, out var store)
            ? store
            : throw new ArgumentOutOfRangeException(nameof(operation));

    public IReadOnlyList<OperationProfile> Localized(OperationKind operation, Func<string, string> localizeDefault) =>
        For(operation).GetAll()
            .Select(profile => profile.IsSystem ? profile with { Name = localizeDefault("Profile.Default") } : profile)
            .ToArray();

    public void Reset(IEnumerable<ProfileSettings> settings)
    {
        var profiles = settings.Select(ToProfile).ToArray();
        foreach (var operation in Enum.GetValues<OperationKind>())
            _stores[operation] = new InMemoryProfileStore(operation, profiles.Where(profile => profile.Operation == operation));
    }

    public List<ProfileSettings> Capture() => Enum.GetValues<OperationKind>()
        .SelectMany(operation => For(operation).GetAll())
        .Where(profile => !profile.IsSystem)
        .Select(profile => new ProfileSettings
        {
            Id = profile.Id,
            Operation = profile.Operation.ToString(),
            Name = profile.Name,
            Values = profile.Values.ToDictionary(),
            EnabledOptions = profile.EnabledOptions.ToHashSet()
        })
        .ToList();

    private static OperationProfile ToProfile(ProfileSettings value) => new(
        value.Id,
        Enum.TryParse<OperationKind>(value.Operation, out var operation) ? operation : OperationKind.Read,
        value.Name,
        value.Values,
        value.EnabledOptions);
}
