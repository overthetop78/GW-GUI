namespace GWGUI.Emulation.Atari;

internal static class AtariMachineCollectionFunctions
{
    internal static async ValueTask StopAndDisposeAsync(IAtariMachine machine)
    {
        try { await machine.StopAsync().ConfigureAwait(false); }
        finally { await machine.DisposeAsync().ConfigureAwait(false); }
    }
}
