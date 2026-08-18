namespace YvanMonkapp.Core;

/// <summary>Une question posée par Yvan, avec sa réponse et de quoi la corriger.</summary>
public sealed record Question
{
    public required int Level { get; init; }

    /// <summary>Chapitre affiché sur le popup ("Fractions", "Dérivées"...).</summary>
    public required string Topic { get; init; }

    /// <summary>L'énoncé, tel qu'écrit au tableau.</summary>
    public required string Prompt { get; init; }

    /// <summary>La réponse sous sa forme lisible, montrée à la correction.</summary>
    public required string Expected { get; init; }

    /// <summary>Valeur numérique attendue, quand la réponse se compare en nombre.</summary>
    public double? Numeric { get; init; }

    /// <summary>Écritures alternatives acceptées (ex. "1/2" pour 0,5).</summary>
    public IReadOnlyList<string> Accepted { get; init; } = Array.Empty<string>();

    /// <summary>La correction d'Yvan, affichée après la réponse.</summary>
    public required string Explanation { get; init; }

    public required int Seconds { get; init; }

    public required int BasePoints { get; init; }

    /// <summary>Niveau le plus élevé que le générateur sait produire.</summary>
    public const int MaxLevel = 6;

    public static string LevelName(int level) => level switch
    {
        1 => "Échauffement",
        2 => "Collège",
        3 => "Brevet",
        4 => "Lycée",
        5 => "Terminale",
        6 => "Post-bac",
        _ => "Bonus"
    };

    public static int SecondsFor(int level) => level switch
    {
        1 => 20,
        2 => 25,
        3 => 35,
        4 => 45,
        5 => 60,
        _ => 75
    };
}
