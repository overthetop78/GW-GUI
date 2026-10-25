namespace GWGUI.Emulation.Sony.Emulators.Common.Interop.Contracts;

internal sealed record CoreControllerPort(int ConfigurationPort, uint NativePort, uint NativeDevice, bool CanChangeWhileRunning = true);
