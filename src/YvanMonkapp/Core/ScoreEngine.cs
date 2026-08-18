namespace YvanMonkapp.Core;

public sealed record AnswerResult(
    AnswerOutcome Outcome,
    int Delta,
    int SpeedBonus,
    int StreakBonus,
    int Points,
    int Streak,
    Rank Rank,
    bool RankChanged,
    bool RankUp,
    bool LeftReview,
    int DailyStreak);

/// <summary>Barème : ce que rapporte une bonne réponse, ce que coûte le reste.</summary>
public static class ScoreEngine
{
    /// <summary>Délais du carnet d'erreurs : dans l'heure, puis demain, puis la semaine prochaine.</summary>
    private static readonly TimeSpan[] ReviewDelays =
    {
        TimeSpan.FromHours(1),
        TimeSpan.FromDays(1),
        TimeSpan.FromDays(7)
    };

    /// <summary>Au-delà, le carnet devient une punition : on oublie les plus anciennes fautes.</summary>
    private const int ReviewCapacity = 40;

    /// <summary>Niveau à poser, selon les réglages et le rang atteint.</summary>
    public static int LevelFor(AppSettings settings, ScoreData score) =>
        settings.Difficulty == DifficultyMode.Fixe
            ? Math.Clamp(settings.FixedLevel, 1, Question.MaxLevel)
            : Math.Clamp(Ranks.Of(score.Points).Level, 1, Question.MaxLevel);

    public static AnswerResult Apply(ScoreData score, Question question, AnswerOutcome outcome,
        string given, double seconds, bool fromReview = false)
    {
        var before = Ranks.Of(score.Points);
        bool good = outcome == AnswerOutcome.Correcte;

        int speedBonus = 0;
        int streakBonus = 0;
        int delta;

        if (good)
        {
            // répondre vite rapporte jusqu'à la moitié des points de base en plus
            double left = Math.Clamp(1 - seconds / Math.Max(1, question.Seconds), 0, 1);
            speedBonus = (int)Math.Round(question.BasePoints * 0.5 * left);
            streakBonus = Math.Min(score.Streak * 2, 20);
            delta = question.BasePoints + speedBonus + streakBonus;

            score.Streak++;
            score.Correct++;
            score.TotalAnswerSeconds += seconds;
            score.Level(question.Level).Correct++;
        }
        else
        {
            delta = -(question.Level * outcome switch
            {
                AnswerOutcome.TempsEcoule => 8,   // ignorer le popup coûte le plus cher
                AnswerOutcome.Fausse => 6,
                _ => 4                            // abandonner reste moins puni que le hasard
            });

            score.Streak = 0;
            switch (outcome)
            {
                case AnswerOutcome.TempsEcoule:
                    score.Timeout++;
                    break;
                case AnswerOutcome.Abandon:
                    score.Abandoned++;
                    break;
                default:
                    score.Wrong++;
                    break;
            }
        }

        bool leftReview = UpdateReview(score, question, good);

        score.Points += delta;
        score.Asked++;
        score.Level(question.Level).Asked++;
        score.LastQuestion = DateTime.Now;
        score.BestPoints = Math.Max(score.BestPoints, score.Points);
        score.BestStreak = Math.Max(score.BestStreak, score.Streak);

        var today = score.Day(DateTime.Today);
        today.Asked++;
        today.Delta += delta;
        if (good) today.Correct++;

        int dailyStreak = score.DailyStreak();
        score.BestDailyStreak = Math.Max(score.BestDailyStreak, dailyStreak);

        score.Push(new HistoryEntry
        {
            Date = DateTime.Now,
            Level = question.Level,
            Topic = question.Topic,
            Prompt = question.Prompt,
            Expected = question.Expected,
            Given = given,
            Outcome = outcome,
            Delta = delta,
            Seconds = Math.Round(seconds, 1),
            Review = fromReview
        });

        var after = Ranks.Of(score.Points);
        bool changed = after.Name != before.Name;

        return new AnswerResult(outcome, delta, speedBonus, streakBonus, score.Points, score.Streak,
            after, changed, changed && score.Points > before.From, leftReview, dailyStreak);
    }

    /// <summary>Points offerts hors question : l'interro sans faute, pour l'instant.</summary>
    public static void AwardBonus(ScoreData score, int points)
    {
        if (points == 0) return;

        score.Points += points;
        score.BestPoints = Math.Max(score.BestPoints, score.Points);
        score.Day(DateTime.Today).Delta += points;
    }

    /// <summary>Questions du carnet dont l'heure est venue, la plus ancienne d'abord.</summary>
    public static ReviewItem? NextDue(ScoreData score, DateTime now) =>
        score.Review.Where(item => item.DueAt <= now).MinBy(item => item.DueAt);

    public static int PendingReview(ScoreData score, DateTime now) =>
        score.Review.Count(item => item.DueAt <= now);

    /// <summary>
    /// Tient le carnet d'erreurs à jour. Renvoie vrai quand la question vient d'en sortir
    /// pour de bon, c'est-à-dire après trois succès d'affilée.
    /// </summary>
    private static bool UpdateReview(ScoreData score, Question question, bool good)
    {
        var existing = score.Review.FirstOrDefault(item => item.Prompt == question.Prompt);

        if (!good)
        {
            if (existing is null)
            {
                existing = ReviewItem.From(question);
                score.Review.Add(existing);
            }

            existing.Stage = 0;
            existing.Misses++;
            existing.DueAt = DateTime.Now + ReviewDelays[0];

            // le carnet ne doit pas enfler sans fin : les fautes les plus anciennes s'effacent
            while (score.Review.Count > ReviewCapacity)
            {
                var oldest = score.Review.MinBy(item => item.DueAt);
                if (oldest is null || ReferenceEquals(oldest, existing)) break;
                score.Review.Remove(oldest);
            }

            return false;
        }

        if (existing is null) return false;

        existing.Stage++;
        if (existing.Stage >= ReviewDelays.Length)
        {
            score.Review.Remove(existing);
            return true;
        }

        existing.DueAt = DateTime.Now + ReviewDelays[existing.Stage];
        return false;
    }
}
