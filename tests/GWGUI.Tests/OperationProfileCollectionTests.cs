using GWGUI.App.Services;
using GWGUI.Domain.Profiles;
using GWGUI.Domain.Settings;

namespace GWGUI.Tests;

public sealed class OperationProfileCollectionTests
{
    [Fact]
    public void ResetKeepsOperationStoresIndependent()
    {
        var profiles = new OperationProfileCollection();
        profiles.Reset(
        [
            new ProfileSettings { Id = "read", Operation = nameof(OperationKind.Read), Name = "Read custom" },
            new ProfileSettings { Id = "write", Operation = nameof(OperationKind.Write), Name = "Write custom" }
        ]);

        Assert.Contains(profiles.For(OperationKind.Read).GetAll(), profile => profile.Id == "read");
        Assert.DoesNotContain(profiles.For(OperationKind.Read).GetAll(), profile => profile.Id == "write");
        Assert.Contains(profiles.For(OperationKind.Write).GetAll(), profile => profile.Id == "write");
        Assert.DoesNotContain(profiles.For(OperationKind.Convert).GetAll(), profile => profile.Id is "read" or "write");
    }

    [Fact]
    public void CaptureExcludesSystemProfilesAndPreservesValues()
    {
        var profiles = new OperationProfileCollection();
        profiles.For(OperationKind.Convert).Save(new OperationProfile(
            "convert", OperationKind.Convert, "Convert custom",
            new Dictionary<string, string> { ["format"] = "ibm.720" },
            new HashSet<string> { "tracks" }));

        var captured = Assert.Single(profiles.Capture());

        Assert.Equal("convert", captured.Id);
        Assert.Equal(nameof(OperationKind.Convert), captured.Operation);
        Assert.Equal("ibm.720", captured.Values["format"]);
        Assert.Contains("tracks", captured.EnabledOptions);
    }

    [Fact]
    public void LocalizedOnlyChangesTheSystemProfileDisplayName()
    {
        var profiles = new OperationProfileCollection();
        profiles.For(OperationKind.Read).Save(new OperationProfile(
            "custom", OperationKind.Read, "Custom", new Dictionary<string, string>(), new HashSet<string>()));

        var localized = profiles.Localized(OperationKind.Read, _ => "Par défaut");

        Assert.Equal("Par défaut", localized.Single(profile => profile.IsSystem).Name);
        Assert.Equal("Custom", localized.Single(profile => profile.Id == "custom").Name);
    }
}
