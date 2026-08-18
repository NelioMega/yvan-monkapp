using System.Runtime.InteropServices;
using System.Text;

namespace YvanMonkapp.Core;

/// <summary>
/// Détecte si la fenêtre active occupe tout l'écran (jeu, film, présentation).
/// Dans ce cas on reporte la question au lieu de couper le joueur en pleine partie.
/// </summary>
public static class ForegroundWatch
{
    [StructLayout(LayoutKind.Sequential)]
    private struct Rect
    {
        public int Left, Top, Right, Bottom;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct MonitorInfo
    {
        public int Size;
        public Rect Monitor;
        public Rect Work;
        public uint Flags;
    }

    private const int MonitorDefaultToNearest = 2;

    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll")]
    private static extern bool GetWindowRect(IntPtr hWnd, out Rect rect);

    [DllImport("user32.dll")]
    private static extern IntPtr MonitorFromWindow(IntPtr hWnd, int flags);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern bool GetMonitorInfoW(IntPtr monitor, ref MonitorInfo info);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern int GetClassNameW(IntPtr hWnd, StringBuilder buffer, int max);

    /// <summary>Vrai si une application occupe l'écran entier au premier plan.</summary>
    public static bool IsFullscreenAppActive()
    {
        try
        {
            IntPtr window = GetForegroundWindow();
            if (window == IntPtr.Zero) return false;

            // le bureau et la barre des tâches couvrent l'écran sans être des applis
            var className = new StringBuilder(256);
            GetClassNameW(window, className, className.Capacity);
            string name = className.ToString();
            if (name is "Progman" or "WorkerW" or "Shell_TrayWnd" or "Windows.UI.Core.CoreWindow") return false;

            if (!GetWindowRect(window, out Rect bounds)) return false;

            IntPtr monitor = MonitorFromWindow(window, MonitorDefaultToNearest);
            var info = new MonitorInfo { Size = Marshal.SizeOf<MonitorInfo>() };
            if (!GetMonitorInfoW(monitor, ref info)) return false;

            // quelques pixels de marge : certains jeux débordent légèrement
            return bounds.Left <= info.Monitor.Left + 2
                   && bounds.Top <= info.Monitor.Top + 2
                   && bounds.Right >= info.Monitor.Right - 2
                   && bounds.Bottom >= info.Monitor.Bottom - 2;
        }
        catch (Exception ex)
        {
            Log.Write($"détection du plein écran impossible : {ex.Message}");
            return false;
        }
    }
}
