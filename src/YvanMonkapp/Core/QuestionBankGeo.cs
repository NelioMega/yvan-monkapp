namespace YvanMonkapp.Core;

/// <summary>
/// Les énoncés qui viennent avec un dessin : géométrie plane, solides de l'espace, lectures
/// graphiques. Ils vivent à part parce qu'une figure prend dix lignes là où un calcul en
/// prend trois, et parce qu'ils partagent une petite bibliothèque de solides.
/// </summary>
public static partial class QuestionGenerator
{
    // --- Solides et repères réutilisables --------------------------------------------

    /// <summary>
    /// Un pavé droit en perspective cavalière. Les trois arêtes qui partent du sommet
    /// arrière-gauche sont en pointillé : c'est ce qui fait lire un volume plutôt qu'un
    /// enchevêtrement de traits.
    /// </summary>
    private static Figure Pave(int largeur, int hauteur, int profondeur,
        string bas = "", string cote = "", string fuite = "", bool diagonale = false)
    {
        double unit = Math.Clamp(Math.Min(Math.Min(42.0 / Math.Max(largeur, 1), 28.0 / Math.Max(hauteur, 1)),
            20.0 / Math.Max(profondeur, 1)), 1.4, 6);

        double w = largeur * unit, h = hauteur * unit, d = profondeur * unit;
        double dx = d * 0.62, dy = d * 0.55;
        double x0 = 50 - (w + dx) / 2, y0 = 30 - (h + dy) / 2;
        double x1 = x0 + w, y1 = y0 + h;

        var figure = Figure.New(100, 64)
            .Forme(x0, y0, x1, y0, x1, y1, x0, y1)
            .Ligne(x0, y1, x0 + dx, y1 + dy, x1 + dx, y1 + dy, x1, y1)
            .Ligne(x1, y0, x1 + dx, y0 + dy, x1 + dx, y1 + dy)
            .Trait(FigureStyle.Pointille, FigureTint.Estompe,
                x1 + dx, y0 + dy, x0 + dx, y0 + dy, x0 + dx, y1 + dy)
            .Trait(FigureStyle.Pointille, FigureTint.Estompe, x0 + dx, y0 + dy, x0, y0);

        // la grande diagonale joint le coin avant-bas-gauche au coin arrière-haut-droit
        if (diagonale) figure.Trait(FigureStyle.Pointille, FigureTint.Bon, x0, y0, x1 + dx, y1 + dy);

        if (bas.Length > 0) figure.Cote((x0 + x1) / 2, y0 - 5, bas);
        if (cote.Length > 0) figure.Cote(x0 - 6, (y0 + y1) / 2, cote);
        if (fuite.Length > 0) figure.Cote(x1 + dx / 2 + 9, y0 + dy / 2 - 5, fuite);

        return figure;
    }

    /// <summary>Un cylindre vu de trois quarts : disque du dessus plein, fond en pointillé.</summary>
    private static Figure Cylindre(int rayon, int hauteur, string rayonLabel, string hauteurLabel)
    {
        double unit = Math.Clamp(Math.Min(19.0 / Math.Max(rayon, 1), 32.0 / Math.Max(hauteur, 1)), 1.4, 7);
        double r = rayon * unit, h = hauteur * unit;
        double bas = 30 - h / 2, haut = bas + h, ry = r * 0.32;

        return Figure.New(100, 64)
            .Ellipse(50, haut, r, ry)
            .Ellipse(50, bas, r, ry, FigureStyle.Pointille, FigureTint.Estompe)
            .Ligne(50 - r, haut, 50 - r, bas)
            .Ligne(50 + r, haut, 50 + r, bas)
            .Trait(FigureStyle.Pointille, FigureTint.Bois, 50, haut, 50 + r, haut)
            .Cote(50 + r / 2, haut + 6, rayonLabel)
            .Cote(50 - r - 8, (bas + haut) / 2, hauteurLabel);
    }

    /// <summary>Un cône : base elliptique et deux génératrices vers le sommet.</summary>
    private static Figure Cone(int rayon, int hauteur, string rayonLabel, string hauteurLabel)
    {
        double unit = Math.Clamp(Math.Min(19.0 / Math.Max(rayon, 1), 34.0 / Math.Max(hauteur, 1)), 1.4, 7);
        double r = rayon * unit, h = hauteur * unit;
        double bas = 30 - h / 2, sommet = bas + h, ry = r * 0.32;

        double axe = 50 - r - 11;

        return Figure.New(100, 64)
            .Ellipse(50, bas, r, ry)
            .Ligne(50 - r, bas, 50, sommet)
            .Ligne(50 + r, bas, 50, sommet)
            .Trait(FigureStyle.Pointille, FigureTint.Bois, 50, bas, 50 + r, bas)
            .Cote(50 + r / 2, bas - 6, rayonLabel)
            // ligne de cote à l'extérieur : posée sur l'axe, la hauteur serait illisible
            .Trait(FigureStyle.Pointille, FigureTint.Bois, axe, bas, axe, sommet)
            .Trait(FigureStyle.Leger, FigureTint.Estompe, axe, bas, 50 - r, bas)
            .Trait(FigureStyle.Leger, FigureTint.Estompe, axe, sommet, 50, sommet)
            .Cote(axe - 9, (bas + sommet) / 2, hauteurLabel);
    }

    /// <summary>Une sphère : le cercle du contour et l'équateur vu de biais.</summary>
    private static Figure Sphere(string rayonLabel) => Figure.New(100, 64)
        .Cercle(50, 32, 24)
        .Ellipse(50, 32, 24, 7.2, FigureStyle.Pointille, FigureTint.Estompe)
        .Fleche(50, 32, 74, 32, FigureTint.Bois)
        .Point(50, 32)
        .Cote(62, 40, rayonLabel);

    /// <summary>Une pyramide à base carrée, base en perspective et arêtes cachées en pointillé.</summary>
    private static Figure Pyramide(string baseLabel, string hauteurLabel)
    {
        double x0 = 22, y0 = 12, w = 40, dx = 14, dy = 11, sommet = 54;
        double cx = x0 + w / 2 + dx / 2;

        return Figure.New(100, 64)
            .Ligne(x0, y0, x0 + w, y0)
            .Ligne(x0 + w, y0, x0 + w + dx, y0 + dy)
            .Trait(FigureStyle.Pointille, FigureTint.Estompe, x0 + w + dx, y0 + dy, x0 + dx, y0 + dy, x0, y0)
            .Ligne(x0, y0, cx, sommet)
            .Ligne(x0 + w, y0, cx, sommet)
            .Ligne(x0 + w + dx, y0 + dy, cx, sommet)
            .Trait(FigureStyle.Pointille, FigureTint.Estompe, x0 + dx, y0 + dy, cx, sommet)
            .Trait(FigureStyle.Pointille, FigureTint.Bois, cx, y0 + dy / 2, cx, sommet)
            .Cote(x0 + w / 2, y0 - 5, baseLabel)
            // même raison que pour le cône : la hauteur se cote à côté du solide, pas dessus
            .Trait(FigureStyle.Pointille, FigureTint.Bois, x0 - 11, y0, x0 - 11, sommet)
            .Trait(FigureStyle.Leger, FigureTint.Estompe, x0 - 11, y0, x0, y0)
            .Trait(FigureStyle.Leger, FigureTint.Estompe, x0 - 11, sommet, cx, sommet)
            .Cote(x0 - 20, (y0 + sommet) / 2, hauteurLabel);
    }

    /// <summary>Un rectangle coté, dessiné aux vraies proportions.</summary>
    private static Figure RectangleFigure(int L, int l, string dedans)
    {
        double unit = Math.Clamp(Math.Min(58.0 / L, 34.0 / l), 1.2, 5);
        double w = L * unit, h = l * unit;
        double x0 = 50 - w / 2, y0 = 30 - h / 2;

        var figure = Figure.New(100, 60)
            .Forme(x0, y0, x0 + w, y0, x0 + w, y0 + h, x0, y0 + h)
            .Cote(x0 + w / 2, y0 - 5, $"{L} cm")
            .Cote(x0 - 8, y0 + h / 2, $"{l} cm");

        if (dedans.Length > 0) figure.Texte(50, 30, dedans, FigureTint.Bon);
        return figure;
    }

    /// <summary>Un repère gradué : une unité de graphique vaut dix unités de figure.</summary>
    private static Figure Plan() => Figure.New(100, 60)
        .Repere(10, 50, 30)
        .Texte(45, 25, "O", FigureTint.Estompe)
        .Texte(60, 25, "1", FigureTint.Estompe)
        .Texte(45, 40, "1", FigureTint.Estompe);

    private static double Px(double x) => 50 + x * 10;

    private static double Py(double y) => 30 + y * 10;

    // --- Niveau 2 : les figures du collège --------------------------------------------

    private static Func<Random, Question>[] GeoLevel2() => new Func<Random, Question>[]
    {
        r =>
        {
            int L = r.Next(5, 16), l = r.Next(3, 11);
            return Num(2, "Aire", "Quelle est l'aire de ce rectangle, en cm² ?", L * l,
                $"Aire = L × l = {L} × {l} = {L * l} cm².", figure: RectangleFigure(L, l, "?"));
        },
        r =>
        {
            int L = r.Next(5, 16), l = r.Next(3, 11);
            return Num(2, "Géométrie", "Quel est le périmètre de ce rectangle, en cm ?", 2 * (L + l),
                $"Périmètre = 2 × (L + l) = 2 × ({L} + {l}) = {2 * (L + l)} cm.",
                figure: RectangleFigure(L, l, ""));
        },
        r =>
        {
            int b = r.Next(3, 13) * 2, h = r.Next(4, 13);
            double unit = Math.Clamp(Math.Min(56.0 / b, 34.0 / h), 1.2, 5);
            double w = b * unit, hh = h * unit;
            double x0 = 50 - w / 2, y0 = 30 - hh / 2;

            var figure = Figure.New(100, 60)
                .Forme(x0, y0, x0 + w, y0, x0, y0 + hh)
                .AngleDroit(x0, y0, x0 + w, y0, x0, y0 + hh)
                .Cote(x0 + w / 2, y0 - 5, $"{b} cm")
                .Cote(x0 - 8, y0 + hh / 2, $"{h} cm");

            return Num(2, "Aire", "Quelle est l'aire de ce triangle rectangle, en cm² ?", b * h / 2,
                $"Aire = base × hauteur ÷ 2 = {b} × {h} ÷ 2 = {b * h / 2} cm².", figure: figure);
        },
        r =>
        {
            int a = r.Next(25, 80), b = r.Next(25, 80);
            while (a + b > 155) b = r.Next(25, 80);

            var figure = Figure.New(100, 56)
                .Forme(14, 10, 86, 10, 58, 46)
                .Arc(14, 10, 12, 0, 27)
                .Arc(86, 10, 12, 153, 40)
                .Cote(32, 16, $"{a}°")
                .Cote(70, 17, $"{b}°")
                .Texte(57, 36, "?", FigureTint.Bon);

            return Num(2, "Angles", "Combien mesure le troisième angle de ce triangle, en degrés ?",
                180 - a - b, $"La somme fait 180° : 180 − {a} − {b} = {180 - a - b}°.", figure: figure);
        },
        r =>
        {
            int a = r.Next(25, 156);

            var figure = Figure.New(100, 56)
                .Ligne(8, 12, 92, 44)
                .Ligne(8, 44, 92, 12)
                .Arc(50, 28, 13, 21, 138)
                .Cote(50, 42, $"{a}°")
                .Texte(74, 28, "?", FigureTint.Bon)
                .Legende("Deux droites sécantes");

            return Num(2, "Angles", "Combien mesure l'angle marqué « ? », en degrés ?", 180 - a,
                $"Les deux angles sont supplémentaires : 180 − {a} = {180 - a}°.", figure: figure);
        },
        r =>
        {
            int a = r.Next(5, 12), b = r.Next(5, 12), c = r.Next(2, a - 1), d = r.Next(2, b - 1);
            double unit = Math.Clamp(Math.Min(46.0 / a, 32.0 / b), 1.2, 4);
            double x0 = 50 - a * unit / 2, y0 = 30 - b * unit / 2;

            var figure = Figure.New(100, 60)
                .Forme(x0, y0,
                    x0 + a * unit, y0,
                    x0 + a * unit, y0 + (b - d) * unit,
                    x0 + c * unit, y0 + (b - d) * unit,
                    x0 + c * unit, y0 + b * unit,
                    x0, y0 + b * unit)
                .Cote(x0 + a * unit / 2, y0 - 5, $"{a}")
                .Cote(x0 - 5, y0 + b * unit / 2, $"{b}")
                .Cote(x0 + c * unit / 2, y0 + b * unit + 5, $"{c}")
                .Cote(x0 + a * unit + 6, y0 + (b - d) * unit / 2, $"{b - d}")
                .Legende("Toutes les longueurs sont en cm");

            return Num(2, "Aire", "Quelle est l'aire de cette figure, en cm² ?", a * b - (a - c) * d,
                $"Grand rectangle moins l'encoche : {a} × {b} − {a - c} × {d} = {a * b} − {(a - c) * d} = {a * b - (a - c) * d} cm².",
                figure: figure);
        },
        r =>
        {
            int rayon = r.Next(2, 12);

            var figure = Figure.New(100, 58)
                .Cercle(50, 28, 24)
                .Fleche(50, 28, 74, 28, FigureTint.Bois)
                .Point(50, 28)
                .Cote(62, 33, $"{rayon} cm");

            return Num(2, "Géométrie", "Quel est le périmètre de ce cercle, en multiples de π ?", 2 * rayon,
                $"Périmètre = 2πr = 2 × π × {rayon} = {2 * rayon}π cm.", figure: figure);
        },
        r =>
        {
            int[] barres = { r.Next(2, 9), r.Next(2, 9), r.Next(2, 9), r.Next(2, 9) };
            string[] noms = { "lun", "mar", "mer", "jeu" };

            var figure = Figure.New(100, 56).Ligne(8, 8, 92, 8);
            for (int i = 0; i < 4; i++)
            {
                double x = 20 + i * 20;
                figure.Forme(x - 7, 8, x + 7, 8, x + 7, 8 + barres[i] * 5, x - 7, 8 + barres[i] * 5)
                    .Texte(x, 3, noms[i], FigureTint.Estompe)
                    .Cote(x, 13 + barres[i] * 5, barres[i].ToString());
            }

            return Num(2, "Statistiques", "Combien font en tout les quatre barres de ce diagramme ?",
                barres.Sum(), $"{string.Join(" + ", barres)} = {barres.Sum()}.", figure: figure);
        }
    };

    // --- Niveau 3 : brevet, plan et premiers solides ----------------------------------

    private static Func<Random, Question>[] GeoLevel3() => new Func<Random, Question>[]
    {
        r =>
        {
            var (p, q, h) = Pick(r, (3, 4, 5), (6, 8, 10), (5, 12, 13), (9, 12, 15), (8, 15, 17), (7, 24, 25));
            double unit = Math.Clamp(Math.Min(52.0 / q, 32.0 / p), 1.1, 4.5);
            double x0 = 50 - q * unit / 2, y0 = 30 - p * unit / 2;

            var figure = Figure.New(100, 60)
                .Forme(x0, y0, x0 + q * unit, y0, x0, y0 + p * unit)
                .AngleDroit(x0, y0, x0 + q * unit, y0, x0, y0 + p * unit)
                .Cote(x0 + q * unit / 2, y0 - 5, $"{q}")
                .Cote(x0 - 5, y0 + p * unit / 2, $"{p}")
                .Texte(x0 + q * unit / 2 - 2, y0 + p * unit / 2 + 5, "?", FigureTint.Bon);

            return Num(3, "Pythagore", "Combien mesure l'hypoténuse de ce triangle rectangle ?", h,
                $"{p}² + {q}² = {p * p} + {q * q} = {h * h}, et √{h * h} = {h}.", figure: figure);
        },
        r =>
        {
            int am = r.Next(2, 8), an = r.Next(2, 8), k = r.Next(2, 4);

            double ax = 30, ay = 52, bx = 10, by = 8, cx = 90, cy = 8;
            double mx = ax + (bx - ax) / k, my = ay + (by - ay) / k;
            double nx = ax + (cx - ax) / k, ny = ay + (cy - ay) / k;

            var figure = Figure.New(100, 60)
                .Forme(ax, ay, bx, by, cx, cy)
                .Ligne(mx, my, nx, ny)
                .Point(ax, ay).Texte(ax, ay + 5, "A", FigureTint.Estompe)
                .Point(bx, by).Texte(bx - 4, by - 4, "B", FigureTint.Estompe)
                .Point(cx, cy).Texte(cx + 4, cy - 4, "C", FigureTint.Estompe)
                .Point(mx, my).Texte(mx - 5, my + 2, "M", FigureTint.Estompe)
                .Point(nx, ny).Texte(nx + 5, ny + 2, "N", FigureTint.Estompe)
                .Cote((ax + mx) / 2 - 4, (ay + my) / 2, $"{am}")
                .Cote((mx + bx) / 2 - 4, (my + by) / 2, $"{am * (k - 1)}")
                .Cote((ax + nx) / 2 + 4, (ay + ny) / 2, $"{an}")
                .Texte((nx + cx) / 2 + 4, (ny + cy) / 2, "?", FigureTint.Bon)
                .Legende("(MN) est parallèle à (BC)");

            return Num(3, "Thalès", "Combien vaut AC ?", an * k,
                $"AB = {am * k} et AM = {am}, donc le rapport vaut {k} : AC = {an} × {k} = {an * k}.",
                figure: figure);
        },
        r =>
        {
            int grand = r.Next(8, 18), petit = r.Next(3, grand - 2), h = r.Next(3, 11) * 2;
            double unit = Math.Clamp(Math.Min(56.0 / grand, 30.0 / h), 1.1, 4);
            double B = grand * unit, b = petit * unit, hh = h * unit;
            double x0 = 50 - B / 2, y0 = 30 - hh / 2;

            var figure = Figure.New(100, 60)
                .Forme(x0, y0, x0 + B, y0, x0 + (B + b) / 2, y0 + hh, x0 + (B - b) / 2, y0 + hh)
                .Cote(x0 + B / 2, y0 - 5, $"{grand}")
                .Cote(x0 + B / 2, y0 + hh + 5, $"{petit}")
                .Trait(FigureStyle.Pointille, FigureTint.Estompe, x0 + (B - b) / 2, y0, x0 + (B - b) / 2, y0 + hh)
                .Cote(x0 + (B - b) / 2 - 6, y0 + hh / 2, $"{h}")
                .Legende("Trapèze, longueurs en cm");

            return Num(3, "Aire", "Quelle est l'aire de ce trapèze, en cm² ?", (grand + petit) * h / 2.0,
                $"Aire = (B + b) × h ÷ 2 = ({grand} + {petit}) × {h} ÷ 2 = {Answers.Format((grand + petit) * h / 2.0)} cm².",
                figure: figure);
        },
        r =>
        {
            int b = r.Next(5, 15), h = r.Next(3, 11);
            double unit = Math.Clamp(Math.Min(44.0 / b, 28.0 / h), 1.2, 4.5);
            double w = b * unit, hh = h * unit, biais = 13;
            double x0 = 24, y0 = 30 - hh / 2;

            var figure = Figure.New(100, 60)
                .Forme(x0, y0, x0 + w, y0, x0 + w + biais, y0 + hh, x0 + biais, y0 + hh)
                .Trait(FigureStyle.Pointille, FigureTint.Estompe, x0 + biais, y0 + hh, x0 + biais, y0)
                .AngleDroit(x0 + biais, y0, x0 + w, y0, x0 + biais, y0 + hh)
                .Cote(x0 + w / 2, y0 - 5, $"{b} cm")
                .Cote(x0 + biais - 7, y0 + hh / 2, $"{h} cm");

            return Num(3, "Aire", "Quelle est l'aire de ce parallélogramme, en cm² ?", b * h,
                $"Aire = base × hauteur = {b} × {h} = {b * h} cm².", figure: figure);
        },
        r =>
        {
            int d1 = r.Next(4, 15) * 2, d2 = r.Next(3, 11) * 2;
            // la grande diagonale est dessinée à l'horizontale : les valeurs doivent suivre
            if (d2 > d1) (d1, d2) = (d2, d1);

            var figure = Figure.New(100, 60)
                .Forme(50, 6, 82, 30, 50, 54, 18, 30)
                .Trait(FigureStyle.Pointille, FigureTint.Bois, 18, 30, 82, 30)
                .Trait(FigureStyle.Pointille, FigureTint.Bois, 50, 6, 50, 54)
                // les cotes se logent dans deux quadrants libres, jamais sur une arête
                .Cote(35, 35, $"{d1} cm")
                .Cote(60, 21, $"{d2} cm")
                .Legende("Losange, diagonales en pointillé");

            return Num(3, "Aire", "Quelle est l'aire de ce losange, en cm² ?", d1 * d2 / 2,
                $"Aire = (D × d) ÷ 2 = ({d1} × {d2}) ÷ 2 = {d1 * d2 / 2} cm².", figure: figure);
        },
        r =>
        {
            int rayon = r.Next(2, 12);

            var figure = Figure.New(100, 58)
                .Cercle(50, 28, 24)
                .Fleche(50, 28, 67, 45, FigureTint.Bois)
                .Point(50, 28)
                .Cote(65, 33, $"{rayon} cm");

            return Num(3, "Aire", "Quelle est l'aire de ce disque, en multiples de π ?", rayon * rayon,
                $"Aire = πr² = π × {rayon}² = {rayon * rayon}π cm².", figure: figure);
        },
        r =>
        {
            int a = r.Next(35, 146);

            var figure = Figure.New(100, 58)
                .Ligne(6, 40, 94, 40)
                .Ligne(6, 16, 94, 16)
                .Ligne(24, 6, 76, 50)
                .Arc(59, 40, 12, 180, 41)
                .Arc(41, 16, 12, 0, 41)
                .Cote(44, 45, $"{a}°")
                .Texte(56, 21, "?", FigureTint.Bon)
                .Legende("Les deux droites sont parallèles");

            return Num(3, "Angles", "Combien mesure l'angle marqué « ? », en degrés ?", a,
                $"Ce sont deux angles alternes-internes : ils sont égaux, donc {a}°.", figure: figure);
        },
        r =>
        {
            int n = r.Next(5, 11);

            var sommets = new List<double>();
            for (int i = 0; i < n; i++)
            {
                double angle = Math.PI / 2 + 2 * Math.PI * i / n;
                sommets.Add(50 + 24 * Math.Cos(angle));
                sommets.Add(31 + 24 * Math.Sin(angle));
            }

            var figure = Figure.New(100, 62)
                .Forme(sommets.ToArray())
                .Texte(50, 31, $"{n} côtés", FigureTint.Estompe);

            return Num(3, "Angles", "Combien vaut la somme des angles de ce polygone, en degrés ?",
                (n - 2) * 180, $"(n − 2) × 180 = ({n} − 2) × 180 = {(n - 2) * 180}°.", figure: figure);
        },
        r =>
        {
            var (dx, dy, d) = Pick(r, (3, 4, 5), (4, 3, 5), (3, -4, 5), (-3, 4, 5), (0, 3, 3), (4, 0, 4));
            int ax = r.Next(-3, 1), ay = r.Next(-1, 1);
            int bx = ax + dx, by = ay + dy;

            var figure = Plan()
                .Trait(FigureStyle.Plein, FigureTint.Bon, Px(ax), Py(ay), Px(bx), Py(by))
                .Point(Px(ax), Py(ay)).Texte(Px(ax) - 6, Py(ay) - 5, "A", FigureTint.Craie)
                .Point(Px(bx), Py(by)).Texte(Px(bx) + 6, Py(by) + 5, "B", FigureTint.Craie)
                .Legende($"A({Nb(ax)} ; {Nb(ay)}) et B({Nb(bx)} ; {Nb(by)})");

            return Num(3, "Pythagore", "Quelle est la longueur AB ?", d,
                $"AB = √(({bx} − {ax})² + ({by} − {ay})²) = √({dx * dx} + {dy * dy}) = {d}.", figure: figure);
        },
        r =>
        {
            int L = r.Next(3, 10), l = r.Next(2, 8), h = r.Next(2, 8);
            return Num(3, "Volume", "Quel est le volume de ce pavé droit, en cm³ ?", L * l * h,
                $"V = L × l × h = {L} × {l} × {h} = {L * l * h} cm³.",
                figure: Pave(L, h, l, $"{L} cm", $"{h} cm", $"{l} cm"));
        },
        r =>
        {
            int c = r.Next(2, 12);
            return Num(3, "Volume", "Quelle est l'aire totale de ce cube, en cm² ?", 6 * c * c,
                $"Six faces carrées : 6 × {c}² = 6 × {c * c} = {6 * c * c} cm².",
                figure: Pave(c, c, c, $"{c} cm", "", ""));
        }
    };

    // --- Niveau 4 : lectures graphiques et solides ------------------------------------

    private static Func<Random, Question>[] GeoLevel4() => new Func<Random, Question>[]
    {
        r =>
        {
            int a = Pick(r, -2, -1, 1, 2), b = r.Next(-1, 2);
            // on ne trace que la portion qui tient dans le repère
            int reach = Math.Max(1, (3 - Math.Abs(b)) / Math.Abs(a));

            var figure = Plan()
                .Trait(FigureStyle.Plein, FigureTint.Bon,
                    Px(-reach), Py(a * -reach + b), Px(reach), Py(a * reach + b))
                .Point(Px(0), Py(b))
                .Point(Px(reach), Py(a * reach + b));

            return Num(4, "Fonction affine", "Quel est le coefficient directeur de cette droite ?", a,
                $"En avançant de 1 en abscisse, la droite monte de {a} : le coefficient directeur vaut {a}.",
                figure: figure);
        },
        r =>
        {
            int a = Pick(r, -2, -1, 1, 2), b = NonZero(r, 2);
            int reach = Math.Max(1, (3 - Math.Abs(b)) / Math.Abs(a));

            var figure = Plan()
                .Trait(FigureStyle.Plein, FigureTint.Bon,
                    Px(-reach), Py(a * -reach + b), Px(reach), Py(a * reach + b))
                .Point(Px(0), Py(b))
                .Texte(Px(0) + 8, Py(b) + 5, "?", FigureTint.Bois);

            return Num(4, "Fonction affine", "Quelle est l'ordonnée à l'origine de cette droite ?", b,
                $"La droite coupe l'axe des ordonnées en {Nb(b)}.", figure: figure);
        },
        r =>
        {
            int s = r.Next(-2, 3), k = r.Next(-2, 2);

            var courbe = new List<double>();
            for (double x = -3.4; x <= 3.4; x += 0.34)
            {
                double y = 0.5 * (x - s) * (x - s) + k;
                if (y > 3 || y < -3) continue;
                courbe.Add(Px(x));
                courbe.Add(Py(y));
            }

            var figure = Plan();
            if (courbe.Count >= 4) figure.Trait(FigureStyle.Plein, FigureTint.Bon, courbe.ToArray());
            figure.Point(Px(s), Py(k)).Texte(Px(s) + 6, Py(k) - 6, "S", FigureTint.Bois);

            return Num(4, "Second degré", "Quelle est l'abscisse du sommet S de cette parabole ?", s,
                $"Le sommet se lit à l'abscisse {Nb(s)}.", figure: figure);
        },
        r =>
        {
            var (angle, top, bottom, cosinus, why) = Pick(r,
                (60, 1, 2, true, "cos(60°) = 1/2."),
                (30, 1, 2, false, "sin(30°) = 1/2."),
                (90, 1, 1, false, "sin(90°) = 1."),
                (180, -1, 1, true, "cos(180°) = −1."),
                (90, 0, 1, true, "cos(90°) = 0."));

            double rad = angle * Math.PI / 180;
            double px = 50 + 24 * Math.Cos(rad), py = 30 + 24 * Math.Sin(rad);

            var figure = Figure.New(100, 62)
                .Cercle(50, 30, 24)
                .Fleche(50, 30, 82, 30, FigureTint.Estompe)
                .Fleche(50, 30, 50, 58, FigureTint.Estompe)
                .Trait(FigureStyle.Plein, FigureTint.Bon, 50, 30, px, py)
                .Arc(50, 30, 10, 0, angle)
                .Point(px, py)
                .Cote(50 + 18 * Math.Cos(rad / 2), 30 + 18 * Math.Sin(rad / 2), $"{angle}°");

            // la projection cherchée, en pointillé : c'est elle que la question demande de lire
            if (cosinus) figure.Trait(FigureStyle.Pointille, FigureTint.Bois, px, py, px, 30).Point(px, 30);
            else figure.Trait(FigureStyle.Pointille, FigureTint.Bois, px, py, 50, py).Point(50, py);

            return Frac(4, "Trigonométrie",
                $"Sur ce cercle trigonométrique, combien vaut {(cosinus ? "cos" : "sin")}({angle}°) ?",
                top, bottom, why, figure: figure);
        },
        r =>
        {
            var (dx, dy, n) = Pick(r, (3, 4, 5), (4, 3, 5), (-3, 4, 5), (3, -4, 5), (0, 3, 3), (2, 0, 2));

            var figure = Plan()
                .Fleche(Px(0), Py(0), Px(dx), Py(dy), FigureTint.Bon)
                .Texte(Px(dx) + 7, Py(dy) + 5, "u", FigureTint.Bon)
                .Legende($"u({Nb(dx)} ; {Nb(dy)})");

            return Num(4, "Vecteurs", "Quelle est la norme du vecteur u ?", n,
                $"‖u‖ = √({dx}² + {dy}²) = √{dx * dx + dy * dy} = {n}.", figure: figure);
        },
        r =>
        {
            int h = r.Next(3, 16) * 2;
            double unit = Math.Clamp(44.0 / h, 1.2, 4);
            double w = h * unit * 0.5, hh = h * unit * 0.866;
            double x0 = 50 - w / 2, y0 = 30 - hh / 2;

            var figure = Figure.New(100, 62)
                .Forme(x0, y0, x0 + w, y0, x0, y0 + hh)
                .AngleDroit(x0, y0, x0 + w, y0, x0, y0 + hh)
                .Arc(x0, y0 + hh, 11, -90, 30)
                .Cote(x0 + 9, y0 + hh - 13, "30°")
                .Cote(x0 + w / 2 + 9, y0 + hh / 2 + 3, $"{h}")
                .Texte(x0 + w / 2, y0 - 5, "?", FigureTint.Bon);

            return Num(4, "Trigonométrie", "Combien mesure le côté marqué « ? » ?", h / 2,
                $"Le côté opposé à l'angle de 30° vaut la moitié de l'hypoténuse : {h} ÷ 2 = {h / 2}.",
                figure: figure);
        },
        r =>
        {
            int cote = r.Next(2, 10), h = r.Next(2, 9) * 3;
            return Num(4, "Volume", "Quel est le volume de cette pyramide à base carrée, en cm³ ?",
                cote * cote * h / 3.0,
                $"V = (aire de la base × hauteur) ÷ 3 = ({cote}² × {h}) ÷ 3 = {Answers.Format(cote * cote * h / 3.0)} cm³.",
                figure: Pyramide($"{cote} cm", $"{h} cm"));
        },
        r =>
        {
            int rayon = r.Next(2, 9), h = r.Next(3, 13);
            return Num(4, "Volume", "Quel est le volume de ce cylindre, en multiples de π ?", rayon * rayon * h,
                $"V = πr²h = π × {rayon}² × {h} = {rayon * rayon * h}π cm³.",
                figure: Cylindre(rayon, h, $"{rayon} cm", $"{h} cm"));
        },
        r =>
        {
            int seuls = r.Next(4, 20), communs = r.Next(2, 10), autres = r.Next(4, 20);

            var figure = Figure.New(100, 54)
                .Cercle(38, 27, 21)
                .Cercle(62, 27, 21)
                .Texte(26, 27, seuls.ToString(), FigureTint.Craie)
                .Texte(50, 27, communs.ToString(), FigureTint.Bon)
                .Texte(74, 27, autres.ToString(), FigureTint.Craie)
                .Texte(20, 50, "A", FigureTint.Estompe)
                .Texte(80, 50, "B", FigureTint.Estompe);

            return Num(4, "Dénombrement", "Combien d'éléments compte la réunion A ∪ B ?",
                seuls + communs + autres,
                $"On additionne les trois zones : {seuls} + {communs} + {autres} = {seuls + communs + autres}.",
                figure: figure);
        },
        r =>
        {
            int p = Pick(r, 20, 40, 50, 60, 80), q = Pick(r, 20, 40, 50, 60, 80);

            var figure = Figure.New(100, 56)
                .Trait(FigureStyle.Plein, FigureTint.Bon, 14, 28, 46, 44)
                .Trait(FigureStyle.Plein, FigureTint.Bon, 46, 44, 78, 50)
                .Ligne(14, 28, 46, 12)
                .Ligne(46, 44, 78, 36)
                .Cote(29, 41, Answers.Format(p / 100.0))
                .Cote(62, 51, Answers.Format(q / 100.0))
                .Texte(88, 50, "A∩B", FigureTint.Bon)
                .Legende("Arbre de probabilités");

            return Num(4, "Probabilités", "Quelle est la probabilité du chemin en vert, en % ?",
                p * q / 100.0,
                $"On multiplie le long du chemin : {Answers.Format(p / 100.0)} × {Answers.Format(q / 100.0)} = {Answers.Format(p * q / 10000.0)}, soit {Answers.Format(p * q / 100.0)} %.",
                figure: figure);
        }
    };

    // --- Niveau 5 : terminale ----------------------------------------------------------

    private static Func<Random, Question>[] GeoLevel5() => new Func<Random, Question>[]
    {
        r =>
        {
            // au-delà de 2, la courbe sortirait du repère et l'aire ne se verrait plus
            int b = r.Next(1, 3);

            var courbe = new List<double>();
            for (double x = -2.2; x <= 2.2; x += 0.22)
            {
                double y = 0.6 * x * x;
                if (y > 3) continue;
                courbe.Add(Px(x));
                courbe.Add(Py(y));
            }

            var figure = Plan().Trait(FigureStyle.Plein, FigureTint.Bon, courbe.ToArray());
            for (double x = 0.18; x < b; x += 0.3)
            {
                figure.Trait(FigureStyle.Leger, FigureTint.Bon, Px(x), Py(0), Px(x), Py(Math.Min(3, 0.6 * x * x)));
            }

            figure.Trait(FigureStyle.Pointille, FigureTint.Bois, Px(b), Py(0), Px(b), Py(Math.Min(3, 0.6 * b * b)))
                .Legende($"Aire sous la courbe de x², entre 0 et {b}");

            return Frac(5, "Intégrales", "Quelle est l'aire hachurée ?", b * b * b, 3,
                $"∫ de 0 à {b} de x² dx = {b}³/3 = {b * b * b}/3.", figure: figure);
        },
        r =>
        {
            int a = r.Next(1, 4);

            var courbe = new List<double>();
            for (double x = -2.4; x <= 2.4; x += 0.24)
            {
                double y = 0.5 * x * x - 1;
                if (y > 3 || y < -3) continue;
                courbe.Add(Px(x));
                courbe.Add(Py(y));
            }

            double ya = 0.5 * a * a - 1;
            var figure = Plan()
                .Trait(FigureStyle.Plein, FigureTint.Bon, courbe.ToArray())
                .Trait(FigureStyle.Pointille, FigureTint.Bois,
                    Px(a - 1.4), Py(ya - 1.4 * a), Px(a + 1.4), Py(ya + 1.4 * a))
                .Point(Px(a), Py(ya))
                .Legende($"f(x) = 0,5x² − 1, et sa tangente en x = {a}");

            return Num(5, "Tangente", "Quel est le coefficient directeur de la tangente tracée ?", a,
                $"f′(x) = x, donc f′({a}) = {a}.", figure: figure);
        },
        r =>
        {
            int rayon = r.Next(2, 9), h = r.Next(2, 9) * 3;
            return Num(5, "Volume", "Quel est le volume de ce cône, en multiples de π ?", rayon * rayon * h / 3.0,
                $"V = πr²h ÷ 3 = π × {rayon}² × {h} ÷ 3 = {Answers.Format(rayon * rayon * h / 3.0)}π.",
                figure: Cone(rayon, h, $"{rayon} cm", $"{h} cm"));
        },
        r =>
        {
            int rayon = Pick(r, 3, 6, 9, 12);
            return Frac(5, "Volume", "Quel est le volume de cette sphère, en multiples de π ?",
                4 * rayon * rayon * rayon, 3,
                $"V = 4πr³/3 = 4 × {rayon}³ ÷ 3 = {Answers.Format(4.0 * rayon * rayon * rayon / 3)}π.",
                figure: Sphere($"{rayon} cm"));
        },
        r =>
        {
            int p = Pick(r, 20, 40, 50, 60, 80);
            int a = Pick(r, 20, 50, 80), b = Pick(r, 10, 40, 90);

            var figure = Figure.New(100, 56)
                .Ligne(14, 28, 46, 46)
                .Ligne(14, 28, 46, 10)
                .Ligne(46, 46, 78, 52)
                .Ligne(46, 10, 78, 16)
                .Cote(29, 43, Answers.Format(p / 100.0))
                .Cote(29, 13, Answers.Format(1 - p / 100.0))
                .Cote(63, 53, Answers.Format(a / 100.0))
                .Cote(63, 17, Answers.Format(b / 100.0))
                .Texte(86, 52, "R", FigureTint.Bon)
                .Texte(86, 16, "R", FigureTint.Bon)
                .Legende("Les deux chemins qui mènent à R");

            return Num(5, "Probabilités", "Combien vaut P(R), en % ?", (p * a + (100 - p) * b) / 100.0,
                $"Probabilités totales : {Answers.Format(p / 100.0)} × {Answers.Format(a / 100.0)} + {Answers.Format(1 - p / 100.0)} × {Answers.Format(b / 100.0)} = {Answers.Format((p * a + (100 - p) * b) / 10000.0)}, soit {Answers.Format((p * a + (100 - p) * b) / 100.0)} %.",
                figure: figure);
        }
    };

    // --- Niveau 6 : le plan complexe et l'espace ---------------------------------------

    private static Func<Random, Question>[] GeoLevel6() => new Func<Random, Question>[]
    {
        r =>
        {
            var (x, y, m) = Pick(r, (3, 4, 5), (4, 3, 5), (-3, 4, 5), (3, -4, 5), (0, 2, 2), (2, 0, 2));
            string imaginaire = Term(y, "i");
            string affixe = $"{Nb(x)} {imaginaire}";

            var figure = Plan()
                .Fleche(Px(0), Py(0), Px(x * 0.6), Py(y * 0.6), FigureTint.Bon)
                .Point(Px(x * 0.6), Py(y * 0.6))
                .Texte(Px(x * 0.6) + 8, Py(y * 0.6) + 6, "M", FigureTint.Craie)
                .Legende($"M a pour affixe z = {affixe}");

            return Num(6, "Complexes", "Quel est le module de l'affixe de M ?", m,
                $"|z| = √({x}² + {y}²) = √{x * x + y * y} = {m}.", figure: figure);
        },
        r =>
        {
            // u part vers la droite, v vers le haut : le déterminant est alors toujours
            // positif, et la somme des deux tient dans le repère
            int a = r.Next(2, 5), b = r.Next(0, 2);
            int c = r.Next(-2, 1), d = r.Next(1, 4 - b);
            int det = a * d - b * c;

            var figure = Plan()
                .Fleche(Px(0), Py(0), Px(a), Py(b), FigureTint.Bon)
                .Fleche(Px(0), Py(0), Px(c), Py(d), FigureTint.Bois)
                .Trait(FigureStyle.Pointille, FigureTint.Estompe,
                    Px(a), Py(b), Px(a + c), Py(b + d), Px(c), Py(d))
                .Legende($"u({a} ; {b}) en vert, v({Nb(c)} ; {d}) en doré");

            return Num(6, "Matrices", "Quelle est l'aire du parallélogramme construit sur u et v ?", det,
                $"C'est la valeur absolue du déterminant : {a}×{d} − {b}×{c} = {det}.", figure: figure);
        },
        r =>
        {
            var (x, y, z, n) = Pick(r, (1, 2, 2, 3), (2, 3, 6, 7), (1, 4, 8, 9), (2, 6, 9, 11), (4, 4, 7, 9));

            return Num(6, "Espace", "Quelle est la longueur de la diagonale de ce pavé (en vert) ?", n,
                $"√({x}² + {y}² + {z}²) = √{x * x + y * y + z * z} = {n}.",
                figure: Pave(x, z, y, $"{x}", $"{z}", $"{y}", diagonale: true));
        },
        r =>
        {
            int ax = r.Next(-3, 4), ay = r.Next(-2, 3), bx = r.Next(-3, 4), by = r.Next(-2, 3);
            if (ax == 0 && ay == 0) ax = 3;
            if (bx == 0 && by == 0) by = 2;

            var figure = Plan()
                .Fleche(Px(0), Py(0), Px(ax), Py(ay), FigureTint.Bon)
                .Fleche(Px(0), Py(0), Px(bx), Py(by), FigureTint.Bois)
                .Legende($"u({Nb(ax)} ; {Nb(ay)}) en vert, v({Nb(bx)} ; {Nb(by)}) en doré");

            return Num(6, "Vecteurs", "Combien vaut le produit scalaire u · v ?", ax * bx + ay * by,
                $"u·v = {ax}×{bx} + {ay}×{by} = {ax * bx + ay * by}.", figure: figure);
        }
    };

    // --- Niveau 7 : prépa ---------------------------------------------------------------

    private static Func<Random, Question>[] GeoLevel7() => new Func<Random, Question>[]
    {
        r =>
        {
            int a = r.Next(2, 6), b = r.Next(2, 6), c = r.Next(2, 6);

            return Num(7, "Espace",
                "Trois arêtes issues du même sommet portent les vecteurs (a ; 0 ; 0), (0 ; b ; 0) et (0 ; 0 ; c). Quel est le volume du parallélépipède ?",
                a * b * c,
                $"Le volume est la valeur absolue du déterminant, soit {a} × {b} × {c} = {a * b * c}.",
                figure: Pave(a, c, b, $"a = {a}", $"c = {c}", $"b = {b}")
                    .Legende("Les trois arêtes sont deux à deux orthogonales"));
        },
        r =>
        {
            int ax = r.Next(-3, 1), bx = r.Next(0, 4), cx = r.Next(-2, 3);
            while ((ax + bx + cx) % 3 != 0) cx++;
            int ay = r.Next(-2, 1), by = r.Next(-2, 2), cy = r.Next(1, 3);

            var figure = Plan()
                .Forme(Px(ax), Py(ay), Px(bx), Py(by), Px(cx), Py(cy))
                .Point(Px(ax), Py(ay)).Point(Px(bx), Py(by)).Point(Px(cx), Py(cy))
                .Point(Px((ax + bx + cx) / 3.0), Py((ay + by + cy) / 3.0), FigureTint.Bon)
                .Legende($"A({Nb(ax)} ; {Nb(ay)}), B({Nb(bx)} ; {Nb(by)}), C({Nb(cx)} ; {Nb(cy)})");

            return Num(7, "Espace", "Quelle est l'abscisse du centre de gravité de ce triangle ?",
                (ax + bx + cx) / 3,
                $"Le centre de gravité est la moyenne des sommets : ({ax} + {bx} + {cx}) ÷ 3 = {(ax + bx + cx) / 3}.",
                figure: figure);
        },
        r =>
        {
            int a = r.Next(2, 5), b = r.Next(0, 2);
            int c = r.Next(-3, 1), d = r.Next(1, 4 - b);
            int det = a * d - b * c;

            var figure = Plan()
                .FormeStyle(FigureStyle.Plein, FigureTint.Bon, Px(0), Py(0), Px(a), Py(b), Px(c), Py(d))
                .Point(Px(0), Py(0)).Point(Px(a), Py(b)).Point(Px(c), Py(d))
                .Legende($"O, A({a} ; {b}) et B({Nb(c)} ; {d})");

            return Frac(7, "Espace", "Quelle est l'aire du triangle OAB ?", det, 2,
                $"Aire = |det(OA ; OB)| ÷ 2 = |{a}×{d} − {b}×{c}| ÷ 2 = {det}/2.", figure: figure);
        }
    };
}
