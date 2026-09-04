namespace GWGUI.App.Rendering.Emulation.Processing;

internal static class VeldridVideoProcessingShaders
{
    internal const string Vertex = """
        #version 450
        layout(location=0) in vec2 Position;
        layout(location=1) in vec2 TexCoord;
        layout(location=0) out vec2 fsin_TexCoord;
        void main()
        {
            gl_Position = vec4(Position, 0, 1);
            fsin_TexCoord = TexCoord;
        }
        """;



    private const string FragmentHeader = """
        #version 450
        layout(set=0,binding=0) uniform VideoParameters
        {
            vec4 Adjustments; vec4 Processing; vec4 Output;
            vec4 CrtDisplay; vec4 CrtBeam; vec4 CrtOptical; vec4 CrtGeometry;
            vec4 CrtScanlines; vec4 CrtPattern; vec4 CrtPatternIntensity;
            vec4 FixedDisplay; vec4 FixedSpatial; vec4 FixedTechnology; vec4 FixedTemporal;
            vec4 PlasmaEffect; vec4 PlasmaTemporal; vec4 PlasmaDisplay;
            vec4 VectorEffect; vec4 VectorTemporal; vec4 VectorDisplay;
            vec4 Restoration; vec4 SegmentDisplay; vec4 SegmentTemporal;
            vec4 General; vec4 Restoration2; vec4 Temporal;
            vec4 Signal; vec4 Signal2; vec4 Stylistic; vec4 Stylistic2;
            vec4 VfdDisplay; vec4 VfdStructure; vec4 VfdOptical;
            vec4 LedMatrixEmission; vec4 LedMatrixStructure;
            vec4 DotMatrixGeometry; vec4 DotMatrixEmission; vec4 DotMatrixTemporal; vec4 EPaper;
            vec4 Projection;
        } Parameters;
        layout(set=0,binding=1) uniform texture2D Source;
        layout(set=0,binding=2) uniform texture2D History;
        layout(set=0,binding=3) uniform sampler PointSampler;
        layout(set=0,binding=4) uniform sampler LinearSampler;
        layout(location=0) in vec2 fsin_TexCoord;
        layout(location=0) out vec4 fsout_Color;

        """;

    private static readonly string FragmentBody = """
        float srgbToLinear(float value)
        {
            value=clamp(value,0.0,1.0);
            return value<=.04045 ? value/12.92 : pow((value+.055)/1.055,2.4);
        }
        float linearToSrgb(float value)
        {
            value=clamp(value,0.0,1.0);
            return value<=.0031308 ? value*12.92 : 1.055*pow(value,1.0/2.4)-.055;
        }
        vec3 adjust(vec3 color)
        {
            vec3 linear=vec3(srgbToLinear(color.r),srgbToLinear(color.g),srgbToLinear(color.b));
            linear=videoBrightnessParameter(linear,Parameters.Adjustments.x);
            linear=videoContrastParameter(linear,Parameters.Adjustments.y);
            linear=videoGammaParameter(linear,Parameters.Adjustments.z);
            return videoSaturationParameter(linear,Parameters.Adjustments.w);
        }
        vec3 detail(vec2 uv)
        {
            vec3 center=adjust(sourceColor(uv));
            float strength=Parameters.Restoration.x;
            if(strength<=0.0) return center;
            vec2 stepSize=1.0/max(Parameters.Output.xy,vec2(1.0));
            vec3 sum=vec3(0.0),minimum=center,maximum=center;
            float localContrast=0.0;
            for(int y=-1;y<=1;y++) for(int x=-1;x<=1;x++)
            {
                if(x==0&&y==0) continue;
                vec3 sampleColor=adjust(sourceColor(uv+vec2(x,y)*stepSize));
                sum+=sampleColor; minimum=min(minimum,sampleColor); maximum=max(maximum,sampleColor);
                localContrast=max(localContrast,length(center-sampleColor));
            }
            if(localContrast<=.001) return center;
            float amount=strength*clamp((.35-localContrast)/.30,0.0,1.0);
            float extension=localContrast*.25*strength;
            return clamp(center+(center-sum/8.0)*amount,minimum-vec3(extension),maximum+vec3(extension));
        }
        float distanceToSegment(vec2 point,vec4 segment)
        {
            vec2 direction=segment.zw-segment.xy;
            float position=clamp(dot(point-segment.xy,direction)/max(dot(direction,direction),.0001),0.0,1.0);
            return length(point-(segment.xy+position*direction));
        }
        float shape(vec2 point,int segmentLayout)
        {
            float d=distanceToSegment(point,vec4(.2,.08,.8,.08));
            d=min(d,distanceToSegment(point,vec4(.82,.1,.82,.48)));
            d=min(d,distanceToSegment(point,vec4(.82,.52,.82,.9)));
            d=min(d,distanceToSegment(point,vec4(.2,.92,.8,.92)));
            d=min(d,distanceToSegment(point,vec4(.18,.52,.18,.9)));
            d=min(d,distanceToSegment(point,vec4(.18,.1,.18,.48)));
            d=min(d,distanceToSegment(point,vec4(.2,.5,.8,.5)));
            if(segmentLayout>=1)
            {
                d=min(d,distanceToSegment(point,vec4(.2,.1,.48,.47)));
                d=min(d,distanceToSegment(point,vec4(.8,.1,.52,.47)));
                d=min(d,distanceToSegment(point,vec4(.2,.9,.48,.53)));
                d=min(d,distanceToSegment(point,vec4(.8,.9,.52,.53)));
                d=min(d,distanceToSegment(point,vec4(.5,.1,.5,.47)));
                d=min(d,distanceToSegment(point,vec4(.5,.53,.5,.9)));
            }
            if(segmentLayout>=2)
            {
                d=min(d,distanceToSegment(point,vec4(.28,.3,.72,.3)));
                d=min(d,distanceToSegment(point,vec4(.28,.7,.72,.7)));
            }
            return d;
        }
        vec3 tint()
        {
            int color=int(Parameters.SegmentTemporal.y+.5);
            if(color==1)return vec3(.04,1.0,.08);
            if(color==2)return vec3(1.0,.42,.015);
            if(color==3)return vec3(.03,.28,1.0);
            if(color==4)return vec3(1.0);
            return vec3(1.0,.025,.01);
        }
        vec3 mapSegments(vec3 source,vec2 uv)
        {
            float luminance=dot(source,vec3(.2126,.7152,.0722));
            float contrast=.65+Parameters.SegmentDisplay.y*2.35;
            float activation=clamp((luminance-.5)*contrast+.5,0.0,1.0);
            vec2 pixel=floor(uv*Parameters.Output.xy);
            vec2 local=(mod(pixel,vec2(8.0,12.0))+.5)/vec2(8.0,12.0);
            float distance=shape(local,int(Parameters.SegmentDisplay.w+.5));
            float thickness=.025+Parameters.SegmentDisplay.x*.105;
            float core=clamp((thickness-distance)*25.0,0.0,1.0);
            float halo=clamp(1.0-distance/.22,0.0,1.0)*Parameters.SegmentDisplay.z*.5;
            return clamp(activation*clamp(core+halo*(1.0-core),0.0,1.0)*tint(),0.0,1.0);
        }
        float hash(vec2 p){return fract(sin(dot(p,vec2(12.9898,78.233))+Parameters.General.z)*43758.5453);}
        vec3 raw(vec2 uv){return adjust(sourceColor(uv));}
        float restorationDistance(vec3 a,vec3 b){return length(a-b);}
        float restorationLuminance(vec3 c){return dot(c,vec3(.2126,.7152,.0722));}
        vec3 restored(vec2 uv)
        {
            vec2 s=1.0/max(Parameters.Output.xy,vec2(1.0));
            int mode=int(Parameters.Restoration2.w+.5);
            if((mode==1||mode==2)&&mod(floor(uv.y*Parameters.Output.y),2.0)!=(mode==1?0.0:1.0))uv.y-=s.y;
            vec3 c=raw(uv),l=raw(uv-vec2(s.x,0)),r=raw(uv+vec2(s.x,0));
            vec3 u=raw(uv-vec2(0,s.y)),d=raw(uv+vec2(0,s.y));
            vec3 ul=raw(uv-s),ur=raw(uv+vec2(s.x,-s.y));
            vec3 dl=raw(uv+vec2(-s.x,s.y)),dr=raw(uv+s),axial=(l+r+u+d)*.25;
            float dedither=Parameters.Restoration2.x;
            if(dedither>0.0)
            {
                int dm=0;if(restorationDistance(c,ul)<=.025)dm++;if(restorationDistance(c,ur)<=.025)dm++;if(restorationDistance(c,dl)<=.025)dm++;if(restorationDistance(c,dr)<=.025)dm++;
                int am=0;if(restorationDistance(axial,l)<=.025)am++;if(restorationDistance(axial,r)<=.025)am++;if(restorationDistance(axial,u)<=.025)am++;if(restorationDistance(axial,d)<=.025)am++;
                float pc=restorationDistance(c,axial);if(dm>=3&&am>=3&&pc>=.015&&pc<=.45)c=mix(c,(c+axial)*.5,dedither);
            }
            float denoise=Parameters.Restoration2.y;
            if(denoise>0.0)
            {
                float sigma=.02+.16*denoise,iv=1.0/(2.0*sigma*sigma);
                float wl=2.0*exp(-dot(c-l,c-l)*iv),wr=2.0*exp(-dot(c-r,c-r)*iv),wu=2.0*exp(-dot(c-u,c-u)*iv),wd=2.0*exp(-dot(c-d,c-d)*iv);
                float wul=exp(-dot(c-ul,c-ul)*iv),wur=exp(-dot(c-ur,c-ur)*iv),wdl=exp(-dot(c-dl,c-dl)*iv),wdr=exp(-dot(c-dr,c-dr)*iv);
                float weight=4.0+wl+wr+wu+wd+wul+wur+wdl+wdr;
                c=mix(c,(c*4.0+l*wl+r*wr+u*wu+d*wd+ul*wul+ur*wur+dl*wdl+dr*wdr)/weight,denoise);
            }
            float deband=Parameters.Restoration2.z;
            if(deband>0.0)
            {
                float threshold=.01+.05*deband,hs=max(restorationDistance(l,c),restorationDistance(r,c)),vs=max(restorationDistance(u,c),restorationDistance(d,c));
                bool hv=hs>.0005&&hs<=threshold&&(restorationLuminance(l)-restorationLuminance(c))*(restorationLuminance(r)-restorationLuminance(c))<=.000001;
                bool vv=vs>.0005&&vs<=threshold&&(restorationLuminance(u)-restorationLuminance(c))*(restorationLuminance(d)-restorationLuminance(c))<=.000001;
                if(hv||vv)c=mix(c,hv&&(!vv||hs<=vs)?(l+c+r)/3.0:(u+c+d)/3.0,deband);
            }
            float details=Parameters.Restoration.x;
            if(details>0.0)
            {
                vec3 average=(l+r+u+d+ul+ur+dl+dr)/8.0;
                float lc=max(max(max(restorationDistance(c,l),restorationDistance(c,r)),max(restorationDistance(c,u),restorationDistance(c,d))),max(max(restorationDistance(c,ul),restorationDistance(c,ur)),max(restorationDistance(c,dl),restorationDistance(c,dr))));
                float amount=details*clamp((.35-lc)/.30,0.0,1.0);vec3 minimum=min(c,min(min(l,r),min(u,d))),maximum=max(c,max(max(l,r),max(u,d))),extension=vec3(lc*.25*details);
                c=clamp(c+(c-average)*amount,minimum-extension,maximum+extension);
            }
            if(mode==3)c=mix(c,(u+d)*.5,.5);
            return videoSharpnessParameter(c,axial,Parameters.Processing.x);
        }
        """ + "\n" + """
        #if DISPLAY_TECHNOLOGY == 2
        """ + "\n" + FilterFixedPixelSubpixels.Shader + FilterFixedPixelGrid.Shader
        + FilterLcdDisplay.Shader + FilterLedBacklitLcdDisplay.Shader + FilterOledDisplay.Shader
        + FilterFixedPixelResponse.Shader + FilterFixedPixelPersistence.Shader + "\n" + """
        #endif
        #if DISPLAY_TECHNOLOGY == 3
        """ + "\n" + FilterPlasmaCellStructure.Shader + FilterPlasmaTemporalDithering.Shader
        + FilterPlasmaLightDiffusion.Shader + FilterPlasmaPersistence.Shader
        + FilterPlasmaBlackDepth.Shader + FilterPlasmaPhosphorIntensity.Shader
        + FilterPlasmaGammaResponse.Shader
        + FilterPlasmaAutomaticBrightnessLimiter.Shader + "\n" + """
        #endif
        #if DISPLAY_TECHNOLOGY == 4
        """ + "\n" + FilterVectorLineDetection.Shader + FilterVectorLineIntensity.Shader
        + FilterVectorBeamWidth.Shader + FilterVectorBeamFocus.Shader
        + FilterVectorHalo.Shader + FilterVectorHaloRadius.Shader
        + FilterVectorPhosphorColor.Shader + FilterVectorPersistence.Shader + "\n" + """
        #endif
        #if DISPLAY_TECHNOLOGY == 5
        """ + "\n" + FilterVfdEmissionThreshold.Shader + FilterVfdPhosphorIntensity.Shader
        + FilterVfdPhosphorColor.Shader + FilterVfdGlass.Shader
        + FilterVfdCellStructure.Shader + FilterVfdHaloRadius.Shader
        + FilterVfdHalo.Shader + FilterVfdPersistence.Shader + "\n" + """
        #endif
        #if DISPLAY_TECHNOLOGY == 6
        """ + "\n" + FilterLedMatrixCellStructure.Shader
        + FilterLedMatrixColor.Shader + FilterLedMatrixBrightness.Shader
        + FilterLedMatrixHalo.Shader + FilterLedMatrixBlackDepth.Shader + "\n" + """
        #endif
        #if DISPLAY_TECHNOLOGY == 7
        """ + "\n" + FilterDotMatrixCellSize.Shader + FilterDotMatrixCellGap.Shader
        + FilterDotMatrixShape.Shader + FilterDotMatrixDotSize.Shader + FilterDotMatrixContrast.Shader
        + FilterDotMatrixBrightness.Shader + FilterDotMatrixPalette.Shader
        + FilterDotMatrixHalo.Shader + FilterDotMatrixResponse.Shader
        + FilterDotMatrixPersistence.Shader + "\n" + """
        #endif
        #if DISPLAY_TECHNOLOGY == 3
        vec3 plasmaPixel(vec3 color,vec2 uv)
        {
            vec2 sourceSize=max(Parameters.Processing.zw,vec2(1.0));
            vec2 pixelScale=Parameters.Output.xy/sourceSize;
            color=filterPlasmaBlackDepth(color,Parameters.PlasmaDisplay.x);
            color=filterPlasmaGammaResponse(color,Parameters.PlasmaDisplay.z);
            color=filterPlasmaPhosphorIntensity(color,Parameters.PlasmaDisplay.y);
            color=filterPlasmaAutomaticBrightnessLimiter(color,Parameters.PlasmaDisplay.w,
                Parameters.PlasmaTemporal.w);
            color=filterPlasmaCellStructure(color,fract(uv*sourceSize),
                Parameters.PlasmaEffect.y,pixelScale);
            color=filterPlasmaTemporalDithering(color,floor(uv*Parameters.Output.xy),
                Parameters.PlasmaEffect.w,Parameters.PlasmaTemporal.z);
            if(Parameters.PlasmaEffect.z>0.0)
            {
                vec2 stepSize=1.0/sourceSize;
                vec3 nearLight=(raw(uv-vec2(stepSize.x,0.0))+raw(uv+vec2(stepSize.x,0.0))
                    +raw(uv-vec2(0.0,stepSize.y))+raw(uv+vec2(0.0,stepSize.y)))*.25;
                vec3 farLight=(raw(uv-vec2(stepSize.x*2.0,0.0))+raw(uv+vec2(stepSize.x*2.0,0.0))
                    +raw(uv-vec2(0.0,stepSize.y*2.0))+raw(uv+vec2(0.0,stepSize.y*2.0)))*.25;
                color=filterPlasmaLightDiffusion(color,nearLight,farLight,
                    Parameters.PlasmaEffect.z);
            }
            return color;
        }
        #endif
        #if DISPLAY_TECHNOLOGY == 4
        float vectorLuminance(vec2 uv)
        {
            return dot(adjust(raw(uv)),vec3(.2126,.7152,.0722));
        }
        float vectorEmission(vec2 uv)
        {
            vec2 s=1.0/max(Parameters.Processing.zw,vec2(1.0));
            float tl=vectorLuminance(uv+vec2(-s.x,-s.y));
            float t=vectorLuminance(uv+vec2(0.0,-s.y));
            float tr=vectorLuminance(uv+vec2(s.x,-s.y));
            float l=vectorLuminance(uv+vec2(-s.x,0.0));
            float r=vectorLuminance(uv+vec2(s.x,0.0));
            float bl=vectorLuminance(uv+vec2(-s.x,s.y));
            float b=vectorLuminance(uv+vec2(0.0,s.y));
            float br=vectorLuminance(uv+vec2(s.x,s.y));
            return filterVectorLineDetection(-tl-2.0*l-bl+tr+2.0*r+br,
                -tl-2.0*t-tr+bl+2.0*b+br,Parameters.VectorEffect.y);
        }
        vec3 vectorPixel(vec3 color,vec2 uv)
        {
            vec2 sourceStep=1.0/max(Parameters.Processing.zw,vec2(1.0));
            float center=vectorEmission(uv);
            float nearEmission=max(max(vectorEmission(uv+vec2(sourceStep.x,0.0)),
                vectorEmission(uv-vec2(sourceStep.x,0.0))),max(
                vectorEmission(uv+vec2(0.0,sourceStep.y)),
                vectorEmission(uv-vec2(0.0,sourceStep.y))));
            float farEmission=max(max(vectorEmission(uv+vec2(sourceStep.x*2.0,0.0)),
                vectorEmission(uv-vec2(sourceStep.x*2.0,0.0))),max(
                vectorEmission(uv+vec2(0.0,sourceStep.y*2.0)),
                vectorEmission(uv-vec2(0.0,sourceStep.y*2.0))));
            float emission=filterVectorBeamWidth(center,nearEmission,farEmission,
                Parameters.VectorDisplay.x);
            emission=filterVectorBeamFocus(emission,(center+nearEmission*4.0)/5.0,
                Parameters.VectorDisplay.y);
            color=filterVectorLineIntensity(color,emission,Parameters.VectorEffect.z);
            if(Parameters.VectorEffect.w>0.0&&Parameters.VectorEffect.z>0.0)
            {
                float radius=filterVectorHaloRadius(Parameters.VectorDisplay.w);
                vec2 s=sourceStep*radius;
                float average=0.0;
                for(int y=-1;y<=1;y++)for(int x=-1;x<=1;x++)
                    average+=vectorEmission(uv+vec2(float(x),float(y))*s);
                color=filterVectorHalo(color,average/9.0,Parameters.VectorEffect.z,
                    Parameters.VectorEffect.w);
            }
            color=filterVectorPhosphorColor(color,Parameters.VectorDisplay.z);
            return clamp(color,0.0,1.0);
        }
        #endif
        #if DISPLAY_TECHNOLOGY == 5
        float vfdEmission(vec2 uv)
        {
            vec3 sampleColor=adjust(sourceColor(clamp(uv,vec2(0.0),vec2(1.0))));
            return filterVfdEmissionThreshold(dot(sampleColor,vec3(.2126,.7152,.0722)),
                Parameters.VfdDisplay.z);
        }
        vec3 vfdPixel(vec3 source,vec2 uv)
        {
            float mask=filterVfdCellStructure(uv,Parameters.Processing.zw,
                Parameters.VfdStructure.x,Parameters.VfdStructure.y,
                Parameters.VfdStructure.z);
            float emission=filterVfdPhosphorIntensity(filterVfdEmissionThreshold(
                dot(source,vec3(.2126,.7152,.0722)),Parameters.VfdDisplay.z)*mask,
                Parameters.VfdDisplay.y);
            vec2 stepSize=1.0/max(Parameters.Processing.zw,vec2(1.0));
            float radius=filterVfdHaloRadius(Parameters.VfdOptical.x);
            float nearRadius=max(1.0,radius*.5);
            float nearEmission=(vfdEmission(uv+vec2(stepSize.x*nearRadius,0.0))
                +vfdEmission(uv-vec2(stepSize.x*nearRadius,0.0))
                +vfdEmission(uv+vec2(0.0,stepSize.y*nearRadius))
                +vfdEmission(uv-vec2(0.0,stepSize.y*nearRadius)))*.25;
            float farEmission=(vfdEmission(uv+vec2(stepSize.x*radius,stepSize.y*radius))
                +vfdEmission(uv+vec2(-stepSize.x*radius,stepSize.y*radius))
                +vfdEmission(uv+vec2(stepSize.x*radius,-stepSize.y*radius))
                +vfdEmission(uv-vec2(stepSize.x*radius,stepSize.y*radius)))*.25;
            float halo=filterVfdPhosphorIntensity(filterVfdHalo(nearEmission,farEmission,
                Parameters.VfdStructure.w),Parameters.VfdDisplay.y);
            return clamp(filterVfdGlass(source,Parameters.VfdDisplay.w)
                +filterVfdPhosphorColor(emission,Parameters.VfdDisplay.x)
                +filterVfdPhosphorColor(halo,Parameters.VfdDisplay.x),0.0,1.0);
        }
        #endif
        #if DISPLAY_TECHNOLOGY == 6
        vec3 ledMatrixPixel(vec2 uv)
        {
            vec2 sourceSize=max(Parameters.Processing.zw,vec2(1.0));
            float pitch=filterLedMatrixPitch(Parameters.LedMatrixStructure.x);
            vec2 sourcePosition=uv*sourceSize;
            vec2 cell=floor(sourcePosition/pitch);
            vec2 centerUv=(cell+.5)*pitch/sourceSize;
            vec2 sampleOffset=vec2(pitch*.28)/sourceSize;
            vec3 emission=(raw(centerUv)*4.0+raw(centerUv-vec2(sampleOffset.x,0.0))
                +raw(centerUv+vec2(sampleOffset.x,0.0))
                +raw(centerUv-vec2(0.0,sampleOffset.y))
                +raw(centerUv+vec2(0.0,sampleOffset.y)))/8.0;
            emission=filterLedMatrixColor(emission,Parameters.LedMatrixEmission.x);
            emission=filterLedMatrixBrightness(emission,Parameters.LedMatrixEmission.y);
            vec2 localPosition=fract(sourcePosition/pitch)-.5;
            float distance=filterLedMatrixDistance(localPosition,Parameters.LedMatrixEmission.w);
            float edgeWidth=max(sourceSize.x/max(Parameters.Output.x,1.0),
                sourceSize.y/max(Parameters.Output.y,1.0))/pitch;
            float core=filterLedMatrixCore(distance,Parameters.LedMatrixStructure.y,edgeWidth);
            float halo=filterLedMatrixHalo(distance,Parameters.LedMatrixStructure.y,
                Parameters.LedMatrixStructure.w,Parameters.LedMatrixStructure.z)*(1.0-core);
            return clamp(filterLedMatrixBlackDepth(Parameters.LedMatrixEmission.z)
                +emission*(core+halo),0.0,1.0);
        }
        #endif
        #if DISPLAY_TECHNOLOGY == 7
        vec3 dotMatrixSample(vec2 uv,bool history)
        {
            vec2 sourceSize=max(Parameters.Processing.zw,vec2(1.0));
            float pitch=filterDotMatrixPitch(Parameters.DotMatrixGeometry.z);
            vec2 cell=floor(uv*sourceSize/pitch);
            vec2 centerUv=(cell+.5)*pitch/sourceSize;
            vec2 offset=vec2(pitch*.28)/sourceSize;
            if(history)
                return (adjust(texture(sampler2D(History,LinearSampler),centerUv).rgb)*4.0
                    +adjust(texture(sampler2D(History,LinearSampler),centerUv-vec2(offset.x,0)).rgb)
                    +adjust(texture(sampler2D(History,LinearSampler),centerUv+vec2(offset.x,0)).rgb)
                    +adjust(texture(sampler2D(History,LinearSampler),centerUv-vec2(0,offset.y)).rgb)
                    +adjust(texture(sampler2D(History,LinearSampler),centerUv+vec2(0,offset.y)).rgb))/8.0;
            return (raw(centerUv)*4.0+raw(centerUv-vec2(offset.x,0))
                +raw(centerUv+vec2(offset.x,0))+raw(centerUv-vec2(0,offset.y))
                +raw(centerUv+vec2(0,offset.y)))/8.0;
        }
        vec3 dotMatrixPixel(vec2 uv,bool history)
        {
            vec2 sourceSize=max(Parameters.Processing.zw,vec2(1.0));
            float pitch=filterDotMatrixPitch(Parameters.DotMatrixGeometry.z);
            vec2 local=fract(uv*sourceSize/pitch)-.5;
            float distance=filterDotMatrixDistance(local,Parameters.DotMatrixGeometry.y);
            float radius=filterDotMatrixRadius(Parameters.DotMatrixGeometry.w,
                Parameters.DotMatrixEmission.x);
            float edge=max(sourceSize.x/max(Parameters.Output.x,1.0),
                sourceSize.y/max(Parameters.Output.y,1.0))/pitch;
            float core=smoothstep(radius+edge,radius-edge,distance);
            float halo=filterDotMatrixHalo(distance,radius,
                Parameters.DotMatrixEmission.w)*(1.0-core);
            vec3 source=dotMatrixSample(uv,history);
            float level=filterDotMatrixBrightness(filterDotMatrixContrast(
                dot(source,vec3(.2126,.7152,.0722)),Parameters.DotMatrixEmission.y),
                Parameters.DotMatrixEmission.z);
            return mix(filterDotMatrixBackground(Parameters.DotMatrixGeometry.x),
                filterDotMatrixForeground(source,Parameters.DotMatrixGeometry.x),
                clamp(level*(core+halo),0.0,1.0));
        }
        #endif
        vec3 displayEffect(vec3 c,vec2 uv){int t=int(Parameters.General.x+.5);vec2 p=floor(uv*Parameters.Output.xy),l=fract(uv*Parameters.Processing.zw);
        #if DISPLAY_TECHNOLOGY == 1
            if(t==1){vec2 q=uv*2.0-1.0;q.x*=1.0+Parameters.CrtGeometry.x*.28*q.y*q.y+Parameters.CrtGeometry.z*.22*q.y;q.y*=1.0+Parameters.CrtGeometry.y*.28*q.x*q.x;vec2 wuv=(q+1.0)*.5;if(any(lessThan(wuv,vec2(0)))||any(greaterThan(wuv,vec2(1))))return vec3(0);vec2 ss=1.0/max(Parameters.Processing.zw,vec2(1));vec3 src=raw(wuv),v=(raw(wuv-vec2(0,ss.y))+raw(wuv+vec2(0,ss.y)))*.5,n=(raw(wuv-ss)+raw(wuv+ss)+raw(wuv+vec2(ss.x,-ss.y))+raw(wuv+vec2(-ss.x,ss.y))+src*5.0)/9.0;c=mix(src,v,Parameters.CrtBeam.y*.45);c=mix(c,n,Parameters.CrtBeam.w*.72);c=clamp(c*(1.0+Parameters.CrtBeam.z*.5)+max(n-vec3(.35),vec3(0))*Parameters.CrtOptical.x*.85,0.0,1.0);if(Parameters.CrtDisplay.y>.5){float lum=dot(c,vec3(.2126,.7152,.0722));c=lum*vec3(Parameters.CrtDisplay.zw,Parameters.CrtBeam.x);}p=floor(uv*Parameters.Processing.zw);int mask=int(Parameters.CrtOptical.y+.5),subpixelLayout=int(Parameters.CrtOptical.z+.5);if(mask!=0&&Parameters.CrtOptical.w>0){int selected=subpixelLayout==0?-1:int(mod(p.x,3.0));if(subpixelLayout==2)selected=2-selected;if(mask==2)selected=int(mod(float(selected)+mod(p.y,2.0),3.0));bool gap=mask==3&&int(mod(p.y,4.0))==3;float strength=Parameters.CrtOptical.w*.88;for(int channel=0;channel<3;channel++){float attenuation=gap||(selected>=0&&channel!=selected)?strength:strength*.12;if(subpixelLayout==0)attenuation=int(mod(p.x+p.y,2.0))==0?strength*.1:strength;c[channel]*=1.0-attenuation;}}if(Parameters.CrtPatternIntensity.y>.5&&Parameters.CrtScanlines.x>0){float a=Parameters.CrtPatternIntensity.z<.5?uv.y*Parameters.Processing.w:uv.x*Parameters.Processing.z,gapStart=mix(.47,.18,Parameters.CrtScanlines.y),cycle=fract((a+Parameters.CrtScanlines.z*.25)*.5),distanceFromBeam=min(abs(cycle-.25),1.0-abs(cycle-.25)),gap=smoothstep(gapStart,min(.5,gapStart+.055),distanceFromBeam),coverage=1.0-gapStart*2.0,comp=1.0+Parameters.CrtScanlines.w*Parameters.CrtScanlines.x*coverage*.45;c*=(1.0-Parameters.CrtScanlines.x*gap*.94)*comp;}if(Parameters.CrtPattern.x>.5&&Parameters.CrtPatternIntensity.x>0){float a=Parameters.CrtPattern.y<.5?p.y:p.x,len=Parameters.CrtPattern.y<.5?Parameters.Processing.w:Parameters.Processing.z,cycles=1.0+Parameters.CrtPattern.z*31.0,w=.5+.5*cos(6.2831853*(a+.5)*cycles/len+Parameters.CrtPattern.w*6.2831853);c*=1.0-Parameters.CrtPatternIntensity.x*.85*w;}float radius=clamp(dot(uv*2.0-1.0,uv*2.0-1.0)*.5,0.0,1.0);c*=1.0-Parameters.CrtGeometry.w*.92*pow(radius,1.5);}
        #endif
        #if DISPLAY_TECHNOLOGY == 2
            if(t==2)
            {
                c=filterFixedPixelSubpixels(c,l,Parameters.FixedDisplay.z,Parameters.FixedSpatial.yzw,
                    Parameters.FixedDisplay.w,Parameters.Output.x/max(Parameters.Processing.z,1.0));
                c=filterFixedPixelGrid(c,l,Parameters.FixedDisplay.w,Parameters.FixedSpatial.x,
                    Parameters.Output.xy/max(Parameters.Processing.zw,vec2(1.0)));
                int technology=int(Parameters.FixedDisplay.y+.5);
                if(technology<2)
                {
                    vec2 ss=1.0/max(Parameters.Processing.zw,vec2(1));
                    vec3 n=(raw(uv-vec2(ss.x,0))+raw(uv+vec2(ss.x,0))
                        +raw(uv-vec2(0,ss.y))+raw(uv+vec2(0,ss.y)))*.25;
                    float light=max(n.r,max(n.g,n.b));
                    c=technology==0
                        ?filterLcdDisplay(c,Parameters.FixedTechnology.x,Parameters.FixedTechnology.y,Parameters.FixedTechnology.z,light)
                        :filterLedBacklitLcdDisplay(c,Parameters.FixedTechnology.x,Parameters.FixedTechnology.y,Parameters.FixedTechnology.z,light);
                }
                else c=filterOledDisplay(c,Parameters.FixedTechnology.y);
            }
        #endif
        #if DISPLAY_TECHNOLOGY == 3
            if(t==3)c=plasmaPixel(c,uv);
        #endif
        #if DISPLAY_TECHNOLOGY == 4
            if(t==4)c=vectorPixel(c,uv);
        #endif
        #if DISPLAY_TECHNOLOGY == 5
            if(t==5)c=vfdPixel(c,uv);
        #endif
        #if DISPLAY_TECHNOLOGY == 6
            if(t==6)c=ledMatrixPixel(uv);
        #endif
        #if DISPLAY_TECHNOLOGY == 7
            if(t==7)c=dotMatrixPixel(uv,false);
        #endif
        #if DISPLAY_TECHNOLOGY == 8
            if(t==8)c=mapSegments(c,uv);
        #endif
        #if DISPLAY_TECHNOLOGY == 9
            if(t==9){float y=dot(c,vec3(.2126,.7152,.0722)),n=int(Parameters.EPaper.x+.5)==0?1.0:15.0;y=floor(y*n+hash(p)*Parameters.EPaper.z)/max(n,1.0);c=int(Parameters.EPaper.x+.5)==2?mix(vec3(y),c,.45):vec3(y);c=mix(vec3(.92),c,.4+Parameters.EPaper.y*.6);}
        #endif
        #if DISPLAY_TECHNOLOGY == 10
            if(t==10){vec2 s=1.0/max(Parameters.Output.xy,vec2(1));c=mix(c,(raw(uv-s)+raw(uv+s))*.5,Parameters.Projection.x*.55+Parameters.Projection.y*.25);c*=1.0-(hash(p)-.5)*Parameters.Projection.z*.12;}
        #endif
            return clamp(c,0.0,1.0);}
        """ + SignalConnectionRgbScart.Shader + SignalConnectionComponent.Shader
        + SignalConnectionSVideo.Shader + SignalConnectionComposite.Shader
        + SignalConnectionRf.Shader + SignalStandardPal.Shader + SignalStandardNtsc.Shader
        + SignalStandardSecam.Shader + FilterGrain.Shader + FilterVhs.Shader
        + FilterChromaticAberration.Shader + FilterBloom.Shader + FilterSepia.Shader + """
        vec3 signalEffects(vec3 color,vec2 uv)
        {
            vec2 stepSize=1.0/max(Parameters.Processing.zw,vec2(1.0));
            vec3 left=raw(uv-vec2(stepSize.x,0.0)),right=raw(uv+vec2(stepSize.x,0.0));
            vec3 up=raw(uv-vec2(0.0,stepSize.y)),down=raw(uv+vec2(0.0,stepSize.y));
            int connection=int(Parameters.Signal.x+.5);float amount=Parameters.Signal.y;
            int standard=int(Parameters.Signal.z+.5);if(standard==0)standard=Parameters.General.w>18.2?1:2;
            float phase=mod(floor(uv.x*Parameters.Processing.z)+floor(uv.y*Parameters.Processing.w)+Parameters.General.z,2.0)*2.0-1.0;
            if(connection==1)color=signalConnectionRgbScart(color,left,amount);
            else if(connection==2)color=signalConnectionComponent(color,left,right,amount);
            else if(connection==3)color=signalConnectionSVideo(color,left,right,amount);
            else if(connection==4)color=signalConnectionComposite(color,left,right,amount,phase);
            else if(connection==5)color=signalConnectionRf(color,left,right,amount,hash(floor(uv*Parameters.Output.xy))-.5,float(standard),floor(uv.y*Parameters.Processing.w));
            if(standard==1)color=signalStandardPal(color,mod(floor(uv.y*Parameters.Processing.w),2.0)<.5?down:up,Parameters.Signal.w);
            else if(standard==2)color=signalStandardNtsc(color,left,Parameters.Signal.w);
            else if(standard==3)color=signalStandardSecam(color,up,Parameters.Signal.w,floor(uv.y*Parameters.Processing.w));
            return color;
        }
        vec3 postEffect(vec3 c,vec2 uv)
        {
            c=signalEffects(c,uv);vec2 s=1.0/max(Parameters.Output.xy,vec2(1));
            if(Parameters.Stylistic.y>0.0){float n=hash(floor(uv*Parameters.Output.xy)+Parameters.General.z)-.5;float w=(sin(uv.y*Parameters.Processing.w*.071+Parameters.General.z*.31)*4.0+n*4.0)*Parameters.Stylistic.y;vec2 q=uv+vec2(w*s.x,0);c=filterVhs(c,raw(q),raw(q-vec2(s.x*2.0,0)),raw(q+vec2(s.x*2.0,0)),Parameters.Stylistic.y,n,floor(uv.y*Parameters.Processing.w),uv.y);}
            if(Parameters.Stylistic.z>0.0){float o=Parameters.Stylistic.z*s.x*7.0;c=filterChromaticAberration(raw(uv+vec2(o,0)),c,raw(uv-vec2(o,0)));}
            if(Parameters.Stylistic.w>0.0)c=filterBloom(c,raw(uv+vec2(s.x*2.0,0)),raw(uv-vec2(s.x*2.0,0)),raw(uv+vec2(0,s.y*2.0)),raw(uv-vec2(0,s.y*2.0)),raw(uv+vec2(s.x*5.0,0)),raw(uv-vec2(s.x*5.0,0)),raw(uv+vec2(0,s.y*5.0)),raw(uv-vec2(0,s.y*5.0)),Parameters.Stylistic.w);
            c=filterSepia(c,Parameters.Stylistic2.x);
            c=filterGrain(c,Parameters.Stylistic.x,hash(floor(uv*Parameters.Output.xy)+Parameters.General.z)*2.0-1.0);
            return clamp(c,0.0,1.0);
        }
        """ + FilterGeneralPersistence.Shader + FilterMotionBlur.Shader
        + FilterFlicker.Shader + FilterInterlacing.Shader
        + FilterBlackFrameInsertion.Shader + """
        void main()
        {
            vec3 c=displayEffect(restored(fsin_TexCoord),fsin_TexCoord);
            c=clamp(postEffect(c,fsin_TexCoord),0.0,1.0);
            vec2 size=vec2(textureSize(sampler2D(History,LinearSampler),0));
            vec2 historyUv=clamp(fsin_TexCoord,.5/size,1.0-.5/size);
            vec3 previous=displayEffect(adjust(texture(sampler2D(History,LinearSampler),historyUv).rgb),fsin_TexCoord);
            previous=clamp(postEffect(previous,fsin_TexCoord),0.0,1.0);
        #if DISPLAY_TECHNOLOGY == 7
            if(Parameters.DotMatrixTemporal.z>.5)
            {
                previous=clamp(postEffect(dotMatrixPixel(fsin_TexCoord,true),fsin_TexCoord),0.0,1.0);
                c=filterDotMatrixResponse(previous,c,Parameters.DotMatrixTemporal.x);
                c=filterDotMatrixPersistence(c,previous,Parameters.DotMatrixTemporal.y,
                    Parameters.DotMatrixGeometry.x,
                    filterDotMatrixBackground(Parameters.DotMatrixGeometry.x));
            }
        #endif
        #if DISPLAY_TECHNOLOGY == 3
            if(Parameters.PlasmaTemporal.y>.5)
                c=filterPlasmaPersistence(c,previous,Parameters.PlasmaTemporal.x);
        #endif
        #if DISPLAY_TECHNOLOGY == 4
            if(Parameters.VectorTemporal.y>.5)
                c=filterVectorPersistence(c,previous,Parameters.VectorTemporal.x);
        #endif
        #if DISPLAY_TECHNOLOGY == 5
            if(Parameters.VfdOptical.w>.5)
                c=filterVfdPersistence(c,previous,Parameters.VfdOptical.y,
                    Parameters.VfdOptical.z);
        #endif
        #if DISPLAY_TECHNOLOGY == 2
            if(Parameters.FixedDisplay.x>.5&&Parameters.FixedTemporal.z>.5)
            {
                c=filterFixedPixelResponse(previous,c,Parameters.FixedTemporal.x,Parameters.FixedTemporal.w);
                c=filterFixedPixelPersistence(c,previous,Parameters.FixedTemporal.y);
            }
        #endif
            c=filterInterlacing(c,previous,fsin_TexCoord,Parameters.Processing.w,Parameters.General.z,Parameters.Temporal.w,Parameters.Signal2.z,Parameters.General.y);
            c=filterFlicker(c,Parameters.General.z,Parameters.Temporal.z);
            if(Parameters.General.y>.5)
            {
                previous=filterFlicker(previous,Parameters.General.z-1.0,Parameters.Temporal.z);
                c=filterMotionBlur(c,previous,Parameters.Temporal.y);
                c=filterGeneralPersistence(c,previous,Parameters.Temporal.x);
            }
            c=filterBlackFrameInsertion(c,Parameters.General.z,Parameters.Signal2.y);
            fsout_Color=vec4(linearToSrgb(c.r),linearToSrgb(c.g),linearToSrgb(c.b),1.0);
        }
        """;

    internal static string Fragment(GWGUI.Emulation.Enums.EmulationVideoSampling sampling,
        GWGUI.Emulation.Enums.EmulationVideoDisplayTechnology displayTechnology =
            GWGUI.Emulation.Enums.EmulationVideoDisplayTechnology.Normal)
    {
        var (dependencies, function) = sampling switch
        {
            GWGUI.Emulation.Enums.EmulationVideoSampling.Nearest =>
                (FilterNormal.VeldridShader, "nearestSample"),
            GWGUI.Emulation.Enums.EmulationVideoSampling.Bilinear =>
                (FilterBilinear.VeldridShader, "linearSampleCompact"),
            GWGUI.Emulation.Enums.EmulationVideoSampling.SharpBilinear =>
                (FilterBilinear.VeldridShader + FilterSharpBilinear.VeldridShader,
                    "sharpBilinearSampleCompact"),
            GWGUI.Emulation.Enums.EmulationVideoSampling.Bicubic =>
                (FilterNormal.VeldridShader + FilterBicubic.VeldridShader,
                    "bicubicSampleCompact"),
            GWGUI.Emulation.Enums.EmulationVideoSampling.Xbr =>
                (FilterNormal.VeldridShader + FilterXbr.VeldridShader, "xbrCompactSample"),
            GWGUI.Emulation.Enums.EmulationVideoSampling.Xbrz =>
                (FilterNormal.VeldridShader + FilterXbr.VeldridShader
                    + FilterXbrz.VeldridShader, "xbrzCompactSample"),
            GWGUI.Emulation.Enums.EmulationVideoSampling.Hqx =>
                (FilterNormal.VeldridShader + FilterBilinear.VeldridShader
                    + FilterXbr.VeldridShader + FilterHqx.VeldridShader, "hqxCompactSample"),
            GWGUI.Emulation.Enums.EmulationVideoSampling.Hq2x =>
                (FilterNormal.VeldridShader + FilterBilinear.VeldridShader
                    + FilterXbr.VeldridShader + FilterHqx.VeldridShader
                    + FilterHq2x.VeldridShader, "hq2xSampleCompact"),
            GWGUI.Emulation.Enums.EmulationVideoSampling.Hq3x =>
                (FilterNormal.VeldridShader + FilterBilinear.VeldridShader
                    + FilterXbr.VeldridShader + FilterHqx.VeldridShader
                    + FilterHq3x.VeldridShader, "hq3xSampleCompact"),
            GWGUI.Emulation.Enums.EmulationVideoSampling.Hq4x =>
                (FilterNormal.VeldridShader + FilterBilinear.VeldridShader
                    + FilterXbr.VeldridShader + FilterHqx.VeldridShader
                    + FilterHq4x.VeldridShader, "hq4xSampleCompact"),
            GWGUI.Emulation.Enums.EmulationVideoSampling.TwoXSai =>
                (FilterNormal.VeldridShader + FilterXbr.VeldridShader
                    + FilterTwoXSai.VeldridShader, "twoXSaiSampleCompact"),
            GWGUI.Emulation.Enums.EmulationVideoSampling.SuperTwoXSai =>
                (FilterNormal.VeldridShader + FilterBilinear.VeldridShader
                    + FilterXbr.VeldridShader + FilterTwoXSai.VeldridShader
                    + FilterSuperTwoXSai.VeldridShader, "superTwoXSaiSampleCompact"),
            GWGUI.Emulation.Enums.EmulationVideoSampling.SuperEagle =>
                (FilterNormal.VeldridShader + FilterBilinear.VeldridShader
                    + FilterXbr.VeldridShader + FilterTwoXSai.VeldridShader
                    + FilterSuperEagle.VeldridShader, "superEagleSampleCompact"),
            GWGUI.Emulation.Enums.EmulationVideoSampling.EpxScale2x =>
                (FilterNormal.VeldridShader + FilterXbr.VeldridShader
                    + FilterEpxScale2x.VeldridShader, "epxScale2xSampleCompact"),
            GWGUI.Emulation.Enums.EmulationVideoSampling.Jinc2 =>
                (FilterNormal.VeldridShader + FilterJinc2.VeldridShader,
                    "jinc2SampleCompact"),
            GWGUI.Emulation.Enums.EmulationVideoSampling.Lanczos =>
                (FilterNormal.VeldridShader + FilterLanczos.VeldridShader,
                    "lanczosSampleCompact"),
            GWGUI.Emulation.Enums.EmulationVideoSampling.ScaleFx =>
                (FilterNormal.VeldridShader + FilterXbr.VeldridShader
                    + FilterScaleFx.VeldridShader, "scaleFxCompactSample"),
            GWGUI.Emulation.Enums.EmulationVideoSampling.ScaleNx =>
                (FilterNormal.VeldridShader + FilterXbr.VeldridShader
                    + FilterScaleNx.VeldridShader, "scaleNxCompactSample"),
            GWGUI.Emulation.Enums.EmulationVideoSampling.Sabr =>
                (FilterNormal.VeldridShader + FilterBilinear.VeldridShader
                    + FilterXbr.VeldridShader + FilterSabr.VeldridShader, "sabrCompactSample"),
            _ => throw new ArgumentOutOfRangeException(nameof(sampling), sampling, null)
        };
        return FragmentHeader + $"#define DISPLAY_TECHNOLOGY {(int)displayTechnology}\n" + dependencies
            + $"vec3 sourceColor(vec2 uv){{return {function}(uv);}}\n"
            + VideoBrightnessParameterFunctions.Shader + VideoContrastParameterFunctions.Shader
            + VideoGammaParameterFunctions.Shader + VideoSaturationParameterFunctions.Shader
            + VideoSharpnessParameterFunctions.Shader + FragmentBody;
    }
}
