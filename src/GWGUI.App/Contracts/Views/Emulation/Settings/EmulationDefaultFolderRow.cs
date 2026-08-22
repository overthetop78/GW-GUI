using System.Windows.Controls;

namespace GWGUI.App.Contracts.Views.Emulation.Settings;

internal sealed record EmulationDefaultFolderRow(string Label, TextBox Value, Func<Task> Browse);
