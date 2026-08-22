namespace GWGUI.App.Contracts.Services.Dialogs;

public sealed record OpenFileRequest(string Filter, string? InitialDirectory = null, string? FileName = null);
