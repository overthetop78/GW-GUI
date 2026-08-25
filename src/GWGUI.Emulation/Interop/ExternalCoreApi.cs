using System.Runtime.InteropServices;

namespace GWGUI.Emulation.Interop;

internal static class ExternalCoreApi
{
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    internal delegate bool EnvironmentCallback(uint command, nint data);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate void VideoCallback(nint data, uint width, uint height, nuint pitch);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate void AudioSampleCallback(short left, short right);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate nuint AudioBatchCallback(nint data, nuint frames);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate void InputPollCallback();

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate short InputStateCallback(uint port, uint device, uint index, uint id);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate void LogCallback(int level, nint format);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] [return: MarshalAs(UnmanagedType.I1)]
    internal delegate bool SetEjectState([MarshalAs(UnmanagedType.I1)] bool ejected);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] [return: MarshalAs(UnmanagedType.I1)]
    internal delegate bool GetEjectState();
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] internal delegate uint GetImageIndex();
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] [return: MarshalAs(UnmanagedType.I1)]
    internal delegate bool SetImageIndex(uint index);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] internal delegate uint GetImageCount();
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] [return: MarshalAs(UnmanagedType.I1)]
    internal delegate bool ReplaceImage(uint index, nint gameInfo);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] [return: MarshalAs(UnmanagedType.I1)]
    internal delegate bool AddImage();
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] [return: MarshalAs(UnmanagedType.I1)]
    internal delegate bool GetImagePath(uint index, nint path, nuint length);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] [return: MarshalAs(UnmanagedType.I1)]
    internal delegate bool GetImageLabel(uint index, nint label, nuint length);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate void KeyboardEvent([MarshalAs(UnmanagedType.I1)] bool down, uint keyCode, uint character, ushort modifiers);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] internal delegate void SetLedState(int led, int state);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] [return: MarshalAs(UnmanagedType.I1)]
    internal delegate bool SetRumbleState(uint port, uint effect, ushort strength);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] [return: MarshalAs(UnmanagedType.I1)]
    internal delegate bool SetSensorState(uint port, uint action, uint rate);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] internal delegate float GetSensorInput(uint port, uint id);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] [return: MarshalAs(UnmanagedType.I1)]
    internal delegate bool UpdateCoreOptionsDisplay();

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] internal delegate void SetEnvironment(EnvironmentCallback callback);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] internal delegate void SetVideo(VideoCallback callback);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] internal delegate void SetAudioSample(AudioSampleCallback callback);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] internal delegate void SetAudioBatch(AudioBatchCallback callback);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] internal delegate void SetInputPoll(InputPollCallback callback);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] internal delegate void SetInputState(InputStateCallback callback);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] internal delegate void SetControllerPortDevice(uint port, uint device);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] internal delegate void VoidCall();
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] internal delegate void GetSystemInfo(out SystemInfo info);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] internal delegate uint GetApiVersion();
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] internal delegate uint GetRegion();
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] internal delegate nint GetMemoryData(uint id);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] internal delegate nuint GetMemorySize(uint id);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] internal delegate void GetSystemAvInfo(out SystemAvInfo info);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    internal delegate bool LoadGame(nint gameInfo);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] internal delegate nuint GetSerializedSize();
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] [return: MarshalAs(UnmanagedType.I1)]
    internal delegate bool Serialize(nint data, nuint size);

    [StructLayout(LayoutKind.Sequential)]
    internal struct SystemInfo
    {
        internal nint LibraryName;
        internal nint LibraryVersion;
        internal nint ValidExtensions;
        [MarshalAs(UnmanagedType.I1)] internal bool NeedFullPath;
        [MarshalAs(UnmanagedType.I1)] internal bool BlockExtract;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct GameInfo
    {
        internal nint Path;
        internal nint Data;
        internal nuint Size;
        internal nint Metadata;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct Geometry
    {
        internal uint BaseWidth;
        internal uint BaseHeight;
        internal uint MaximumWidth;
        internal uint MaximumHeight;
        internal float AspectRatio;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct Timing
    {
        internal double FramesPerSecond;
        internal double SampleRate;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct SystemAvInfo
    {
        internal Geometry Geometry;
        internal Timing Timing;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct Variable
    {
        internal nint Key;
        internal nint Value;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct CoreOptionDisplay
    {
        internal nint Key;
        [MarshalAs(UnmanagedType.I1)] internal bool Visible;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct CoreOptionsUpdateDisplayCallback
    {
        internal nint Callback;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct LogInterface
    {
        internal nint Log;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct DiskControl
    {
        internal nint SetEjectState;
        internal nint GetEjectState;
        internal nint GetImageIndex;
        internal nint SetImageIndex;
        internal nint GetImageCount;
        internal nint ReplaceImage;
        internal nint AddImage;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct DiskControlExtended
    {
        internal DiskControl Basic;
        internal nint SetInitialImage;
        internal nint GetImagePath;
        internal nint GetImageLabel;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct KeyboardCallback
    {
        internal nint Callback;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct ControllerDescription
    {
        internal nint Description;
        internal uint Id;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct ControllerInfo
    {
        internal nint Types;
        internal uint Count;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct Message
    {
        internal nint Text;
        internal uint Frames;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct MessageExtended
    {
        internal nint Text;
        internal uint DurationMilliseconds;
        internal uint Priority;
        internal uint Level;
        internal uint Target;
        internal uint Type;
        internal sbyte Progress;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct LedInterface
    {
        internal nint SetLedState;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct InputDescriptor
    {
        internal uint Port;
        internal uint Device;
        internal uint Index;
        internal uint Id;
        internal nint Description;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct MemoryDescriptor
    {
        internal ulong Flags;
        internal nint Pointer;
        internal nuint Offset;
        internal nuint Start;
        internal nuint Select;
        internal nuint Disconnect;
        internal nuint Length;
        internal nint AddressSpace;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct MemoryMap
    {
        internal nint Descriptors;
        internal uint Count;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct RumbleInterface
    {
        internal nint SetState;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct SensorInterface
    {
        internal nint SetState;
        internal nint GetInput;
    }
}
