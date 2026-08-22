using GWGUI.Domain.Settings.Logging;
namespace GWGUI.App.ViewModels.Options;

public sealed record LogOptionRow(string Action, string Label, ActionLogSettings Settings);
