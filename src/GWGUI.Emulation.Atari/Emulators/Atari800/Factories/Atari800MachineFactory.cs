using GWGUI.Emulation.Atari.Emulators.Atari800.Constants;
using GWGUI.Emulation.Atari.Emulators.Atari800.Exceptions;
using GWGUI.Emulation.Atari.Emulators.Atari800.Contracts;
using GWGUI.Emulation.Atari.Emulators.Atari800.Enums;
using GWGUI.Emulation.Atari.Emulators.Atari800.Functions;

namespace GWGUI.Emulation.Atari.Emulators.Atari800.Factories;

internal sealed class Atari800MachineFactory() : MachineFactory(EmulatorConstants.Entry)
{
    internal override IReadOnlySet<string> GetCartridgeExtensions(MachineConfiguration configuration) =>
        configuration.Model == MachineModel.Atari5200
            ? Atari800MediaConstants.ConsoleCartridgeExtensions
            : Atari800MediaConstants.ComputerCartridgeExtensions;

    public override IReadOnlyDictionary<string, string> PrepareOptions(
        IReadOnlyDictionary<string, string> options) => Atari800OptionFunctions.ToNative(options);

    public override MediaConfiguration? SelectPrimaryMedia(MachineConfiguration configuration) =>
        Atari800MediaFunctions.Primary(configuration.Media);

    public override EmulatorPreparedContent? PrepareContent(MachineConfiguration configuration,
        MediaConfiguration? media, string sessionDirectory, ExternalCoreInfo coreInfo)
    {
        if (media is null) return null;
        var prepared = Atari800MediaFunctions.Prepare(configuration, media, sessionDirectory, coreInfo.Extensions);
        return new EmulatorPreparedContent(media, prepared.RuntimePath, coreInfo.NeedsFullPath,
            Atari800MediaFunctions.ApplyOptions(configuration, prepared), prepared.SessionMedia,
            RequiresDiskControl: prepared.ContentType is Atari800ContentType.Floppy or Atari800ContentType.Cassette,
            State: prepared);
    }

    public override SessionMedia? PrepareInsertedMedia(MachineConfiguration configuration,
        MediaConfiguration media, string sessionDirectory, ExternalCoreInfo coreInfo)
    {
        if (media.Category == MediaCategory.Cartridge)
            throw new NotSupportedException(Atari800MediaErrors.DynamicCartridgeUnsupported);
        return Atari800MediaFunctions.Prepare(configuration, media, sessionDirectory, coreInfo.Extensions)
            .SessionMedia ?? throw new NotSupportedException(Atari800MediaErrors.DynamicCartridgeUnsupported);
    }

    public override void ValidatePreparedContent(EmulatorPreparedContent? preparedContent,
        bool diskControlAvailable)
    {
        if (preparedContent?.RequiresDiskControl == true && !diskControlAvailable)
            throw new EmulationException(ErrorCategory.Content, ErrorCode.ContentUnsupported,
                Atari800MediaErrors.MediaControlRequired);
    }

    public override bool SupportsDiskControl(MediaConfiguration media) =>
        media.Category is MediaCategory.Floppy or MediaCategory.Cassette;

    public override bool SupportsDiskControlOperations => true;
}
