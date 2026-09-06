namespace GWGUI.Emulation.HardDisks;

public static class HardDiskImageCreation
{
    public static void Create(string path, DiskImagePlan plan, DiskFormatRegistry? registry = null, DiskConsumerProfile? consumer = null)
    {
        var write = DiskImageBuilder.Prepare(plan, registry, consumer);
        Publish(path, plan.FixedSize && string.Equals(plan.ContainerId, "raw", StringComparison.OrdinalIgnoreCase)
            ? plan.CapacityBytes : 0, write);
    }

    public static void Create(string path, long bytes, HardDiskImageFormat format, bool preallocate,
        HardDiskPreparation preparation = HardDiskPreparation.Blank)
    {
        HardDiskImageValidation.Validate(path, bytes, format);
        if (preparation != HardDiskPreparation.Blank && format.Preparations?.Contains(preparation) != true)
            throw new ArgumentException("Unsupported disk preparation.", nameof(preparation));
        Publish(path, preallocate && format.Container == Containers.DiskContainerKind.Raw ? bytes : 0,
            stream => Containers.DiskContainerWriter.Write(stream, bytes, format.Container,
                content => HardDiskPreparationWriter.Prepare(content, preparation), fixedSize: preallocate));
    }

    private static void Publish(string path, long preallocationBytes, Action<Stream> write)
    {
        var target = Path.GetFullPath(path);
        if (File.Exists(target)) throw new IOException("The disk image already exists.");
        var temporary = target + "." + Guid.NewGuid().ToString("N") + ".creating";
        var ownsTemporary = false;
        try
        {
            using (var stream = new FileStream(temporary, new FileStreamOptions
            {
                Mode = FileMode.CreateNew, Access = FileAccess.ReadWrite, Share = FileShare.None,
                PreallocationSize = preallocationBytes
            }))
            {
                ownsTemporary = true;
                write(stream);
                stream.Flush(flushToDisk: true);
            }
            // Atomic publication, without overwriting an image created in the meantime.
            File.Move(temporary, target, overwrite: false);
        }
        finally
        {
            if (ownsTemporary && File.Exists(temporary)) File.Delete(temporary);
        }
    }
}
