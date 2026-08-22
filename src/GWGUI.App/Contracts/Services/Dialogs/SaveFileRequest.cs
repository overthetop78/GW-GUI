namespace GWGUI.App.Contracts.Services.Dialogs;

public sealed record SaveFileRequest(string Filter, string FileName, string? DefaultExtension = null);
