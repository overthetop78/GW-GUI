using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Security;
using Microsoft.Win32;

namespace GWGUI.App.Services.Input.GameInput;

internal static class GameInputNative
{
    internal static readonly Guid InterfaceId = new("20EFC1C7-5D9A-43BA-B26F-B807FA48609C");
    private static IntPtr _module;
    internal static string? SelectedRuntimePath { get; private set; }

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate int InitializeDelegate(in Guid interfaceId, out IntPtr gameInput);

    internal static int GameInputInitialize(in Guid interfaceId, out IGameInput? gameInput)
    {
        gameInput = null;
        var path = FindRuntime();
        SelectedRuntimePath = path;
        if (path is null) return unchecked((int)0x80070002);
        _module = LoadLibrary(path);
        if (_module == IntPtr.Zero) return Marshal.GetHRForLastWin32Error();
        var address = GetProcAddress(_module, "GameInputInitialize");
        if (address == IntPtr.Zero) return unchecked((int)0x80004002);
        var initialize = Marshal.GetDelegateForFunctionPointer<InitializeDelegate>(address);
        var result = initialize(interfaceId, out var pointer);
        if (result < 0 || pointer == IntPtr.Zero) return result;
        try { gameInput = (IGameInput)Marshal.GetObjectForIUnknown(pointer); }
        finally { Marshal.Release(pointer); }
        return result;
    }

    private static string? FindRuntime()
    {
        var candidates = new List<GameInputRuntimeCandidate>();
        var appLocal = Path.Combine(AppContext.BaseDirectory, "GameInputRedist.dll");
        AddCandidate(candidates, appLocal, GameInputRuntimeSource.AppLocal);

        try
        {
            using var key = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32)
                .OpenSubKey(@"SOFTWARE\Microsoft\GameInput");
            if (key?.GetValue("RedistDir") is string directory)
                AddCandidate(candidates, Path.Combine(directory, "GameInputRedist.dll"),
                    GameInputRuntimeSource.Registered);
        }
        catch (Exception exception) when (exception is SecurityException or UnauthorizedAccessException) { }

        AddCandidate(candidates,
            Path.Combine(Environment.SystemDirectory, "GameInputRedist.dll"),
            GameInputRuntimeSource.SystemFallback);
        return SelectRuntime(candidates);
    }

    private static void AddCandidate(
        ICollection<GameInputRuntimeCandidate> candidates,
        string path,
        GameInputRuntimeSource source)
    {
        if (File.Exists(path))
            candidates.Add(new GameInputRuntimeCandidate(path, VersionOf(path), source));
    }

    internal static string? SelectRuntime(IEnumerable<GameInputRuntimeCandidate> candidates)
    {
        var available = candidates.ToArray();
        var preferred = available
            .Where(candidate => candidate.Source != GameInputRuntimeSource.SystemFallback)
            .OrderByDescending(candidate => candidate.Version)
            .ThenByDescending(candidate => candidate.Source)
            .FirstOrDefault();
        return preferred?.Path ?? available
            .Where(candidate => candidate.Source == GameInputRuntimeSource.SystemFallback)
            .OrderByDescending(candidate => candidate.Version)
            .Select(candidate => candidate.Path)
            .FirstOrDefault();
    }

    private static Version VersionOf(string path)
    {
        var info = FileVersionInfo.GetVersionInfo(path);
        return new Version(info.FileMajorPart, info.FileMinorPart, info.FileBuildPart, info.FilePrivatePart);
    }

    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern IntPtr LoadLibrary(string fileName);

    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Ansi)]
    private static extern IntPtr GetProcAddress(IntPtr module, string name);
}
