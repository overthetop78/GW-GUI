using System.Runtime.InteropServices;

namespace GWGUI.Emulation.Interop;

/// <summary>Déclarations ABI partagées des cœurs externes : délégués Cdecl et structures natives séquentielles.</summary>
/// <remarks>Les pointeurs restent empruntés selon le contrat natif. Le propriétaire conserve les délégués vivants pendant leur enregistrement et libère ses allocations après désenregistrement et arrêt du cœur. La visibilité ne dépend pas du nom du module consommateur.</remarks>
public static class ExternalCoreApi
{
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    public delegate bool EnvironmentCallback(uint command, nint data);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void VideoCallback(nint data, uint width, uint height, nuint pitch);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void AudioSampleCallback(short left, short right);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate nuint AudioBatchCallback(nint data, nuint frames);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void InputPollCallback();

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate short InputStateCallback(uint port, uint device, uint index, uint id);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void LogCallback(int level, nint format);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate nint VfsGetPath(nint stream);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate nint VfsOpen(nint path, uint mode, uint hints);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate int VfsClose(nint stream);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate long VfsSize(nint stream);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate long VfsTell(nint stream);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate long VfsSeek(nint stream, long offset, int origin);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate long VfsRead(nint stream, nint destination, ulong length);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate long VfsWrite(nint stream, nint source, ulong length);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate int VfsFlush(nint stream);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate int VfsRemove(nint path);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate int VfsRename(nint oldPath, nint newPath);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate int VfsStat(nint path, nint size);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate long VfsTruncate(nint stream, long length);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate int VfsMakeDirectory(nint path);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate nint VfsOpenDirectory(nint path, [MarshalAs(UnmanagedType.I1)] bool includeHidden);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] [return: MarshalAs(UnmanagedType.I1)] public delegate bool VfsReadDirectory(nint directory);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate nint VfsDirectoryEntryName(nint directory);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] [return: MarshalAs(UnmanagedType.I1)] public delegate bool VfsDirectoryEntryIsDirectory(nint directory);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate int VfsCloseDirectory(nint directory);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] [return: MarshalAs(UnmanagedType.I1)]
    public delegate bool SetEjectState([MarshalAs(UnmanagedType.I1)] bool ejected);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] [return: MarshalAs(UnmanagedType.I1)]
    public delegate bool GetEjectState();
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate uint GetImageIndex();
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] [return: MarshalAs(UnmanagedType.I1)]
    public delegate bool SetImageIndex(uint index);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate uint GetImageCount();
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] [return: MarshalAs(UnmanagedType.I1)]
    public delegate bool ReplaceImage(uint index, nint gameInfo);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] [return: MarshalAs(UnmanagedType.I1)]
    public delegate bool AddImage();
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] [return: MarshalAs(UnmanagedType.I1)]
    public delegate bool GetImagePath(uint index, nint path, nuint length);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] [return: MarshalAs(UnmanagedType.I1)]
    public delegate bool GetImageLabel(uint index, nint label, nuint length);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void KeyboardEvent([MarshalAs(UnmanagedType.I1)] bool down, uint keyCode, uint character, ushort modifiers);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate void SetLedState(int led, int state);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] [return: MarshalAs(UnmanagedType.I1)]
    public delegate bool SetRumbleState(uint port, uint effect, ushort strength);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] [return: MarshalAs(UnmanagedType.I1)]
    public delegate bool SetSensorState(uint port, uint action, uint rate);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate float GetSensorInput(uint port, uint id);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] [return: MarshalAs(UnmanagedType.I1)]
    public delegate bool UpdateCoreOptionsDisplay();

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate void SetEnvironment(EnvironmentCallback callback);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate void HardwareContextReset();
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate nint HardwareGetCurrentFramebuffer();
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate nint HardwareGetProcAddress(nint symbol);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate void SetVideo(VideoCallback callback);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate void SetAudioSample(AudioSampleCallback callback);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate void SetAudioBatch(AudioBatchCallback callback);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate void SetInputPoll(InputPollCallback callback);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate void SetInputState(InputStateCallback callback);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate void SetControllerPortDevice(uint port, uint device);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate void VoidCall();
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate void GetSystemInfo(out SystemInfo info);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate uint GetApiVersion();
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate uint GetRegion();
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate nint GetMemoryData(uint id);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate nuint GetMemorySize(uint id);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate void GetSystemAvInfo(out SystemAvInfo info);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    public delegate bool LoadGame(nint gameInfo);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    public delegate bool LoadGameSpecial(uint subsystemId, nint gameInfos, nuint count);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate nuint GetSerializedSize();
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] [return: MarshalAs(UnmanagedType.I1)]
    public delegate bool Serialize(nint data, nuint size);

    /// <summary>Métadonnées natives du cœur ; les chaînes UTF-8 sont empruntées à la bibliothèque.</summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct SystemInfo
    {
        public nint LibraryName;
        public nint LibraryVersion;
        public nint ValidExtensions;
        [MarshalAs(UnmanagedType.I1)] public bool NeedFullPath;
        [MarshalAs(UnmanagedType.I1)] public bool BlockExtract;
    }

    /// <summary>Contenu transmis au cœur ; les pointeurs et octets restent valides pendant leur utilisation native.</summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct GameInfo
    {
        public nint Path;
        public nint Data;
        public nuint Size;
        public nint Metadata;
    }

    /// <summary>Dimensions et rapport de forme de la sortie vidéo native.</summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct Geometry
    {
        public uint BaseWidth;
        public uint BaseHeight;
        public uint MaximumWidth;
        public uint MaximumHeight;
        public float AspectRatio;
    }

    /// <summary>Cadence vidéo et fréquence audio annoncées par le cœur.</summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct Timing
    {
        public double FramesPerSecond;
        public double SampleRate;
    }

    /// <summary>Géométrie et temporisations regroupées selon la disposition ABI.</summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct SystemAvInfo
    {
        public Geometry Geometry;
        public Timing Timing;
    }

    /// <summary>Description native du rendu matériel et des callbacks de contexte possédés par leur fournisseur.</summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct HardwareRenderCallback
    {
        public int ContextType;
        public nint ContextReset;
        public nint GetCurrentFramebuffer;
        public nint GetProcAddress;
        [MarshalAs(UnmanagedType.I1)] public bool Depth;
        [MarshalAs(UnmanagedType.I1)] public bool Stencil;
        [MarshalAs(UnmanagedType.I1)] public bool BottomLeftOrigin;
        public uint VersionMajor;
        public uint VersionMinor;
        [MarshalAs(UnmanagedType.I1)] public bool CacheContext;
        public nint ContextDestroy;
        [MarshalAs(UnmanagedType.I1)] public bool DebugContext;
    }

    /// <summary>Paire de pointeurs UTF-8 pour une option native.</summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct Variable
    {
        public nint Key;
        public nint Value;
    }

    /// <summary>Visibilité native d’une option identifiée par sa clé UTF-8.</summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct CoreOptionDisplay
    {
        public nint Key;
        [MarshalAs(UnmanagedType.I1)] public bool Visible;
    }

    /// <summary>Pointeur vers le callback natif de mise à jour des options.</summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct CoreOptionsUpdateDisplayCallback
    {
        public nint Callback;
    }

    /// <summary>Pointeur vers le callback de journalisation conservé par son fournisseur.</summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct LogInterface
    {
        public nint Log;
    }

    /// <summary>Version demandée et pointeur emprunté vers la table du système de fichiers virtuel.</summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct VfsInterfaceInfo
    {
        public uint RequiredInterfaceVersion;
        public nint Interface;
    }

    /// <summary>Table ABI des opérations de fichiers et de dossiers ; chaque fournisseur possède ses handles.</summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct VfsInterface
    {
        public nint GetPath;
        public nint Open;
        public nint Close;
        public nint Size;
        public nint Tell;
        public nint Seek;
        public nint Read;
        public nint Write;
        public nint Flush;
        public nint Remove;
        public nint Rename;
        public nint Truncate;
        public nint Stat;
        public nint MakeDirectory;
        public nint OpenDirectory;
        public nint ReadDirectory;
        public nint DirectoryEntryName;
        public nint DirectoryEntryIsDirectory;
        public nint CloseDirectory;
    }

    /// <summary>Table native des opérations de sélection et de remplacement des disques.</summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct DiskControl
    {
        public nint SetEjectState;
        public nint GetEjectState;
        public nint GetImageIndex;
        public nint SetImageIndex;
        public nint GetImageCount;
        public nint ReplaceImage;
        public nint AddImage;
    }

    /// <summary>Extension de la table de disques avec image initiale, chemins et libellés.</summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct DiskControlExtended
    {
        public DiskControl Basic;
        public nint SetInitialImage;
        public nint GetImagePath;
        public nint GetImageLabel;
    }

    /// <summary>Pointeur emprunté vers le callback clavier du cœur.</summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct KeyboardCallback
    {
        public nint Callback;
    }

    /// <summary>Description UTF-8 empruntée et identifiant natif d’un périphérique.</summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct ControllerDescription
    {
        public nint Description;
        public uint Id;
    }

    /// <summary>Tableau natif de périphériques compatibles et nombre de ses éléments.</summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct ControllerInfo
    {
        public nint Types;
        public uint Count;
    }

    /// <summary>Message UTF-8 emprunté avec durée exprimée en frames.</summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct Message
    {
        public nint Text;
        public uint Frames;
    }

    /// <summary>Message natif étendu avec durée, priorité, destination et progression.</summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct MessageExtended
    {
        public nint Text;
        public uint DurationMilliseconds;
        public uint Priority;
        public uint Level;
        public uint Target;
        public uint Type;
        public sbyte Progress;
    }

    /// <summary>Pointeur vers le callback d’activité des lecteurs.</summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct LedInterface
    {
        public nint SetLedState;
    }

    /// <summary>Description native d’une entrée par port, périphérique, index et identifiant.</summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct InputDescriptor
    {
        public uint Port;
        public uint Device;
        public uint Index;
        public uint Id;
        public nint Description;
    }

    /// <summary>Description d’une plage mémoire native possédée par le cœur.</summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct MemoryDescriptor
    {
        public ulong Flags;
        public nint Pointer;
        public nuint Offset;
        public nuint Start;
        public nuint Select;
        public nuint Disconnect;
        public nuint Length;
        public nint AddressSpace;
    }

    /// <summary>Pointeur emprunté vers les descripteurs mémoire et nombre de leurs éléments.</summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct MemoryMap
    {
        public nint Descriptors;
        public uint Count;
    }

    /// <summary>Pointeur vers le callback de vibration fourni par l’hôte.</summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct RumbleInterface
    {
        public nint SetState;
    }

    /// <summary>Pointeurs vers les callbacks de configuration et de lecture des capteurs.</summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct SensorInterface
    {
        public nint SetState;
        public nint GetInput;
    }
    /// <summary>Notification native du remplissage de la sortie audio, empruntée au cœur.</summary>
    /// <param name="active">Indique que la sortie audio accepte des échantillons.</param>
    /// <param name="occupancy">Pourcentage de remplissage borné entre zéro et cent.</param>
    /// <param name="underrunLikely">Indique que moins d’une frame d’émulation reste disponible.</param>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void AudioBufferStatusCallback([MarshalAs(UnmanagedType.I1)] bool active, uint occupancy, [MarshalAs(UnmanagedType.I1)] bool underrunLikely);
}
