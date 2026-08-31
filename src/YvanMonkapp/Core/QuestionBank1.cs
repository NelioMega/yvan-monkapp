namespace YvanMonkapp.Core;

/// <summary>Les modèles d'énoncés du calcul mental au brevet (niveaux 1 à 3).</summary>
public static partial class QuestionGenerator
{
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
            int a = Pick(r, 11, 12, 15, 25), b = r.Next(3, 10);
            return Num(1, "Tables", $"{a} × {b}", a * b, $"{a} × {b} = {a * b}. Celle-là, il faut la connaître par cœur.");
        },
        r =>
        {
            int n = r.Next(23, 250);
            return Num(1, "Double", $"Le double de {n}", n * 2, $"Doubler, c'est multiplier par 2 : {n} × 2 = {n * 2}.");
        },
        r =>
        {
            int n = r.Next(12, 90);
            return Num(1, "Double", $"Le triple de {n}", n * 3, $"{n} × 3 = {n * 2} + {n} = {n * 3}.");
        },
        r =>
        {
            int half = r.Next(17, 140);
            return Num(1, "Moitié", $"La moitié de {half * 2}", half, $"{half * 2} ÷ 2 = {half}.");
        },
        r =>
        {
            int quarter = r.Next(6, 45);
            return Num(1, "Quart", $"Le quart de {quarter * 4}", quarter,
                $"On coupe deux fois en deux : {quarter * 4} → {quarter * 2} → {quarter}.");
        },
        r =>
        {
            int n = r.Next(11, 96);
            return Num(1, "Complément", $"Combien manque-t-il à {n} pour aller jusqu'à 100 ?", 100 - n,
                $"100 − {n} = {100 - n}. Le complément à 100, c'est un réflexe.");
        },
        r =>
        {
            int n = r.Next(11, 99) * 10;
            return Num(1, "Complément", $"Combien manque-t-il à {n} pour aller jusqu'à 1000 ?", 1000 - n,
                $"1000 − {n} = {1000 - n}.");
        },
        r =>
        {
            int a = r.Next(5, 40), b = r.Next(5, 40), c = r.Next(5, 40);
            return Num(1, "Somme", $"{a} + {b} + {c}", a + b + c, $"On regroupe : {a} + {b} = {a + b}, puis + {c} = {a + b + c}.");
        },
        r =>
        {
            int a = r.Next(11, 45), b = r.Next(11, 45);
            return Num(1, "Soustraction", $"100 − {a} − {b}", 100 - a - b,
                $"100 − {a} = {100 - a}, puis {100 - a} − {b} = {100 - a - b}.");
        },
        r =>
        {
            int start = r.Next(2, 12), step = r.Next(2, 9);
            int fourth = start + 3 * step;
            return Num(1, "Suite",
                $"Quel est le terme suivant ? {start} ; {start + step} ; {start + 2 * step} ; {fourth} ; ...",
                fourth + step, $"On ajoute {step} à chaque fois : {fourth} + {step} = {fourth + step}.");
        },
        r =>
        {
            int start = r.Next(2, 9);
            int fourth = start * 8;
            return Num(1, "Suite", $"Quel est le terme suivant ? {start} ; {start * 2} ; {start * 4} ; {fourth} ; ...",
                fourth * 2, $"On double à chaque fois : {fourth} × 2 = {fourth * 2}.");
        },
        r =>
        {
            int b = r.Next(3, 10), q = r.Next(3, 13);
            return Num(1, "Division", $"{b * q} ÷ {b}", q, $"{b} × {q} = {b * q}, donc le quotient vaut {q}.");
        },
        r =>
        {
            int n = r.Next(12, 99), zeros = r.Next(1, 4);
            int factor = (int)Math.Pow(10, zeros);
            return Num(1, "Numération", $"{n} × {factor}", n * factor,
                $"Multiplier par {factor}, c'est ajouter {zeros} zéro(s) : {n * factor}.");
        },
        r =>
        {
            int tens = r.Next(12, 99);
            return Num(1, "Numération", $"Combien y a-t-il de dizaines dans {tens * 10} ?", tens,
                $"{tens * 10} = {tens} × 10, donc {tens} dizaines.");
        },
        r =>
        {
            int a = r.Next(3, 13), q = r.Next(4, 13);
            return Num(1, "Nombre manquant", $"{a} × ? = {a * q}", q, $"On divise : {a * q} ÷ {a} = {q}.");
        },
        r =>
        {
            int a = r.Next(14, 70), sum = a + r.Next(15, 60);
            return Num(1, "Nombre manquant", $"? + {a} = {sum}", sum - a, $"On soustrait : {sum} − {a} = {sum - a}.");
        },
        r =>
        {
            int cents = r.Next(120, 1980);
            double price = cents / 100.0;
            double change = (2000 - cents) / 100.0;
            return Num(1, "Monnaie",
                $"Vous payez avec un billet de 20 € un article à {Answers.Format(price)} €. Combien vous rend-on ?",
                change, $"20 − {Answers.Format(price)} = {Answers.Format(change)} €.");
        },
        r =>
        {
            int hour = r.Next(8, 20), minute = r.Next(0, 12) * 5;
            int span = r.Next(5, 34) * 5;
            int end = hour * 60 + minute + span;
            return Num(1, "Durée",
                $"Combien de minutes s'écoulent entre {hour} h {minute:00} et {end / 60} h {end % 60:00} ?", span,
                $"De {hour} h {minute:00} à {end / 60} h {end % 60:00}, il y a {span} minutes.");
        },
        r =>
        {
            int n = r.Next(3, 13);
            return Num(1, "Tables", $"Combien font {n} douzaines ?", n * 12,
                $"Une douzaine vaut 12 : {n} × 12 = {n * 12}.");
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
            int a = r.Next(3, 10), b = r.Next(4, 20), c = r.Next(3, 15);
            return Num(2, "Priorités", $"{a} × ({b} + {c})", a * (b + c),
                $"La parenthèse d'abord : {b} + {c} = {b + c}, puis {a} × {b + c} = {a * (b + c)}.");
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
            int n = r.Next(3, 10);
            return Num(2, "Puissance", $"{n}³", n * n * n, $"{n}³ = {n} × {n} × {n} = {n * n * n}.");
        },
        r =>
        {
            int t = Pick(r, 10, 20, 25, 50, 75);
            int baseValue = r.Next(2, 26) * 20;
            double result = baseValue * t / 100.0;
            return Num(2, "Pourcentage", $"{t} % de {baseValue}", result,
                $"{t} % c'est {t}/100 : {baseValue} × {t} ÷ 100 = {Answers.Format(result)}.");
        },
        r =>
        {
            int bottom = Pick(r, 2, 3, 4, 5), top = r.Next(1, bottom);
            int n = bottom * r.Next(4, 21);
            return Num(2, "Fraction d'une quantité", $"{top}/{bottom} de {n}", n / bottom * top,
                $"{n} ÷ {bottom} = {n / bottom}, puis × {top} = {n / bottom * top}.");
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
            int b = r.Next(3, 16) * 2, h = r.Next(3, 15);
            return Num(2, "Aire", $"Aire d'un triangle de base {b} cm et de hauteur {h} cm (en cm²)", b * h / 2,
                $"Aire = base × hauteur ÷ 2 = {b} × {h} ÷ 2 = {b * h / 2} cm².");
        },
        r =>
        {
            int side = r.Next(3, 20);
            return Num(2, "Aire", $"Un carré a un périmètre de {4 * side} cm. Quelle est son aire (en cm²) ?", side * side,
                $"Le côté vaut {4 * side} ÷ 4 = {side} cm, donc l'aire vaut {side}² = {side * side} cm².");
        },
        r =>
        {
            int c = r.Next(3, 12);
            return Num(2, "Volume", $"Volume d'un cube de {c} cm d'arête (en cm³)", c * c * c,
                $"V = côté³ = {c}³ = {c * c * c} cm³.");
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
            int grams = r.Next(11, 99) * 100;
            return Num(2, "Conversion", $"{grams} g en kilogrammes", grams / 1000.0,
                $"1 kg = 1 000 g, donc {grams} ÷ 1 000 = {Answers.Format(grams / 1000.0)} kg.");
        },
        r =>
        {
            double litres = r.Next(11, 99) / 10.0;
            return Num(2, "Conversion", $"{Answers.Format(litres)} L en centilitres", litres * 100,
                $"1 L = 100 cL, donc {Answers.Format(litres)} × 100 = {Answers.Format(litres * 100)} cL.");
        },
        r =>
        {
            int a = r.Next(4, 30), b = r.Next(4, 30);
            return Num(2, "Relatifs", $"(−{a}) + {b}", b - a, $"On retire {a} à {b} : le résultat vaut {b - a}.");
        },
        r =>
        {
            int a = r.Next(3, 13), b = r.Next(3, 13);
            return Num(2, "Relatifs", $"(−{a}) × (−{b})", a * b, $"Moins par moins donne plus : {a} × {b} = {a * b}.");
        },
        r =>
        {
            int a = r.Next(2, 10), b = r.Next(2, 10);
            double v = a * b / 100.0;
            return Num(2, "Décimaux", $"{Answers.Format(a / 10.0)} × {Answers.Format(b / 10.0)}", v,
                $"{a} × {b} = {a * b}, et il y a deux chiffres après la virgule en tout : {Answers.Format(v)}.");
        },
        r =>
        {
            int q = r.Next(2, 10), d = r.Next(2, 10);
            return Num(2, "Décimaux", $"{Answers.Format(q * d / 10.0)} ÷ {Answers.Format(d / 10.0)}", q,
                $"On multiplie les deux par 10 : {q * d} ÷ {d} = {q}.");
        },
        r =>
        {
            int a = r.Next(20, 90), b = r.Next(20, 90);
            while (a + b > 160) b = r.Next(20, 90);
            int third = 180 - a - b;
            return Num(2, "Angles", $"Dans un triangle, deux angles mesurent {a}° et {b}°. Combien mesure le troisième ?",
                third, $"180 − {a} − {b} = {third}°.");
        },
        r =>
        {
            int v = Pick(r, 40, 50, 60, 80, 90, 100, 120), t = r.Next(2, 7);
            return Num(2, "Vitesse", $"Une voiture roule à {v} km/h pendant {t} h. Quelle distance parcourt-elle (en km) ?",
                v * t, $"distance = vitesse × temps = {v} × {t} = {v * t} km.");
        },
        r =>
        {
            int a = r.Next(2, 10), b = r.Next(2, 10);
            while (b == a) b = r.Next(2, 10);
            return Num(2, "Multiples", $"Quel est le plus petit multiple commun à {a} et {b} ?", Lcm(a, b),
                $"Les multiples de {a} et de {b} se rejoignent pour la première fois en {Lcm(a, b)}.");
        },
        r =>
        {
            int price = r.Next(2, 20), qty = r.Next(3, 13);
            return Num(2, "Proportionnalité", $"{qty} articles à {price} € l'unité : combien coûte le lot ?", price * qty,
                $"{qty} × {price} = {price * qty} €.");
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
            int a = r.Next(6, 25), b = r.Next(6, 25);
            while (b == a) b = r.Next(6, 25);
            return Num(3, "Multiples", $"PPCM({a} ; {b})", Lcm(a, b),
                $"PPCM = produit ÷ PGCD = {a * b} ÷ {Gcd(a, b)} = {Lcm(a, b)}.");
        },
        r =>
        {
            int k = r.Next(2, 10), p = r.Next(2, 9), q = r.Next(2, 9);
            while (Gcd(p, q) != 1 || p == q) { p = r.Next(2, 9); q = r.Next(3, 11); }
            return Frac(3, "Fractions", $"Simplifie la fraction {p * k}/{q * k}", p, q,
                $"On divise en haut et en bas par {k} : {p * k}/{q * k} = {p}/{q}.");
        },
        r =>
        {
            int b = r.Next(2, 7), d = r.Next(2, 7), a = r.Next(1, 6), c = r.Next(1, 6);
            return Frac(3, "Fractions", $"{a}/{b} + {c}/{d} (fraction irréductible)", a * d + c * b, b * d,
                $"Même dénominateur {b * d} : {a * d}/{b * d} + {c * b}/{b * d} = {a * d + c * b}/{b * d}, à simplifier.");
        },
        r =>
        {
            int b = r.Next(2, 8), d = r.Next(2, 8), a = r.Next(1, 7), c = r.Next(1, 7);
            return Frac(3, "Fractions", $"{a}/{b} × {c}/{d} (fraction irréductible)", a * c, b * d,
                $"On multiplie en ligne : {a * c}/{b * d}, puis on simplifie.");
        },
        r =>
        {
            int bottom = Pick(r, 2, 4, 5, 8, 10, 20, 25), top = r.Next(1, bottom);
            return Num(3, "Fractions", $"Écris {top}/{bottom} sous forme décimale", (double)top / bottom,
                $"{top} ÷ {bottom} = {Answers.Format((double)top / bottom)}.");
        },
        r =>
        {
            int a = r.Next(2, 10), x = r.Next(-9, 12), b = NonZero(r, 15);
            int c = a * x + b;
            return Num(3, "Équation", $"Résous : {a}x {Signed(b)} = {Nb(c)}", x,
                $"{a}x = {c} {Signed(-b)} = {c - b}, donc x = {c - b} ÷ {a} = {x}.");
        },
        r =>
        {
            int c = r.Next(2, 4), a = c + r.Next(2, 7), x = r.Next(-6, 9), b = NonZero(r, 12);
            int d = (a - c) * x + b;
            // un second membre nul écrirait "+ 0" au tableau
            while (d == 0) { b = NonZero(r, 12); d = (a - c) * x + b; }
            return Num(3, "Équation", $"Résous : {a}x {Signed(b)} = {c}x {Signed(d)}", x,
                $"On rassemble les x : {a - c}x = {d} {Signed(-b)} = {d - b}, donc x = {x}.");
        },
        r =>
        {
            int a = r.Next(2, 8), x = r.Next(1, 10), b = r.Next(1, 16);
            int c = a * x + b + r.Next(1, a);
            return Num(3, "Inéquation", $"Quel est le plus grand entier x tel que {a}x + {b} < {c} ?", x,
                $"{a}x < {c - b}, donc x < {Answers.Format((c - b) / (double)a)} : le plus grand entier est {x}.");
        },
        r =>
        {
            var (p, q, h) = Pick(r, (3, 4, 5), (6, 8, 10), (5, 12, 13), (9, 12, 15), (8, 15, 17), (7, 24, 25), (20, 21, 29));
            return Num(3, "Pythagore", $"Triangle rectangle de côtés {p} et {q} : quelle est l'hypoténuse ?", h,
                $"{p}² + {q}² = {p * p} + {q * q} = {h * h}, et √{h * h} = {h}.");
        },
        r =>
        {
            var (p, q, h) = Pick(r, (3, 4, 5), (6, 8, 10), (5, 12, 13), (9, 12, 15), (8, 15, 17), (7, 24, 25), (20, 21, 29));
            return Num(3, "Pythagore",
                $"Triangle rectangle d'hypoténuse {h} et de côté {p} : combien mesure l'autre côté ?", q,
                $"{h}² − {p}² = {h * h} − {p * p} = {q * q}, et √{q * q} = {q}.");
        },
        r =>
        {
            int am = r.Next(2, 9), an = r.Next(2, 9), k = r.Next(2, 5);
            return Num(3, "Thalès",
                $"(MN) est parallèle à (BC). AM = {am}, AB = {am * k}, AN = {an}. Combien vaut AC ?", an * k,
                $"AB/AM = {k}, donc AC = {an} × {k} = {an * k}.");
        },
        r =>
        {
            int n = r.Next(1, 5);
            double v = Math.Pow(10, -n);
            return Num(3, "Puissances", $"10^(−{n})", v, $"10^(−{n}) = 1 ÷ 10^{n} = {Answers.Format(v)}.");
        },
        r =>
        {
            int a = r.Next(2, 8), m = r.Next(2, 7), n = r.Next(2, 7);
            return Num(3, "Puissances", $"{a}^{m} × {a}^{n} = {a}^k. Combien vaut k ?", m + n,
                $"Les exposants s'ajoutent : {m} + {n} = {m + n}.");
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
            int a = r.Next(1, 8), b = r.Next(1, 8), x = r.Next(1, 7);
            return Num(3, "Développement", $"Développe (x + {a})(x + {b}), puis calcule sa valeur pour x = {x}.",
                (x + a) * (x + b),
                $"(x + {a})(x + {b}) = x² + {a + b}x + {a * b}, et pour x = {x} cela vaut {(x + a) * (x + b)}.");
        },
        r =>
        {
            int n = r.Next(3, 14);
            return Num(3, "Factorisation", $"Résous x² − {n * n} = 0 : quelle est la solution positive ?", n,
                $"x² − {n * n} = (x − {n})(x + {n}), donc x = {n} ou x = −{n}.");
        },
        r =>
        {
            int old = r.Next(2, 13) * 10;
            int t = Pick(r, 10, 20, 25, 50);
            double v = old * (100 - t) / 100.0;
            return Num(3, "Pourcentage", $"Un article à {old} € baisse de {t} %. Quel est le nouveau prix ?", v,
                $"Baisser de {t} %, c'est multiplier par {Answers.Format((100 - t) / 100.0)} : {old} × {Answers.Format((100 - t) / 100.0)} = {Answers.Format(v)} €.");
        },
        r =>
        {
            int a = r.Next(2, 8), b = r.Next(2, 8);
            return Num(3, "Volume", $"Volume d'un pavé de {a} × {b} × {a} cm (en cm³)", a * b * a,
                $"V = L × l × h = {a} × {b} × {a} = {a * b * a} cm³.");
        },
        r =>
        {
            int radius = r.Next(2, 12);
            return Num(3, "Aire", $"Aire d'un disque de rayon {radius} cm, exprimée en multiples de π", radius * radius,
                $"Aire = πr² = π × {radius}² = {radius * radius}π cm².");
        },
        r =>
        {
            int radius = r.Next(2, 8), h = r.Next(3, 12);
            return Num(3, "Volume", $"Volume d'un cylindre de rayon {radius} et de hauteur {h}, en multiples de π",
                radius * radius * h, $"V = πr²h = π × {radius}² × {h} = {radius * radius * h}π.");
        },
        r =>
        {
            int v = Pick(r, 30, 40, 45, 60, 80, 90, 120), t = r.Next(2, 6);
            return Num(3, "Vitesse", $"Un cycliste parcourt {v * t} km en {t} h. Quelle est sa vitesse moyenne (en km/h) ?",
                v, $"vitesse = distance ÷ temps = {v * t} ÷ {t} = {v} km/h.");
        },
        r =>
        {
            int scale = Pick(r, 1000, 2000, 5000, 25000, 50000), cm = r.Next(2, 10);
            double metres = cm * scale / 100.0;
            return Num(3, "Échelle", $"Sur une carte au 1/{scale}, {cm} cm représentent combien de mètres ?", metres,
                $"{cm} × {scale} = {cm * scale} cm, soit {Answers.Format(metres)} m.");
        },
        r =>
        {
            int p = Pick(r, 2, 3, 5, 7), q = Pick(r, 11, 13, 17, 19, 23);
            return Num(3, "Nombres premiers", $"Quel est le plus petit facteur premier de {p * q} ?", p,
                $"{p * q} = {p} × {q}, et {p} est premier.");
        },
        r =>
        {
            int red = r.Next(2, 7), blue = r.Next(2, 7);
            return Frac(3, "Probabilités",
                $"Un sac contient {red} billes rouges et {blue} bleues. Probabilité de tirer une rouge ?",
                red, red + blue, $"p = favorables ÷ total = {red}/{red + blue}.");
        },
        r =>
        {
            var values = Enumerable.Range(0, 5).Select(_ => r.Next(2, 40)).ToArray();
            int median = values.OrderBy(v => v).ElementAt(2);
            return Num(3, "Statistiques", $"Médiane de la série : {string.Join(" ; ", values)}", median,
                $"Rangée, la série donne {string.Join(" ; ", values.OrderBy(v => v))} : la valeur du milieu est {median}.");
        },
        r =>
        {
            int m = r.Next(2, 10);
            return Num(3, "Conversion", $"{m} m² en cm²", m * 10000,
                $"1 m² = 10 000 cm², donc {m} × 10 000 = {m * 10000} cm².");
        },
        r =>
        {
            int n = r.Next(6, 26);
            return Num(3, "Racine carrée", $"√{n * n}", n, $"{n} × {n} = {n * n}, donc √{n * n} = {n}.");
        }
    };
}
