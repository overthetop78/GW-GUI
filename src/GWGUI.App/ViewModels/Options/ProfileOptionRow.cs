using GWGUI.App.Localization.Extensions;
namespace GWGUI.App.ViewModels.Options;

public sealed record ProfileOptionRow(string Id, string Operation, string Name, bool IsSystem)
{
    public string OperationLabel => Operation switch
    {
        "Read" => LocExtension.Get("Tab.Read"),
        "Write" => LocExtension.Get("Tab.Write"),
        "Convert" => LocExtension.Get("Tab.Convert"),
        _ => Operation
    };
}
