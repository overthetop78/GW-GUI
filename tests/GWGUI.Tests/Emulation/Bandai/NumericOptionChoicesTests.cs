using System.Security.Cryptography;
using System.Text;
using GWGUI.Emulation.Bandai.Emulators.BeetleWonderSwan.Dictionaries;

namespace GWGUI.Tests.Emulation.Bandai;

public sealed class NumericOptionChoicesTests
{
    // Digests freeze the ordered native values, raw labels and invariant displays
    // from the original explicit catalog, independently of the range generator.
    [Theory]
    [InlineData("wswan_birth_year", "1989", 110, false, "CF5A277497550840549E2E9F82495397DED3310734F7DCA93BF16B362686CB0B")]
    [InlineData("wswan_birth_day", "23", 31, false, "432D79958A62DDEA0FBD34CA152BCFE8C2BCBD52C6D6AF67A2D825ABA3428C59")]
    [InlineData("wswan_frameskip_threshold", "33", 16, true, "C9FC0F8CB56C0A74581FF498F2879EE61889E4A435CEA9B5E1C33C77E639E62B")]
    [InlineData("wswan_sound_sample_rate", "44100", 4, true, "ADB343B68906D19C9E9C607BBB33CB1D28B53163369AC4289A57EDD3A71789FE")]
    public void NumericChoicesRetainTheirCompleteNativeCatalog(string key, string defaultValue, int count, bool visible, string digest)
    {
        var option = Assert.Single(BeetleWonderSwanOptionCatalog.All, option => option.Key == key);
        Assert.Equal(defaultValue, option.DefaultValue);
        Assert.Equal(defaultValue, option.CurrentValue);
        Assert.Equal(count, option.Values.Count);
        Assert.Equal(visible, option.IsVisible);
        Assert.Equal(!visible, option.RequiresRuntimeReload);
        Assert.All(option.Values, choice => Assert.Null(choice.ResourceKey));
        var rows = option.Values.Select(choice => $"{choice.Value}|{choice.Label}|{choice.InvariantDisplayValue}");
        var actual = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(string.Join("\n", rows))));
        Assert.Equal(digest, actual);
    }
}
