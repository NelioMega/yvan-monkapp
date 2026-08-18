using Microsoft.Win32;

namespace YvanMonkapp.Core;

/// <summary>
/// Démarrage avec Windows via HKCU\...\Run : pas d'élévation, pas de tâche planifiée,
/// et l'utilisateur peut le désactiver depuis le Gestionnaire des tâches.
/// </summary>
public static class StartupManager
{
    private const string RunKey = @"Software\Microsoft\Windows\CurrentVersion\Run";
    private const string ValueName = "Yvan Monk'app";

    /// <summary>Chemin de l'exe en cours, tel qu'on l'inscrit dans la base de registre.</summary>
    public static string ExecutablePath =>
        Environment.ProcessPath ?? Path.Combine(AppContext.BaseDirectory, "YvanMonkapp.exe");

    private static string Command => $"\"{ExecutablePath}\" --background";

    /// <summary>Vrai si l'entrée existe et pointe bien sur cet exe.</summary>
    public static bool IsEnabled()
    {
        try
        {
            using RegistryKey? key = Registry.CurrentUser.OpenSubKey(RunKey, writable: false);
            return key?.GetValue(ValueName) as string == Command;
        }
        catch (Exception ex)
        {
            Log.Write($"lecture du démarrage automatique impossible : {ex.Message}");
            return false;
        }
    }

    /// <summary>Vrai si une entrée existe, même si elle vise un ancien emplacement.</summary>
    public static bool HasEntry()
    {
        try
        {
            using RegistryKey? key = Registry.CurrentUser.OpenSubKey(RunKey, writable: false);
            return key?.GetValue(ValueName) is string;
        }
        catch
        {
            return false;
        }
    }

    public static bool Set(bool enabled)
    {
        try
        {
            using RegistryKey key = Registry.CurrentUser.CreateSubKey(RunKey, writable: true)
                                    ?? throw new InvalidOperationException("clé Run inaccessible");

            if (enabled) key.SetValue(ValueName, Command, RegistryValueKind.String);
            else key.DeleteValue(ValueName, throwOnMissingValue: false);

            Log.Write($"démarrage automatique {(enabled ? "activé" : "désactivé")}");
            return true;
        }
        catch (Exception ex)
        {
            Log.Write($"écriture du démarrage automatique impossible : {ex.Message}");
            return false;
        }
    }
}
