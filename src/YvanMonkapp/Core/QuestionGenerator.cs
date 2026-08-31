namespace YvanMonkapp.Core;

/// <summary>
/// Fabrique les énoncés, du calcul mental de 6e à la théorie des groupes.
/// Chaque niveau tire au sort parmi une famille de modèles paramétrés ; les modèles
/// eux-mêmes vivent dans les fichiers QuestionBank*.cs.
/// </summary>
public static partial class QuestionGenerator
{
    private static readonly Random Rng = new();

    /// <summary>Énoncés déjà tombés, pour ne pas reposer la même chose deux fois de suite.</summary>
    private static readonly Queue<string> Recent = new();

    /// <summary>
    /// Un seul énoncé mémorisé suffisait quand il y avait huit familles par niveau ;
    /// avec une vingtaine, se souvenir des seize derniers reste large sans bloquer le tirage.
    /// </summary>
    private const int RecentMemory = 16;

    private static readonly Func<Random, Question>[][] Levels =
    {
        Level1(), Geo(Level2(), GeoLevel2()), Geo(Level3(), GeoLevel3()), Geo(Level4(), GeoLevel4()),
        Geo(Level5(), GeoLevel5()), Geo(Level6(), GeoLevel6()), Geo(Level7(), GeoLevel7()), Level8()
    };

    /// <summary>
    /// Recolle les familles dessinées à celles du niveau. Elles vivent dans un fichier à
    /// part parce qu'une figure prend dix lignes là où un énoncé en prend trois.
    /// </summary>
    private static Func<Random, Question>[] Geo(Func<Random, Question>[] plain, Func<Random, Question>[] drawn) =>
        plain.Concat(drawn).ToArray();

    /// <summary>Nombre de familles d'énoncés, tous niveaux confondus.</summary>
    public static int FamilyCount => Levels.Sum(level => level.Length);

    public static int FamilyCountFor(int level) => Levels[Math.Clamp(level, 1, Levels.Length) - 1].Length;

    /// <summary>
    /// Tire une question du niveau demandé. <paramref name="focus"/> liste les chapitres
    /// que le joueur rate le plus : à énoncé neuf égal, ils passent devant.
    /// </summary>
    public static Question Next(int level, IReadOnlySet<string>? focus = null)
    {
        var family = Levels[Math.Clamp(level, 1, Levels.Length) - 1];

        Question best = family[Rng.Next(family.Length)](Rng);
        int bestRating = Rate(best, focus);

        for (int retry = 0; retry < 10 && bestRating < 2; retry++)
        {
            Question candidate = family[Rng.Next(family.Length)](Rng);
            int rating = Rate(candidate, focus);
            if (rating > bestRating) (best, bestRating) = (candidate, rating);
        }

        Remember(best.Prompt);
        return best;
    }

    /// <summary>
    /// Une question du chapitre demandé. On tire parmi les seules familles de ce chapitre
    /// plutôt que d'espérer tomber dessus : avec une trentaine de familles par niveau, tirer
    /// au hasard manquait un chapitre rare une fois sur huit.
    /// </summary>
    public static Question NextFrom(int level, string topic)
    {
        int index = Math.Clamp(level, 1, Levels.Length);
        var family = Levels[index - 1];

        if (!FamiliesByTopic(index).TryGetValue(topic, out int[]? choices)) return Next(index);

        Question chosen = family[choices[Rng.Next(choices.Length)]](Rng);
        for (int retry = 0; retry < 8 && Recent.Contains(chosen.Prompt); retry++)
        {
            chosen = family[choices[Rng.Next(choices.Length)]](Rng);
        }

        Remember(chosen.Prompt);
        return chosen;
    }

    /// <summary>Les chapitres d'un niveau, dans l'ordre alphabétique.</summary>
    public static IReadOnlyList<string> TopicsFor(int level)
    {
        int index = Math.Clamp(level, 1, Levels.Length);
        if (TopicNames.TryGetValue(index, out string[]? known)) return known;

        string[] names = FamiliesByTopic(index).Keys
            .OrderBy(name => name, StringComparer.CurrentCulture)
            .ToArray();

        TopicNames[index] = names;
        return names;
    }

    /// <summary>
    /// Quelles familles d'un niveau relèvent de quel chapitre. Chaque famille est tirée une
    /// fois pour lire son chapitre : il est fixé par le modèle, jamais par le hasard, donc
    /// un seul tirage suffit — et le Selftest vérifie que ça reste vrai.
    /// </summary>
    private static Dictionary<string, int[]> FamiliesByTopic(int level)
    {
        if (TopicFamilies.TryGetValue(level, out var known)) return known;

        var probe = new Random(level);
        var family = Levels[level - 1];
        var groups = new Dictionary<string, List<int>>();

        for (int i = 0; i < family.Length; i++)
        {
            string topic = family[i](probe).Topic;
            if (!groups.TryGetValue(topic, out var members)) groups[topic] = members = new List<int>();
            members.Add(i);
        }

        var built = groups.ToDictionary(pair => pair.Key, pair => pair.Value.ToArray());
        TopicFamilies[level] = built;
        return built;
    }

    /// <summary>
    /// Le niveau le plus proche du niveau courant où ce chapitre existe. « Volume » se pose
    /// du collège à la terminale : on s'entraîne à sa hauteur, pas à celle du dernier tirage.
    /// </summary>
    public static int LevelWith(string topic, int preferred)
    {
        preferred = Math.Clamp(preferred, 1, Levels.Length);
        if (TopicsFor(preferred).Contains(topic)) return preferred;

        for (int spread = 1; spread < Levels.Length; spread++)
        {
            if (preferred - spread >= 1 && TopicsFor(preferred - spread).Contains(topic)) return preferred - spread;
            if (preferred + spread <= Levels.Length && TopicsFor(preferred + spread).Contains(topic)) return preferred + spread;
        }

        return preferred;
    }

    private static readonly Dictionary<int, string[]> TopicNames = new();

    private static readonly Dictionary<int, Dictionary<string, int[]>> TopicFamilies = new();

    /// <summary>2 = énoncé neuf et chapitre visé, 1 = neuf seulement, 0 = déjà vu.</summary>
    private static int Rate(Question question, IReadOnlySet<string>? focus)
    {
        if (Recent.Contains(question.Prompt)) return 0;
        if (focus is null || focus.Count == 0) return 2;
        return focus.Contains(question.Topic) ? 2 : 1;
    }

    private static void Remember(string prompt)
    {
        Recent.Enqueue(prompt);
        while (Recent.Count > RecentMemory) Recent.Dequeue();
    }

    // --- Fabrication ---------------------------------------------------------------

    private static Question Num(int level, string topic, string prompt, double value, string explanation,
        string? hint = null, Figure? figure = null) => new()
    {
        Level = level,
        Topic = topic,
        Prompt = prompt,
        Expected = Answers.Format(value),
        Numeric = value,
        Explanation = explanation,
        Hint = hint ?? Hints.For(topic),
        Figure = figure,
        Seconds = Question.SecondsFor(level),
        BasePoints = level * 10
    };

    private static Question Text(int level, string topic, string prompt, string expected, double numeric,
        string explanation, string? hint = null, Figure? figure = null) => new()
    {
        Level = level,
        Topic = topic,
        Prompt = prompt,
        Expected = expected,
        Numeric = numeric,
        Accepted = new[] { expected, Answers.Format(numeric) },
        Explanation = explanation,
        Hint = hint ?? Hints.For(topic),
        Figure = figure,
        Seconds = Question.SecondsFor(level),
        BasePoints = level * 10
    };

    /// <summary>
    /// Une réponse fractionnaire, réduite et affichée « a/b ». Un dénominateur ramené à 1
    /// s'écrit en entier : personne ne tape « 3/1 ».
    /// </summary>
    private static Question Frac(int level, string topic, string prompt, int top, int bottom,
        string explanation, string? hint = null, Figure? figure = null)
    {
        if (bottom < 0) (top, bottom) = (-top, -bottom);
        int g = Gcd(top, bottom);
        int a = top / g, b = bottom / g;

        string written = b == 1 ? $"{a}" : $"{a}/{b}";
        return Text(level, topic, prompt, written, (double)a / b, explanation, hint, figure);
    }

    // --- Petits outils --------------------------------------------------------------

    private static int Gcd(int a, int b)
    {
        a = Math.Abs(a);
        b = Math.Abs(b);
        while (b != 0) (a, b) = (b, a % b);
        return a == 0 ? 1 : a;
    }

    private static long Lcm(int a, int b) => (long)Math.Abs(a) / Gcd(a, b) * Math.Abs(b);

    private static long Factorial(int n)
    {
        long v = 1;
        for (int i = 2; i <= n; i++) v *= i;
        return v;
    }

    private static long Binomial(int n, int k)
    {
        long v = 1;
        for (int i = 1; i <= k; i++) v = v * (n - k + i) / i;
        return v;
    }

    /// <summary>Puissance modulaire, pour les restes de très grands nombres.</summary>
    private static long PowMod(long a, long exponent, long modulus)
    {
        long result = 1;
        a %= modulus;
        while (exponent > 0)
        {
            if ((exponent & 1) == 1) result = result * a % modulus;
            a = a * a % modulus;
            exponent >>= 1;
        }
        return result;
    }

    /// <summary>Indicatrice d'Euler : combien d'entiers de 1 à n sont premiers avec n.</summary>
    private static int Totient(int n)
    {
        int result = n;
        for (int p = 2; (long)p * p <= n; p++)
        {
            if (n % p != 0) continue;
            while (n % p == 0) n /= p;
            result -= result / p;
        }
        if (n > 1) result -= result / n;
        return result;
    }

    /// <summary>Inverse de a modulo n, ou 0 quand a n'est pas inversible.</summary>
    private static int ModInverse(int a, int n)
    {
        a = ((a % n) + n) % n;
        for (int x = 1; x < n; x++)
        {
            if (a * x % n == 1) return x;
        }
        return 0;
    }

    /// <summary>Plus petit k &gt; 0 tel que a^k ≡ 1 [n], ou 0 s'il n'en existe pas.</summary>
    private static int MultiplicativeOrder(int a, int n)
    {
        if (Gcd(a, n) != 1) return 0;

        long value = a % n;
        for (int k = 1; k <= n; k++)
        {
            if (value == 1) return k;
            value = value * a % n;
        }
        return 0;
    }

    /// <summary>
    /// Plus petit entier naturel qui satisfait toutes les congruences, ou −1.
    /// La recherche est bornée par le produit des modules : un système sans solution
    /// (modules non premiers entre eux) doit rendre la main, pas boucler.
    /// </summary>
    private static int SolveCongruences(params (int Remainder, int Modulus)[] system)
    {
        long span = 1;
        foreach (var (_, modulus) in system) span *= modulus;

        for (int x = 0; x < span; x++)
        {
            bool all = true;
            foreach (var (remainder, modulus) in system)
            {
                if (x % modulus == remainder) continue;
                all = false;
                break;
            }
            if (all) return x;
        }

        return -1;
    }

    /// <summary>Les classes inversibles modulo n, celles qui sont premières avec lui.</summary>
    private static int[] Units(int n) => Enumerable.Range(2, Math.Max(1, n - 2)).Where(v => Gcd(v, n) == 1).ToArray();

    private static int DivisorCount(int n)
    {
        int count = 0;
        for (int d = 1; d <= n; d++)
        {
            if (n % d == 0) count++;
        }
        return count;
    }

    /// <summary>Un entier de −max à max, jamais nul : évite les "+ 0x" dans les énoncés.</summary>
    private static int NonZero(Random r, int max)
    {
        int v = r.Next(1, max + 1);
        return r.Next(2) == 0 ? v : -v;
    }

    private static T Pick<T>(Random r, params T[] values) => values[r.Next(values.Length)];

    /// <summary>"+ 3" ou "− 3", pour écrire un polynôme proprement.</summary>
    private static string Signed(int v) => v < 0 ? $"− {-v}" : $"+ {v}";

    /// <summary>Chiffres en indice, pour écrire u₁₂ sans passer par du XAML.</summary>
    private static string Sub(int n) => string.Concat(n.ToString().Select(c => "₀₁₂₃₄₅₆₇₈₉"[c - '0']));

    /// <summary>
    /// Un nombre tel qu'il s'écrit au tableau : le moins typographique, pas le trait d'union
    /// du clavier. Sans ça, un même énoncé mélange "− 11" et "-7".
    /// </summary>
    private static string Nb(int value) => value < 0 ? $"−{-value}" : value.ToString();

    /// <summary>Un coefficient à coller devant une inconnue : "1x" s'écrit "x".</summary>
    private static string Coef(int value) => value == 1 ? "" : value == -1 ? "−" : value.ToString();

    /// <summary>"+ 3x", "− x" : un terme signé, sans le 1 inutile collé à la variable.</summary>
    private static string Term(int coefficient, string variable) => Math.Abs(coefficient) == 1
        ? $"{(coefficient < 0 ? "−" : "+")} {variable}"
        : $"{Signed(coefficient)}{variable}";

    private static string Poly(int a, int b, int c)
    {
        string head = a == 1 ? "x²" : $"{a}x²";
        return $"{head} {Term(b, "x")} {Signed(c)}";
    }
}
