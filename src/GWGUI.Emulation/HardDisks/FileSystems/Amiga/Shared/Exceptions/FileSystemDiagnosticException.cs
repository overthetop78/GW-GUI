using System.IO;
using Hst.Amiga.FileSystems;

namespace Hst.Amiga.FileSystems.Exceptions;

public sealed class FileSystemDiagnosticException : IOException
{
    public FileSystemDiagnosticException(FileSystemErrorCode errorCode, string message,
        params object[] arguments) : base(message)
    {
        ErrorCode = errorCode;
        Arguments = arguments;
    }

    public FileSystemErrorCode ErrorCode { get; }

    public IReadOnlyList<object> Arguments { get; }
}
