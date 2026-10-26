using Phase = GWGUI.Emulation.Acorn.Emulators.AcornEmu.Enums.AcornScsiPhase;
using System.Buffers.Binary;
using System.Text;
using GWGUI.Emulation.Constants;
using C = GWGUI.Emulation.Acorn.Emulators.AcornEmu.Constants.AcornScsiConstants;

namespace GWGUI.Emulation.Acorn.Emulators.AcornEmu.Services;

/// <summary>Commandes du contrôleur Winchester et construction des réponses d'identité, capacité et géométrie.</summary>
internal sealed partial class AcornScsi
{
    private void BeginCommand()
    {
        _unit = _command[1] >> 5; _offset = 0; _sectorTransfer = false;
        _lba = _commandLength == 6 ? ((_command[1] & 31) << 16) | _command[2] << 8 | _command[3] : BinaryPrimitives.ReadUInt32BigEndian(_command.AsSpan(2, 4));
        _remaining = _commandLength == 6 ? _command[4] == 0 ? 256 : _command[4] : BinaryPrimitives.ReadUInt16BigEndian(_command.AsSpan(7, 2));
        _phase = Phase.Executing;
        var latency = _unit < C.UnitCount ? _command[0] is 4 or 0x0a or 0x2a or 0x15 or 0x35 ? _writeLatencies[_unit] : _latencies[_unit] : 0;
        _delay = _unit < C.UnitCount && _speeds[_unit] != 0 ? checked((int)((long)_clock * latency / 1000 * 100 / _speeds[_unit])) : 0;
        _activityTicks = _delay + _clock / 20;
    }

    private void Execute()
    {
        if (_unit >= C.UnitCount) { Finish(C.NotReady); return; }
        if (_command[0] == 3)
        {
            var bytes = new byte[_command[4] == 0 ? 4 : _command[4]];
            bytes[0] = _sense[_unit];
            if (bytes.Length > 1) bytes[1] = (byte)(_senseAddress[_unit] >> 16);
            if (bytes.Length > 2) bytes[2] = (byte)(_senseAddress[_unit] >> 8);
            if (bytes.Length > 3) bytes[3] = (byte)_senseAddress[_unit];
            _sense[_unit] = 0; _senseAddress[_unit] = 0; Reply(bytes); return;
        }
        if (_command[0] == 0x12)
        {
            var bytes = new byte[36]; bytes[2] = 1; bytes[3] = 1; bytes[4] = 31;
            Encoding.ASCII.GetBytes("GWGUI   ACORN WINCHESTER 0001").CopyTo(bytes, 8);
            Reply(bytes.Take(_command[4]).ToArray()); return;
        }
        var image = Image;
        if (image is null) { Finish(C.NotReady); return; }
        switch (_command[0])
        {
            case 0: case 0x1b: case 0x1e: Finish(); return;
            case 0x35:
                try { image.FlushAsync().GetAwaiter().GetResult(); Finish(); }
                catch (Exception error) when (MediaFailure(error)) { Finish(C.WriteError); }
                return;
            case 4:
                if (image.IsReadOnly) { Finish(C.WriteProtected); return; }
                _lba = 0; _phase = Phase.Formatting; _delay = 0; return;
            case 0x08: case 0x28: case 0x0a: case 0x2a:
                if (!RangeAvailable()) { Finish(C.InvalidAddress); return; }
                if (_remaining == 0) { Finish(); return; }
                _sectorTransfer = true;
                if (_command[0] is 0x08 or 0x28) PrepareRead();
                else if (image.IsReadOnly) Finish(C.WriteProtected);
                else { _phase = Phase.Write; _buffer = new byte[C.SectorSize]; _offset = 0; _requestDelay = 0; }
                return;
            case 0x0b: case 0x2f:
                if (_lba < VirtualSectorCount) TrackHeadPosition();
                Finish(_lba < VirtualSectorCount ? (byte)0 : C.InvalidAddress); return;
            case 0x0f: Reply([(byte)_lba, (byte)(_lba >> 8), (byte)(_lba >> 16), 0]); return;
            case 0x15:
                if (image.IsReadOnly) { Finish(C.WriteProtected); return; }
                if (_command[4] < 22) { Finish(C.InvalidParameter); return; }
                _phase = Phase.Write; _buffer = new byte[_command[4]]; _offset = 0; _requestDelay = 0; return;
            case 0x1a:
                var descriptor = Descriptor();
                if (descriptor is null) { Finish(C.InvalidParameter); return; }
                Reply(descriptor.Take(_command[4] == 0 ? 22 : _command[4]).ToArray()); return;
            case 0x25:
                var capacity = new byte[8]; BinaryPrimitives.WriteUInt32BigEndian(capacity, (uint)Math.Min(uint.MaxValue, VirtualSectorCount - 1));
                BinaryPrimitives.WriteUInt32BigEndian(capacity.AsSpan(4), C.SectorSize); Reply(capacity); return;
            default: Finish(C.InvalidCommand); return;
        }
    }

    private void Reply(byte[] bytes)
    {
        _sectorTransfer = false;
        if (bytes.Length == 0) { Finish(); return; }
        _buffer = bytes; _offset = 0; _phase = Phase.Read; _requestDelay = 0;
    }

    private byte[]? Descriptor()
    {
        var image = Image!;
        if (image.Metadata.TryGetValue(EmulationDiskMetadataKeys.AcornScsiParameters, out var encoded))
        {
            try { return Convert.FromBase64String(encoded); }
            catch (FormatException) { return null; }
        }
        if (image.Cylinders is <= 0 or > 65535 || image.Heads is <= 0 or > 255 || image.SectorsPerTrack != C.SectorsPerTrack || image.SectorSize != C.SectorSize) return null;
        var bytes = new byte[C.DescriptorLength];
        bytes[13] = (byte)(image.Cylinders >> 8); bytes[14] = (byte)image.Cylinders; bytes[15] = (byte)image.Heads;
        return bytes;
    }
}
