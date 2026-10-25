using System.Collections.Concurrent;

namespace GWGUI.Emulation.Contracts;

/// <summary>Commande exécutée sur le thread d’une machine et signal de fin attendu par son appelant.</summary>
public sealed record EmulationMachineCommand(Action Action, TaskCompletionSource Completion)
{
    /// <summary>Exécute la commande, signale son résultat et propage éventuellement son exception à la boucle.</summary>
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

    /// <summary>Place une commande dans la file de la machine et réveille sa boucle.</summary>
    /// <remarks>La validation facultative s’effectue sous le verrou. Le jeton annule l’attente de l’appelant, sans retirer une commande déjà placée dans la file.</remarks>
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

    /// <summary>Termine en échec toutes les commandes encore en attente lors de l’arrêt ou de la destruction.</summary>
    internal static void FailPending(ConcurrentQueue<EmulationMachineCommand> commands, Exception error)
    {
        while (commands.TryDequeue(out var command)) command.Completion.TrySetException(error);
    }
}
