namespace GWGUI.Emulation.Sony.Emulators.Common.Interop.Constants;

internal static class HardwareRenderConstants
{
    internal const string VulkanApiName = "Vulkan";
    internal const uint SetHardwareRender = 14;
    internal const uint GetPreferredHardwareRender = 56 | 0x10000;
    internal static readonly nint HardwareFramebuffer = new(-1);
    internal const int PixelByteCount = sizeof(int);
}
