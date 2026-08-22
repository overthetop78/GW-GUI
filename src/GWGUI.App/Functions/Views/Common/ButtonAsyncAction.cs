using System.Windows.Controls;

namespace GWGUI.App.Functions.Views.Common;

internal static class ButtonAsyncAction
{
    internal static async Task RunAsync(
        Button button,
        Func<Task> action,
        Action<Exception>? errorHandler = null,
        Action? completed = null,
        Func<bool>? restoreEnabled = null)
    {
        button.IsEnabled = false;
        try
        {
            await action();
        }
        catch (Exception error) when (errorHandler is not null)
        {
            errorHandler(error);
        }
        finally
        {
            completed?.Invoke();
            if (restoreEnabled?.Invoke() ?? true) button.IsEnabled = true;
        }
    }
}
