using System.Runtime.InteropServices;
using GWGUI.Emulation.Amiga.Cores;

namespace GWGUI.Tests;

public sealed class AmigaDiskControlTests
{
    [Fact]
    public void Select_RestoresTheInsertedDiskWhenIndexSelectionFails()
    {
        var ejected = false;
        uint index = 0;
        var ejectCalls = new List<bool>();
        AmigaExternalApi.SetEjectState setEject = value => { ejected = value; ejectCalls.Add(value); return true; };
        AmigaExternalApi.GetEjectState getEject = () => ejected;
        AmigaExternalApi.GetImageIndex getIndex = () => index;
        AmigaExternalApi.SetImageIndex setIndex = value => value == 0 && (index = value) == 0;
        AmigaExternalApi.GetImageCount getCount = () => 2;
        AmigaExternalApi.ReplaceImage replace = (_, _) => true;
        AmigaExternalApi.AddImage add = () => true;
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
        AmigaExternalApi.SetEjectState setEject = value =>
        {
            if (!value && ++insertionAttempts == 1) return false;
            ejected = value;
            return true;
        };
        AmigaExternalApi.GetEjectState getEject = () => ejected;
        AmigaExternalApi.GetImageIndex getIndex = () => index;
        AmigaExternalApi.SetImageIndex setIndex = value => { index = value; return true; };
        AmigaExternalApi.GetImageCount getCount = () => 2;
        AmigaExternalApi.ReplaceImage replace = (_, _) => true;
        AmigaExternalApi.AddImage add = () => true;
        var control = Capture(setEject, getEject, getIndex, setIndex, getCount, replace, add);

        Assert.Throws<InvalidOperationException>(() => control.Select(1));

        Assert.False(ejected);
        Assert.Equal(0u, index);
        Assert.Equal(2, insertionAttempts);
        GC.KeepAlive((setEject, getEject, getIndex, setIndex, getCount, replace, add));
    }

    private static AmigaExternalDiskControl Capture(AmigaExternalApi.SetEjectState setEject,
        AmigaExternalApi.GetEjectState getEject, AmigaExternalApi.GetImageIndex getIndex,
        AmigaExternalApi.SetImageIndex setIndex, AmigaExternalApi.GetImageCount getCount,
        AmigaExternalApi.ReplaceImage replace, AmigaExternalApi.AddImage add)
    {
        var native = new AmigaExternalApi.DiskControl
        {
            SetEjectState = Marshal.GetFunctionPointerForDelegate(setEject),
            GetEjectState = Marshal.GetFunctionPointerForDelegate(getEject),
            GetImageIndex = Marshal.GetFunctionPointerForDelegate(getIndex),
            SetImageIndex = Marshal.GetFunctionPointerForDelegate(setIndex),
            GetImageCount = Marshal.GetFunctionPointerForDelegate(getCount),
            ReplaceImage = Marshal.GetFunctionPointerForDelegate(replace),
            AddImage = Marshal.GetFunctionPointerForDelegate(add)
        };
        var pointer = Marshal.AllocHGlobal(Marshal.SizeOf<AmigaExternalApi.DiskControl>());
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
