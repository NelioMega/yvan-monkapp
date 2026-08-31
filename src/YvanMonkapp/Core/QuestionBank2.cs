namespace YvanMonkapp.Core;

/// <summary>Les modèles d'énoncés du lycée et de la terminale (niveaux 4 et 5).</summary>
public static partial class QuestionGenerator
{
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
            int r1 = NonZero(r, 7), r2 = NonZero(r, 7);
            while (r1 + r2 == 0) r2 = NonZero(r, 7);
            int b = -(r1 + r2), c = r1 * r2;
            return Num(4, "Second degré", $"Somme des racines de {Poly(1, b, c)} = 0", r1 + r2,
                $"La somme des racines vaut −b/a = {-b}.");
        },
        r =>
        {
            int r1 = NonZero(r, 7), r2 = NonZero(r, 7);
            while (r1 + r2 == 0) r2 = NonZero(r, 7);
            int b = -(r1 + r2), c = r1 * r2;
            return Num(4, "Second degré", $"Produit des racines de {Poly(1, b, c)} = 0", c,
                $"Le produit des racines vaut c/a = {c}.");
        },
        r =>
        {
            // un sommet en 0 donnerait "+ 0x" dans l'énoncé
            int a = r.Next(1, 4), s = NonZero(r, 5);
            int b = -2 * a * s, c = NonZero(r, 9);
            return Num(4, "Second degré", $"Abscisse du sommet de la parabole {Poly(a, b, c)}", s,
                $"L'abscisse du sommet vaut −b/(2a) = {-b} ÷ {2 * a} = {s}.");
        },
        r =>
        {
            int a = r.Next(1, 4), b = NonZero(r, 7), c = NonZero(r, 7);
            int delta = b * b - 4 * a * c;
            int roots = delta > 0 ? 2 : delta == 0 ? 1 : 0;
            return Num(4, "Second degré", $"Combien de solutions réelles a l'équation {Poly(a, b, c)} = 0 ?", roots,
                $"Δ = {delta} : {(delta > 0 ? "positif, donc deux racines" : delta == 0 ? "nul, donc une racine double" : "négatif, donc aucune racine réelle")}.");
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
            int a = r.Next(11, 40);
            return Num(4, "Identités remarquables", $"Calcule {a}² − {a - 1}² sans poser l'opération.", 2 * a - 1,
                $"a² − b² = (a − b)(a + b) = 1 × ({a} + {a - 1}) = {2 * a - 1}.");
        },
        r =>
        {
            int u0 = r.Next(-5, 10), raison = r.Next(2, 9), n = r.Next(6, 16);
            int v = u0 + n * raison;
            return Num(4, "Suites", $"Suite arithmétique : u₀ = {Nb(u0)}, raison {raison}. Combien vaut u{Sub(n)} ?", v,
                $"uₙ = u₀ + n × r = {u0} + {n} × {raison} = {v}.");
        },
        r =>
        {
            int u0 = r.Next(1, 9), raison = r.Next(2, 8), n = r.Next(5, 15);
            return Num(4, "Suites",
                $"Suite arithmétique : u₀ = {u0}, raison {raison}. Pour quel rang n a-t-on uₙ = {u0 + n * raison} ?", n,
                $"{u0} + n × {raison} = {u0 + n * raison} donne n × {raison} = {n * raison}, donc n = {n}.");
        },
        r =>
        {
            int u0 = r.Next(1, 8), raison = r.Next(2, 7), n = r.Next(5, 13);
            int sum = (n + 1) * (2 * u0 + n * raison) / 2;
            return Num(4, "Suites",
                $"Suite arithmétique : u₀ = {u0}, raison {raison}. Combien vaut u₀ + u₁ + … + u{Sub(n)} ?", sum,
                $"S = (nombre de termes) × (premier + dernier) ÷ 2 = {n + 1} × ({u0} + {u0 + n * raison}) ÷ 2 = {sum}.");
        },
        r =>
        {
            int x = r.Next(1, 9), y = r.Next(1, 9);
            int s = x + y, d = x - y;
            return Num(4, "Systèmes", $"x + y = {s} et x − y = {Nb(d)}. Combien vaut x ?", x,
                $"En additionnant les deux lignes : 2x = {s + d}, donc x = {x}.");
        },
        r =>
        {
            int y = r.Next(1, 8), x = y + r.Next(1, 6), a = r.Next(2, 5), b = r.Next(2, 5);
            return Num(4, "Systèmes", $"{a}x + {b}y = {a * x + b * y} et x − y = {x - y}. Combien vaut y ?", y,
                $"x = y + {x - y}, d'où {a}(y + {x - y}) + {b}y = {a * x + b * y} et y = {y}.");
        },
        r =>
        {
            int a = NonZero(r, 5), b = r.Next(-8, 9), x1 = r.Next(-4, 5), x2 = x1 + r.Next(1, 5);
            int y1 = a * x1 + b, y2 = a * x2 + b;
            return Num(4, "Fonction affine",
                $"Une droite passe par A({Nb(x1)} ; {Nb(y1)}) et B({Nb(x2)} ; {Nb(y2)}). Quel est son coefficient directeur ?", a,
                $"m = (y_B − y_A) / (x_B − x_A) = ({y2} − {y1}) / ({x2} − {x1}) = {a}.");
        },
        r =>
        {
            int a = r.Next(2, 8), x = r.Next(-6, 9), b = NonZero(r, 12);
            return Num(4, "Fonction affine", $"f(x) = {a}x {Signed(b)}. Quel antécédent a l'image {Nb(a * x + b)} ?", x,
                $"{a}x {Signed(b)} = {a * x + b} donne {a}x = {a * x}, donc x = {x}.");
        },
        r =>
        {
            int a = r.Next(2, 6), b = NonZero(r, 9), x = r.Next(-4, 6);
            string tail = Term(b, "x");
            return Num(4, "Dérivées", $"f(x) = {a}x² {tail}. Combien vaut f′({Nb(x)}) ?", 2 * a * x + b,
                $"f′(x) = {2 * a}x {Signed(b)}, donc f′({x}) = {2 * a * x + b}.");
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
            int a = r.Next(2, 7), b = r.Next(2, 7);
            return Num(4, "Exponentielle", $"e^{a} × e^{b} = e^k. Combien vaut k ?", a + b,
                $"Les exposants s'ajoutent : {a} + {b} = {a + b}.");
        },
        r =>
        {
            int start = r.Next(2, 13) * 10;
            int t = Pick(r, 10, 20, 25, 50);
            double v = start * (100 + t) / 100.0 * (100 - t) / 100.0;
            return Num(4, "Évolutions", $"{start} augmente de {t} %, puis le résultat baisse de {t} %. Valeur finale ?", v,
                $"×{Answers.Format((100 + t) / 100.0)} puis ×{Answers.Format((100 - t) / 100.0)} : on ne revient pas au départ, on obtient {Answers.Format(v)}.");
        },
        r =>
        {
            int start = r.Next(2, 16) * 20;
            int t = Pick(r, 10, 20, 25, 50);
            double after = start * (100 + t) / 100.0;
            return Num(4, "Évolutions",
                $"Après une hausse de {t} %, un prix atteint {Answers.Format(after)} €. Quel était le prix de départ ?",
                start, $"On divise par {Answers.Format((100 + t) / 100.0)} : {Answers.Format(after)} ÷ {Answers.Format((100 + t) / 100.0)} = {start} €.");
        },
        r =>
        {
            // valeurs choisies pour que le taux tombe juste : personne ne tape 13,513514
            int n = Pick(r, 20, 25, 40, 50, 80, 200);
            int t = Pick(r, 5, 10, 20, 25, 50) * (r.Next(2) == 0 ? 1 : -1);
            int after = n + n * t / 100;
            return Num(4, "Taux d'évolution", $"Une valeur passe de {n} à {after}. Taux d'évolution en % ?", t,
                $"({after} − {n}) ÷ {n} × 100 = {t} %.");
        },
        r =>
        {
            var (prompt, top, bottom, why) = Pick(r,
                ("cos(0°)", 1, 1, "cos(0°) = 1."),
                ("sin(90°)", 1, 1, "sin(90°) = 1."),
                ("cos(60°)", 1, 2, "cos(60°) = 1/2."),
                ("sin(30°)", 1, 2, "sin(30°) = 1/2."),
                ("tan(45°)", 1, 1, "tan(45°) = sin/cos = 1."),
                ("sin(0°)", 0, 1, "sin(0°) = 0."),
                ("cos(90°)", 0, 1, "cos(90°) = 0."));
            return Frac(4, "Trigonométrie", prompt, top, bottom, why);
        },
        r =>
        {
            int h = r.Next(3, 15) * 2;
            return Num(4, "Trigonométrie",
                $"Dans un triangle rectangle, l'hypoténuse mesure {h} et un angle aigu vaut 60°. Combien mesure le côté adjacent à cet angle ?",
                h / 2, $"adjacent = hypoténuse × cos(60°) = {h} × 0,5 = {h / 2}.");
        },
        r =>
        {
            var (p, q, n) = Pick(r, (3, 4, 5), (6, 8, 10), (5, 12, 13), (8, 15, 17), (9, 12, 15), (7, 24, 25));
            int ax = r.Next(-5, 6), ay = r.Next(-5, 6);
            int sx = r.Next(2) == 0 ? 1 : -1, sy = r.Next(2) == 0 ? 1 : -1;
            return Num(4, "Vecteurs",
                $"A({Nb(ax)} ; {Nb(ay)}) et B({Nb(ax + sx * p)} ; {Nb(ay + sy * q)}). Quelle est la longueur AB ?", n,
                $"AB a pour coordonnées ({sx * p} ; {sy * q}), donc AB = √({p}² + {q}²) = √{p * p + q * q} = {n}.");
        },
        r =>
        {
            int r1 = NonZero(r, 9), r2 = NonZero(r, 9);
            while (r2 == r1) r2 = NonZero(r, 9);
            return Num(4, "Factorisation",
                $"Résous (x {Signed(-r1)})(x {Signed(-r2)}) = 0 : quelle est la plus grande solution ?",
                Math.Max(r1, r2), $"Un produit est nul si l'un des facteurs l'est : x = {r1} ou x = {r2}.");
        },
        r =>
        {
            int pa = r.Next(20, 60), pb = r.Next(20, 60), pab = r.Next(5, 20);
            return Num(4, "Probabilités",
                $"P(A) = {pa} %, P(B) = {pb} %, P(A ∩ B) = {pab} %. Combien vaut P(A ∪ B), en % ?",
                pa + pb - pab, $"P(A ∪ B) = P(A) + P(B) − P(A ∩ B) = {pa} + {pb} − {pab} = {pa + pb - pab} %.");
        },
        r =>
        {
            int a = r.Next(-30, 10), b = a + r.Next(3, 40);
            return Num(4, "Valeur absolue", $"|{Nb(a)} − {b}|", b - a,
                $"{a} − {b} = {a - b}, et la valeur absolue en fait {b - a}.");
        },
        r =>
        {
            int a = r.Next(-20, 10), b = a + r.Next(3, 30);
            return Num(4, "Intervalles", $"Combien d'entiers l'intervalle [{Nb(a)} ; {Nb(b)}] contient-il ?", b - a + 1,
                $"De {a} à {b} il y a {b} − {a} + 1 = {b - a + 1} entiers, bornes comprises.");
        },
        r =>
        {
            int k = Pick(r, 12, 18, 24, 36, 48, 60), x = Pick(r, 2, 3, 4, 6);
            return Num(4, "Fonction inverse", $"f(x) = {k}/x. Combien vaut f({x}) ?", (double)k / x,
                $"f({x}) = {k} ÷ {x} = {Answers.Format((double)k / x)}.");
        },
        r =>
        {
            int n1 = r.Next(8, 16), n2 = r.Next(8, 16), c1 = r.Next(1, 4), c2 = r.Next(1, 4);
            double v = (double)(n1 * c1 + n2 * c2) / (c1 + c2);
            return Num(4, "Moyenne",
                $"Un devoir noté {n1} compte coefficient {c1}, un autre noté {n2} compte coefficient {c2}. Quelle est la moyenne ?",
                v, $"({n1} × {c1} + {n2} × {c2}) ÷ ({c1} + {c2}) = {n1 * c1 + n2 * c2} ÷ {c1 + c2} = {Answers.Format(v)}.");
        }
    };

    // --- Niveau 5 : terminale -----------------------------------------------------

    private static Func<Random, Question>[] Level5() => new Func<Random, Question>[]
    {
        r =>
        {
            int a = r.Next(1, 4), b = NonZero(r, 5), c = NonZero(r, 8), x = r.Next(-3, 4);
            int v = 3 * a * x * x + 2 * b * x + c;
            string square = Term(b, "x²"), linear = Term(c, "x"), derived = Term(2 * b, "x");
            return Num(5, "Dérivées", $"f(x) = {a}x³ {square} {linear}. Combien vaut f′({Nb(x)}) ?", v,
                $"f′(x) = {3 * a}x² {derived} {Signed(c)}, donc f′({x}) = {v}.");
        },
        r =>
        {
            int k = r.Next(2, 6);
            return Num(5, "Dérivées", $"f(x) = e^({k}x). Combien vaut f′(0) ?", k,
                $"f′(x) = {k}e^({k}x), et e⁰ = 1, donc f′(0) = {k}.");
        },
        r =>
        {
            int a = r.Next(2, 13);
            return Frac(5, "Dérivées", $"f(x) = ln(x). Combien vaut f′({a}) ?", 1, a,
                $"f′(x) = 1/x, donc f′({a}) = 1/{a}.");
        },
        r =>
        {
            int a = r.Next(1, 8);
            return Num(5, "Dérivées", $"f(x) = (x + {a})e^x. Combien vaut f′(0) ?", a + 1,
                $"f′(x) = (x + {a + 1})e^x, et e⁰ = 1, donc f′(0) = {a + 1}.");
        },
        r =>
        {
            int a = r.Next(2, 8);
            return Num(5, "Tangente", $"f(x) = x². Quel est le coefficient directeur de la tangente en x = {a} ?", 2 * a,
                $"f′(x) = 2x, donc f′({a}) = {2 * a}.");
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
            int u0 = r.Next(1, 6), a = r.Next(2, 4), b = NonZero(r, 5);
            int u1 = a * u0 + b, u2 = a * u1 + b, u3 = a * u2 + b;
            return Num(5, "Récurrence",
                $"u₀ = {u0} et uₙ₊₁ = {a}uₙ {Signed(b)}. Combien vaut u₃ ?", u3,
                $"u₁ = {u1}, u₂ = {u2}, u₃ = {u3}.");
        },
        r =>
        {
            int a = r.Next(2, 9), c = r.Next(2, 9), b = r.Next(1, 9), d = r.Next(1, 9);
            return Frac(5, "Limites", $"Limite en +∞ de ({a}n + {b}) / ({c}n + {d})", a, c,
                $"On compare les termes dominants : {a}n / {c}n → {a}/{c}.");
        },
        r =>
        {
            int a = r.Next(2, 8), b = r.Next(1, 9);
            return Num(5, "Limites", $"Limite en +∞ de ({a}x² + {b}x) / (x² + 1)", a,
                $"On compare les termes de plus haut degré : {a}x²/x² → {a}.");
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
            int u0 = r.Next(1, 5), q = r.Next(2, 4), n = r.Next(3, 8);
            double v = u0 * (Math.Pow(q, n + 1) - 1) / (q - 1);
            return Num(5, "Sommes", $"u₀ = {u0}, raison {q}. Somme u₀ + u₁ + … + u{Sub(n)} ?", v,
                $"S = u₀ × (q^(n+1) − 1) / (q − 1) = {u0} × ({Answers.Format(Math.Pow(q, n + 1))} − 1) / {q - 1} = {Answers.Format(v)}.");
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
            int k = r.Next(2, 7);
            return Num(5, "Dénombrement", $"Combien de codes de {k} chiffres peut-on écrire avec les chiffres de 0 à 9 ?",
                Math.Pow(10, k), $"Chaque rang offre 10 choix : 10^{k} = {Answers.Format(Math.Pow(10, k))}.");
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
            int a = r.Next(2, 8), x = r.Next(2, 9);
            return Num(5, "Logarithme", $"Résous ln({a}) + ln(x) = ln({a * x})", x,
                $"ln({a}) + ln(x) = ln({a}x), donc {a}x = {a * x} et x = {x}.");
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
            int a = r.Next(2, 5), k = a * r.Next(2, 6);
            return Num(5, "Exponentielle", $"Résous e^({a}x) = e^{k}", k / a,
                $"Les exponentielles sont égales quand les exposants le sont : {a}x = {k}, donc x = {k / a}.");
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
            int k = r.Next(2, 9);
            return Num(5, "Intégrales", $"∫ de 0 à ln({k}) de e^x dx", k - 1,
                $"Une primitive de e^x est e^x : e^ln({k}) − e⁰ = {k} − 1 = {k - 1}.");
        },
        r =>
        {
            int a = r.Next(2, 8), b = r.Next(2, 7);
            return Num(5, "Intégrales", $"∫ de 0 à {b} de {a} dx", a * b,
                $"L'aire du rectangle vaut hauteur × largeur = {a} × {b} = {a * b}.");
        },
        r =>
        {
            int red = r.Next(2, 7), blue = r.Next(2, 7);
            return Frac(5, "Probabilités",
                $"Une urne contient {red} boules rouges et {blue} bleues. Probabilité de tirer une rouge ?",
                red, red + blue, $"p = favorables / total = {red}/{red + blue}.");
        },
        r =>
        {
            int sum = Pick(r, 5, 6, 7, 8, 9);
            int ways = 6 - Math.Abs(7 - sum);
            return Frac(5, "Probabilités", $"On lance deux dés. Quelle est la probabilité que la somme vaille {sum} ?",
                ways, 36, $"{ways} tirages sur 36 donnent {sum}.");
        },
        r =>
        {
            int pb = Pick(r, 20, 25, 40, 50, 80), pab = pb / Pick(r, 2, 4, 5);
            return Num(5, "Probabilités",
                $"P(A ∩ B) = {pab} % et P(B) = {pb} %. Combien vaut P(A | B), en % ?", pab * 100.0 / pb,
                $"P(A | B) = P(A ∩ B) / P(B) = {pab} ÷ {pb} = {Answers.Format(pab * 100.0 / pb)} %.");
        },
        r =>
        {
            int gain = r.Next(2, 12), loss = r.Next(1, 6), faces = Pick(r, 2, 3, 6);
            return Frac(5, "Probabilités",
                $"Un jeu à {faces} issues équiprobables fait gagner {gain} € sur une seule issue et perdre {loss} € sur les autres. Espérance de gain, en euros ?",
                gain - loss * (faces - 1), faces,
                $"E = ({gain} − {loss} × {faces - 1}) ÷ {faces}, soit {gain - loss * (faces - 1)}/{faces} €.");
        },
        r =>
        {
            var (prompt, top, bottom, why) = Pick(r,
                ("cos(π/3)", 1, 2, "cos(π/3) = 1/2."),
                ("sin(π/6)", 1, 2, "sin(π/6) = 1/2."),
                ("cos(π)", -1, 1, "cos(π) = −1."),
                ("sin(π/2)", 1, 1, "sin(π/2) = 1."),
                ("sin(π)", 0, 1, "sin(π) = 0."),
                ("tan(π/4)", 1, 1, "tan(π/4) = 1."));
            return Frac(5, "Trigonométrie", prompt, top, bottom, why);
        },
        r =>
        {
            int n = r.Next(4, 10);
            return Num(5, "Dénombrement", $"Combien de sous-ensembles un ensemble de {n} éléments possède-t-il ?",
                Math.Pow(2, n), $"Chaque élément est pris ou non : 2^{n} = {(int)Math.Pow(2, n)}.");
        },
        r =>
        {
            int a = Pick(r, 2, 3, 4, 7, 8, 9), k = r.Next(5, 40);
            long unit = PowMod(a, k, 10);
            return Num(5, "Congruences", $"Quel est le chiffre des unités de {a}^{k} ?", unit,
                $"Les unités de {a}ⁿ tournent en boucle, et {a}^{k} se termine par {unit}.");
        },
        r =>
        {
            int n = Pick(r, 7, 9, 11, 12, 13), a = r.Next(20, 200), b = r.Next(20, 200);
            return Num(5, "Congruences", $"Quel est le reste de {a} + {b} modulo {n} ?", (a + b) % n,
                $"{a} + {b} = {a + b}, et {a + b} = {n} × {(a + b) / n} + {(a + b) % n}.");
        },
        r =>
        {
            int n = Pick(r, 5, 7, 9, 11, 13), a = r.Next(11, 60), b = r.Next(11, 60);
            return Num(5, "Congruences", $"Combien vaut {a} × {b} modulo {n} ?", a * b % n,
                $"On réduit d'abord : {a} ≡ {a % n} et {b} ≡ {b % n}, donc le produit vaut {a % n} × {b % n} = {a % n * (b % n)} ≡ {a * b % n} [{n}].");
        },
        r =>
        {
            int n = Pick(r, 5, 7, 9, 11, 12, 13), a = -r.Next(2, 60);
            int v = ((a % n) + n) % n;
            return Num(5, "Congruences", $"Quel est le plus petit entier naturel congru à {Nb(a)} modulo {n} ?", v,
                $"On ajoute des {n} jusqu'à devenir positif : {Nb(a)} + {n} × {(v - a) / n} = {v}.");
        }
    };
}
