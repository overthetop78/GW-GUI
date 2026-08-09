using System.Collections.ObjectModel;
using GWGUI.Domain.Settings;

namespace GWGUI.App.Options;

internal sealed class ProfileOptionsState
{
    public ProfileOptionsState(IEnumerable<ProfileSettings> profiles)
    {
        foreach (var profile in profiles)
            For(profile.Operation).Add(new ProfileOptionRow(profile.Id, profile.Operation, profile.Name, false));
    }

    public ObservableCollection<ProfileOptionRow> Read { get; } = [];
    public ObservableCollection<ProfileOptionRow> Write { get; } = [];
    public ObservableCollection<ProfileOptionRow> Convert { get; } = [];

    public ObservableCollection<ProfileOptionRow> For(string operation) => operation switch
    {
        "Read" => Read,
        "Write" => Write,
        "Convert" => Convert,
        _ => throw new ArgumentOutOfRangeException(nameof(operation))
    };

    public bool ContainsName(ProfileOptionRow row, string name) =>
        For(row.Operation).Any(item =>
            item.Id != row.Id &&
            string.Equals(item.Name, name, StringComparison.CurrentCultureIgnoreCase));

    public void Rename(ProfileOptionRow row, string name)
    {
        var profiles = For(row.Operation);
        var index = profiles.IndexOf(row);
        profiles[index] = row with { Name = name };
    }

    public void ApplyTo(AppSettings settings)
    {
        var retained = Read.Concat(Write).Concat(Convert).ToDictionary(item => item.Id);
        settings.Profiles = settings.Profiles
            .Where(item => retained.ContainsKey(item.Id))
            .Select(item =>
            {
                item.Name = retained[item.Id].Name;
                return item;
            })
            .ToList();
    }
}
