using GWGUI.Domain.Settings.Logging;
namespace GWGUI.App.Contracts.Services.Navigation;

public sealed record GwToolWindowRequest(string Executable, string Verb, string? Device, string? Drive, string LogsDirectory, OperationLogSettings Logging);
