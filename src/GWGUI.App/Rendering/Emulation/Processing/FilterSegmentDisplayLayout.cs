using GWGUI.Emulation.Enums;

namespace GWGUI.App.Rendering.Emulation.Processing;

internal static class FilterSegmentDisplayLayout
{
    internal const string Shader = """
        int filterSegmentCount(int segmentLayout,float flags)
        {
            int count=segmentLayout==0?7:(segmentLayout==1?14:16);
            if(mod(flags,2.0)>=1.0)count++;
            if(flags>=2.0)count+=2;
            return count;
        }
        vec4 filterSegmentLine(int index,int segmentLayout,float flags,out float point)
        {
            point=0.0;
            int baseCount=segmentLayout==0?7:(segmentLayout==1?14:16);
            if(index>=baseCount)
            {
                point=1.0;
                int extra=index-baseCount;
                if(mod(flags,2.0)>=1.0){if(extra==0)return vec4(.91,.9,.91,.9);extra--;}
                return extra==0?vec4(.91,.35,.91,.35):vec4(.91,.65,.91,.65);
            }
            if(segmentLayout==0)
            {
                if(index==0)return vec4(.2,.08,.8,.08);
                if(index==1)return vec4(.82,.1,.82,.48);
                if(index==2)return vec4(.82,.52,.82,.9);
                if(index==3)return vec4(.2,.92,.8,.92);
                if(index==4)return vec4(.18,.52,.18,.9);
                if(index==5)return vec4(.18,.1,.18,.48);
                return vec4(.2,.5,.8,.5);
            }
            if(segmentLayout==2)
            {
                if(index==0)return vec4(.2,.08,.48,.08);
                if(index==1)return vec4(.52,.08,.8,.08);
                if(index==2)return vec4(.82,.1,.82,.48);
                if(index==3)return vec4(.82,.52,.82,.9);
                if(index==4)return vec4(.2,.92,.48,.92);
                if(index==5)return vec4(.52,.92,.8,.92);
                if(index==6)return vec4(.18,.52,.18,.9);
                if(index==7)return vec4(.18,.1,.18,.48);
                index-=2;
            }
            if(index==0)return vec4(.2,.08,.8,.08);
            if(index==1)return vec4(.82,.1,.82,.48);
            if(index==2)return vec4(.82,.52,.82,.9);
            if(index==3)return vec4(.2,.92,.8,.92);
            if(index==4)return vec4(.18,.52,.18,.9);
            if(index==5)return vec4(.18,.1,.18,.48);
            if(index==6)return vec4(.2,.5,.48,.5);
            if(index==7)return vec4(.52,.5,.8,.5);
            if(index==8)return vec4(.2,.1,.46,.46);
            if(index==9)return vec4(.8,.1,.54,.46);
            if(index==10)return vec4(.2,.9,.46,.54);
            if(index==11)return vec4(.8,.9,.54,.54);
            if(index==12)return vec4(.5,.1,.5,.46);
            return vec4(.5,.54,.5,.9);
        }
        """;

    private static readonly SegmentDisplayElement[] Seven =
    [
        new(.2f,.08f,.8f,.08f), new(.82f,.1f,.82f,.48f), new(.82f,.52f,.82f,.9f),
        new(.2f,.92f,.8f,.92f), new(.18f,.52f,.18f,.9f), new(.18f,.1f,.18f,.48f),
        new(.2f,.5f,.8f,.5f)
    ];

    private static readonly SegmentDisplayElement[] Fourteen =
    [
        new(.2f,.08f,.8f,.08f), new(.82f,.1f,.82f,.48f), new(.82f,.52f,.82f,.9f),
        new(.2f,.92f,.8f,.92f), new(.18f,.52f,.18f,.9f), new(.18f,.1f,.18f,.48f),
        new(.2f,.5f,.48f,.5f), new(.52f,.5f,.8f,.5f),
        new(.2f,.1f,.46f,.46f), new(.8f,.1f,.54f,.46f),
        new(.2f,.9f,.46f,.54f), new(.8f,.9f,.54f,.54f),
        new(.5f,.1f,.5f,.46f), new(.5f,.54f,.5f,.9f)
    ];

    private static readonly SegmentDisplayElement[] Sixteen =
    [
        new(.2f,.08f,.48f,.08f), new(.52f,.08f,.8f,.08f),
        new(.82f,.1f,.82f,.48f), new(.82f,.52f,.82f,.9f),
        new(.2f,.92f,.48f,.92f), new(.52f,.92f,.8f,.92f),
        new(.18f,.52f,.18f,.9f), new(.18f,.1f,.18f,.48f),
        new(.2f,.5f,.48f,.5f), new(.52f,.5f,.8f,.5f),
        new(.2f,.1f,.46f,.46f), new(.8f,.1f,.54f,.46f),
        new(.2f,.9f,.46f,.54f), new(.8f,.9f,.54f,.54f),
        new(.5f,.1f,.5f,.46f), new(.5f,.54f,.5f,.9f)
    ];

    internal static SegmentDisplayElement[] Elements(EmulationSegmentDisplayLayout layout,
        bool decimalPoint, bool colon)
    {
        var elements = (layout switch
        {
            EmulationSegmentDisplayLayout.Fourteen => Fourteen,
            EmulationSegmentDisplayLayout.Sixteen => Sixteen,
            _ => Seven
        }).ToList();
        FilterSegmentDisplayDecimalPoint.AddTo(elements, decimalPoint);
        FilterSegmentDisplayColon.AddTo(elements, colon);
        return elements.ToArray();
    }
}
