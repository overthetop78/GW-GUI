using System.Runtime.InteropServices;

namespace GWGUI.Emulation.Amiga.Cores;

internal static class AmigaExternalApi
{
    internal const uint GetCanDuplicateFrames = 3;
    internal const uint GetSystemDirectory = 9;
    internal const uint SetPixelFormat = 10;
    internal const uint SetInputDescriptors = 11;
    internal const uint SetKeyboardCallback = 12;
    internal const uint SetDiskControl = 13;
    internal const uint GetVariable = 15;
    internal const uint SetVariables = 16;
    internal const uint GetVariableUpdate = 17;
    internal const uint SetSupportNoGame = 18;
    internal const uint GetLogInterface = 27;
    internal const uint GetContentDirectory = 30;
    internal const uint GetSaveDirectory = 31;
    internal const uint SetSystemAvInfo = 32;
    internal const uint SetControllerInfo = 35;
    internal const uint SetMemoryMaps = 36 | 0x10000;
    internal const uint SetGeometry = 37;
    internal const uint SetSupportAchievements = 42 | 0x10000;
    internal const uint GetVfsInterface = 45 | 0x10000;
    internal const uint GetLedInterface = 46 | 0x10000;
    internal const uint GetInputBitmasks = 51 | 0x10000;
    internal const uint GetCoreOptionsVersion = 52;
    internal const uint SetCoreOptionsDisplay = 55;
    internal const uint GetDiskControlVersion = 57;
    internal const uint SetDiskControlExtended = 58;
    internal const uint SetFastForwardingOverride = 64;
    internal const uint SetCoreOptionsV2 = 67;
    internal const uint SetCoreOptionsV2International = 68;
    internal const uint SetCoreOptionsUpdateDisplayCallback = 69;

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

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] internal delegate void SetEnvironment(EnvironmentCallback callback);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] internal delegate void SetVideo(VideoCallback callback);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] internal delegate void SetAudioSample(AudioSampleCallback callback);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] internal delegate void SetAudioBatch(AudioBatchCallback callback);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] internal delegate void SetInputPoll(InputPollCallback callback);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] internal delegate void SetInputState(InputStateCallback callback);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] internal delegate void VoidCall();
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] internal delegate void GetSystemInfo(out SystemInfo info);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] internal delegate uint GetApiVersion();
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
}
