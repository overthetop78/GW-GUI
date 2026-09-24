namespace GWGUI.App.Rendering.Emulation.Processing;

internal static partial class VeldridVideoProcessingShaders
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

}
