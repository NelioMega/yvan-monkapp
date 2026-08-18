using System.Text.Json;

namespace YvanMonkapp.Core;

/// <summary>Lecture / écriture JSON des réglages et du score.</summary>
public static class Storage
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    public static AppSettings LoadSettings() => Load<AppSettings>(Paths.Settings) ?? new AppSettings();

    public static ScoreData LoadScore() => Load<ScoreData>(Paths.Score) ?? new ScoreData();

    public static void Save(AppSettings settings) => Write(Paths.Settings, settings);

    public static void Save(ScoreData score) => Write(Paths.Score, score);

    private static T? Load<T>(string path) where T : class
    {
        try
        {
            if (!File.Exists(path)) return null;

            string json = File.ReadAllText(path);
            return string.IsNullOrWhiteSpace(json) ? null : JsonSerializer.Deserialize<T>(json, Options);
        }
        catch (Exception ex)
        {
            Log.Write($"lecture de {Path.GetFileName(path)} impossible : {ex.Message}");
            return null;
        }
    }

    private static void Write<T>(string path, T value)
    {
        try
        {
            Paths.EnsureRoot();

            // écriture en deux temps : un plantage ne laisse jamais un fichier à moitié écrit
            string temp = path + ".tmp";
            File.WriteAllText(temp, JsonSerializer.Serialize(value, Options));
            File.Move(temp, path, overwrite: true);
        }
        catch (Exception ex)
        {
            Log.Write($"écriture de {Path.GetFileName(path)} impossible : {ex.Message}");
        }
    }
}
