using System.Collections.Concurrent;

namespace GWGUI.Emulation.Contracts;

internal sealed record EmulationMachineCommand(Action Action, TaskCompletionSource Completion)
{
    internal void Execute(bool rethrow = false)
    {
        try
        {
            Action();
            Completion.TrySetResult();
        }
        catch (Exception error)
        {
            Completion.TrySetException(error);
            if (rethrow) throw;
        }
    }

    internal static ValueTask Enqueue(ConcurrentQueue<EmulationMachineCommand> commands, object gate,
        Action action, CancellationToken cancellationToken, Action? validateInsideGate = null)
    {
        var completion = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        if (validateInsideGate is null)
        {
            commands.Enqueue(new EmulationMachineCommand(action, completion));
            lock (gate) Monitor.PulseAll(gate);
        }
        else
        {
            lock (gate)
            {
                validateInsideGate();
                commands.Enqueue(new EmulationMachineCommand(action, completion));
                Monitor.PulseAll(gate);
            }
        }
        return new ValueTask(completion.Task.WaitAsync(cancellationToken));
    }

    internal static void FailPending(ConcurrentQueue<EmulationMachineCommand> commands, Exception error)
    {
        while (commands.TryDequeue(out var command)) command.Completion.TrySetException(error);
    }
}
