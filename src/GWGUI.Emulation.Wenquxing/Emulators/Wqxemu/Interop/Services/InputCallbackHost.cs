using GWGUI.Emulation.Interop;

namespace GWGUI.Emulation.Wenquxing.Emulators.Wqxemu.Interop.Services;

internal sealed class InputCallbackHost
{
    private readonly EnvironmentCallbackHost _environment;
    private readonly KeyboardState _keyboard;
    private EmulationInputSnapshot _input = EmulationInputSnapshot.Empty;
    internal InputCallbackHost(MachineModel model, EnvironmentCallbackHost environment) { _environment = environment; _keyboard = new KeyboardState(model); PollCallback = OnPoll; StateCallback = OnState; }
    internal ExternalCoreApi.InputPollCallback PollCallback { get; }
    internal ExternalCoreApi.InputStateCallback StateCallback { get; }
    internal Exception? Failure { get; private set; }
    internal void Update(EmulationInputSnapshot input) => _input = input;
    private void OnPoll()
    {
        try { _keyboard.Publish(_input.Keys, _environment.Keyboard); }
        catch (Exception error) { Failure ??= error; }
    }
    private short OnState(uint port, uint device, uint index, uint id) => 0;
    internal void Release()
    {
        try { _keyboard.Release(_environment.Keyboard); }
        finally { _input = EmulationInputSnapshot.Empty; Failure = null; }
    }
}