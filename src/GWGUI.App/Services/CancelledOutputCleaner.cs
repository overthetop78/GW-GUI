using System.IO;

namespace GWGUI.App.Services;

public static class CancelledOutputCleaner
{
    public static Exception? TryDelete(string path)
    {
        try
        {
            if (File.Exists(path)) File.Delete(path);
            return null;
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            return exception;
        }
    }
}
