namespace YvanMonkapp.Core;

public readonly record struct Rank(string Name, int From, int To, int Level);

/// <summary>Échelle de rangs : sert de titre au joueur et de niveau en difficulté Auto.</summary>
public static class Ranks
{
    private static readonly Rank[] All =
    {
        new("Cancre du fond de la classe", int.MinValue, 0, 1),
        new("Élève en difficulté", 0, 60, 1),
        new("Élève appliqué", 60, 180, 2),
        new("Bon élève", 180, 400, 3),
        new("Tête de classe", 400, 800, 3),
        new("Délégué de maths", 800, 1500, 4),
        new("Major de promo", 1500, 2600, 4),
        new("Futur agrégé", 2600, 4200, 5),
        new("Agrégé de mathématiques", 4200, 6400, 6),
        new("Colleur de prépa", 6400, 9200, 6),
        new("Docteur en mathématiques", 9200, 13000, 7),
        new("Médaille Fields du quartier", 13000, 18000, 7),
        new("Yvan Monka lui-même", 18000, int.MaxValue, 8)
    };

    public static Rank Of(int points)
    {
        foreach (var rank in All)
        {
            if (points >= rank.From && points < rank.To) return rank;
        }
        return All[^1];
    }

    /// <summary>Le rang qui suit celui du score donné, ou null pour le dernier de la liste.</summary>
    public static Rank? After(int points)
    {
        var current = Of(points);
        for (int i = 0; i < All.Length - 1; i++)
        {
            if (All[i].Name == current.Name) return All[i + 1];
        }
        return null;
    }

    /// <summary>Largeur, en points, prêtée au rang des scores négatifs qui n'a pas de plancher.</summary>
    private const int CellarWidth = 60;

    /// <summary>Progression dans le rang courant, de 0 à 1 (1 sur le dernier rang).</summary>
    public static double Progress(int points)
    {
        var rank = Of(points);
        if (rank.To == int.MaxValue) return 1;

        // le rang du bas descend à l'infini : on lui invente un plancher pour la barre
        double from = rank.From == int.MinValue ? rank.To - CellarWidth : rank.From;
        double span = rank.To - from;

        return Math.Clamp((points - from) / span, 0, 1);
    }

    public static int NextThreshold(int points) => Of(points).To;
}
