namespace GWGUI.Emulation.Sony.Emulators.Common.Interop.Constants;

internal static class HardwareRenderConstants
{
    internal const string VulkanApiName = "Vulkan";
    internal static readonly nint HardwareFramebuffer = new(-1);
    internal const int PixelByteCount = sizeof(int);
}
