using System.Text.Json.Serialization;

namespace YvanMonkapp.Core;

public enum DifficultyMode
{
    /// <summary>Le niveau suit le rang du joueur.</summary>
    Auto,
    Fixe
}

/// <summary>Réglages persistants, relus à chaque démarrage.</summary>
public sealed class AppSettings
{
    /// <summary>Intervalle minimum entre deux questions, en minutes.</summary>
    public int MinMinutes { get; set; } = 5;

    /// <summary>Intervalle maximum entre deux questions, en minutes.</summary>
    public int MaxMinutes { get; set; } = 20;

    /// <summary>Faux = plus aucune question tant que la pause dure.</summary>
    public bool Enabled { get; set; } = true;

    /// <summary>La voix d'Yvan : « bonjour » à l'ouverture du popup, intro aux passages de rang.</summary>
    public bool PlayVoice { get; set; } = true;

    /// <summary>Autorise les interros surprises de cinq questions.</summary>
    public bool Exams { get; set; } = true;

    /// <summary>Bips de bonne / mauvaise réponse.</summary>
    public bool PlaySfx { get; set; } = true;

    /// <summary>Volume général, de 0 à 1.</summary>
    public double Volume { get; set; } = 0.7;

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public DifficultyMode Difficulty { get; set; } = DifficultyMode.Auto;

    /// <summary>Niveau imposé quand <see cref="Difficulty"/> vaut Fixe.</summary>
    public int FixedLevel { get; set; } = 2;

    /// <summary>Reporte la question quand une appli tourne en plein écran (jeu, vidéo).</summary>
    public bool SkipWhenFullscreen { get; set; } = true;

    /// <summary>Plage horaire sans question.</summary>
    public bool QuietHours { get; set; } = true;

    public int QuietFromHour { get; set; } = 23;

    public int QuietToHour { get; set; } = 9;

    /// <summary>Vrai une fois que l'écran de bienvenue a été validé.</summary>
    public bool FirstRunDone { get; set; }

    public int ClampedMin => Math.Clamp(MinMinutes, 1, 24 * 60);

    public int ClampedMax => Math.Clamp(Math.Max(MaxMinutes, ClampedMin), 1, 24 * 60);

    /// <summary>Vrai si l'heure donnée tombe dans la plage calme (qui peut passer minuit).</summary>
    public bool IsQuiet(DateTime now)
    {
        if (!QuietHours) return false;

        int h = now.Hour;
        int from = Math.Clamp(QuietFromHour, 0, 23);
        int to = Math.Clamp(QuietToHour, 0, 23);
        if (from == to) return false;

        return from < to ? h >= from && h < to : h >= from || h < to;
    }
}
