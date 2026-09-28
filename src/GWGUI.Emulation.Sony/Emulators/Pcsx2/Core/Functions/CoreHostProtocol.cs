using GWGUI.Emulation.Sony.Emulators.Pcsx2.Constants;
using GWGUI.Emulation.Sony.Emulators.Pcsx2.Contracts;
using GWGUI.Emulation.Sony.Emulators.Pcsx2.Factories;
using GWGUI.Emulation.Sony.Emulators.Pcsx2.Functions;
using GWGUI.Emulation.Sony.Emulators.Pcsx2.Services;

using System.IO.Pipes;
using System.IO.MemoryMappedFiles;
using System.Runtime.Versioning;
using System.Text.Json;
using GWGUI.Emulation;

using System.IO;

namespace GWGUI.Emulation.Sony.Emulators.Pcsx2.Functions;


internal static class CoreHostProtocol
{
    internal static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    internal static void WriteString(BinaryWriter writer, string? value) => EmulationHostProtocolFunctions.WriteString(writer, value);
    internal static string? ReadString(BinaryReader reader) => EmulationHostProtocolFunctions.ReadString(reader);
    internal static void WriteBytes(BinaryWriter writer, ReadOnlySpan<byte> bytes) => EmulationHostProtocolFunctions.WriteBytes(writer, bytes);
    internal static byte[] ReadBytes(BinaryReader reader) =>
        EmulationHostProtocolFunctions.ReadBytes(reader, Pcsx2Constants.DisplayName);
    internal static void WriteInput(BinaryWriter writer, EmulationInputSnapshot input) => EmulationHostProtocolFunctions.WriteInput(writer, input);
    internal static EmulationInputSnapshot ReadInput(BinaryReader reader) =>
        EmulationHostProtocolFunctions.ReadInput(reader, Pcsx2Constants.DisplayName);
    internal static void WriteFrame(BinaryWriter writer, VideoFrame? frame) => EmulationHostProtocolFunctions.WriteFrame(writer, frame);
    internal static VideoFrame? ReadFrame(BinaryReader reader) =>
        EmulationHostProtocolFunctions.ReadFrame(reader, Pcsx2Constants.DisplayName);
    internal static void WriteSharedFrame(BinaryWriter writer, VideoFrame? frame, MemoryMappedViewAccessor videoMap) =>
        EmulationHostProtocolFunctions.WriteSharedFrame(writer, frame, videoMap, Pcsx2Constants.DisplayName);
    internal static VideoFrame? ReadSharedFrame(BinaryReader reader, MemoryMappedViewAccessor videoMap) =>
        EmulationHostProtocolFunctions.ReadSharedFrame(reader, videoMap, Pcsx2Constants.DisplayName);
    internal static void WriteAudio(BinaryWriter writer, IReadOnlyList<AudioChunk> chunks) => EmulationHostProtocolFunctions.WriteAudio(writer, chunks);
    internal static IReadOnlyList<AudioChunk> ReadAudio(BinaryReader reader) =>
        EmulationHostProtocolFunctions.ReadAudio(reader, Pcsx2Constants.DisplayName);
    internal static void WriteLedStates(BinaryWriter writer, IReadOnlyDictionary<int, bool> states) => EmulationHostProtocolFunctions.WriteLedStates(writer, states);
    internal static IReadOnlyDictionary<int, bool> ReadLedStates(BinaryReader reader) =>
        EmulationHostProtocolFunctions.ReadLedStates(reader, Pcsx2Constants.DisplayName);
}


