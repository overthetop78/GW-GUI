using GWGUI.App.Constants.Rendering.Emulation;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;

namespace GWGUI.App.Rendering.Emulation.Processing.OpenGl;

internal sealed class OpenGlNativeApi
{
    private readonly DeleteProgramDelegate _deleteProgram =
        Load<DeleteProgramDelegate>(OpenGlNativeFunctionNames.DeleteProgram);
    private readonly GetUniformLocationDelegate _getUniformLocation =
        Load<GetUniformLocationDelegate>(OpenGlNativeFunctionNames.GetUniformLocation);
    private readonly Uniform1iDelegate _uniform1i =
        Load<Uniform1iDelegate>(OpenGlNativeFunctionNames.Uniform1Integer);
    private readonly Uniform4fDelegate _uniform4f =
        Load<Uniform4fDelegate>(OpenGlNativeFunctionNames.Uniform4Float);
    private readonly UseProgramDelegate _useProgram =
        Load<UseProgramDelegate>(OpenGlNativeFunctionNames.UseProgram);

    internal uint CreateProgram(string vertexSource, string fragmentSource)
    {
        uint vertex = 0;
        uint fragment = 0;
        uint program = 0;
        try
        {
            vertex = Compile(OpenGlVideoConstants.VertexShader, vertexSource);
            fragment = Compile(OpenGlVideoConstants.FragmentShader, fragmentSource);
            var createProgram = Load<CreateProgramDelegate>(OpenGlNativeFunctionNames.CreateProgram);
            var attachShader = Load<AttachShaderDelegate>(OpenGlNativeFunctionNames.AttachShader);
            var linkProgram = Load<LinkProgramDelegate>(OpenGlNativeFunctionNames.LinkProgram);
            var getProgram = Load<GetProgramivDelegate>(OpenGlNativeFunctionNames.GetProgram);
            program = createProgram();
            attachShader(program, vertex);
            attachShader(program, fragment);
            linkProgram(program);
            getProgram(program, OpenGlVideoConstants.LinkStatus, out var linked);
            if (linked == 0)
                throw new InvalidOperationException(ProgramLog(program));
            var result = program;
            program = 0;
            return result;
        }
        finally
        {
            var deleteShader = Load<DeleteShaderDelegate>(OpenGlNativeFunctionNames.DeleteShader);
            if (vertex != 0)
                deleteShader(vertex);
            if (fragment != 0)
                deleteShader(fragment);
            if (program != 0)
                _deleteProgram(program);
        }
    }

    internal int GetUniformLocation(uint program, string name) =>
        _getUniformLocation(program, name);

    internal void SetUniform(int location, int value) => _uniform1i(location, value);

    internal void SetUniform(int location, Vector4 value) =>
        _uniform4f(location, value.X, value.Y, value.Z, value.W);

    internal void SetUniform(int location, float x, float y, float z, float w) =>
        _uniform4f(location, x, y, z, w);

    internal void UseProgram(uint program) => _useProgram(program);

    internal void DeleteProgram(uint program) => _deleteProgram(program);

    private static uint Compile(uint stage, string source)
    {
        var createShader = Load<CreateShaderDelegate>(OpenGlNativeFunctionNames.CreateShader);
        var shaderSource = Load<ShaderSourceDelegate>(OpenGlNativeFunctionNames.ShaderSource);
        var compileShader = Load<CompileShaderDelegate>(OpenGlNativeFunctionNames.CompileShader);
        var getShader = Load<GetShaderivDelegate>(OpenGlNativeFunctionNames.GetShader);
        var deleteShader = Load<DeleteShaderDelegate>(OpenGlNativeFunctionNames.DeleteShader);
        var shader = createShader(stage);
        var bytes = Encoding.UTF8.GetBytes(source);
        var sourcePointer = Marshal.AllocHGlobal(
            bytes.Length + OpenGlVideoConstants.ShaderSourceTerminatorByteCount);
        var pointers = Marshal.AllocHGlobal(IntPtr.Size);
        var lengths = Marshal.AllocHGlobal(sizeof(int));
        try
        {
            Marshal.Copy(bytes, 0, sourcePointer, bytes.Length);
            Marshal.WriteByte(sourcePointer, bytes.Length, 0);
            Marshal.WriteIntPtr(pointers, sourcePointer);
            Marshal.WriteInt32(lengths, bytes.Length);
            shaderSource(shader, OpenGlVideoConstants.ShaderSourceCount, pointers, lengths);
            compileShader(shader);
            getShader(shader, OpenGlVideoConstants.CompileStatus, out var compiled);
            if (compiled != 0)
                return shader;
            var error = ShaderLog(shader);
            deleteShader(shader);
            throw new InvalidOperationException(error);
        }
        finally
        {
            Marshal.FreeHGlobal(lengths);
            Marshal.FreeHGlobal(pointers);
            Marshal.FreeHGlobal(sourcePointer);
        }
    }

    private static string ShaderLog(uint shader)
    {
        var log = new StringBuilder(OpenGlVideoConstants.InfoLogCapacity);
        Load<GetShaderInfoLogDelegate>(OpenGlNativeFunctionNames.GetShaderInfoLog)(
            shader, log.Capacity, out _, log);
        return log.ToString();
    }

    private static string ProgramLog(uint program)
    {
        var log = new StringBuilder(OpenGlVideoConstants.InfoLogCapacity);
        Load<GetProgramInfoLogDelegate>(OpenGlNativeFunctionNames.GetProgramInfoLog)(
            program, log.Capacity, out _, log);
        return log.ToString();
    }

    private static T Load<T>(string name) where T : Delegate
    {
        var address = WglGetProcAddress(name);
        if (address == IntPtr.Zero
            || address == new IntPtr(OpenGlVideoConstants.InvalidAddressOne)
            || address == new IntPtr(OpenGlVideoConstants.InvalidAddressTwo)
            || address == new IntPtr(OpenGlVideoConstants.InvalidAddressThree)
            || address == new IntPtr(OpenGlVideoConstants.InvalidAddressMinusOne))
            throw new InvalidOperationException(name);
        return Marshal.GetDelegateForFunctionPointer<T>(address);
    }

    [DllImport(OpenGlNativeFunctionNames.Library,
        EntryPoint = OpenGlNativeFunctionNames.GetProcAddress,
        CharSet = CharSet.Ansi)]
    private static extern IntPtr WglGetProcAddress(string name);

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    private delegate uint CreateShaderDelegate(uint stage);
    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    private delegate void ShaderSourceDelegate(
        uint shader, int count, IntPtr strings, IntPtr lengths);
    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    private delegate void CompileShaderDelegate(uint shader);
    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    private delegate void GetShaderivDelegate(uint shader, uint property, out int value);
    [UnmanagedFunctionPointer(CallingConvention.Winapi, CharSet = CharSet.Ansi)]
    private delegate void GetShaderInfoLogDelegate(
        uint shader, int capacity, out int length, StringBuilder log);
    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    private delegate void DeleteShaderDelegate(uint shader);
    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    private delegate uint CreateProgramDelegate();
    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    private delegate void AttachShaderDelegate(uint program, uint shader);
    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    private delegate void LinkProgramDelegate(uint program);
    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    private delegate void GetProgramivDelegate(uint program, uint property, out int value);
    [UnmanagedFunctionPointer(CallingConvention.Winapi, CharSet = CharSet.Ansi)]
    private delegate void GetProgramInfoLogDelegate(
        uint program, int capacity, out int length, StringBuilder log);
    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    private delegate void DeleteProgramDelegate(uint program);
    [UnmanagedFunctionPointer(CallingConvention.Winapi, CharSet = CharSet.Ansi)]
    private delegate int GetUniformLocationDelegate(uint program, string name);
    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    private delegate void Uniform1iDelegate(int location, int value);
    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    private delegate void Uniform4fDelegate(int location, float x, float y, float z, float w);
    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    private delegate void UseProgramDelegate(uint program);
}
