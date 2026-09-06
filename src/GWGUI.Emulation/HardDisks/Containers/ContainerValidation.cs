namespace GWGUI.Emulation.HardDisks.Containers;

internal static class ContainerValidation
{
    internal static void Validate(Stream destination, long capacity)
    {
        if (!destination.CanSeek || !destination.CanWrite || destination.Length != 0)
            throw new ArgumentException("An empty writable seekable stream is required.", nameof(destination));
        if (capacity < 512 || capacity % 512 != 0) throw new ArgumentOutOfRangeException(nameof(capacity));
    }
}
