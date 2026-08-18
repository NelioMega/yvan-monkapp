namespace YvanMonkapp.Core;

/// <summary>Une ligne du bulletin : un chapitre, sa note, et l'écart avec la semaine d'avant.</summary>
public sealed record BulletinLine(string Topic, int Asked, int Correct, double Note, double? Previous)
{
    public double Accuracy => Asked == 0 ? 0 : (double)Correct / Asked;

    /// <summary>« ↗ », « ↘ » ou « = » selon la semaine précédente, vide si elle est inconnue.</summary>
    public string Trend => Previous is not double before
        ? "—"
        : (Note - before) switch
        {
            > 1.5 => "▲",
            < -1.5 => "▼",
            _ => "="
        };
}

/// <summary>Le bulletin hebdomadaire, calculé à partir du score et de l'historique.</summary>
public sealed record Bulletin(
    DateTime From,
    DateTime To,
    IReadOnlyList<BulletinLine> Lines,
    int Asked,
    int Correct,
    int Delta,
    double Note,
    string Appreciation,
    int DailyStreak,
    int ReviewPending)
{
    public string Period => From.Month == To.Month
        ? $"Semaine du {From.Day} au {To:d MMMM yyyy}"
        : $"Semaine du {From:d MMMM} au {To:d MMMM yyyy}";

    /// <summary>Lundi de la semaine contenant la date donnée.</summary>
    public static DateTime StartOfWeek(DateTime day) =>
        day.Date.AddDays(-(((int)day.DayOfWeek + 6) % 7));

    public static Bulletin ForWeek(ScoreData score, DateTime anyDayOfWeek)
    {
        var from = StartOfWeek(anyDayOfWeek);
        var to = from.AddDays(6);

        // tout vient de l'historique : moyenne, lignes et points doivent se répondre.
        // S'appuyer sur les journées pour le total donnerait un bulletin qui se contredit
        // dès que l'historique a été tronqué.
        var week = score.History.Where(entry => entry.Date >= from && entry.Date < to.AddDays(1)).ToList();

        int asked = week.Count;
        int correct = week.Count(entry => entry.Outcome == AnswerOutcome.Correcte);
        int delta = week.Sum(entry => entry.Delta);

        var lines = Chapters(score, from, to.AddDays(1));
        var previous = Chapters(score, from.AddDays(-7), from)
            .ToDictionary(line => line.Topic, line => line.Note);

        lines = lines
            .Select(line => line with { Previous = previous.TryGetValue(line.Topic, out double before) ? before : null })
            .OrderByDescending(line => line.Asked)
            .ThenBy(line => line.Topic)
            .ToList();

        double note = asked == 0 ? 0 : Round(20.0 * correct / asked);

        return new Bulletin(from, to, lines, asked, correct, delta, note,
            Appreciate(note, asked), score.DailyStreak(), score.Review.Count);
    }

    private static List<BulletinLine> Chapters(ScoreData score, DateTime from, DateTime until)
    {
        return score.History
            .Where(entry => entry.Date >= from && entry.Date < until)
            .GroupBy(entry => string.IsNullOrEmpty(entry.Topic) ? "Divers" : entry.Topic)
            .Select(group =>
            {
                int asked = group.Count();
                int correct = group.Count(entry => entry.Outcome == AnswerOutcome.Correcte);
                return new BulletinLine(group.Key, asked, correct, Round(20.0 * correct / asked), null);
            })
            .ToList();
    }

    /// <summary>Notes au demi-point, comme un vrai bulletin.</summary>
    private static double Round(double note) => Math.Round(note * 2, MidpointRounding.AwayFromZero) / 2;

    private static string Appreciate(double note, int asked) => asked switch
    {
        0 => "Absent toute la semaine. Difficile de vous noter dans ces conditions.",
        < 5 => "Trop peu de travail pour se prononcer. On se revoit la semaine prochaine.",
        _ => note switch
        {
            < 5 => "Catastrophique. Il va falloir tout reprendre depuis le début.",
            < 8 => "Insuffisant. Le travail n'y est pas, et ça se voit.",
            < 10 => "Juste sous la moyenne. Ça se joue à peu de choses, accrochez-vous.",
            < 12 => "Passable. Des efforts, mais beaucoup trop d'inattention.",
            < 14 => "Assez bien. On tient quelque chose, ne lâchez rien.",
            < 16 => "Bien. Le travail régulier finit toujours par payer.",
            < 18 => "Très bien. Vous maîtrisez, continuez sur cette lancée.",
            _ => "Excellent. Je n'ai plus grand-chose à vous apprendre."
        }
    };
}
