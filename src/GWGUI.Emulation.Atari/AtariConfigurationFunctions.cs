using GWGUI.Emulation;

namespace GWGUI.Emulation.Atari;

internal static class AtariConfigurationFunctions
{
    internal static AtariCoreKind GetCore(AtariMachineModel model) => model switch
    {
        AtariMachineModel.St or AtariMachineModel.Stf or AtariMachineModel.Stfm or AtariMachineModel.MegaSt
            or AtariMachineModel.Ste or AtariMachineModel.MegaSte or AtariMachineModel.Tt or AtariMachineModel.Falcon
            => AtariCoreKind.Hatari,
        AtariMachineModel.Atari400 or AtariMachineModel.Atari800 or AtariMachineModel.Atari800Xl
            or AtariMachineModel.Atari130Xe or AtariMachineModel.ModernXlXe320K or AtariMachineModel.ModernXlXe576K
            or AtariMachineModel.ModernXlXe1088K or AtariMachineModel.Xegs or AtariMachineModel.Atari5200
            => AtariCoreKind.Atari800,
        AtariMachineModel.Atari2600 => AtariCoreKind.Stella,
        AtariMachineModel.Atari7800 => AtariCoreKind.ProSystem,
        AtariMachineModel.Lynx => AtariCoreKind.BeetleLynx,
        AtariMachineModel.Jaguar or AtariMachineModel.JaguarCd => AtariCoreKind.VirtualJaguar,
        _ => throw new ArgumentOutOfRangeException(nameof(model), model, null)
    };

    internal static AtariMachineFamily GetFamily(AtariMachineModel model) => model switch
    {
        AtariMachineModel.St or AtariMachineModel.Stf or AtariMachineModel.Stfm or AtariMachineModel.MegaSt
            or AtariMachineModel.Ste or AtariMachineModel.MegaSte or AtariMachineModel.Tt or AtariMachineModel.Falcon
            => AtariMachineFamily.St,
        AtariMachineModel.Atari400 or AtariMachineModel.Atari800 or AtariMachineModel.Atari800Xl
            or AtariMachineModel.Atari130Xe or AtariMachineModel.ModernXlXe320K or AtariMachineModel.ModernXlXe576K
            or AtariMachineModel.ModernXlXe1088K or AtariMachineModel.Xegs => AtariMachineFamily.EightBit,
        AtariMachineModel.Atari5200 => AtariMachineFamily.Atari5200,
        AtariMachineModel.Atari2600 => AtariMachineFamily.Atari2600,
        AtariMachineModel.Atari7800 => AtariMachineFamily.Atari7800,
        AtariMachineModel.Lynx => AtariMachineFamily.Lynx,
        AtariMachineModel.Jaguar or AtariMachineModel.JaguarCd => AtariMachineFamily.Jaguar,
        _ => throw new ArgumentOutOfRangeException(nameof(model), model, null)
    };

    internal static void Validate(int schemaVersion, AtariMachineModel model,
        IReadOnlyList<AtariFirmwareConfiguration> firmwares, IReadOnlyList<AtariMediaConfiguration> media,
        AtariInputConfiguration input)
    {
        if (schemaVersion != AtariConstants.CurrentConfigurationSchemaVersion)
            throw new ArgumentOutOfRangeException(nameof(schemaVersion), AtariErrorMessages.UnsupportedSchema);

        ValidateFirmware(model, firmwares);
        ValidateMedia(model, media);
        ValidateInput(input);
    }

    private static void ValidateFirmware(AtariMachineModel model, IReadOnlyList<AtariFirmwareConfiguration> firmwares)
    {
        var kinds = new HashSet<AtariFirmwareKind>();
        foreach (var firmware in firmwares)
        {
            if (string.IsNullOrWhiteSpace(firmware.Path))
                throw new ArgumentException(AtariErrorMessages.EmptyFirmwarePath, nameof(firmwares));
            if (!kinds.Add(firmware.Kind))
                throw new ArgumentException(AtariErrorMessages.DuplicateFirmware, nameof(firmwares));
            if (!IsFirmwareCompatible(model, firmware.Kind))
                throw new ArgumentException(AtariErrorMessages.IncompatibleFirmware, nameof(firmwares));
        }
    }

    private static bool IsFirmwareCompatible(AtariMachineModel model, AtariFirmwareKind kind)
    {
        var family = GetFamily(model);
        return family switch
        {
            AtariMachineFamily.St => kind == AtariFirmwareKind.Tos,
            AtariMachineFamily.EightBit => kind is AtariFirmwareKind.AtariOsA or AtariFirmwareKind.AtariOsB
                or AtariFirmwareKind.AtariXlOs or AtariFirmwareKind.AtariBasic or AtariFirmwareKind.AtariXegsBios,
            AtariMachineFamily.Atari5200 => kind == AtariFirmwareKind.Atari5200Bios,
            AtariMachineFamily.Atari7800 => kind == AtariFirmwareKind.Atari7800Bios,
            AtariMachineFamily.Lynx => kind == AtariFirmwareKind.LynxBootRom,
            AtariMachineFamily.Jaguar when model == AtariMachineModel.JaguarCd => kind == AtariFirmwareKind.JaguarCdBios,
            _ => false
        };
    }

    private static void ValidateMedia(AtariMachineModel model, IReadOnlyList<AtariMediaConfiguration> media)
    {
        var slots = new HashSet<EmulationMediaSlot>();
        foreach (var item in media)
        {
            if (string.IsNullOrWhiteSpace(item.Path))
                throw new ArgumentException(AtariErrorMessages.EmptyMediaPath, nameof(media));
            if (!slots.Add(item.Slot))
                throw new ArgumentException(AtariErrorMessages.DuplicateMediaSlot, nameof(media));
            if (!IsMediaCompatible(model, item.Kind, item.Slot))
                throw new ArgumentException(AtariErrorMessages.IncompatibleMedia, nameof(media));
        }
    }

    private static bool IsMediaCompatible(AtariMachineModel model, AtariMediaKind kind, EmulationMediaSlot slot)
    {
        var family = GetFamily(model);
        return family switch
        {
            AtariMachineFamily.St => kind switch
            {
                AtariMediaKind.Floppy => slot is >= EmulationMediaSlot.Floppy0 and <= EmulationMediaSlot.Floppy3,
                AtariMediaKind.HardDisk => slot == EmulationMediaSlot.HardDisk0,
                AtariMediaKind.Directory => slot == EmulationMediaSlot.HardDisk0,
                _ => false
            },
            AtariMachineFamily.EightBit => kind switch
            {
                AtariMediaKind.Floppy => slot is >= EmulationMediaSlot.Floppy0 and <= EmulationMediaSlot.Floppy3,
                AtariMediaKind.Cassette => slot == EmulationMediaSlot.Cassette0,
                AtariMediaKind.Cartridge => slot == EmulationMediaSlot.Cartridge0,
                _ => false
            },
            AtariMachineFamily.Atari5200 or AtariMachineFamily.Atari2600 or AtariMachineFamily.Atari7800
                or AtariMachineFamily.Lynx => kind == AtariMediaKind.Cartridge && slot == EmulationMediaSlot.Cartridge0,
            AtariMachineFamily.Jaguar when model == AtariMachineModel.JaguarCd =>
                (kind == AtariMediaKind.CompactDisc && slot == EmulationMediaSlot.Cd0)
                || (kind == AtariMediaKind.Cartridge && slot == EmulationMediaSlot.Cartridge0),
            AtariMachineFamily.Jaguar => kind == AtariMediaKind.Cartridge && slot == EmulationMediaSlot.Cartridge0,
            _ => false
        };
    }

    private static void ValidateInput(AtariInputConfiguration input)
    {
        var ports = new HashSet<int>();
        foreach (var controller in input.Controllers ?? [])
        {
            if (controller.Port < AtariConstants.MinimumControllerPort
                || controller.Port >= AtariConstants.MaximumControllerPortCount)
                throw new ArgumentOutOfRangeException(nameof(input), AtariErrorMessages.InvalidControllerPort);
            if (!ports.Add(controller.Port))
                throw new ArgumentException(AtariErrorMessages.DuplicateControllerPort, nameof(input));
        }
    }
}
