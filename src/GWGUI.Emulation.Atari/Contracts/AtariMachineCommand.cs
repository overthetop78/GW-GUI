namespace GWGUI.Emulation.Atari.Contracts;

internal sealed record AtariMachineCommand(Action Action, TaskCompletionSource Completion)
{
    internal void Execute()
    {
        try
        {
            Action();
            Completion.TrySetResult();
        }
        catch (Exception error)
        {
            Completion.TrySetException(error);
            throw;
        }
    }
}
