using GWGUI.App.Functions.Views.Emulation.Settings;

namespace GWGUI.Tests;

public sealed class EmulationConfigurationSaveDebouncerTests
{
    [Fact]
    public async Task RapidChangesPersistOnlyTheLatestValue()
    {
        using var debouncer = new EmulationConfigurationSaveDebouncer();
        var completed = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var saves = 0;
        var savedValue = -1;
        Exception? failure = null;

        for (var value = 0; value < 12; value++)
        {
            var current = value;
            debouncer.Schedule(() =>
            {
                Interlocked.Increment(ref saves);
                savedValue = current;
                completed.TrySetResult();
                return Task.CompletedTask;
            }, error => failure = error);
        }

        await completed.Task.WaitAsync(TimeSpan.FromSeconds(3));
        await Task.Delay(80);

        Assert.Null(failure);
        Assert.Equal(1, saves);
        Assert.Equal(11, savedValue);
    }
}
