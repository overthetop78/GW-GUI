using System.IO;
using System.Text;
using GWGUI.MediaEngine.Constants;
using GWGUI.MediaEngine.Contracts;
using GWGUI.MediaEngine.Enums;
using GWGUI.MediaEngine.Images.Formats.Optical.BinCue;
using GWGUI.MediaEngine.Images.Models.Optical;
using GWGUI.MediaEngine.Images.Reading.Recognition;
using GWGUI.MediaEngine.Images.Writing;
using GWGUI.MediaEngine.Interfaces.Reading;

namespace GWGUI.Tests.Media;

public sealed class BinCueWaveMediaFormatTests
{
    private const int AudioSectorSize = 2_352;
    private const int SectorCount = 2;

    [Fact]
    public async Task ReadsWavePcmCueTracksAndWritesBinaryCue()
    {
        var directory = Path.Combine(Path.GetTempPath(), $"gwgui-cue-wave-{Guid.NewGuid():N}");
        Directory.CreateDirectory(directory);
        try
        {
            var cuePath = Path.Combine(directory, "audio.cue");
            var wavePath = Path.Combine(directory, "audio.wav");
            var outputCuePath = Path.Combine(directory, "roundtrip.cue");
            var pcm = CreatePcm();
            await File.WriteAllBytesAsync(wavePath, CreateWave(pcm));
            await File.WriteAllTextAsync(cuePath,
                "FILE \"audio.wav\" WAVE\n  TRACK 01 AUDIO\n    INDEX 01 00:00:00\n",
                Encoding.ASCII);

            IMediaImageReader reader = new BinCueReader();
            var context = new MediaRecognitionContext(new MediaSourceDescriptor(cuePath, []));
            Assert.True(await reader.CanReadAsync(context, CancellationToken.None));
            var document = await reader.ReadAsync(context, CancellationToken.None);
            var optical = Assert.IsType<OpticalMediaImageRepresentation>(document.Representation);
            var track = Assert.Single(optical.Tracks!);
            Assert.Equal(OpticalTrackMode.Audio, track.Mode);
            Assert.Equal(SectorCount, track.SectorCount);
            var readPcm = new byte[pcm.Length];
            await track.DataSource.ReadExactlyAsync(0, readPcm, CancellationToken.None);
            Assert.Equal(pcm, readPcm);

            var outputs = await new BinCueWriter().WriteAsync(
                document, outputCuePath, OpticalImageFormatIds.BinCue);
            var outputBinPath = Path.Combine(directory, "roundtrip.track01.bin");
            Assert.Contains(outputCuePath, outputs, StringComparer.OrdinalIgnoreCase);
            Assert.Contains(outputBinPath, outputs, StringComparer.OrdinalIgnoreCase);
            Assert.Equal(pcm, await File.ReadAllBytesAsync(outputBinPath));
        }
        finally
        {
            if (Directory.Exists(directory)) Directory.Delete(directory, recursive: true);
        }
    }

    private static byte[] CreatePcm()
    {
        var pcm = new byte[SectorCount * AudioSectorSize];
        for (var index = 0; index < pcm.Length; index++) pcm[index] = (byte)(index % byte.MaxValue);
        return pcm;
    }

    private static byte[] CreateWave(byte[] pcm)
    {
        const int fmtSize = 16;
        const int chunkHeaderSize = 8;
        const ushort format = 1;
        const ushort channels = 2;
        const int sampleRate = 44_100;
        const ushort bitsPerSample = 16;
        const ushort blockAlign = 4;
        const int byteRate = 176_400;
        var riffSize = checked(sizeof(uint) + chunkHeaderSize + fmtSize + chunkHeaderSize + pcm.Length);
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream, Encoding.ASCII, leaveOpen: true);
        writer.Write(Encoding.ASCII.GetBytes("RIFF"));
        writer.Write(riffSize);
        writer.Write(Encoding.ASCII.GetBytes("WAVE"));
        writer.Write(Encoding.ASCII.GetBytes("fmt "));
        writer.Write(fmtSize);
        writer.Write(format);
        writer.Write(channels);
        writer.Write(sampleRate);
        writer.Write(byteRate);
        writer.Write(blockAlign);
        writer.Write(bitsPerSample);
        writer.Write(Encoding.ASCII.GetBytes("data"));
        writer.Write(pcm.Length);
        writer.Write(pcm);
        writer.Flush();
        return stream.ToArray();
    }
}
