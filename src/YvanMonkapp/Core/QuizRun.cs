namespace YvanMonkapp.Core;

/// <summary>
/// Ce qu'un popup a à poser : une question ordinaire, une question repêchée du carnet
/// d'erreurs, ou une interro surprise de plusieurs questions d'affilée.
/// </summary>
public sealed class QuizRun
{
    /// <summary>Nombre de questions d'une interro surprise.</summary>
    public const int ExamLength = 5;

    /// <summary>Une interro sans faute rapporte en plus cette part des points de base.</summary>
    private const double PerfectShare = 0.5;

    public required IReadOnlyList<Question> Questions { get; init; }

    /// <summary>Vrai si la question sort du carnet d'erreurs.</summary>
    public bool FromReview { get; init; }

    public bool IsExam => Questions.Count > 1;

    public Question First => Questions[0];

    /// <summary>Bonus accordé si l'interro est parcourue sans aucune faute.</summary>
    public int PerfectBonus => IsExam
        ? (int)Math.Round(Questions.Sum(q => q.BasePoints) * PerfectShare)
        : 0;

    public static QuizRun Single(Question question, bool fromReview = false) => new()
    {
        Questions = new[] { question },
        FromReview = fromReview
    };

    public static QuizRun Exam(IReadOnlyList<Question> questions) => new() { Questions = questions };
}
