using System.Globalization;

namespace YvanMonkapp.Core;

public enum AnswerOutcome
{
    Correcte,
    Fausse,
    TempsEcoule,
    Abandon
}

public sealed class HistoryEntry
{
    public DateTime Date { get; set; }
    public int Level { get; set; }
    public string Topic { get; set; } = "";
    public string Prompt { get; set; } = "";
    public string Expected { get; set; } = "";
    public string Given { get; set; } = "";
    public AnswerOutcome Outcome { get; set; }
    public int Delta { get; set; }
    public double Seconds { get; set; }

    /// <summary>Vrai si la question venait du carnet d'erreurs.</summary>
    public bool Review { get; set; }
}

public sealed class LevelStat
{
    public int Asked { get; set; }
    public int Correct { get; set; }

    public double Accuracy => Asked == 0 ? 0 : (double)Correct / Asked;
}

/// <summary>Activité d'une journée, gardée indéfiniment : c'est la matière du calendrier.</summary>
public sealed class DayStat
{
    public int Asked { get; set; }
    public int Correct { get; set; }
    public int Delta { get; set; }
}

/// <summary>
/// Une question ratée, remise en file. Elle revient à <see cref="DueAt"/>, et chaque succès
/// la repousse d'un palier ; trois succès d'affilée la sortent du carnet.
/// </summary>
public sealed class ReviewItem
{
    public int Level { get; set; }
    public string Topic { get; set; } = "";
    public string Prompt { get; set; } = "";
    public string Expected { get; set; } = "";
    public double? Numeric { get; set; }
    public List<string> Accepted { get; set; } = new();
    public string Explanation { get; set; } = "";
    public int Seconds { get; set; }
    public int BasePoints { get; set; }

    public DateTime DueAt { get; set; }

    /// <summary>0 = revient dans l'heure, 1 = demain, 2 = la semaine prochaine.</summary>
    public int Stage { get; set; }

    public int Misses { get; set; }

    public static ReviewItem From(Question question) => new()
    {
        Level = question.Level,
        Topic = question.Topic,
        Prompt = question.Prompt,
        Expected = question.Expected,
        Numeric = question.Numeric,
        Accepted = question.Accepted.ToList(),
        Explanation = question.Explanation,
        Seconds = question.Seconds,
        BasePoints = question.BasePoints
    };

    public Question ToQuestion() => new()
    {
        Level = Level,
        Topic = Topic,
        Prompt = Prompt,
        Expected = Expected,
        Numeric = Numeric,
        Accepted = Accepted,
        Explanation = Explanation,
        Seconds = Seconds,
        BasePoints = BasePoints
    };
}

/// <summary>Score, séries et historique. Sauvegardé après chaque question.</summary>
public sealed class ScoreData
{
    public int Points { get; set; }
    public int BestPoints { get; set; }
    public int Streak { get; set; }
    public int BestStreak { get; set; }

    public int Asked { get; set; }
    public int Correct { get; set; }
    public int Wrong { get; set; }
    public int Timeout { get; set; }
    public int Abandoned { get; set; }

    public double TotalAnswerSeconds { get; set; }
    public DateTime? LastQuestion { get; set; }

    /// <summary>Meilleure série de jours consécutifs avec au moins une bonne réponse.</summary>
    public int BestDailyStreak { get; set; }

    /// <summary>Dernière interro surprise, pour ne pas les enchaîner.</summary>
    public DateTime? LastExam { get; set; }

    /// <summary>Date du dernier bulletin affiché, pour ne pas le repasser en boucle.</summary>
    public DateTime? LastBulletin { get; set; }

    public Dictionary<int, LevelStat> ByLevel { get; set; } = new();

    /// <summary>Activité par jour, clé "aaaa-mm-jj".</summary>
    public Dictionary<string, DayStat> Days { get; set; } = new();

    public List<ReviewItem> Review { get; set; } = new();
    public List<HistoryEntry> History { get; set; } = new();

    public double Accuracy => Asked == 0 ? 0 : (double)Correct / Asked;

    public double AverageSeconds => Correct == 0 ? 0 : TotalAnswerSeconds / Correct;

    public static string Key(DateTime day) => day.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

    public LevelStat Level(int level)
    {
        if (!ByLevel.TryGetValue(level, out var stat))
        {
            stat = new LevelStat();
            ByLevel[level] = stat;
        }
        return stat;
    }

    public DayStat Day(DateTime day)
    {
        string key = Key(day);
        if (!Days.TryGetValue(key, out var stat))
        {
            stat = new DayStat();
            Days[key] = stat;
        }
        return stat;
    }

    public DayStat? DayOrNull(DateTime day) => Days.GetValueOrDefault(Key(day));

    /// <summary>
    /// Jours consécutifs avec au moins une bonne réponse. La journée en cours ne casse
    /// pas la série tant qu'elle n'est pas finie : on repart d'hier si aujourd'hui est vide.
    /// </summary>
    public int DailyStreak()
    {
        var day = DateTime.Today;
        if (DayOrNull(day)?.Correct is not > 0) day = day.AddDays(-1);

        int streak = 0;
        while (DayOrNull(day)?.Correct is > 0)
        {
            streak++;
            day = day.AddDays(-1);
        }

        return streak;
    }

    public void Push(HistoryEntry entry)
    {
        // large de quoi couvrir plusieurs semaines : le bulletin s'y appuie entierement
        const int keep = 400;

        History.Insert(0, entry);
        if (History.Count > keep) History.RemoveRange(keep, History.Count - keep);
    }
}
