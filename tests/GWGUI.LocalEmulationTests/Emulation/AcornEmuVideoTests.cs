using GWGUI.Emulation.Acorn.Common.Machines.Common.Contracts;
using GWGUI.Emulation.Acorn.Common.Machines.Common.Enums;
using GWGUI.Emulation.Acorn.Common.Services;
using GWGUI.Emulation.Acorn.Emulators.AcornEmu.Services;
using BbcVideo = GWGUI.Emulation.Acorn.Emulators.AcornEmu.Machines.BbcMicro.Common.Services.Video;
using GWGUI.Emulation.Contracts;
using GWGUI.Emulation.Enums;
using Firmware = GWGUI.Emulation.Acorn.Common.Machines.BbcMicro.Functions.BbcMicroFirmwareFunctions;
using NativeFirmware = GWGUI.Emulation.Acorn.Emulators.AcornEmu.Functions.FirmwareFunctions;

namespace GWGUI.Tests.Emulation;

/// <summary>Contrôles vidéo autonomes : RAM et glyphes synthétiques, sans ROM ni écran externes.</summary>
public sealed class AcornEmuVideoTests
{
    [Fact]
    public void UnknownCharacterRomHasItsOwnSelectableSlot()
    {
        var candidate = Assert.Single(Firmware.Inspect(new byte[960], "characters", MachineModel.BbcMicroModelA, NativeFirmware.Field));
        Assert.Equal(EmulationFirmwareCompatibility.Unknown, candidate.Compatibility);
        Assert.Equal("acornemu.characters", candidate.DestinationFieldId);
        Assert.Equal(960, Firmware.GetRomBytes(new byte[960], FirmwareCategory.CharacterGenerator).Length);
        Assert.Throws<System.IO.InvalidDataException>(() => Firmware.GetRomBytes(new byte[16384], FirmwareCategory.CharacterGenerator));
    }

    [Fact]
    public void CrtcCountsProgrammedLinesAdjustmentAndReadableCursorAddress()
    {
        var crtc = new Crtc6845();
        void Set(byte index, byte value) { crtc.Select(index); crtc.Write(value); }
        Set(0, 3); Set(1, 2); Set(4, 3); Set(5, 1); Set(6, 3); Set(7, 2); Set(9, 1);
        Set(14, 255); Set(15, 0x45);
        crtc.Select(14); Assert.Equal(63, crtc.Read());
        crtc.Select(0); Assert.Equal(0, crtc.Read());
        for (var tick = 0; tick < 35; tick++) Assert.False(crtc.Tick());
        Assert.True(crtc.Tick()); Assert.Equal(1, crtc.Frame); Assert.Equal(0, crtc.Scanline);
    }

    [Fact]
    public void PaletteWritesAffectOnlyPixelsNotYetScanned()
    {
        var ram = Enumerable.Repeat((byte)255, 32768).ToArray();
        var video = Bitmap(ram);
        video.WriteUla(0xfe21, 0xf6); // rouge : entrée logique 15, couleur physique 1.
        Advance(video, 39936);
        Advance(video, 128 * 128);
        video.WriteUla(0xfe21, 0xf3); // bleu pour la partie restante.
        Advance(video, 39936 - 128 * 128);
        var image = video.Render();
        Assert.Equal(255, image[(64 * 640) * 4 + 2]);
        Assert.Equal(0, image[(64 * 640) * 4]);
        Assert.Equal(255, image[(192 * 640) * 4]);
        Assert.Equal(0, image[(192 * 640) * 4 + 2]);
        video.WriteUla(0xfe21, 0xf0);
        Assert.Equal(image, video.Render());
    }

    [Fact]
    public void BitmapCursorRespectsStartEndAndDisableRegisters()
    {
        var video = Bitmap(new byte[32768]);
        Set(video, 10, 7); Set(video, 11, 7); Set(video, 14, 0x0b); Set(video, 15, 0);
        Set(video, 8, 0x90); // retards CRTC utilisés par le MOS : le curseur reste sur sa cellule utile.
        Advance(video, 39936 * 2);
        var image = video.Render();
        Assert.Equal(0, image[(6 * 640) * 4 + 2]);
        Assert.Equal(255, image[(7 * 640) * 4 + 2]);
        Set(video, 10, 32); Advance(video, 39936);
        Assert.Equal(0, video.Render()[(7 * 640) * 4 + 2]);
    }

    [Fact]
    public void UlaFlashPhaseFollowsTheControlBitDuringTheFrame()
    {
        var video = Bitmap(Enumerable.Repeat((byte)255, 32768).ToArray());
        video.WriteUla(0xfe21, 0xf8);
        Advance(video, 39936 + 128 * 128);
        video.WriteUla(0xfe20, 0x89);
        Advance(video, 39936 - 128 * 128);
        var image = video.Render();
        Assert.Equal(255, image[(64 * 640) * 4 + 2]);
        Assert.Equal(0, image[(192 * 640) * 4 + 2]);
    }

    [Fact]
    public void TeletextUsesExternalGlyphsAndHoldsMosaicsAcrossControls()
    {
        var font = new byte[960];
        Array.Fill(font, (byte)31, ('A' - 32) * 10, 10);
        var text = new TeletextSaa5050(font);
        text.BeginLine(0);
        Assert.NotEqual(0, text.Character('A', 4, 0).Mask);
        Assert.Equal(0, text.Character('B', 4, 0).Mask);
        text.Character(17, 4, 0);
        var mosaic = text.Character(127, 4, 0);
        Assert.Equal(1, mosaic.Foreground);
        Assert.Equal(0xfff, mosaic.Mask);
        Assert.Equal(mosaic.Mask, text.Character(30, 4, 0).Mask);
        Assert.Equal(mosaic.Mask, text.Character(29, 4, 0).Mask);
        Assert.Equal(1, text.Character(29, 4, 0).Background);
        text.BeginLine(1);
        Assert.Equal(7, text.Character('A', 4, 0).Foreground);
    }

    [Fact]
    public void VideoOutputIsStoredAndMonochromeProducesEqualChannels()
    {
        var configuration = new MachineConfiguration(MachineModel.BbcMicroModelA, core: Emulator.AcornEmu);
        var engine = new Engine();
        var updated = Assert.IsType<MachineConfiguration>(engine.ApplySettings(configuration, new Dictionary<string, string?> { ["acornemu.video-output"] = "composite-mono" }));
        Assert.Equal("composite-mono", updated.Options["acornemu.video-output"]);
        Assert.Throws<ArgumentException>(() => engine.ApplySettings(configuration, new Dictionary<string, string?> { ["acornemu.video-output"] = "invalid" }));
        var video = Bitmap(Enumerable.Repeat((byte)255, 32768).ToArray());
        video.Monochrome = true; video.WriteUla(0xfe21, 0xf6);
        Advance(video, 39936 * 2);
        var image = video.Render();
        Assert.True(image[0] > 0); Assert.Equal(image[0], image[1]); Assert.Equal(image[1], image[2]);
    }

    [Fact]
    public void VideoOutputCanChangeThroughTheRuntimeWithoutResettingTheMachine()
    {
        var machine = new GWGUI.Emulation.Acorn.Emulators.AcornEmu.Machines.BbcMicro.ModelA.Services.ModelAMachine(new byte[16384], new byte[16384]);
        machine.Write(0x1234, 0x56);
        var pc = machine.Cpu.PC; var cycles = machine.Cpu.Cycles;
        GWGUI.Emulation.Acorn.Emulators.AcornEmu.Interfaces.IAcornMachine runtime = machine;
        runtime.SetOption("acornemu.video-output", "composite-mono");
        Assert.True(machine.Video.Monochrome);
        runtime.SetOption("acornemu.video-output", "rgb");
        Assert.False(machine.Video.Monochrome);
        Assert.Equal(0x56, machine.Read(0x1234)); Assert.Equal(pc, machine.Cpu.PC); Assert.Equal(cycles, machine.Cpu.Cycles);
        Assert.Throws<ArgumentOutOfRangeException>(() => runtime.SetOption("acornemu.video-output", "invalid"));
        using var core = new EmulatorCore(MachineModel.BbcMicroModelA);
        var available = Assert.Single(core.Options);
        Assert.False(available.RequiresRestart);
        var hot = GWGUI.App.Functions.Emulation.Machine.EmulationRuntimeOptionFunctions.HotChanges(new Dictionary<string, string> { [available.Key] = "rgb" }, core.Options);
        Assert.Equal("rgb", hot[available.Key]);
        var settings = new GWGUI.Emulation.Acorn.Emulators.AcornEmu.Factories.AcornEmuMachineFactory().DescribeSettings(new MachineConfiguration(MachineModel.BbcMicroModelA, core: Emulator.AcornEmu));
        Assert.False(Assert.Single(settings.SelectMany(block => block.Fields), field => field.Id == available.Key).RequiresRestart);
    }

    [Fact]
    public void TeletextDoubleHeightUsesTopAndBottomHalvesOfTheExternalGlyph()
    {
        var font = new byte[960]; font[('A' - 32) * 10] = 1; font[('A' - 32) * 10 + 5] = 16;
        var text = new TeletextSaa5050(font);
        text.BeginLine(0); text.Character(13, 0, 0);
        Assert.Equal(3, text.Character('A', 0, 0).Mask);
        text.BeginLine(1);
        Assert.Equal(0, text.Character('A', 0, 0).Mask);
        text.Character(13, 0, 0);
        Assert.Equal(0x300, text.Character('A', 0, 0).Mask);
        text.BeginLine(2);
        Assert.Equal(3, text.Character('A', 0, 0).Mask);
    }

    [Fact]
    public void TeletextSeparatesMosaicsAndAppliesFlashConcealAndSteady()
    {
        var text = new TeletextSaa5050(new byte[960]);
        text.BeginLine(0); text.Character(17, 4, 0);
        Assert.Equal(0xfff, text.Character(127, 4, 0).Mask);
        text.Character(26, 4, 0);
        Assert.Equal(0, text.Character(127, 4, 0).Mask);
        Assert.NotEqual(0, text.Character(127, 2, 0).Mask);
        text.Character(8, 2, 0);
        Assert.Equal(0, text.Character(127, 2, 48).Mask);
        text.Character(9, 2, 48);
        Assert.NotEqual(0, text.Character(127, 2, 48).Mask);
        text.Character(24, 2, 0);
        Assert.Equal(0, text.Character(127, 2, 0).Mask);
    }

    private static BbcVideo Bitmap(byte[] ram)
    {
        var video = new BbcVideo(ram, ReadOnlySpan<byte>.Empty);
        Set(video, 0, 63); Set(video, 1, 40); Set(video, 2, 51); Set(video, 3, 0x24); Set(video, 4, 38);
        Set(video, 6, 32); Set(video, 7, 34); Set(video, 9, 7); Set(video, 10, 32); Set(video, 12, 0x0b);
        video.WriteUla(0xfe20, 0x88); return video;
    }
    private static void Set(BbcVideo video, byte register, byte value) { video.Select(register); video.WriteRegister(value); }
    private static void Advance(BbcVideo video, int cycles) { for (var cycle = 0; cycle < cycles; cycle++) video.Tick(); }
}
