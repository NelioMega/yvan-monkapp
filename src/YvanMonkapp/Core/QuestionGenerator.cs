namespace YvanMonkapp.Core;

/// <summary>
/// Fabrique les énoncés, du calcul mental de 6e aux matrices et aux complexes.
/// Chaque niveau tire au sort parmi une famille de modèles paramétrés.
/// </summary>
public static class QuestionGenerator
{
    private static readonly Random Rng = new();
    private static string _lastPrompt = "";

    private static readonly Func<Random, Question>[][] Levels =
    {
        Level1(), Level2(), Level3(), Level4(), Level5(), Level6()
    };

    /// <summary>Tire une question du niveau demandé, en évitant de répéter la précédente.</summary>
    public static Question Next(int level)
    {
        int index = Math.Clamp(level, 1, Levels.Length) - 1;
        var family = Levels[index];

        Question question = family[Rng.Next(family.Length)](Rng);
        for (int retry = 0; retry < 6 && question.Prompt == _lastPrompt; retry++)
        {
            question = family[Rng.Next(family.Length)](Rng);
        }

        _lastPrompt = question.Prompt;
        return question;
    }

    // --- Niveau 1 : calcul mental -------------------------------------------------

    private static Func<Random, Question>[] Level1() => new Func<Random, Question>[]
    {
        r =>
        {
            int a = r.Next(12, 90), b = r.Next(12, 90);
            return Num(1, "Addition", $"{a} + {b}", a + b, $"{a} + {b} = {a + b}. On pose les dizaines, puis les unités.");
        },
        r =>
        {
            int a = r.Next(40, 130), b = r.Next(11, 39);
            return Num(1, "Soustraction", $"{a} − {b}", a - b, $"{a} − {b} = {a - b}.");
        },
        r =>
        {
            int a = r.Next(3, 13), b = r.Next(3, 13);
            return Num(1, "Tables", $"{a} × {b}", a * b, $"La table de {a} : {a} × {b} = {a * b}.");
        },
        r =>
        {
            int n = r.Next(23, 250);
            return Num(1, "Double", $"Le double de {n}", n * 2, $"Doubler, c'est multiplier par 2 : {n} × 2 = {n * 2}.");
        },
        r =>
        {
            int half = r.Next(17, 140);
            return Num(1, "Moitié", $"La moitié de {half * 2}", half, $"{half * 2} ÷ 2 = {half}.");
        },
        r =>
        {
            int n = r.Next(11, 96);
            return Num(1, "Complément", $"Combien manque-t-il à {n} pour aller jusqu'à 100 ?", 100 - n,
                $"100 − {n} = {100 - n}. Le complément à 100, c'est un réflexe.");
        },
        r =>
        {
            int a = r.Next(5, 40), b = r.Next(5, 40), c = r.Next(5, 40);
            return Num(1, "Somme", $"{a} + {b} + {c}", a + b + c, $"On regroupe : {a} + {b} = {a + b}, puis + {c} = {a + b + c}.");
        },
        r =>
        {
            int start = r.Next(2, 12), step = r.Next(2, 9);
            int fourth = start + 3 * step;
            return Num(1, "Suite",
                $"Quel est le terme suivant ? {start} ; {start + step} ; {start + 2 * step} ; {fourth} ; ...",
                fourth + step, $"On ajoute {step} à chaque fois : {fourth} + {step} = {fourth + step}.");
        }
    };

    // --- Niveau 2 : collège -------------------------------------------------------

    private static Func<Random, Question>[] Level2() => new Func<Random, Question>[]
    {
        r =>
        {
            int a = r.Next(12, 99), b = r.Next(3, 10);
            return Num(2, "Multiplication", $"{a} × {b}", a * b,
                $"{a} × {b} = {a / 10 * 10} × {b} + {a % 10} × {b} = {a / 10 * 10 * b} + {a % 10 * b} = {a * b}.");
        },
        r =>
        {
            int b = r.Next(3, 13), q = r.Next(4, 20);
            return Num(2, "Division", $"{b * q} ÷ {b}", q, $"{b} × {q} = {b * q}, donc le quotient vaut {q}.");
        },
        r =>
        {
            int a = r.Next(5, 40), b = r.Next(3, 12), c = r.Next(3, 12);
            return Num(2, "Priorités", $"{a} + {b} × {c}", a + b * c,
                $"La multiplication passe avant : {b} × {c} = {b * c}, puis {a} + {b * c} = {a + b * c}.");
        },
        r =>
        {
            int n = r.Next(11, 26);
            return Num(2, "Carré", $"{n}²", n * n, $"{n}² = {n} × {n} = {n * n}.");
        },
        r =>
        {
            int n = r.Next(4, 11);
            return Num(2, "Puissance", $"2^{n}", Math.Pow(2, n), $"2^{n} = {(int)Math.Pow(2, n)}. On double {n} fois.");
        },
        r =>
        {
            int[] taux = { 10, 20, 25, 50, 75 };
            int t = taux[r.Next(taux.Length)];
            int baseValue = r.Next(2, 26) * 20;
            double result = baseValue * t / 100.0;
            return Num(2, "Pourcentage", $"{t} % de {baseValue}", result,
                $"{t} % c'est {t}/100 : {baseValue} × {t} ÷ 100 = {Answers.Format(result)}.");
        },
        r =>
        {
            int L = r.Next(4, 20), l = r.Next(3, 15);
            return Num(2, "Géométrie", $"Aire d'un rectangle de {L} cm sur {l} cm (en cm²)", L * l,
                $"Aire = longueur × largeur = {L} × {l} = {L * l} cm².");
        },
        r =>
        {
            int L = r.Next(4, 20), l = r.Next(3, 15);
            return Num(2, "Géométrie", $"Périmètre d'un rectangle de {L} cm sur {l} cm (en cm)", 2 * (L + l),
                $"Périmètre = 2 × (L + l) = 2 × ({L} + {l}) = {2 * (L + l)} cm.");
        },
        r =>
        {
            int m = r.Next(6, 20);
            int a = m + r.Next(-5, 6), b = m + r.Next(-4, 5);
            int c = 3 * m - a - b;
            return Num(2, "Moyenne", $"Moyenne de {a} ; {b} ; {c}", m,
                $"({a} + {b} + {c}) ÷ 3 = {a + b + c} ÷ 3 = {m}.");
        },
        r =>
        {
            double km = r.Next(11, 99) / 10.0;
            return Num(2, "Conversion", $"{Answers.Format(km)} km en mètres", km * 1000,
                $"1 km = 1 000 m, donc {Answers.Format(km)} × 1 000 = {Answers.Format(km * 1000)} m.");
        },
        r =>
        {
            int n = r.Next(3, 10);
            return Num(2, "Puissance", $"{n}³", n * n * n, $"{n}³ = {n} × {n} × {n} = {n * n * n}.");
        }
    };

    // --- Niveau 3 : brevet --------------------------------------------------------

    private static Func<Random, Question>[] Level3() => new Func<Random, Question>[]
    {
        r =>
        {
            int g = r.Next(3, 13), a = g * r.Next(2, 9), b = g * r.Next(2, 9);
            int pgcd = Gcd(a, b);
            return Num(3, "PGCD", $"PGCD({a} ; {b})", pgcd,
                $"Par l'algorithme d'Euclide, PGCD({a} ; {b}) = {pgcd}.");
        },
        r =>
        {
            int k = r.Next(2, 10), p = r.Next(2, 9), q = r.Next(2, 9);
            while (Gcd(p, q) != 1 || p == q) { p = r.Next(2, 9); q = r.Next(3, 11); }
            string expected = $"{p}/{q}";
            return Text(3, "Fractions", $"Simplifie la fraction {p * k}/{q * k}", expected, (double)p / q,
                $"On divise en haut et en bas par {k} : {p * k}/{q * k} = {expected}.");
        },
        r =>
        {
            int b = r.Next(2, 7), d = r.Next(2, 7), a = r.Next(1, 6), c = r.Next(1, 6);
            int top = a * d + c * b, bottom = b * d;
            int g = Gcd(top, bottom);
            // un dénominateur ramené à 1 s'écrit en entier, pas "2/1"
            string expected = bottom / g == 1 ? $"{top / g}" : $"{top / g}/{bottom / g}";
            return Text(3, "Fractions", $"{a}/{b} + {c}/{d} (fraction irréductible)", expected, (double)top / bottom,
                $"Même dénominateur {bottom} : {a * d}/{bottom} + {c * b}/{bottom} = {top}/{bottom} = {expected}.");
        },
        r =>
        {
            int a = r.Next(2, 10), x = r.Next(-9, 12), b = NonZero(r, 15);
            int c = a * x + b;
            return Num(3, "Équation", $"Résous : {a}x {Signed(b)} = {c}", x,
                $"{a}x = {c} {Signed(-b)} = {c - b}, donc x = {c - b} ÷ {a} = {x}.");
        },
        r =>
        {
            (int, int, int)[] triples = { (3, 4, 5), (6, 8, 10), (5, 12, 13), (9, 12, 15), (8, 15, 17), (7, 24, 25), (20, 21, 29) };
            var (p, q, h) = triples[r.Next(triples.Length)];
            return Num(3, "Pythagore", $"Triangle rectangle de côtés {p} et {q} : quelle est l'hypoténuse ?", h,
                $"{p}² + {q}² = {p * p} + {q * q} = {h * h}, et √{h * h} = {h}.");
        },
        r =>
        {
            int n = r.Next(1, 5);
            double v = Math.Pow(10, -n);
            return Num(3, "Puissances", $"10^(−{n})", v,
                $"10^(−{n}) = 1 ÷ 10^{n} = {Answers.Format(v)}.");
        },
        r =>
        {
            double mantisse = r.Next(11, 99) / 10.0;
            int exp = r.Next(2, 5);
            double v = mantisse * Math.Pow(10, exp);
            return Num(3, "Notation scientifique", $"{Answers.Format(mantisse)} × 10^{exp}", v,
                $"On décale la virgule de {exp} rangs : {Answers.Format(v)}.");
        },
        r =>
        {
            int unitPrice = r.Next(2, 9);
            int qty1 = r.Next(3, 8), qty2 = r.Next(9, 16);
            return Num(3, "Proportionnalité",
                $"{qty1} cahiers coûtent {qty1 * unitPrice} €. Combien coûtent {qty2} cahiers ?", qty2 * unitPrice,
                $"Un cahier vaut {qty1 * unitPrice} ÷ {qty1} = {unitPrice} €, donc {qty2} × {unitPrice} = {qty2 * unitPrice} €.");
        },
        r =>
        {
            int a = r.Next(2, 7), b = r.Next(2, 9), c = r.Next(2, 7), x = r.Next(2, 9);
            int v = a * (x - b) + c * x;
            return Num(3, "Calcul littéral", $"f(x) = {a}(x − {b}) + {c}x. Calcule f({x}).", v,
                $"f({x}) = {a} × ({x} − {b}) + {c} × {x} = {a * (x - b)} + {c * x} = {v}.");
        },
        r =>
        {
            int old = r.Next(2, 13) * 10;
            int[] taux = { 10, 20, 25, 50 };
            int t = taux[r.Next(taux.Length)];
            double v = old * (100 - t) / 100.0;
            return Num(3, "Pourcentage", $"Un article à {old} € baisse de {t} %. Quel est le nouveau prix ?", v,
                $"Baisser de {t} %, c'est multiplier par {Answers.Format((100 - t) / 100.0)} : {old} × {Answers.Format((100 - t) / 100.0)} = {Answers.Format(v)} €.");
        },
        r =>
        {
            int a = r.Next(2, 8), b = r.Next(2, 8);
            return Num(3, "Volume", $"Volume d'un pavé de {a} × {b} × {a} cm (en cm³)", a * b * a,
                $"V = L × l × h = {a} × {b} × {a} = {a * b * a} cm³.");
        }
    };

    // --- Niveau 4 : lycée ---------------------------------------------------------

    private static Func<Random, Question>[] Level4() => new Func<Random, Question>[]
    {
        r =>
        {
            int a = r.Next(1, 4), b = NonZero(r, 8), c = NonZero(r, 6);
            int delta = b * b - 4 * a * c;
            return Num(4, "Second degré", $"Discriminant de {Poly(a, b, c)}", delta,
                $"Δ = b² − 4ac = ({b})² − 4 × {a} × ({c}) = {b * b} − {4 * a * c} = {delta}.");
        },
        r =>
        {
            int r1 = NonZero(r, 6), r2 = NonZero(r, 6);
            // racines opposées : la somme s'annule et l'énoncé afficherait "+ 0x"
            while (r1 == r2 || r1 + r2 == 0) r2 = NonZero(r, 6);
            int b = -(r1 + r2), c = r1 * r2;
            int big = Math.Max(r1, r2);
            return Num(4, "Second degré", $"La plus grande solution de {Poly(1, b, c)} = 0", big,
                $"Les racines sont {Math.Min(r1, r2)} et {big} (somme {-b}, produit {c}).");
        },
        r =>
        {
            int n = r.Next(12, 41);
            return Num(4, "Racine carrée", $"√{n * n}", n, $"{n} × {n} = {n * n}, donc √{n * n} = {n}.");
        },
        r =>
        {
            int a = r.Next(2, 5), b = r.Next(1, 7), x = r.Next(1, 6);
            int v = (a * x + b) * (a * x + b);
            return Num(4, "Identités remarquables", $"f(x) = ({a}x + {b})². Calcule f({x}).", v,
                $"{a} × {x} + {b} = {a * x + b}, et {a * x + b}² = {v}.");
        },
        r =>
        {
            int u0 = r.Next(-5, 10), raison = r.Next(2, 9), n = r.Next(6, 16);
            int v = u0 + n * raison;
            return Num(4, "Suites", $"Suite arithmétique : u₀ = {u0}, raison {raison}. Combien vaut u{Sub(n)} ?", v,
                $"uₙ = u₀ + n × r = {u0} + {n} × {raison} = {v}.");
        },
        r =>
        {
            int x = r.Next(1, 9), y = r.Next(1, 9);
            int s = x + y, d = x - y;
            return Num(4, "Systèmes", $"x + y = {s} et x − y = {d}. Combien vaut x ?", x,
                $"En additionnant les deux lignes : 2x = {s + d}, donc x = {x}.");
        },
        r =>
        {
            int a = r.Next(-5, 6);
            while (a == 0) a = r.Next(-5, 6);
            int b = r.Next(-8, 9), x1 = r.Next(-4, 5), x2 = x1 + r.Next(1, 5);
            int y1 = a * x1 + b, y2 = a * x2 + b;
            return Num(4, "Fonction affine",
                $"Une droite passe par A({x1} ; {y1}) et B({x2} ; {y2}). Quel est son coefficient directeur ?", a,
                $"m = (y_B − y_A) / (x_B − x_A) = ({y2} − {y1}) / ({x2} − {x1}) = {a}.");
        },
        r =>
        {
            int n = r.Next(2, 7);
            double v = Math.Pow(10, n);
            return Num(4, "Logarithme", $"log({Answers.Format(v)})", n,
                $"log(10^{n}) = {n} : le logarithme décimal compte les zéros.");
        },
        r =>
        {
            int start = r.Next(2, 13) * 10;
            int[] taux = { 10, 20, 25, 50 };
            int t = taux[r.Next(taux.Length)];
            double v = start * (100 + t) / 100.0;
            return Num(4, "Évolutions", $"{start} augmente de {t} %, puis le résultat baisse de {t} %. Valeur finale ?",
                v * (100 - t) / 100.0,
                $"×{Answers.Format((100 + t) / 100.0)} puis ×{Answers.Format((100 - t) / 100.0)} : on ne revient pas au départ, on obtient {Answers.Format(v * (100 - t) / 100.0)}.");
        },
        r =>
        {
            (string, double, string)[] table =
            {
                ("cos(0°)", 1, "cos(0°) = 1."),
                ("sin(90°)", 1, "sin(90°) = 1."),
                ("cos(60°)", 0.5, "cos(60°) = 1/2."),
                ("sin(30°)", 0.5, "sin(30°) = 1/2."),
                ("tan(45°)", 1, "tan(45°) = sin/cos = 1."),
                ("sin(0°)", 0, "sin(0°) = 0."),
                ("cos(90°)", 0, "cos(90°) = 0.")
            };
            var (prompt, value, why) = table[r.Next(table.Length)];
            var q = Num(4, "Trigonométrie", prompt, value, why);
            return Math.Abs(value - 0.5) < 1e-9 ? q with { Accepted = new[] { "1/2" } } : q;
        },
        r =>
        {
            // valeurs choisies pour que le taux tombe juste : personne ne tape 13,513514
            int[] bases = { 20, 25, 40, 50, 80, 200 };
            int[] taux = { 5, 10, 20, 25, 50 };
            int n = bases[r.Next(bases.Length)];
            int t = taux[r.Next(taux.Length)] * (r.Next(2) == 0 ? 1 : -1);
            int after = n + n * t / 100;
            return Num(4, "Taux d'évolution", $"Une valeur passe de {n} à {after}. Taux d'évolution en % ?", t,
                $"({after} − {n}) ÷ {n} × 100 = {t} %.");
        }
    };

    // --- Niveau 5 : terminale -----------------------------------------------------

    private static Func<Random, Question>[] Level5() => new Func<Random, Question>[]
    {
        r =>
        {
            int a = r.Next(1, 4), b = NonZero(r, 5), c = NonZero(r, 8), x = r.Next(-3, 4);
            int v = 3 * a * x * x + 2 * b * x + c;
            return Num(5, "Dérivées", $"f(x) = {a}x³ {Signed(b)}x² {Signed(c)}x. Combien vaut f′({x}) ?", v,
                $"f′(x) = {3 * a}x² {Signed(2 * b)}x {Signed(c)}, donc f′({x}) = {v}.");
        },
        r =>
        {
            int u0 = r.Next(2, 6), q = r.Next(2, 4), n = r.Next(4, 9);
            double v = u0 * Math.Pow(q, n);
            return Num(5, "Suites", $"Suite géométrique : u₀ = {u0}, raison {q}. Combien vaut u{Sub(n)} ?", v,
                $"uₙ = u₀ × qⁿ = {u0} × {q}^{n} = {Answers.Format(v)}.");
        },
        r =>
        {
            int n = r.Next(10, 61);
            double v = n * (n + 1) / 2.0;
            return Num(5, "Sommes", $"1 + 2 + 3 + … + {n}", v,
                $"n(n+1)/2 = {n} × {n + 1} ÷ 2 = {Answers.Format(v)}.");
        },
        r =>
        {
            int n = r.Next(4, 9), k = r.Next(2, n - 1);
            long v = Binomial(n, k);
            return Num(5, "Dénombrement", $"Combien vaut le coefficient binomial C({n} ; {k}) ?", v,
                $"C({n} ; {k}) = {n}! / ({k}! × {n - k}!) = {v}.");
        },
        r =>
        {
            int n = r.Next(4, 9);
            long v = Factorial(n);
            return Num(5, "Factorielle", $"{n}!", v, $"{n}! = {string.Join(" × ", Enumerable.Range(1, n))} = {v}.");
        },
        r =>
        {
            int k = r.Next(2, 10);
            return Num(5, "Logarithme", $"ln(e^{k})", k, $"ln et exp s'annulent : ln(e^{k}) = {k}.");
        },
        r =>
        {
            int x = r.Next(3, 11);
            double v = Math.Pow(2, x);
            return Num(5, "Exponentielle", $"Résous 2^x = {Answers.Format(v)}", x,
                $"2^{x} = {Answers.Format(v)}, donc x = {x}.");
        },
        r =>
        {
            // b multiple de n+1 : la primitive tombe alors sur un entier
            int n = r.Next(1, 4);
            int b = (n + 1) * r.Next(1, 3);
            double v = Math.Pow(b, n + 1) / (n + 1);
            return Num(5, "Intégrales", $"∫ de 0 à {b} de x^{n} dx", v,
                $"Une primitive est x^{n + 1}/{n + 1}, donc {b}^{n + 1}/{n + 1} = {Answers.Format(v)}.");
        },
        r =>
        {
            int red = r.Next(2, 7), blue = r.Next(2, 7);
            int total = red + blue;
            double p = (double)red / total;
            return Text(5, "Probabilités",
                $"Une urne contient {red} boules rouges et {blue} bleues. Probabilité de tirer une rouge ?",
                $"{red}/{total}", p,
                $"p = favorables / total = {red}/{total} = {Answers.Format(p)}.");
        },
        r =>
        {
            int a = r.Next(2, 8), b = r.Next(1, 9);
            return Num(5, "Limites", $"Limite en +∞ de ({a}x² + {b}x) / (x² + 1)", a,
                $"On compare les termes de plus haut degré : {a}x²/x² → {a}.");
        },
        r =>
        {
            int k = r.Next(2, 6);
            return Num(5, "Dérivées", $"f(x) = e^({k}x). Combien vaut f′(0) ?", k,
                $"f′(x) = {k}e^({k}x), et e⁰ = 1, donc f′(0) = {k}.");
        },
        r =>
        {
            int u0 = r.Next(1, 5), q = r.Next(2, 4), n = r.Next(3, 8);
            double v = u0 * (Math.Pow(q, n + 1) - 1) / (q - 1);
            return Num(5, "Sommes", $"u₀ = {u0}, raison {q}. Somme u₀ + u₁ + … + u{Sub(n)} ?", v,
                $"S = u₀ × (1 − q^(n+1)) / (1 − q) = {u0} × ({Answers.Format(Math.Pow(q, n + 1))} − 1) / {q - 1} = {Answers.Format(v)}.");
        }
    };

    // --- Niveau 6 : post-bac ------------------------------------------------------

    private static Func<Random, Question>[] Level6() => new Func<Random, Question>[]
    {
        r =>
        {
            int a = NonZero(r, 8), b = NonZero(r, 8), c = NonZero(r, 8), d = NonZero(r, 8);
            return Num(6, "Matrices", $"Déterminant de la matrice [ {a} {b} ; {c} {d} ]", a * d - b * c,
                $"det = ad − bc = {a}×{d} − {b}×{c} = {a * d} − {b * c} = {a * d - b * c}.");
        },
        r =>
        {
            (int, int, int)[] triples = { (3, 4, 5), (6, 8, 10), (5, 12, 13), (8, 15, 17), (7, 24, 25), (9, 12, 15) };
            var (x, y, m) = triples[r.Next(triples.Length)];
            return Num(6, "Complexes", $"Module de z = {x} + {y}i", m,
                $"|z| = √({x}² + {y}²) = √{x * x + y * y} = {m}.");
        },
        r =>
        {
            int a = NonZero(r, 6), b = NonZero(r, 6), c = NonZero(r, 6), d = NonZero(r, 6);
            return Num(6, "Complexes", $"Partie réelle de ({a} + {b}i)({c} + {d}i)", a * c - b * d,
                $"i² = −1, donc la partie réelle vaut ac − bd = {a * c} − {b * d} = {a * c - b * d}.");
        },
        r =>
        {
            int a = r.Next(2, 5), b = NonZero(r, 5), n = r.Next(2, 4), x = r.Next(0, 3);
            double inner = a * x + b;
            double v = n * a * Math.Pow(inner, n - 1);
            return Num(6, "Dérivées", $"f(x) = ({a}x {Signed(b)})^{n}. Combien vaut f′({x}) ?", v,
                $"f′(x) = {n}×{a}×({a}x {Signed(b)})^{n - 1}, et {a}×{x} {Signed(b)} = {Answers.Format(inner)}, donc f′({x}) = {Answers.Format(v)}.");
        },
        r =>
        {
            int a = r.Next(1, 4), b = NonZero(r, 6), t = r.Next(-3, 4);
            int v = 12 * a * t * t + 2 * b;
            return Num(6, "Dérivées", $"f(x) = {a}x⁴ {Signed(b)}x². Combien vaut f″({t}) ?", v,
                $"f′(x) = {4 * a}x³ {Signed(2 * b)}x, puis f″(x) = {12 * a}x² {Signed(2 * b)}, donc f″({t}) = {v}.");
        },
        r =>
        {
            int[] tirages = { 20, 40, 50, 80, 100, 200 };
            int[] pourcents = { 5, 10, 20, 25, 50 };
            int n = tirages[r.Next(tirages.Length)];
            int p = pourcents[r.Next(pourcents.Length)];
            double v = n * p / 100.0;
            return Num(6, "Probabilités", $"X suit une loi binomiale B({n} ; {Answers.Format(p / 100.0)}). Espérance de X ?", v,
                $"E(X) = np = {n} × {Answers.Format(p / 100.0)} = {Answers.Format(v)}.");
        },
        r =>
        {
            int[] tirages = { 20, 40, 50, 100, 200 };
            int[] pourcents = { 10, 20, 50 };
            int n = tirages[r.Next(tirages.Length)];
            int p = pourcents[r.Next(pourcents.Length)];
            double v = n * (p / 100.0) * (1 - p / 100.0);
            return Num(6, "Probabilités", $"X suit B({n} ; {Answers.Format(p / 100.0)}). Variance de X ?", v,
                $"V(X) = np(1−p) = {n} × {Answers.Format(p / 100.0)} × {Answers.Format(1 - p / 100.0)} = {Answers.Format(v)}.");
        },
        r =>
        {
            int n = r.Next(5, 10), k = r.Next(2, 4);
            long v = 1;
            for (int i = 0; i < k; i++) v *= n - i;
            return Num(6, "Dénombrement", $"Nombre d'arrangements A({n} ; {k})", v,
                $"A({n} ; {k}) = {n}! / ({n - k})! = {string.Join(" × ", Enumerable.Range(0, k).Select(i => n - i))} = {v}.");
        },
        r =>
        {
            int n = r.Next(6, 21);
            double v = n * (n + 1) * (2 * n + 1) / 6.0;
            return Num(6, "Sommes", $"1² + 2² + 3² + … + {n}²", v,
                $"n(n+1)(2n+1)/6 = {n} × {n + 1} × {2 * n + 1} ÷ 6 = {Answers.Format(v)}.");
        },
        r =>
        {
            int k = r.Next(2, 8);
            return Num(6, "Intégrales", $"∫ de 1 à e^{k} de 1/x dx", k,
                $"Une primitive de 1/x est ln(x) : ln(e^{k}) − ln(1) = {k} − 0 = {k}.");
        },
        r =>
        {
            (string, double, string)[] limites =
            {
                ("sin(x) / x", 1, "C'est la limite de référence : sin(x)/x → 1."),
                ("(e^x − 1) / x", 1, "Le taux d'accroissement de exp en 0 vaut exp′(0) = 1."),
                ("ln(1 + x) / x", 1, "Le taux d'accroissement de ln(1+x) en 0 vaut 1."),
                ("(1 − cos(x)) / x²", 0.5, "1 − cos(x) ≈ x²/2, donc la limite vaut 1/2."),
                ("tan(x) / x", 1, "tan(x) ≈ x au voisinage de 0.")
            };
            var (expression, value, why) = limites[r.Next(limites.Length)];
            var q = Num(6, "Limites", $"Limite en 0 de {expression}", value, why);
            return Math.Abs(value - 0.5) < 1e-9 ? q with { Accepted = new[] { "1/2" } } : q;
        },
        r =>
        {
            int ax = NonZero(r, 7), ay = NonZero(r, 7), bx = NonZero(r, 7), by = NonZero(r, 7);
            return Num(6, "Vecteurs", $"Produit scalaire de u({ax} ; {ay}) et v({bx} ; {by})", ax * bx + ay * by,
                $"u·v = {ax}×{bx} + {ay}×{by} = {ax * bx} + {ay * by} = {ax * bx + ay * by}.");
        }
    };

    // --- Fabrication ---------------------------------------------------------------

    private static Question Num(int level, string topic, string prompt, double value, string explanation) => new()
    {
        Level = level,
        Topic = topic,
        Prompt = prompt,
        Expected = Answers.Format(value),
        Numeric = value,
        Explanation = explanation,
        Seconds = Question.SecondsFor(level),
        BasePoints = level * 10
    };

    private static Question Text(int level, string topic, string prompt, string expected, double numeric, string explanation) => new()
    {
        Level = level,
        Topic = topic,
        Prompt = prompt,
        Expected = expected,
        Numeric = numeric,
        Accepted = new[] { expected, Answers.Format(numeric) },
        Explanation = explanation,
        Seconds = Question.SecondsFor(level),
        BasePoints = level * 10
    };

    // --- Petits outils --------------------------------------------------------------

    private static int Gcd(int a, int b)
    {
        a = Math.Abs(a);
        b = Math.Abs(b);
        while (b != 0) (a, b) = (b, a % b);
        return a == 0 ? 1 : a;
    }

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

    /// <summary>Un entier de −max à max, jamais nul : évite les "+ 0x" dans les énoncés.</summary>
    private static int NonZero(Random r, int max)
    {
        int v = r.Next(1, max + 1);
        return r.Next(2) == 0 ? v : -v;
    }

    /// <summary>"+ 3" ou "− 3", pour écrire un polynôme proprement.</summary>
    private static string Signed(int v) => v < 0 ? $"− {-v}" : $"+ {v}";

    /// <summary>Chiffres en indice, pour écrire u₁₂ sans passer par du XAML.</summary>
    private static string Sub(int n) => string.Concat(n.ToString().Select(c => "₀₁₂₃₄₅₆₇₈₉"[c - '0']));

    private static string Poly(int a, int b, int c)
    {
        string head = a == 1 ? "x²" : $"{a}x²";
        return $"{head} {Signed(b)}x {Signed(c)}";
    }
}
