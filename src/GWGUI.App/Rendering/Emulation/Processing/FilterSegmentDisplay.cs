using GWGUI.Emulation.Contracts;

namespace GWGUI.App.Rendering.Emulation.Processing;

internal static class FilterSegmentDisplay
{
    internal static readonly string VeldridShader = FilterSegmentDisplayCellSize.Shader
        + FilterSegmentDisplayHorizontalGap.Shader + FilterSegmentDisplayVerticalGap.Shader
        + FilterSegmentDisplaySegmentGap.Shader
        + FilterSegmentDisplayThickness.Shader + FilterSegmentDisplayEndShape.Shader
        + FilterSegmentDisplayActivationThreshold.Shader + FilterSegmentDisplayContrast.Shader
        + FilterSegmentDisplayColor.Shader
        + FilterSegmentDisplayBrightness.Shader + FilterSegmentDisplayBlackDepth.Shader
        + FilterSegmentDisplayOffSegmentVisibility.Shader
        + FilterSegmentDisplayHaloRadius.Shader + FilterSegmentDisplayGlow.Shader
        + FilterSegmentDisplayLayout.Shader + """
        vec3 segmentSource(vec2 uv,float history)
        {
            if(history>.5)return adjust(texture(sampler2D(History,LinearSampler),clamp(uv,vec2(0.0),vec2(1.0))).rgb);
            return raw(clamp(uv,vec2(0.0),vec2(1.0)));
        }
        float segmentActivation(vec2 cell,vec2 cellSize,vec4 line,float point,float history)
        {
            float sum=0.0;
            for(int sampleIndex=0;sampleIndex<5;sampleIndex++)
            {
                float position=point>.5?0.0:(float(sampleIndex)+.5)/5.0;
                vec2 local=mix(line.xy,line.zw,position);
                vec2 uv=(cell+local)*cellSize/max(Parameters.Processing.zw,vec2(1.0));
                sum+=dot(segmentSource(uv,history),vec3(.2126,.7152,.0722));
            }
            return filterSegmentContrast(filterSegmentThreshold(sum/5.0,
                Parameters.SegmentEmission.z),Parameters.SegmentEmission.w);
        }
        vec3 segmentDisplayPixel(vec2 uv)
        {
            float cellWidth=filterSegmentCellWidth(Parameters.SegmentGeometry.y);
            vec2 cellSize=vec2(cellWidth,max(9.0,floor(cellWidth*1.55+.5)));
            vec2 logical=uv*Parameters.Processing.zw;
            vec2 cell=floor(logical/cellSize);
            vec2 fraction=fract(logical/cellSize);
            vec2 local=vec2(filterSegmentHorizontalGap(fraction.x,Parameters.SegmentGeometry.z),
                filterSegmentVerticalGap(fraction.y,Parameters.SegmentGeometry.w));
            int segmentLayout=int(Parameters.SegmentGeometry.x+.5);
            int count=filterSegmentCount(segmentLayout,Parameters.SegmentShape.w);
            float nearestDistance=1000.0,nearestPoint=0.0;vec4 nearestLine=vec4(0.0);
            for(int element=0;element<19;element++)
            {
                if(element>=count)continue;
                float point;vec4 line=filterSegmentLine(element,segmentLayout,Parameters.SegmentShape.w,point);
                line=filterSegmentGap(line,Parameters.SegmentShape.y);
                float distance=filterSegmentDistance(local,line,Parameters.SegmentShape.z);
                if(distance<nearestDistance){nearestDistance=distance;nearestLine=line;nearestPoint=point;}
            }
            float current=segmentActivation(cell,cellSize,nearestLine,nearestPoint,0.0);
            if(Parameters.SegmentTemporal.z>.5)
            {
                float previous=segmentActivation(cell,cellSize,nearestLine,nearestPoint,1.0);
                current=current>=previous?mix(previous,current,Parameters.SegmentTemporal.x)
                    :max(current,previous*Parameters.SegmentTemporal.y);
            }
            current=filterSegmentOffVisibility(current,Parameters.SegmentOptical.x);
            float radius=filterSegmentThickness(Parameters.SegmentShape.x);
            float edge=max(Parameters.Processing.z/max(Parameters.Output.x,1.0),
                Parameters.Processing.w/max(Parameters.Output.y,1.0))/min(cellSize.x,cellSize.y);
            float core=smoothstep(radius+edge,radius-edge,nearestDistance);
            float halo=filterSegmentGlow(filterSegmentHaloFalloff(
                max(0.0,nearestDistance-radius),Parameters.SegmentOptical.w),
                Parameters.SegmentOptical.z)*(1.0-core);
            float emission=filterSegmentBrightness(current*clamp(core+halo,0.0,1.0),
                Parameters.SegmentEmission.y);
            return clamp(vec3(filterSegmentBlackDepth(Parameters.SegmentOptical.y))
                +emission*filterSegmentColor(Parameters.SegmentEmission.x),0.0,1.0);
        }
        """;

    internal static readonly string OpenGlShader = FilterSegmentDisplayCellSize.Shader
        + FilterSegmentDisplayHorizontalGap.Shader + FilterSegmentDisplayVerticalGap.Shader
        + FilterSegmentDisplaySegmentGap.Shader
        + FilterSegmentDisplayThickness.Shader + FilterSegmentDisplayEndShape.Shader
        + FilterSegmentDisplayActivationThreshold.Shader + FilterSegmentDisplayContrast.Shader
        + FilterSegmentDisplayColor.Shader
        + FilterSegmentDisplayBrightness.Shader + FilterSegmentDisplayBlackDepth.Shader
        + FilterSegmentDisplayOffSegmentVisibility.Shader
        + FilterSegmentDisplayHaloRadius.Shader + FilterSegmentDisplayGlow.Shader
        + FilterSegmentDisplayLayout.Shader + """
        vec3 segmentSource(vec2 uv,float history)
        {
            if(history>.5)return adjustColor(texture2D(History,clamp(uv,vec2(0.0),vec2(1.0))).rgb);
            return adjustColor(sampleConfigured(clamp(uv,vec2(0.0),vec2(1.0))).rgb);
        }
        float segmentActivation(vec2 cell,vec2 cellSize,vec4 line,float point,float history)
        {
            float sum=0.0;
            for(int sampleIndex=0;sampleIndex<5;sampleIndex++)
            {
                float position=point>.5?0.0:(float(sampleIndex)+.5)/5.0;
                vec2 local=mix(line.xy,line.zw,position);
                vec2 sampleUv=(cell+local)*cellSize/max(Processing.zw,vec2(1.0));
                sum+=dot(segmentSource(sampleUv,history),vec3(.2126,.7152,.0722));
            }
            return filterSegmentContrast(filterSegmentThreshold(sum/5.0,
                SegmentEmission.z),SegmentEmission.w);
        }
        vec3 segmentDisplayPixel(vec2 uv)
        {
            float cellWidth=filterSegmentCellWidth(SegmentGeometry.y);
            vec2 cellSize=vec2(cellWidth,max(9.0,floor(cellWidth*1.55+.5)));
            vec2 logical=uv*Processing.zw;
            vec2 cell=floor(logical/cellSize);
            vec2 fraction=fract(logical/cellSize);
            vec2 local=vec2(filterSegmentHorizontalGap(fraction.x,SegmentGeometry.z),
                filterSegmentVerticalGap(fraction.y,SegmentGeometry.w));
            int segmentLayout=int(SegmentGeometry.x+.5);
            int count=filterSegmentCount(segmentLayout,SegmentShape.w);
            float nearestDistance=1000.0,nearestPoint=0.0;vec4 nearestLine=vec4(0.0);
            for(int element=0;element<19;element++)
            {
                if(element>=count)continue;
                float point;vec4 line=filterSegmentLine(element,segmentLayout,SegmentShape.w,point);
                line=filterSegmentGap(line,SegmentShape.y);
                float distance=filterSegmentDistance(local,line,SegmentShape.z);
                if(distance<nearestDistance){nearestDistance=distance;nearestLine=line;nearestPoint=point;}
            }
            float current=segmentActivation(cell,cellSize,nearestLine,nearestPoint,0.0);
            if(SegmentTemporal.z>.5)
            {
                float previous=segmentActivation(cell,cellSize,nearestLine,nearestPoint,1.0);
                current=current>=previous?mix(previous,current,SegmentTemporal.x)
                    :max(current,previous*SegmentTemporal.y);
            }
            current=filterSegmentOffVisibility(current,SegmentOptical.x);
            float radius=filterSegmentThickness(SegmentShape.x);
            float edge=max(Processing.z/max(Output.x,1.0),Processing.w/max(Output.y,1.0))/min(cellSize.x,cellSize.y);
            float core=smoothstep(radius+edge,radius-edge,nearestDistance);
            float halo=filterSegmentGlow(filterSegmentHaloFalloff(
                max(0.0,nearestDistance-radius),SegmentOptical.w),SegmentOptical.z)*(1.0-core);
            float emission=filterSegmentBrightness(current*clamp(core+halo,0.0,1.0),SegmentEmission.y);
            return clamp(vec3(filterSegmentBlackDepth(SegmentOptical.y))+emission*filterSegmentColor(SegmentEmission.x),0.0,1.0);
        }
        """;

    internal static void Apply(float[] colors, int sourceWidth, int sourceHeight,
        int width, int height, EmulationSegmentDisplayVideoConfiguration configuration)
    {
        var source = colors.ToArray();
        var cellWidth = FilterSegmentDisplayCellSize.Width(configuration.CellSize);
        var cellHeight = FilterSegmentDisplayCellSize.Height(configuration.CellSize);
        var columns = Math.Max(1, (sourceWidth + cellWidth - 1) / cellWidth);
        var rows = Math.Max(1, (sourceHeight + cellHeight - 1) / cellHeight);
        var elements = FilterSegmentDisplayLayout.Elements(configuration.Layout,
            configuration.DecimalPoint, configuration.Colon)
            .Select(element => FilterSegmentDisplaySegmentGap.Apply(element,
                configuration.SegmentGap)).ToArray();
        var activations = SampleActivations(source, sourceWidth, sourceHeight, width, height,
            columns, rows, cellWidth, cellHeight, elements, configuration);
        var tint = FilterSegmentDisplayColor.Apply(configuration.Color);
        var radius = FilterSegmentDisplayThickness.Radius(configuration.Thickness);
        var background = FilterSegmentDisplayBlackDepth.Apply(configuration.BlackDepth);

        for (var y = 0; y < height; y++)
        for (var x = 0; x < width; x++)
        {
            var logicalX = (x + .5f) * sourceWidth / width;
            var logicalY = (y + .5f) * sourceHeight / height;
            var cellX = Math.Min(columns - 1, (int)(logicalX / cellWidth));
            var cellY = Math.Min(rows - 1, (int)(logicalY / cellHeight));
            var localX = FilterSegmentDisplayHorizontalGap.Apply(
                logicalX / cellWidth - MathF.Floor(logicalX / cellWidth),
                configuration.HorizontalGap);
            var localY = FilterSegmentDisplayVerticalGap.Apply(
                logicalY / cellHeight - MathF.Floor(logicalY / cellHeight),
                configuration.VerticalGap);
            var nearest = 0;
            var distance = float.MaxValue;
            for (var element = 0; element < elements.Length; element++)
            {
                var candidate = FilterSegmentDisplayEndShape.Distance(localX, localY,
                    elements[element], configuration.EndShape);
                if (candidate >= distance) continue;
                distance = candidate;
                nearest = element;
            }
            var edge = MathF.Max(sourceWidth / (float)width, sourceHeight / (float)height)
                / Math.Min(cellWidth, cellHeight);
            var core = SmoothStep(radius + edge, radius - edge, distance);
            var halo = FilterSegmentDisplayGlow.Apply(FilterSegmentDisplayHaloRadius.Apply(
                MathF.Max(0f, distance - radius), configuration.HaloRadius),
                configuration.Glow) * (1f - core);
            var cell = (cellY * columns + cellX) * elements.Length + nearest;
            var activation = FilterSegmentDisplayOffSegmentVisibility.Apply(
                activations[cell], configuration.OffSegmentVisibility);
            var emission = FilterSegmentDisplayBrightness.Apply(
                activation * Math.Clamp(core + halo, 0f, 1f), configuration.Brightness);
            var index = (y * width + x) * 3;
            colors[index] = Math.Clamp(background + emission * tint.R, 0f, 1f);
            colors[index + 1] = Math.Clamp(background + emission * tint.G, 0f, 1f);
            colors[index + 2] = Math.Clamp(background + emission * tint.B, 0f, 1f);
        }
    }

    private static float[] SampleActivations(float[] source, int sourceWidth, int sourceHeight,
        int width, int height, int columns, int rows, int cellWidth, int cellHeight,
        SegmentDisplayElement[] elements, EmulationSegmentDisplayVideoConfiguration configuration)
    {
        var result = new float[checked(columns * rows * elements.Length)];
        for (var cellY = 0; cellY < rows; cellY++)
        for (var cellX = 0; cellX < columns; cellX++)
        for (var elementIndex = 0; elementIndex < elements.Length; elementIndex++)
        {
            var element = elements[elementIndex];
            var sum = 0f;
            const int samples = 5;
            for (var sample = 0; sample < samples; sample++)
            {
                var position = element.IsPoint ? 0f : (sample + .5f) / samples;
                var localX = element.StartX + (element.EndX - element.StartX) * position;
                var localY = element.StartY + (element.EndY - element.StartY) * position;
                var logicalX = Math.Clamp((cellX + localX) * cellWidth, 0f, sourceWidth - 1f);
                var logicalY = Math.Clamp((cellY + localY) * cellHeight, 0f, sourceHeight - 1f);
                var x = Math.Clamp((int)(logicalX * width / sourceWidth), 0, width - 1);
                var y = Math.Clamp((int)(logicalY * height / sourceHeight), 0, height - 1);
                var index = (y * width + x) * 3;
                sum += source[index] * .2126f + source[index + 1] * .7152f
                    + source[index + 2] * .0722f;
            }
            result[(cellY * columns + cellX) * elements.Length + elementIndex] =
                FilterSegmentDisplayContrast.Apply(FilterSegmentDisplayActivationThreshold.Apply(
                    sum / samples, configuration.ActivationThreshold), configuration.Contrast);
        }
        return result;
    }

    private static float SmoothStep(float edge0, float edge1, float value)
    {
        if (Math.Abs(edge1 - edge0) < float.Epsilon) return value < edge0 ? 0f : 1f;
        var position = Math.Clamp((value - edge0) / (edge1 - edge0), 0f, 1f);
        return position * position * (3f - 2f * position);
    }
}
