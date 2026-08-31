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
        "On reprend là où on s'était arrêtés.",
        "Allez, une petite question pour se réveiller.",
        "Rien de méchant. Enfin, ça dépend de vous.",
        "Deux minutes de votre temps, pas plus.",
        "Je vous préviens : je regarde le chrono.",
        "On se met en condition. Respirez."
    };

    /// <summary>À partir du post-bac, Yvan prévient qu'on change de cour de récréation.</summary>
    private static readonly string[] IntroHard =
    {
        "Là, on passe aux choses sérieuses.",
        "Attention, celle-ci n'est plus du programme du lycée.",
        "Bon. Vous avez demandé du niveau, en voilà.",
        "Sortez le cours d'algèbre, on ne rigole plus.",
        "Celle-là, je ne la donne pas à tout le monde.",
        "Concentration maximale. C'est du lourd.",
        "Si vous séchez sur celle-ci, je ne vous en voudrai pas. Un peu, quand même."
    };

    private static readonly string[] Correct =
    {
        "Et voilà ! C'était pas si compliqué.",
        "Parfait. On continue comme ça.",
        "Très bien. Vous suivez, ça fait plaisir.",
        "Exact. Vous avez bien travaillé.",
        "C'est ça ! Bravo.",
        "Impeccable. Au suivant.",
        "Voilà, exactement. C'est la bonne méthode.",
        "Juste. Et proprement, en plus.",
        "Rien à redire. C'est bien."
    };

    private static readonly string[] Streak =
    {
        "Alors là, chapeau. Vous enchaînez.",
        "Vous êtes lancé, ne vous arrêtez pas !",
        "Franchement, du très bon travail.",
        "Je n'ai plus rien à vous apprendre. Ou presque.",
        "Vous êtes en forme aujourd'hui, ça se voit.",
        "À ce rythme, c'est vous qui allez faire le cours."
    };

    private static readonly string[] Wrong =
    {
        "NON ! Mais enfin, on vient de le voir !",
        "Alors là, non. Vraiment pas.",
        "Mais qu'est-ce que vous me racontez ?!",
        "Vous n'avez pas révisé, ça se voit.",
        "Non non non. On reprend depuis le début.",
        "Sérieusement ? Ça, c'est du niveau sixième.",
        "Vous avez répondu au hasard, avouez.",
        "Faux. Et je pense que vous le saviez.",
        "Relisez l'énoncé. Relisez-le vraiment.",
        "Ce n'est pas ça du tout. Pas du tout."
    };

    private static readonly string[] Timeout =
    {
        "Trop tard ! Le temps, ça se gère.",
        "Fini ! Vous rêvassiez, je le sais.",
        "Le chrono ne vous attend pas, lui.",
        "Zéro. Vous n'avez même pas répondu.",
        "Le temps est écoulé. Comme votre concentration.",
        "Une copie blanche, c'est toujours zéro."
    };

    private static readonly string[] GaveUp =
    {
        "Abandonner ? Devant moi ?",
        "On ne sèche pas, on cherche !",
        "Bon. Au moins vous êtes honnête.",
        "La prochaine fois, vous essayez. C'est tout ce que je demande.",
        "Un essai, même faux, vaut mieux que rien.",
        "D'accord. Mais on la reverra, celle-là."
    };

    private static readonly string[] Review =
    {
        "On reprend celle-là, vous l'aviez ratée.",
        "Revoyons ce qui n'était pas passé.",
        "Deuxième chance sur cet exercice.",
        "Celle-ci vous avait posé problème. On y retourne.",
        "Le carnet d'erreurs, ça sert à ça. On recommence.",
        "Je vous l'avais dit qu'elle reviendrait."
    };

    private static readonly string[] Exam =
    {
        "Interro surprise ! Cinq questions, on ne triche pas.",
        "Rangez tout, sortez une feuille : interro.",
        "Petit contrôle. Cinq questions à la suite.",
        "On va voir ce que vous avez retenu. Cinq questions.",
        "Contrôle surprise. Non, ce n'était pas annoncé.",
        "Cinq questions d'affilée. Sans faute, il y a une prime."
    };

    private static readonly string[] Training =
    {
        "Vous l'avez demandé : on va travailler.",
        "Bon. Puisque vous insistez, série complète.",
        "Voilà une bonne idée. On enchaîne dix questions.",
        "On révise sérieusement. Je note tout.",
        "Entraînement libre. Le chrono tourne quand même."
    };

    private static readonly string[] Next =
    {
        "On enchaîne.",
        "Question suivante.",
        "Et ça continue.",
        "Allez, la suivante.",
        "On ne s'arrête pas là.",
        "Deuxième service.",
        "On garde le rythme."
    };

    /// <summary>À partir de ce niveau, Yvan sort les répliques de fin de cursus.</summary>
    private const int HardLevel = 6;

    public static string ForIntro() => Pick(Intro);

    public static string ForIntro(int level) =>
        level >= HardLevel && Rng.Next(2) == 0 ? Pick(IntroHard) : Pick(Intro);

    public static string ForReview() => Pick(Review);

    public static string ForExam() => Pick(Exam);

    public static string ForTraining() => Pick(Training);

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
