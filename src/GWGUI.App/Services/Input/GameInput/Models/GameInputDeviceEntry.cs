namespace GWGUI.App.Services.Input.GameInput;

internal sealed record GameInputDeviceEntry(
    string Id,
    string Name,
    GameInputKind InputKinds,
    IGameInputDevice Device,
    IntPtr DevicePointer,
    IGameInputMapper? Mapper,
    bool IsController,
    GameInputDeviceDescriptor Descriptor,
    HidReportDecoder? HidDecoder = null,
    ulong RawReadingToken = 0,
    IntPtr RawReadingContext = default);
