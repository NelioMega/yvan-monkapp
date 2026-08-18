namespace YvanMonkapp.Core;

/// <summary>Les trois moments où Yvan peut donner de la voix.</summary>
public enum VoiceKind
{
    Greeting,
    Correct,
    Wrong
}

/// <summary>Emplacements sur disque. Tout vit dans %LOCALAPPDATA%\YvanMonkapp.</summary>
internal static class Paths
{
    public static string Root { get; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "YvanMonkapp");

    public static string Settings => Path.Combine(Root, "settings.json");
    public static string Score => Path.Combine(Root, "score.json");
    public static string Intro => Path.Combine(Root, "intro.mp3");
    public static string Greeting => Path.Combine(Root, "bonjour.mp3");
    public static string Log => Path.Combine(Root, "yvanmonkapp.log");

    /// <summary>Dossier où déposer ses propres extraits de voix.</summary>
    public static string Voices => Path.Combine(Root, "voix");

    public static string VoiceFolder(VoiceKind kind) => Path.Combine(Voices, kind switch
    {
        VoiceKind.Greeting => "bonjour",
        VoiceKind.Correct => "bonne",
        _ => "mauvaise"
    });

    public static void EnsureRoot() => Directory.CreateDirectory(Root);
}
