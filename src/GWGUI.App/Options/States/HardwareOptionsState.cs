using GWGUI.Domain.Hardware;
using GWGUI.Domain.Settings;
using GWGUI.Domain.Settings.Hardware;
using GWGUI.App.Dictionaries.Options;
using GWGUI.App.Localization.Extensions;
using GWGUI.App.Services.Hardware;
using GWGUI.App.ViewModels.Options;

namespace GWGUI.App.Options.States;

internal sealed class HardwareOptionsState
{
    public HardwareOptionsState(AppSettings settings)
    {
        Controllers = settings.Controllers.Select(CloneController).ToList();
        UnconfiguredControllers = settings.UnconfiguredControllers.Select(CloneController).ToList();
        Drives = settings.Drives.Select(CloneDrive).ToList();
        AssignAllDriveSelections();
    }

    public List<ControllerSettings> Controllers { get; }
    public List<ControllerSettings> UnconfiguredControllers { get; }
    public List<DriveSettings> Drives { get; }

    public bool HasMaximumDrives(string controllerId) =>
        Drives.Count(drive => drive.ControllerUsbId == controllerId) >= 2;

    public bool Save(HardwareRow row)
    {
        var drive = row.DriveId is null ? null : Drives.FirstOrDefault(item => item.Id == row.DriveId);
        if (drive is null)
        {
            if (HasMaximumDrives(row.UsbId)) return false;
            var controller = UnconfiguredControllers.FirstOrDefault(item => item.UsbId == row.UsbId);
            if (controller is not null)
            {
                UnconfiguredControllers.Remove(controller);
                Controllers.Add(controller);
            }

            drive = new DriveSettings { ControllerUsbId = row.UsbId };
            Drives.Add(drive);
        }

        drive.Size = row.Size;
        drive.Density = row.Density;
        drive.NominalRpm = row.Rpm == HardwareChoices.UnknownSpeed ? null : int.Parse(row.Rpm.AsSpan(0, 3));
        AssignDriveSelections(row.UsbId);
        return true;
    }

    public void Remove(HardwareRow row)
    {
        if (row.DriveId is not null)
        {
            Drives.RemoveAll(item => item.Id == row.DriveId);
            if (!Drives.Any(item => item.ControllerUsbId == row.UsbId))
                Controllers.RemoveAll(item => item.UsbId == row.UsbId);
            else
                AssignDriveSelections(row.UsbId);
            return;
        }

        UnconfiguredControllers.RemoveAll(item => item.UsbId == row.UsbId);
        Controllers.RemoveAll(item => item.UsbId == row.UsbId);
    }

    public IReadOnlyList<HardwareRow> CreateRows()
    {
        var rows = new List<HardwareRow>();
        foreach (var controller in Controllers)
        {
            var drives = Drives.Where(item => item.ControllerUsbId == controller.UsbId).ToArray();
            if (drives.Length == 0) rows.Add(CreateRow(null, controller.UsbId, true));
            foreach (var drive in drives) rows.Add(CreateRow(drive, controller.UsbId, true));
        }

        foreach (var controller in UnconfiguredControllers)
            rows.Add(CreateRow(null, controller.UsbId, false));
        return rows;
    }

    public HardwareRow CreateDraftRow(string controllerId) => CreateRow(null, controllerId, true);

    public void MergeUnconfigured(IReadOnlyList<ControllerSettings> detectedControllers)
    {
        foreach (var controller in UnconfiguredControllers) controller.IsAvailable = false;
        foreach (var detected in detectedControllers)
        {
            if (Drives.Any(drive => string.Equals(drive.ControllerUsbId, detected.UsbId, StringComparison.OrdinalIgnoreCase)))
            {
                var configured = Controllers.FirstOrDefault(item => StartupHardwareMonitor.SameController(item, detected));
                if (configured is null) Controllers.Add(CloneController(detected));
                UnconfiguredControllers.RemoveAll(item => StartupHardwareMonitor.SameController(item, detected));
                continue;
            }

            var known = UnconfiguredControllers.FirstOrDefault(item => StartupHardwareMonitor.SameController(item, detected));
            if (known is null)
            {
                UnconfiguredControllers.Add(detected);
                continue;
            }

            known.UsbSerialNumber = detected.UsbSerialNumber;
            known.PnpDeviceId = detected.PnpDeviceId;
            known.LastUsbLocation = detected.LastUsbLocation;
            known.VendorId = detected.VendorId;
            known.ProductId = detected.ProductId;
            known.LastPort = detected.LastPort;
            known.Model = detected.Model;
            known.IsAvailable = detected.IsAvailable;
        }
    }

    private HardwareRow CreateRow(DriveSettings? drive, string controllerId, bool configured)
    {
        var controller = Controllers.Concat(UnconfiguredControllers).First(item => item.UsbId == controllerId);
        var index = drive is null
            ? Drives.Count(item => item.ControllerUsbId == controllerId) + 1
            : Drives.Where(item => item.ControllerUsbId == controllerId).ToList().IndexOf(drive) + 1;
        return new HardwareRow(
            drive?.Id,
            controller.LastPort,
            controllerId,
            LocExtension.Get("Hardware.ReaderNumber", index),
            drive?.Size ?? "3.5",
            drive?.Density ?? "Unknown",
            drive?.NominalRpm is int rpm ? $"{rpm} RPM" : HardwareChoices.UnknownSpeed,
            controller.IsAvailable,
            configured,
            LocExtension.Get(configured ? "Hardware.Configured" : "Hardware.NotConfiguredState"));
    }

    private void AssignAllDriveSelections()
    {
        foreach (var controllerId in Drives.Select(item => item.ControllerUsbId).Distinct(StringComparer.OrdinalIgnoreCase))
            AssignDriveSelections(controllerId);
    }

    private void AssignDriveSelections(string controllerId) =>
        HardwareRoutingPolicy.AssignAutomaticDriveSelections(Drives, controllerId);

    private static ControllerSettings CloneController(ControllerSettings source) => new()
    {
        UsbId = source.UsbId,
        UsbSerialNumber = source.UsbSerialNumber,
        PnpDeviceId = source.PnpDeviceId,
        LastUsbLocation = source.LastUsbLocation,
        VendorId = source.VendorId,
        ProductId = source.ProductId,
        LastPort = source.LastPort,
        Model = source.Model,
        IsAvailable = source.IsAvailable
    };

    private static DriveSettings CloneDrive(DriveSettings source) => new()
    {
        Id = source.Id,
        ControllerUsbId = source.ControllerUsbId,
        Selection = source.Selection,
        Size = source.Size,
        Density = source.Density,
        NominalRpm = source.NominalRpm
    };
}
