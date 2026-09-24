namespace GWGUI.App.Constants.Rendering.Emulation;

internal static class OpenGlNativeFunctionNames
{
    internal const string Library = "opengl32.dll";
    internal const string GetProcAddress = "wglGetProcAddress";
    internal const string CreateShader = "glCreateShader";
    internal const string ShaderSource = "glShaderSource";
    internal const string CompileShader = "glCompileShader";
    internal const string GetShader = "glGetShaderiv";
    internal const string GetShaderInfoLog = "glGetShaderInfoLog";
    internal const string DeleteShader = "glDeleteShader";
    internal const string CreateProgram = "glCreateProgram";
    internal const string AttachShader = "glAttachShader";
    internal const string LinkProgram = "glLinkProgram";
    internal const string GetProgram = "glGetProgramiv";
    internal const string GetProgramInfoLog = "glGetProgramInfoLog";
    internal const string DeleteProgram = "glDeleteProgram";
    internal const string GetUniformLocation = "glGetUniformLocation";
    internal const string Uniform1Integer = "glUniform1i";
    internal const string Uniform4Float = "glUniform4f";
    internal const string UseProgram = "glUseProgram";
}
