using GWGUI.Emulation.Amiga.Cores;
using GWGUI.Emulation.Common;
using System.Runtime.InteropServices;

namespace GWGUI.Tests;

public sealed class AmigaDiskControlTests
{
    [Fact]
    public void Select_RestoresTheInsertedDiskWhenIndexSelectionFails()
    {
        var ejected = false;
        uint index = 0;
        var ejectCalls = new List<bool>();
        ExternalCoreApi.SetEjectState setEject = value => { ejected = value; ejectCalls.Add(value); return true; };
        ExternalCoreApi.GetEjectState getEject = () => ejected;
        ExternalCoreApi.GetImageIndex getIndex = () => index;
        ExternalCoreApi.SetImageIndex setIndex = value => value == 0 && (index = value) == 0;
        ExternalCoreApi.GetImageCount getCount = () => 2;
        ExternalCoreApi.ReplaceImage replace = (_, _) => true;
        ExternalCoreApi.AddImage add = () => true;
        var control = Capture(setEject, getEject, getIndex, setIndex, getCount, replace, add);

        Assert.Throws<InvalidOperationException>(() => control.Select(1));

        Assert.False(ejected);
        Assert.Equal(0u, index);
        Assert.Equal([true, false], ejectCalls);
        GC.KeepAlive((setEject, getEject, getIndex, setIndex, getCount, replace, add));
    }

    [Fact]
    public void Select_RestoresThePreviousIndexWhenInsertionFails()
    {
        var ejected = false;
        uint index = 0;
        var insertionAttempts = 0;
        ExternalCoreApi.SetEjectState setEject = value =>
        {
            if (!value && ++insertionAttempts == 1) return false;
            ejected = value;
            return true;
        };
        ExternalCoreApi.GetEjectState getEject = () => ejected;
        ExternalCoreApi.GetImageIndex getIndex = () => index;
        ExternalCoreApi.SetImageIndex setIndex = value => { index = value; return true; };
        ExternalCoreApi.GetImageCount getCount = () => 2;
        ExternalCoreApi.ReplaceImage replace = (_, _) => true;
        ExternalCoreApi.AddImage add = () => true;
        var control = Capture(setEject, getEject, getIndex, setIndex, getCount, replace, add);

        Assert.Throws<InvalidOperationException>(() => control.Select(1));

        Assert.False(ejected);
        Assert.Equal(0u, index);
        Assert.Equal(2, insertionAttempts);
        GC.KeepAlive((setEject, getEject, getIndex, setIndex, getCount, replace, add));
    }

    private static AmigaExternalDiskControl Capture(ExternalCoreApi.SetEjectState setEject,
        ExternalCoreApi.GetEjectState getEject, ExternalCoreApi.GetImageIndex getIndex,
        ExternalCoreApi.SetImageIndex setIndex, ExternalCoreApi.GetImageCount getCount,
        ExternalCoreApi.ReplaceImage replace, ExternalCoreApi.AddImage add)
    {
        var native = new ExternalCoreApi.DiskControl
        {
            SetEjectState = Marshal.GetFunctionPointerForDelegate(setEject),
            GetEjectState = Marshal.GetFunctionPointerForDelegate(getEject),
            GetImageIndex = Marshal.GetFunctionPointerForDelegate(getIndex),
            SetImageIndex = Marshal.GetFunctionPointerForDelegate(setIndex),
            GetImageCount = Marshal.GetFunctionPointerForDelegate(getCount),
            ReplaceImage = Marshal.GetFunctionPointerForDelegate(replace),
            AddImage = Marshal.GetFunctionPointerForDelegate(add)
        };
        var pointer = Marshal.AllocHGlobal(Marshal.SizeOf<ExternalCoreApi.DiskControl>());
        try
        {
            Marshal.StructureToPtr(native, pointer, false);
            var control = new AmigaExternalDiskControl();
            control.Capture(pointer);
            return control;
        }
        finally { Marshal.FreeHGlobal(pointer); }
    }
}
