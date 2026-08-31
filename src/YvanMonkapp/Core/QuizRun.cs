namespace YvanMonkapp.Core;

/// <summary>Ce qui a décidé d'ouvrir le popup.</summary>
public enum RunKind
{
    /// <summary>Une question, posée par le planificateur ou à la demande.</summary>
    Simple,

    /// <summary>L'interro surprise : cinq questions, et une prime au sans-faute.</summary>
    Interro,

    /// <summary>Une série demandée par le joueur depuis le tableau de bord.</summary>
    Entrainement
}

/// <summary>
/// Ce qu'un popup a à poser : une question ordinaire, une question repêchée du carnet
/// d'erreurs, une interro surprise, ou la série d'entraînement qu'on s'est infligée soi-même.
/// </summary>
public sealed class QuizRun
{
    /// <summary>Nombre de questions d'une interro surprise.</summary>
    public const int ExamLength = 5;

    /// <summary>Nombre de questions d'une série d'entraînement.</summary>
    public const int TrainingLength = 10;

    /// <summary>Une interro sans faute rapporte en plus cette part des points de base.</summary>
    private const double PerfectShare = 0.5;

    public required IReadOnlyList<Question> Questions { get; init; }

    /// <summary>Vrai si la question sort du carnet d'erreurs.</summary>
    public bool FromReview { get; init; }

    public RunKind Kind { get; init; } = RunKind.Simple;

    /// <summary>Ce que le badge du popup annonce, quand ce n'est ni le niveau ni une interro.</summary>
    public string Label { get; init; } = "";

    /// <summary>Vrai pour l'interro surprise, elle seule : c'est elle qui donne la prime.</summary>
    public bool IsExam => Kind == RunKind.Interro;

    /// <summary>Vrai dès que plusieurs questions s'enchaînent dans la même fenêtre.</summary>
    public bool IsSeries => Questions.Count > 1;

    public Question First => Questions[0];

    /// <summary>
    /// Bonus accordé si l'interro est parcourue sans aucune faute. L'entraînement n'y a pas
    /// droit : on choisit soi-même quand le déclencher, la surprise est ce qui se paie.
    /// </summary>
    public int PerfectBonus => IsExam
        ? (int)Math.Round(Questions.Sum(q => q.BasePoints) * PerfectShare)
        : 0;

    public static QuizRun Single(Question question, bool fromReview = false) => new()
    {
        Questions = new[] { question },
        FromReview = fromReview
    };

    public static QuizRun Exam(IReadOnlyList<Question> questions) => new()
    {
        Questions = questions,
        Kind = RunKind.Interro
    };

    public static QuizRun Training(IReadOnlyList<Question> questions, string label) => new()
    {
        Questions = questions,
        Kind = RunKind.Entrainement,
        Label = label
    };
}
