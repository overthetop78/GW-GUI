using System.IO;
using System.IO.Compression;
using System.Reflection;
using GWGUI.Emulation.Example.Modules;
using GWGUI.Emulation.Example.Common.Machines.Common.Contracts;
using GWGUI.Emulation.Example.Common.Machines.Common.Dictionaries;
using GWGUI.Emulation.Example.Common.Machines.Common.Enums;

namespace GWGUI.Tests.Emulation;

public sealed class ExampleFirmwareSamplesTests
{
    [Theory]
    [InlineData(1, 8, 1)]
    [InlineData(1, 8, 2)]
    [InlineData(1, 16, 1)]
    [InlineData(1, 16, 2)]
    [InlineData(3, 32, 1)]
    [InlineData(3, 32, 2)]
    public void BankKeepsTheExistingPcmFormatsAndLeavesBorrowedArchivesOpen(int format, int bits, int channels)
    {
        foreach (var entries in Readers())
        {
            using var bytes = Archive(("nested/E889.wav", Wave(format, bits, channels)), ("readme.txt", new byte[] { 1 }));
            using var archive = new ZipArchive(bytes, ZipArchiveMode.Read);
            var entry = Assert.Single(entries(archive));
            Assert.Equal("E889.wav", entry.Name);
            using var reread = entry.Open();
            Assert.Equal((int)'R', reread.ReadByte());
        }
    }

    [Fact]
    public void BankRejectsDuplicateNamesAndInvalidWaveStructuresWithoutClosingItsCaller()
    {
        var invalid = new List<byte[]> { Array.Empty<byte>(), new byte[44], Wave(1, 24, 1), Wave(1, 16, 3), Wave(3, 16, 1) };
        var truncated = Wave();
        System.Buffers.Binary.BinaryPrimitives.WriteUInt32LittleEndian(truncated.AsSpan(40), 100);
        invalid.Add(truncated);
        var unaligned = Wave();
        System.Buffers.Binary.BinaryPrimitives.WriteUInt32LittleEndian(unaligned.AsSpan(40), 3);
        invalid.Add(unaligned);
        foreach (var entries in Readers())
        {
            foreach (var wave in invalid)
            {
                using var bytes = Archive(("E889.wav", wave));
                using var archive = new ZipArchive(bytes, ZipArchiveMode.Read);
                Assert.Throws<InvalidDataException>(() => entries(archive));
                Assert.Single(archive.Entries);
            }
            using var duplicateBytes = Archive(("first/E889.wav", Wave()), ("second/e889.WAV", Wave()));
            using var duplicates = new ZipArchive(duplicateBytes, ZipArchiveMode.Read);
            Assert.Throws<InvalidDataException>(() => entries(duplicates));
            using var emptyBytes = Archive(("readme.txt", new byte[] { 1 }));
            using var empty = new ZipArchive(emptyBytes, ZipArchiveMode.Read);
            Assert.Throws<InvalidDataException>(() => entries(empty));
        }
    }

    [Fact]
    public void BothProfilesAndAllThreeMocksDescribeTheSameOptionalBank()
    {
        var assembly = typeof(ExampleModuleFactory).Assembly;
        var contract = assembly.GetType("GWGUI.Emulation.Example.Common.Interfaces.IEmulatorFirmwareAdapter", true)!;
        foreach (var profile in HardwareModelCatalog.All)
        foreach (var core in Enum.GetValues<Emulator>().Where(core => core != Emulator.None))
        {
            var configuration = new MachineConfiguration(profile.Model, core: core);
            var adapter = Adapter(core);
            var slots = ((System.Collections.IEnumerable)contract.GetMethod("GetFirmwareSlots")!.Invoke(adapter, [configuration])!).Cast<object>();
            var bank = Assert.Single(slots, slot => (string)slot.GetType().GetProperty("FieldId")!.GetValue(slot)! == "VoiceSamples");
            Assert.False((bool)bank.GetType().GetProperty("IsRequired")!.GetValue(bank)!);
            Assert.Equal("voice", bank.GetType().GetProperty("SystemSubdirectory")!.GetValue(bank));
            Assert.Equal("Emulation.Firmware.VoiceSamples", bank.GetType().GetProperty("LabelResourceKey")!.GetValue(bank));
            Assert.False((bool)contract.GetMethod("RequiresExternalFirmware")!.Invoke(adapter, [configuration])!);
        }
    }

    internal static object Adapter(Emulator core) => Activator.CreateInstance(typeof(ExampleModuleFactory).Assembly
        .GetType($"GWGUI.Emulation.Example.Emulators.{core}.Factories.{core}MachineFactory", true)!, true)!;

    internal static byte[] Wave(int format = 1, int bits = 16, int channels = 1)
    {
        var block = channels * bits / 8;
        using var stream = new MemoryStream();
        using (var writer = new BinaryWriter(stream, System.Text.Encoding.UTF8, leaveOpen: true))
        {
            writer.Write("RIFF"u8); writer.Write(36 + block * 2); writer.Write("WAVEfmt "u8); writer.Write(16);
            writer.Write((ushort)format); writer.Write((ushort)channels); writer.Write(44100); writer.Write(44100 * block);
            writer.Write((ushort)block); writer.Write((ushort)bits); writer.Write("data"u8); writer.Write(block * 2);
            writer.Write(new byte[block * 2]);
        }
        return stream.ToArray();
    }

    internal static MemoryStream Archive(params (string Name, byte[] Data)[] entries)
    {
        var stream = new MemoryStream();
        try
        {
            using (var archive = new ZipArchive(stream, ZipArchiveMode.Create, leaveOpen: true))
                foreach (var item in entries) { using var output = archive.CreateEntry(item.Name).Open(); output.Write(item.Data); }
            stream.Position = 0;
            return stream;
        }
        catch { stream.Dispose(); throw; }
    }

    private static IEnumerable<Func<ZipArchive, IReadOnlyList<ZipArchiveEntry>>> Readers()
    {
        foreach (var assembly in new[] { typeof(ExampleModuleFactory).Assembly, Assembly.Load("gwgui.emulation.philips") })
        {
            var root = assembly == typeof(ExampleModuleFactory).Assembly ? "GWGUI.Emulation.Example" : "GWGUI.Emulation.Philips";
            yield return assembly.GetType(root + ".Common.Machines.Common.Functions.VoiceSampleFunctions", true)!
                .GetMethod("Entries", BindingFlags.Static | BindingFlags.NonPublic)!.CreateDelegate<Func<ZipArchive, IReadOnlyList<ZipArchiveEntry>>>();
        }
    }
}
