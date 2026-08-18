namespace YvanMonkapp.Core;

/// <summary>Journal minimal : l'app tourne sans fenêtre, il faut une trace en cas de pépin.</summary>
public static class Log
{
    private static readonly object Gate = new();

    public static void Write(string message)
    {
        try
        {
            lock (Gate)
            {
                Paths.EnsureRoot();
                var file = new FileInfo(Paths.Log);
                if (file.Exists && file.Length > 256 * 1024) file.Delete();
                File.AppendAllText(Paths.Log, $"{DateTime.Now:yyyy-MM-dd HH:mm:ss}  {message}{Environment.NewLine}");
            }
        }
        catch
        {
            // journaliser ne doit jamais faire tomber l'application
        }
    }
}
