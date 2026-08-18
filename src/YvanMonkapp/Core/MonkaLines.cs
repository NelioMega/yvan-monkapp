namespace YvanMonkapp.Core;

/// <summary>Les répliques du prof. Le vrai Yvan est calme : ici il a des nerfs, c'est le jeu.</summary>
public static class MonkaLines
{
    private static readonly Random Rng = new();

    private static readonly string[] Intro =
    {
        "Bonjour à tous ! Allez, c'est parti.",
        "On se retrouve pour un petit exercice.",
        "Aujourd'hui, on va faire simple. Enfin, j'espère.",
        "Prenez une feuille, un stylo, et on y va.",
        "Petit calcul de rien du tout. Vous allez voir.",
        "Vous avez cinq secondes pour vous concentrer. C'est parti.",
        "On reprend là où on s'était arrêtés."
    };

    private static readonly string[] Correct =
    {
        "Et voilà ! C'était pas si compliqué.",
        "Parfait. On continue comme ça.",
        "Très bien. Vous suivez, ça fait plaisir.",
        "Exact. Vous avez bien travaillé.",
        "C'est ça ! Bravo.",
        "Impeccable. Au suivant."
    };

    private static readonly string[] Streak =
    {
        "Alors là, chapeau. Vous enchaînez.",
        "Vous êtes lancé, ne vous arrêtez pas !",
        "Franchement, du très bon travail.",
        "Je n'ai plus rien à vous apprendre. Ou presque."
    };

    private static readonly string[] Wrong =
    {
        "NON ! Mais enfin, on vient de le voir !",
        "Alors là, non. Vraiment pas.",
        "Mais qu'est-ce que vous me racontez ?!",
        "Vous n'avez pas révisé, ça se voit.",
        "Non non non. On reprend depuis le début.",
        "Sérieusement ? Ça, c'est du niveau sixième.",
        "Vous avez répondu au hasard, avouez."
    };

    private static readonly string[] Timeout =
    {
        "Trop tard ! Le temps, ça se gère.",
        "Fini ! Vous rêvassiez, je le sais.",
        "Le chrono ne vous attend pas, lui.",
        "Zéro. Vous n'avez même pas répondu."
    };

    private static readonly string[] GaveUp =
    {
        "Abandonner ? Devant moi ?",
        "On ne sèche pas, on cherche !",
        "Bon. Au moins vous êtes honnête.",
        "La prochaine fois, vous essayez. C'est tout ce que je demande."
    };

    private static readonly string[] Review =
    {
        "On reprend celle-là, vous l'aviez ratée.",
        "Revoyons ce qui n'était pas passé.",
        "Deuxième chance sur cet exercice.",
        "Celle-ci vous avait posé problème. On y retourne."
    };

    private static readonly string[] Exam =
    {
        "Interro surprise ! Cinq questions, on ne triche pas.",
        "Rangez tout, sortez une feuille : interro.",
        "Petit contrôle. Cinq questions à la suite.",
        "On va voir ce que vous avez retenu. Cinq questions."
    };

    private static readonly string[] Next =
    {
        "On enchaîne.",
        "Question suivante.",
        "Et ça continue.",
        "Allez, la suivante.",
        "On ne s'arrête pas là."
    };

    public static string ForIntro() => Pick(Intro);

    public static string ForReview() => Pick(Review);

    public static string ForExam() => Pick(Exam);

    public static string ForNext() => Pick(Next);

    public static string ForCorrect(int streak) => streak >= 4 && Rng.Next(2) == 0 ? Pick(Streak) : Pick(Correct);

    public static string ForOutcome(AnswerOutcome outcome) => outcome switch
    {
        AnswerOutcome.Fausse => Pick(Wrong),
        AnswerOutcome.TempsEcoule => Pick(Timeout),
        AnswerOutcome.Abandon => Pick(GaveUp),
        _ => Pick(Correct)
    };

    private static string Pick(string[] lines) => lines[Rng.Next(lines.Length)];
}
