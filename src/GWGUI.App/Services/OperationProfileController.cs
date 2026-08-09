using System.Windows.Controls;
using GWGUI.Domain.Profiles;

namespace GWGUI.App.Services;

public sealed class OperationProfileController(
    OperationProfileCollection profiles,
    IBusinessDialogService businessDialogs,
    IMessageDialogService dialogs,
    Func<string, object[], string> localize)
{
    public void Refresh(ComboBox selector, OperationKind operation, string? selectedId = null)
    {
        var items = profiles.Localized(operation, key => localize(key, []));
        selector.ItemsSource = items;
        selector.SelectedItem = items.FirstOrDefault(profile => profile.Id == selectedId) ?? items[0];
    }

    public OperationProfile? Save(
        OperationKind operation,
        Func<string, OperationProfile> createProfile)
    {
        var name = businessDialogs.PromptProfileName();
        if (name is null) return null;

        var profile = createProfile(name);
        try
        {
            return profiles.For(operation).Save(profile);
        }
        catch (InvalidOperationException)
        {
            if (dialogs.Show(
                    localize("Profile.Replace", []),
                    localize("Profile.Title", []),
                    UserDialogButtons.YesNo,
                    UserDialogIcon.Question) != UserDialogResult.Yes)
                return null;

            return profiles.For(operation).Save(profile, true);
        }
    }
}
