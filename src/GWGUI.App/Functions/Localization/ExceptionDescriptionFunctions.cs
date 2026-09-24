using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Reflection;
using System.Text.Json;
using GWGUI.App.Constants.Localization;
using GWGUI.App.Localization.Extensions;
using AmigaDirectoryNotEmptyException = Hst.Amiga.FileSystems.Exceptions.DirectoryNotEmptyException;
using AmigaDiskFullException = Hst.Amiga.FileSystems.Exceptions.DiskFullException;
using AmigaFileSystemException = Hst.Amiga.FileSystems.Exceptions.FileSystemException;
using AmigaNotAFileException = Hst.Amiga.FileSystems.Exceptions.NotAFileException;
using AmigaPathAlreadyExistsException = Hst.Amiga.FileSystems.Exceptions.PathAlreadyExistsException;
using AmigaPathNotFoundException = Hst.Amiga.FileSystems.Exceptions.PathNotFoundException;
using AmigaFileSystemDiagnosticException = Hst.Amiga.FileSystems.Exceptions.FileSystemDiagnosticException;

namespace GWGUI.App.Functions.Localization;

internal static class ExceptionDescriptionFunctions
{
    internal static string Describe(Exception error)
    {
        var exceptions = ExceptionChain(error).ToArray();

        if (exceptions.OfType<HttpRequestException>().FirstOrDefault(candidate => candidate.StatusCode is not null)
            is { StatusCode: { } statusCode })
            return LocExtension.Get(ErrorDescriptionResourceKeys.HttpStatus,
                (int)statusCode);

        if (exceptions.OfType<SocketException>().FirstOrDefault() is { } socketError)
            return Describe(socketError.SocketErrorCode);

        if (exceptions.Any(candidate => candidate is HttpRequestException))
            return LocExtension.Get(ErrorDescriptionResourceKeys.NetworkUnavailable);
        if (exceptions.Any(candidate => candidate is TimeoutException or TaskCanceledException))
            return LocExtension.Get(ErrorDescriptionResourceKeys.OperationTimeout);
        if (exceptions.Any(candidate => candidate is OperationCanceledException))
            return LocExtension.Get(ErrorDescriptionResourceKeys.OperationCancelled);
        if (exceptions.Any(candidate => candidate is UnauthorizedAccessException))
            return LocExtension.Get(ErrorDescriptionResourceKeys.AccessDenied);
        if (exceptions.OfType<FileNotFoundException>().FirstOrDefault() is { } fileNotFound)
            return LocExtension.Get(ErrorDescriptionResourceKeys.FileNotFound,
                DisplayFileName(fileNotFound.FileName));
        if (exceptions.Any(candidate => candidate is DirectoryNotFoundException))
            return LocExtension.Get(ErrorDescriptionResourceKeys.DirectoryNotFound);
        if (exceptions.Any(candidate => candidate is DriveNotFoundException))
            return LocExtension.Get(ErrorDescriptionResourceKeys.DriveNotFound);
        if (exceptions.Any(candidate => candidate is PathTooLongException))
            return LocExtension.Get(ErrorDescriptionResourceKeys.PathTooLong);
        if (exceptions.Any(candidate => candidate is AmigaDiskFullException) || exceptions.Any(IsDiskFull))
            return LocExtension.Get(ErrorDescriptionResourceKeys.DiskFull);
        if (exceptions.OfType<AmigaFileSystemDiagnosticException>().FirstOrDefault() is { } diagnostic)
            return Describe(diagnostic);
        if (exceptions.Any(candidate => candidate is AmigaPathNotFoundException))
            return LocExtension.Get(ErrorDescriptionResourceKeys.FileNotFound,
                LocExtension.Get(CommonResourceKeys.Unknown));
        if (exceptions.Any(candidate => candidate is AmigaDirectoryNotEmptyException
                or AmigaPathAlreadyExistsException))
            return LocExtension.Get(ErrorDescriptionResourceKeys.InvalidOperation);
        if (exceptions.Any(candidate => candidate is AmigaNotAFileException
                or AmigaFileSystemException))
            return LocExtension.Get(ErrorDescriptionResourceKeys.InvalidData);
        if (exceptions.Any(candidate => candidate is InvalidDataException or JsonException or FormatException))
            return LocExtension.Get(ErrorDescriptionResourceKeys.InvalidData);
        if (exceptions.Any(candidate => candidate is ArgumentException))
            return LocExtension.Get(ErrorDescriptionResourceKeys.InvalidArgument);
        if (exceptions.Any(candidate => candidate is InvalidOperationException))
            return LocExtension.Get(ErrorDescriptionResourceKeys.InvalidOperation);
        if (exceptions.Any(candidate => candidate is NotSupportedException))
            return LocExtension.Get(ErrorDescriptionResourceKeys.NotSupported);
        if (exceptions.Any(candidate => candidate is OutOfMemoryException))
            return LocExtension.Get(ErrorDescriptionResourceKeys.OutOfMemory);

        return LocExtension.Get(ErrorDescriptionResourceKeys.Unexpected);
    }

    private static string Describe(SocketError error) => error switch
    {
        SocketError.TimedOut => LocExtension.Get(ErrorDescriptionResourceKeys.NetworkTimeout),
        SocketError.HostNotFound or SocketError.NoData or SocketError.TryAgain =>
            LocExtension.Get(ErrorDescriptionResourceKeys.NetworkDns),
        SocketError.ConnectionRefused => LocExtension.Get(ErrorDescriptionResourceKeys.NetworkRefused),
        SocketError.NetworkDown or SocketError.NetworkUnreachable or SocketError.HostDown
            or SocketError.HostUnreachable => LocExtension.Get(ErrorDescriptionResourceKeys.NetworkUnavailable),
        _ => LocExtension.Get(ErrorDescriptionResourceKeys.NetworkUnavailable)
    };

    private static string Describe(AmigaFileSystemDiagnosticException error) => error.ErrorCode switch
    {
        Hst.Amiga.FileSystems.FileSystemErrorCode.ReadFailed =>
            LocExtension.Get(ErrorDescriptionResourceKeys.FileSystemReadFailed, error.Arguments.ToArray()),
        Hst.Amiga.FileSystems.FileSystemErrorCode.CountExceedsBuffer =>
            LocExtension.Get(ErrorDescriptionResourceKeys.FileSystemCountExceedsBuffer, error.Arguments.ToArray()),
        Hst.Amiga.FileSystems.FileSystemErrorCode.IncorrectDataBlockSequence =>
            LocExtension.Get(ErrorDescriptionResourceKeys.FileSystemIncorrectDataBlockSequence),
        Hst.Amiga.FileSystems.FileSystemErrorCode.NoFreeSectorAvailable =>
            LocExtension.Get(ErrorDescriptionResourceKeys.FileSystemNoFreeSectorAvailable),
        Hst.Amiga.FileSystems.FileSystemErrorCode.OnlyOffsetZeroSupported =>
            LocExtension.Get(ErrorDescriptionResourceKeys.FileSystemOnlyOffsetZeroSupported),
        Hst.Amiga.FileSystems.FileSystemErrorCode.SetLengthUnsupported =>
            LocExtension.Get(ErrorDescriptionResourceKeys.FileSystemSetLengthUnsupported),
        Hst.Amiga.FileSystems.FileSystemErrorCode.CachedChainBitmapMissing =>
            LocExtension.Get(ErrorDescriptionResourceKeys.FileSystemCachedChainBitmapMissing,
                error.Arguments.ToArray()),
        Hst.Amiga.FileSystems.FileSystemErrorCode.CachedBitmapMissing =>
            LocExtension.Get(ErrorDescriptionResourceKeys.FileSystemCachedBitmapMissing,
                error.Arguments.ToArray()),
        Hst.Amiga.FileSystems.FileSystemErrorCode.BitmapBlockMissing =>
            LocExtension.Get(ErrorDescriptionResourceKeys.FileSystemBitmapBlockMissing),
        Hst.Amiga.FileSystems.FileSystemErrorCode.FreeBlockLoop =>
            LocExtension.Get(ErrorDescriptionResourceKeys.FileSystemFreeBlockLoop, error.Arguments.ToArray()),
        Hst.Amiga.FileSystems.FileSystemErrorCode.AnodeMissing =>
            LocExtension.Get(ErrorDescriptionResourceKeys.FileSystemAnodeMissing, error.Arguments.ToArray()),
        Hst.Amiga.FileSystems.FileSystemErrorCode.AnodeBlockAllocationFailed =>
            LocExtension.Get(ErrorDescriptionResourceKeys.FileSystemAnodeBlockAllocationFailed),
        Hst.Amiga.FileSystems.FileSystemErrorCode.SeekError =>
            LocExtension.Get(ErrorDescriptionResourceKeys.FileSystemSeekError),
        Hst.Amiga.FileSystems.FileSystemErrorCode.AnodeOperationFailed =>
            LocExtension.Get(ErrorDescriptionResourceKeys.FileSystemAnodeOperationFailed),
        Hst.Amiga.FileSystems.FileSystemErrorCode.AnodeBitmapIndexBlockMissing =>
            LocExtension.Get(ErrorDescriptionResourceKeys.FileSystemAnodeBitmapIndexBlockMissing,
                error.Arguments.ToArray()),
        _ => LocExtension.Get(ErrorDescriptionResourceKeys.Unexpected)
    };

    private static IEnumerable<Exception> ExceptionChain(Exception error)
    {
        for (Exception? current = error; current is not null; current = Next(current))
            yield return current;
    }

    private static Exception? Next(Exception error) => error switch
    {
        AggregateException { InnerExceptions.Count: 1 } aggregate => aggregate.InnerExceptions[0],
        TargetInvocationException { InnerException: { } inner } => inner,
        _ => error.InnerException
    };

    private static string DisplayFileName(string? path) => string.IsNullOrWhiteSpace(path)
        ? LocExtension.Get(CommonResourceKeys.Unknown)
        : Path.GetFileName(path);

    private static bool IsDiskFull(Exception error)
    {
        var code = error.HResult & 0xffff;
        return code is 0x27 or 0x70;
    }
}
