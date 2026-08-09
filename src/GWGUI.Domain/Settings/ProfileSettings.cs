namespace GWGUI.Domain.Settings;

public sealed class ProfileSettings
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Operation { get; set; } = "Read";
    public string Name { get; set; } = "";
    public Dictionary<string, string> Values { get; set; } = new();
    public HashSet<string> EnabledOptions { get; set; } = [];
}
