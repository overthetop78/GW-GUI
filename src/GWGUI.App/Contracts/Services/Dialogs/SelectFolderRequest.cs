namespace GWGUI.App.Contracts.Services.Dialogs;

public sealed record SelectFolderRequest(string Title, string? InitialDirectory = null);
