namespace YvanMonkapp.Core;

/// <summary>Les modèles d'énoncés d'après le bac : post-bac, prépa et agrégation (niveaux 6 à 8).</summary>
public static partial class QuestionGenerator
{
    // --- Niveau 6 : post-bac ------------------------------------------------------

    private static Func<Random, Question>[] Level6() => new Func<Random, Question>[]
    {
        r =>
        {
            int a = NonZero(r, 8), b = NonZero(r, 8), c = NonZero(r, 8), d = NonZero(r, 8);
            return Num(6, "Matrices", $"Déterminant de la matrice [ {Nb(a)} {Nb(b)} ; {Nb(c)} {Nb(d)} ]", a * d - b * c,
                $"det = ad − bc = {a}×{d} − {b}×{c} = {a * d} − {b * c} = {a * d - b * c}.");
        },
        r =>
        {
            int a = NonZero(r, 9), b = NonZero(r, 9), c = NonZero(r, 9), d = NonZero(r, 9);
            return Num(6, "Matrices", $"Trace de la matrice [ {Nb(a)} {Nb(b)} ; {Nb(c)} {Nb(d)} ]", a + d,
                $"La trace est la somme de la diagonale : {a} + {d} = {a + d}.");
        },
        r =>
        {
            int a = NonZero(r, 6), b = NonZero(r, 6), c = NonZero(r, 6), d = NonZero(r, 6);
            return Num(6, "Matrices", $"A = [ {Nb(a)} {Nb(b)} ; {Nb(c)} {Nb(d)} ]. Quel est le coefficient en haut à gauche de A² ?",
                a * a + b * c, $"(A²)₁₁ = {a}×{a} + {b}×{c} = {a * a} + {b * c} = {a * a + b * c}.");
        },
        r =>
        {
            var (x, y, m) = Pick(r, (3, 4, 5), (6, 8, 10), (5, 12, 13), (8, 15, 17), (7, 24, 25), (9, 12, 15));
            return Num(6, "Complexes", $"Module de z = {x} + {y}i", m,
                $"|z| = √({x}² + {y}²) = √{x * x + y * y} = {m}.");
        },
        r =>
        {
            int a = NonZero(r, 6), b = NonZero(r, 6), c = NonZero(r, 6), d = NonZero(r, 6);
            string left = $"{Nb(a)} {Term(b, "i")}", right = $"{Nb(c)} {Term(d, "i")}";
            return Num(6, "Complexes", $"Partie réelle de ({left})({right})", a * c - b * d,
                $"i² = −1, donc la partie réelle vaut ac − bd = {a * c} − {b * d} = {a * c - b * d}.");
        },
        r =>
        {
            int a = NonZero(r, 7), b = NonZero(r, 7);
            string z = $"{Nb(a)} {Term(b, "i")}", conjugate = $"{Nb(a)} {Term(-b, "i")}";
            return Num(6, "Complexes", $"Partie imaginaire du conjugué de z = {z}", -b,
                $"Le conjugué de z est {conjugate} : sa partie imaginaire vaut {Nb(-b)}.");
        },
        r =>
        {
            var (z, deg, why) = Pick(r,
                ("1", 0, "1 est sur l'axe des réels positifs."),
                ("i", 90, "i est sur l'axe des imaginaires positifs."),
                ("−1", 180, "−1 est sur l'axe des réels négatifs."),
                ("1 + i", 45, "1 + i est sur la bissectrice du premier quadrant."),
                ("−1 + i", 135, "−1 + i est sur la bissectrice du deuxième quadrant."));
            return Num(6, "Complexes", $"Argument de z = {z}, en degrés entre 0 et 360", deg, why);
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
            string square = Term(b, "x²"), derived = Term(2 * b, "x");
            return Num(6, "Dérivées", $"f(x) = {a}x⁴ {square}. Combien vaut f″({Nb(t)}) ?", v,
                $"f′(x) = {4 * a}x³ {derived}, puis f″(x) = {12 * a}x² {Signed(2 * b)}, donc f″({t}) = {v}.");
        },
        r =>
        {
            int a = r.Next(2, 6), b = r.Next(2, 6), x = r.Next(1, 5), y = r.Next(1, 5);
            return Num(6, "Dérivées",
                $"f(x ; y) = {a}x²y {Signed(b)}y². Combien vaut ∂f/∂x au point ({x} ; {y}) ?", 2 * a * x * y,
                $"À y fixé, ∂f/∂x = {2 * a}xy, donc {2 * a} × {x} × {y} = {2 * a * x * y}.");
        },
        r =>
        {
            int n = Pick(r, 20, 40, 50, 80, 100, 200);
            int p = Pick(r, 5, 10, 20, 25, 50);
            double v = n * p / 100.0;
            return Num(6, "Probabilités", $"X suit une loi binomiale B({n} ; {Answers.Format(p / 100.0)}). Espérance de X ?", v,
                $"E(X) = np = {n} × {Answers.Format(p / 100.0)} = {Answers.Format(v)}.");
        },
        r =>
        {
            int n = Pick(r, 20, 40, 50, 100, 200);
            int p = Pick(r, 10, 20, 50);
            double v = n * (p / 100.0) * (1 - p / 100.0);
            return Num(6, "Probabilités", $"X suit B({n} ; {Answers.Format(p / 100.0)}). Variance de X ?", v,
                $"V(X) = np(1−p) = {n} × {Answers.Format(p / 100.0)} × {Answers.Format(1 - p / 100.0)} = {Answers.Format(v)}.");
        },
        r =>
        {
            int lambda = r.Next(2, 12);
            return Num(6, "Probabilités", $"X suit une loi de Poisson de paramètre λ = {lambda}. Variance de X ?", lambda,
                $"Pour une loi de Poisson, espérance et variance valent toutes deux λ = {lambda}.");
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
            int n = r.Next(4, 15);
            double v = Math.Pow(n * (n + 1) / 2.0, 2);
            return Num(6, "Sommes", $"1³ + 2³ + 3³ + … + {n}³", v,
                $"La somme des cubes vaut (1 + 2 + … + n)² = {n * (n + 1) / 2}² = {Answers.Format(v)}.");
        },
        r =>
        {
            int k = r.Next(2, 8);
            return Num(6, "Intégrales", $"∫ de 1 à e^{k} de 1/x dx", k,
                $"Une primitive de 1/x est ln(x) : ln(e^{k}) − ln(1) = {k} − 0 = {k}.");
        },
        r =>
        {
            return Num(6, "Intégrales", "∫ de 0 à 1 de x·e^x dx", 1,
                "Par parties : [x e^x] − ∫ e^x = e − (e − 1) = 1.");
        },
        r =>
        {
            var (expression, top, bottom, why) = Pick(r,
                ("sin(x) / x", 1, 1, "C'est la limite de référence : sin(x)/x → 1."),
                ("(e^x − 1) / x", 1, 1, "Le taux d'accroissement de exp en 0 vaut exp′(0) = 1."),
                ("ln(1 + x) / x", 1, 1, "Le taux d'accroissement de ln(1+x) en 0 vaut 1."),
                ("(1 − cos(x)) / x²", 1, 2, "1 − cos(x) ≈ x²/2, donc la limite vaut 1/2."),
                ("tan(x) / x", 1, 1, "tan(x) ≈ x au voisinage de 0."));
            return Frac(6, "Limites", $"Limite en 0 de {expression}", top, bottom, why);
        },
        r =>
        {
            int k = r.Next(2, 6);
            return Num(6, "Limites", $"Limite en +∞ de x^{k} / e^x", 0,
                "L'exponentielle l'emporte sur toute puissance : la limite vaut 0.");
        },
        r =>
        {
            int ax = NonZero(r, 7), ay = NonZero(r, 7), bx = NonZero(r, 7), by = NonZero(r, 7);
            return Num(6, "Vecteurs", $"Produit scalaire de u({Nb(ax)} ; {Nb(ay)}) et v({Nb(bx)} ; {Nb(by)})", ax * bx + ay * by,
                $"u·v = {ax}×{bx} + {ay}×{by} = {ax * bx} + {ay * by} = {ax * bx + ay * by}.");
        },
        r =>
        {
            var (x, y, z, n) = Pick(r, (1, 2, 2, 3), (2, 3, 6, 7), (1, 4, 8, 9), (2, 6, 9, 11), (4, 4, 7, 9), (3, 4, 12, 13));
            return Num(6, "Espace", $"Norme du vecteur u({x} ; {y} ; {z})", n,
                $"‖u‖ = √({x}² + {y}² + {z}²) = √{x * x + y * y + z * z} = {n}.");
        },
        r =>
        {
            int a = Pick(r, 2, 3, 7, 8, 12, 13), k = r.Next(5, 30), n = Pick(r, 5, 7, 9, 10, 11);
            long v = PowMod(a, k, n);
            return Num(6, "Congruences", $"Quel est le reste de {a}^{k} dans la division par {n} ?", v,
                $"Les puissances de {a} modulo {n} tournent en boucle : on retombe sur {v}.");
        },
        r =>
        {
            int value = r.Next(1000, 99999);
            int digits = value.ToString().Sum(c => c - '0');
            return Num(6, "Congruences", $"Quel est le reste de {value} modulo 9 ?", value % 9,
                $"Un nombre est congru à la somme de ses chiffres modulo 9 : elle vaut {digits}, donc le reste est {value % 9}.");
        },
        r =>
        {
            int n = Pick(r, 7, 9, 11, 13, 16);
            int[] units = Units(n);
            int a = units[r.Next(units.Length)], x = r.Next(1, n);
            return Num(6, "Congruences", $"Résous {a}x ≡ {a * x % n} [{n}], avec x entre 0 et {n - 1}.", x,
                $"{a} est inversible modulo {n} : son inverse est {ModInverse(a, n)}, donc x ≡ {ModInverse(a, n)} × {a * x % n} ≡ {x} [{n}].");
        },
        r =>
        {
            int n = r.Next(7, 40), q = r.Next(40, 900);
            int rest = r.Next(0, n);
            return Num(6, "Arithmétique", $"Quel est le reste de la division euclidienne de {n * q + rest} par {n} ?",
                rest, $"{n * q + rest} = {n} × {q} + {rest}, et 0 ≤ {rest} < {n}.");
        },
        r =>
        {
            int q = r.Next(2, 8);
            return Frac(6, "Séries", $"Somme de la série 1 + 1/{q} + 1/{q}² + 1/{q}³ + …", q, q - 1,
                $"Série géométrique de raison 1/{q} : la somme vaut 1/(1 − 1/{q}) = {q}/{q - 1}.");
        },
        r =>
        {
            int a = r.Next(2, 12);
            return Num(6, "Récurrence", $"u₀ = {r.Next(1, 20)} et uₙ₊₁ = (uₙ + {a}) / 2. Vers quelle limite la suite converge-t-elle ?",
                a, $"La limite ℓ vérifie ℓ = (ℓ + {a})/2, donc ℓ = {a}.");
        },
        r =>
        {
            return Num(6, "Probabilités", "On lance deux dés équilibrés. Quelle est l'espérance de la somme obtenue ?", 7,
                "Chaque dé a pour espérance 3,5, et les espérances s'ajoutent : 3,5 + 3,5 = 7.");
        }
    };

    // --- Niveau 7 : prépa ---------------------------------------------------------

    private static Func<Random, Question>[] Level7() => new Func<Random, Question>[]
    {
        r =>
        {
            int n = r.Next(2, 7);
            return Frac(7, "Développements limités", $"Quel est le coefficient de x^{n} dans le développement de e^x en 0 ?",
                1, (int)Factorial(n), $"e^x = Σ xⁿ/n!, donc le coefficient de x^{n} vaut 1/{n}! = 1/{Factorial(n)}.");
        },
        r =>
        {
            var (f, top, bottom, why) = Pick(r,
                ("cos(x)", -1, 2, "cos(x) = 1 − x²/2 + …"),
                ("ch(x)", 1, 2, "ch(x) = 1 + x²/2 + …"),
                ("ln(1 + x)", -1, 2, "ln(1 + x) = x − x²/2 + …"),
                ("(1 + x)^(1/2)", -1, 8, "√(1 + x) = 1 + x/2 − x²/8 + …"));
            return Frac(7, "Développements limités", $"Quel est le coefficient de x² dans le développement de {f} en 0 ?",
                top, bottom, why);
        },
        r =>
        {
            int q = r.Next(2, 8);
            return Frac(7, "Séries", $"Somme de la série alternée 1 − 1/{q} + 1/{q}² − 1/{q}³ + …", q, q + 1,
                $"Série géométrique de raison −1/{q} : la somme vaut 1/(1 + 1/{q}) = {q}/{q + 1}.");
        },
        r =>
        {
            return Num(7, "Séries", "Quel est le plus petit entier α pour lequel la série de terme 1/n^α converge ?", 2,
                "La série de Riemann converge si et seulement si α > 1 : le plus petit entier est 2.");
        },
        r =>
        {
            int k = r.Next(2, 9);
            return Num(7, "Séries", $"Quel est le rayon de convergence de la série entière Σ xⁿ / {k}ⁿ ?", k,
                $"Le terme général se comporte comme (x/{k})ⁿ : le rayon vaut {k}.");
        },
        r =>
        {
            return Num(7, "Intégrales", "∫ de 1 à e de ln(x) dx", 1,
                "Par parties : [x ln x − x] de 1 à e = (e − e) − (0 − 1) = 1.");
        },
        r =>
        {
            int a = r.Next(2, 6), b = r.Next(2, 6), c = r.Next(2, 6);
            return Num(7, "Algèbre linéaire",
                $"Déterminant de la matrice triangulaire [ {a} 1 2 ; 0 {b} 3 ; 0 0 {c} ]", a * b * c,
                $"Pour une matrice triangulaire, le déterminant est le produit de la diagonale : {a} × {b} × {c} = {a * b * c}.");
        },
        r =>
        {
            int l1 = r.Next(2, 8), l2 = r.Next(2, 8), k = r.Next(2, 5);
            return Num(7, "Algèbre linéaire", $"Quel est le rang de la matrice [ {l1} {l2} ; {l1 * k} {l2 * k} ] ?", 1,
                $"La deuxième ligne est {k} fois la première : la matrice n'a qu'une ligne indépendante.");
        },
        r =>
        {
            int n = r.Next(2, 7);
            return Num(7, "Algèbre linéaire", $"Quelle est la dimension de l'espace des matrices carrées de taille {n} ?",
                n * n, $"Une base est donnée par les matrices élémentaires : il y en a {n} × {n} = {n * n}.");
        },
        r =>
        {
            int n = r.Next(2, 9);
            return Num(7, "Algèbre linéaire",
                $"Quelle est la dimension de l'espace des polynômes de degré inférieur ou égal à {n} ?", n + 1,
                $"La base est 1, X, …, X^{n} : elle compte {n + 1} vecteurs.");
        },
        r =>
        {
            int n = r.Next(4, 9), rank = r.Next(1, n);
            return Num(7, "Algèbre linéaire",
                $"Une application linéaire de R^{n} dans R^{n} est de rang {rank}. Quelle est la dimension de son noyau ?",
                n - rank, $"Théorème du rang : dim ker = {n} − {rank} = {n - rank}.");
        },
        r =>
        {
            int l1 = r.Next(1, 7), l2 = l1 + r.Next(1, 6);
            return Num(7, "Valeurs propres",
                $"Une matrice 2×2 a pour trace {l1 + l2} et pour déterminant {l1 * l2}. Quelle est sa plus grande valeur propre ?",
                l2, $"Les valeurs propres ont pour somme {l1 + l2} et pour produit {l1 * l2} : ce sont {l1} et {l2}.");
        },
        r =>
        {
            int n = Pick(r, 12, 15, 18, 20, 21, 24, 25, 27, 30, 32, 35, 36);
            return Num(7, "Arithmétique", $"Combien vaut l'indicatrice d'Euler φ({n}) ?", Totient(n),
                $"φ({n}) compte les entiers de 1 à {n} premiers avec {n} : il y en a {Totient(n)}.");
        },
        r =>
        {
            int p = Pick(r, 5, 7, 11, 13), a = r.Next(2, p);
            return Num(7, "Congruences", $"D'après le petit théorème de Fermat, que vaut {a}^{p - 1} modulo {p} ?", 1,
                $"{p} est premier et ne divise pas {a}, donc {a}^{p - 1} ≡ 1 [{p}].");
        },
        r =>
        {
            // m et n doivent être premiers entre eux, sinon le système n'a pas toujours de solution
            int m = Pick(r, 3, 4, 5), n = Pick(r, 7, 11, 13);
            int a = r.Next(0, m), b = r.Next(0, n);
            int x = 0;
            while (x < m * n && (x % m != a || x % n != b)) x++;
            return Num(7, "Congruences",
                $"Quel est le plus petit entier naturel x tel que x ≡ {a} [{m}] et x ≡ {b} [{n}] ?", x,
                $"{m} et {n} sont premiers entre eux : le théorème chinois donne une unique solution modulo {m * n}, ici {x}.");
        },
        r =>
        {
            var (n, v) = Pick(r, (3, 2), (4, 9), (5, 44), (6, 265), (7, 1854));
            return Num(7, "Dénombrement",
                $"Combien y a-t-il de dérangements de {n} objets (permutations sans point fixe) ?", v,
                $"!{n} = {n}! × Σ (−1)^k/k! = {v}.");
        },
        r =>
        {
            int n = Pick(r, 7, 9, 11, 13, 16, 17);
            int[] units = Units(n);
            int a = units[r.Next(units.Length)];
            int inverse = ModInverse(a, n);
            return Num(7, "Congruences", $"Quel est l'inverse de {a} modulo {n}, pris entre 1 et {n - 1} ?", inverse,
                $"{a} × {inverse} = {a * inverse} = {n} × {a * inverse / n} + 1, donc {a} × {inverse} ≡ 1 [{n}].");
        },
        r =>
        {
            int p = Pick(r, 7, 11, 13), a = r.Next(2, p);
            int order = MultiplicativeOrder(a, p);
            return Num(7, "Congruences", $"Quel est le plus petit entier k > 0 tel que {a}^k ≡ 1 [{p}] ?", order,
                $"On déroule les puissances de {a} modulo {p} : on retombe sur 1 au rang {order}. Cet ordre divise toujours {p - 1}.");
        },
        r =>
        {
            int k = r.Next(2, 11);
            return Num(7, "Probabilités",
                $"X suit une loi géométrique de paramètre p = 1/{k}. Quelle est son espérance ?", k,
                $"E(X) = 1/p = {k}.");
        },
        r =>
        {
            int k = r.Next(2, 9), c = r.Next(2, 9);
            return Num(7, "Équations différentielles",
                $"Les solutions de y′ = {k}y valant {c} en 0 s'écrivent y(x) = {c}·e^(ax). Combien vaut a ?", k,
                $"y′ = ky a pour solutions les C·e^(kx), donc a = {k}.");
        },
        r =>
        {
            int n = r.Next(8, 16);
            long a = 0, b = 1;
            for (int i = 1; i < n; i++) (a, b) = (b, a + b);
            return Num(7, "Récurrence",
                $"u₀ = 0, u₁ = 1 et uₙ₊₂ = uₙ₊₁ + uₙ. Combien vaut u{Sub(n)} ?", b,
                $"On déroule la suite de Fibonacci : u{Sub(n)} = {b}.");
        },
        r =>
        {
            int n = r.Next(2, 9);
            return Num(7, "Complexes", $"Combien vaut la somme des {n} racines {n}-ièmes de l'unité ?", 0,
                $"Ce sont les racines de zⁿ − 1 : leur somme vaut −(coefficient de z^{n - 1}) = 0.");
        },
        r =>
        {
            int x = NonZero(r, 9), y = NonZero(r, 9), z = NonZero(r, 9);
            return Num(7, "Espace", $"Norme 1 du vecteur u({Nb(x)} ; {Nb(y)} ; {Nb(z)}), c'est-à-dire |x| + |y| + |z|",
                Math.Abs(x) + Math.Abs(y) + Math.Abs(z),
                $"|{x}| + |{y}| + |{z}| = {Math.Abs(x)} + {Math.Abs(y)} + {Math.Abs(z)} = {Math.Abs(x) + Math.Abs(y) + Math.Abs(z)}.");
        },
        r =>
        {
            int x = r.Next(1, 8), y = r.Next(1, 8), a = r.Next(1, 5), b = r.Next(1, 5), c = r.Next(1, 5), d = r.Next(1, 5);
            while (a * d - b * c == 0) d = r.Next(1, 6);
            // "1x" ne s'écrit pas : on laisse tomber le coefficient quand il vaut 1
            string first = $"{Coef(a)}x + {Coef(b)}y", second = $"{Coef(c)}x + {Coef(d)}y";
            return Num(7, "Algèbre linéaire",
                $"Résous par Cramer : {first} = {a * x + b * y} et {second} = {c * x + d * y}. Combien vaut x ?", x,
                $"det = {a}×{d} − {b}×{c} = {a * d - b * c}, et x = {(a * x + b * y) * d - b * (c * x + d * y)} ÷ {a * d - b * c} = {x}.");
        },
        r =>
        {
            return Num(7, "Limites", "Limite en +∞ de n^(1/n)", 1,
                "n^(1/n) = e^(ln n / n), et ln n / n → 0, donc la limite vaut 1.");
        },
        r =>
        {
            int n = r.Next(3, 9);
            return Num(7, "Sommes", $"Combien vaut la somme des coefficients binomiaux C({n} ; 0) + … + C({n} ; {n}) ?",
                Math.Pow(2, n), $"La formule du binôme avec x = 1 donne 2^{n} = {(int)Math.Pow(2, n)}.");
        }
    };

    // --- Niveau 8 : agrégation ----------------------------------------------------

    private static Func<Random, Question>[] Level8() => new Func<Random, Question>[]
    {
        r =>
        {
            int n = r.Next(3, 9);
            return Num(8, "Groupes", $"Combien d'éléments compte le groupe symétrique S{Sub(n)} ?", Factorial(n),
                $"|S{Sub(n)}| = {n}! = {Factorial(n)}.");
        },
        r =>
        {
            int n = r.Next(4, 9);
            return Num(8, "Groupes", $"Combien de permutations paires compte S{Sub(n)} ?", Factorial(n) / 2,
                $"Le groupe alterné est d'indice 2 : {n}!/2 = {Factorial(n) / 2}.");
        },
        r =>
        {
            int n = Pick(r, 8, 10, 12, 14, 15, 16, 18, 20, 24), k = r.Next(2, n);
            return Num(8, "Groupes", $"Dans Z/{n}Z, quel est l'ordre de l'élément {k} ?", n / Gcd(n, k),
                $"L'ordre vaut n / pgcd(n ; k) = {n} ÷ {Gcd(n, k)} = {n / Gcd(n, k)}.");
        },
        r =>
        {
            int n = Pick(r, 12, 16, 18, 20, 24, 28, 30, 36);
            return Num(8, "Groupes", $"Combien de sous-groupes le groupe cyclique Z/{n}Z possède-t-il ?", DivisorCount(n),
                $"Un cyclique a exactement un sous-groupe par diviseur de son ordre : {n} a {DivisorCount(n)} diviseurs.");
        },
        r =>
        {
            int n = Pick(r, 9, 14, 15, 16, 18, 21, 22, 25, 26, 27);
            return Num(8, "Groupes", $"Combien de générateurs le groupe cyclique Z/{n}Z possède-t-il ?", Totient(n),
                $"Les générateurs sont les classes inversibles : il y en a φ({n}) = {Totient(n)}.");
        },
        r =>
        {
            int n = r.Next(3, 13);
            return Num(8, "Groupes",
                $"Quel est l'ordre du groupe diédral D{Sub(n)}, groupe des isométries du polygone régulier à {n} côtés ?",
                2 * n, $"{n} rotations et {n} réflexions : |D{Sub(n)}| = 2 × {n} = {2 * n}.");
        },
        r =>
        {
            int g = Pick(r, 24, 36, 48, 60, 72, 120), h = Pick(r, 2, 3, 4, 6, 12);
            return Num(8, "Groupes", $"|G| = {g} et |H| = {h}. Quel est l'indice [G : H] ?", g / h,
                $"Le théorème de Lagrange donne [G : H] = |G| / |H| = {g} ÷ {h} = {g / h}.");
        },
        r =>
        {
            int n = Pick(r, 15, 21, 26, 33, 35, 38, 39, 46, 51, 55);
            return Num(8, "Arithmétique", $"Combien l'anneau Z/{n}Z compte-t-il d'éléments inversibles ?", Totient(n),
                $"Ce sont les classes premières avec {n} : φ({n}) = {Totient(n)}.");
        },
        r =>
        {
            int n = Pick(r, 36, 48, 60, 72, 90, 96, 100, 120, 144, 180);
            return Num(8, "Arithmétique", $"Combien {n} a-t-il de diviseurs positifs ?", DivisorCount(n),
                $"En décomposant {n} en facteurs premiers puis en multipliant les exposants augmentés de 1 : {DivisorCount(n)}.");
        },
        r =>
        {
            int n = Pick(r, 12, 18, 20, 24, 28, 30, 36, 40);
            int sigma = Enumerable.Range(1, n).Where(d => n % d == 0).Sum();
            return Num(8, "Arithmétique", $"Combien vaut la somme des diviseurs positifs de {n} ?", sigma,
                $"σ({n}) = {string.Join(" + ", Enumerable.Range(1, n).Where(d => n % d == 0))} = {sigma}.");
        },
        r =>
        {
            int p = Pick(r, 2, 3, 5, 7), n = r.Next(2, 5);
            return Num(8, "Corps finis",
                $"Combien de vecteurs compte un espace vectoriel de dimension {n} sur le corps F{Sub(p)} ?",
                Math.Pow(p, n), $"Chaque coordonnée prend {p} valeurs : {p}^{n} = {(int)Math.Pow(p, n)}.");
        },
        r =>
        {
            int k = r.Next(2, 6), n = r.Next(2, 5), det = r.Next(2, 8);
            double v = Math.Pow(k, n) * det;
            return Num(8, "Matrices",
                $"A est de taille {n}×{n} et det(A) = {det}. Combien vaut det({k}A) ?", v,
                $"det(kA) = kⁿ det(A) = {k}^{n} × {det} = {Answers.Format(v)}.");
        },
        r =>
        {
            int n = r.Next(2, 7);
            return Num(8, "Matrices", $"Quel est le degré du polynôme caractéristique d'une matrice de taille {n}×{n} ?",
                n, $"Le polynôme caractéristique det(A − XI) est de degré {n}.");
        },
        r =>
        {
            int n = r.Next(3, 10);
            return Num(8, "Algèbre linéaire",
                $"Combien de racines complexes, comptées avec leur multiplicité, un polynôme de degré {n} admet-il ?", n,
                $"Le théorème de d'Alembert-Gauss en donne exactement {n}.");
        },
        r =>
        {
            var (n, v) = Pick(r, (2, 2), (3, 5), (4, 14), (5, 42), (6, 132), (7, 429));
            return Num(8, "Dénombrement",
                $"Le nombre de Catalan C{Sub(n)} vaut C({2 * n} ; {n}) / {n + 1}. Combien fait-il ?", v,
                $"C({2 * n} ; {n}) = {Binomial(2 * n, n)}, divisé par {n + 1} : {v}.");
        },
        r =>
        {
            int k = r.Next(2, 6), n = r.Next(2, 6);
            return Num(8, "Dénombrement",
                $"Combien y a-t-il d'applications d'un ensemble à {k} éléments vers un ensemble à {n} éléments ?",
                Math.Pow(n, k), $"Chaque élément de départ a {n} images possibles : {n}^{k} = {(int)Math.Pow(n, k)}.");
        },
        r =>
        {
            var (s, power, m, why) = Pick(r,
                ("ζ(2) = 1 + 1/4 + 1/9 + …", "π²", 6, "ζ(2) = π²/6 : c'est le problème de Bâle, résolu par Euler."),
                ("ζ(4) = 1 + 1/16 + 1/81 + …", "π⁴", 90, "ζ(4) = π⁴/90."));
            return Num(8, "Séries", $"{s} s'écrit {power}/m avec m entier. Combien vaut m ?", m, why);
        },
        r =>
        {
            return Num(8, "Analyse complexe",
                "L'intégrale de 1/z sur le cercle unité parcouru une fois vaut k·iπ. Combien vaut k ?", 2,
                "Le théorème des résidus donne 2iπ × Rés(1/z ; 0) = 2iπ.");
        },
        r =>
        {
            int a = NonZero(r, 6), b = NonZero(r, 9);
            return Num(8, "Analyse complexe",
                $"Quel est le résidu en {Nb(a)} de la fonction (z {Signed(b)}) / (z {Signed(-a)}) ?", a + b,
                $"Le résidu d'un pôle simple vaut le numérateur pris en {a} : {a} {Signed(b)} = {a + b}.");
        },
        r =>
        {
            int c = r.Next(2, 9);
            return Num(8, "Fourier",
                $"f vaut {c} sur ]0 ; π[ et −{c} sur ]π ; 2π[. Combien vaut son coefficient a₀ (valeur moyenne) ?", 0,
                "La fonction est impaire autour de π : sa moyenne sur une période est nulle.");
        },
        r =>
        {
            int n = r.Next(1, 7);
            return Num(8, "Transformées",
                $"La transformée de Laplace de t^{n} s'écrit {n}! / s^k. Combien vaut k ?", n + 1,
                $"L(t^n) = n!/s^(n+1), donc k = {n} + 1 = {n + 1}.");
        },
        r =>
        {
            return Num(8, "Probabilités",
                "Pour une loi normale, quelle est, en % arrondi à l'unité, la probabilité que X s'écarte de moins de deux écarts-types de sa moyenne ?",
                95, "La règle des trois sigmas : 68 % à un écart-type, 95 % à deux, 99,7 % à trois.");
        },
        r =>
        {
            int a = r.Next(1, 9), b = r.Next(1, 9);
            return Frac(8, "Markov",
                $"Une chaîne à deux états passe de A à B avec la probabilité 0,{a} et de B à A avec la probabilité 0,{b}. Quelle est la proportion stationnaire de l'état A ?",
                b, a + b, $"π_A = b / (a + b) = 0,{b} ÷ (0,{a} + 0,{b}) = {b}/{a + b}.");
        },
        r =>
        {
            int n = r.Next(4, 10), rank = r.Next(1, n);
            return Num(8, "Algèbre linéaire",
                $"u va de R^{n} dans R^{n + 2} et dim ker u = {n - rank}. Quel est le rang de u ?", rank,
                $"Théorème du rang : rang = {n} − {n - rank} = {rank}.");
        },
        r =>
        {
            int p = Pick(r, 5, 7, 11, 13, 17);
            return Num(8, "Congruences", $"D'après le théorème de Wilson, que vaut ({p} − 1)! modulo {p} ?", p - 1,
                $"Wilson : (p − 1)! ≡ −1 [p] dès que p est premier, et −1 vaut {p - 1} modulo {p}.");
        },
        r =>
        {
            int m = Pick(r, 3, 4, 5), n = Pick(r, 7, 11), q = Pick(r, 13, 17);
            int a = r.Next(0, m), b = r.Next(0, n), c = r.Next(0, q);
            int x = SolveCongruences((a, m), (b, n), (c, q));
            return Num(8, "Congruences",
                $"Plus petit entier naturel x tel que x ≡ {a} [{m}], x ≡ {b} [{n}] et x ≡ {c} [{q}] ?", x,
                $"Les trois modules sont premiers entre eux deux à deux : le théorème chinois donne une unique solution modulo {m * n * q}, ici {x}.");
        },
        r =>
        {
            var (p, q) = Pick(r, (3, 5), (3, 7), (5, 7), (3, 11), (5, 11));
            int phi = (p - 1) * (q - 1);
            int[] exponents = Units(phi);
            int e = exponents[r.Next(exponents.Length)];
            int d = ModInverse(e, phi);
            return Num(8, "Congruences",
                $"Clé RSA : n = {p} × {q} = {p * q}, donc φ(n) = {phi}. Quel exposant privé d vérifie {e}d ≡ 1 [{phi}], avec 0 < d < {phi} ?",
                d, $"d est l'inverse de {e} modulo {phi} : {e} × {d} = {e * d} ≡ 1 [{phi}].");
        }
    };
}
