using System.Runtime.InteropServices;
using GWGUI.Emulation.Interfaces;
using CommonCoreHostFunctions = GWGUI.Emulation.Sony.Common.Functions.CoreHostFunctions;

namespace GWGUI.Emulation.Sony.Emulators.Common.Interop.Services;

/// <summary>Maintient le délégué ABI vivant et confie les intensités au stockage partagé via Common.</summary>
internal sealed class NativeControllerFeedback : IDisposable
{
    private readonly object _gate = new();
    private readonly IEmulationControllerFeedbackAccumulator _feedback = CommonCoreHostFunctions.EnvironmentProcessing.CreateControllerFeedbackAccumulator(ControllerPortConstants.MaximumGamepadPortCount);
    private readonly ExternalCoreApi.SetRumbleState _setState;
    private bool _disposed;

    internal NativeControllerFeedback() => _setState = SetState;

    internal bool Register(nint destination)
    {
        if (destination == nint.Zero) return false;
        lock (_gate)
        {
            if (_disposed) return false;
            Marshal.StructureToPtr(new ExternalCoreApi.RumbleInterface { SetState = Marshal.GetFunctionPointerForDelegate(_setState) }, destination, false);
            return true;
        }
    }

    internal bool SetState(uint port, uint effect, ushort strength) => _feedback.SetState(port, effect, strength);

    internal IReadOnlyList<ControllerFeedbackState> Snapshot() => _feedback.Snapshot();

    internal void ResetPort(int port) => _feedback.ResetPort(port);

    public void Dispose()
    {
        lock (_gate)
        {
            _disposed = true;
            _feedback.Dispose();
        }
    }
}
