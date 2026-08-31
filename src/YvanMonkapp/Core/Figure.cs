using System.Text.Json.Serialization;

namespace YvanMonkapp.Core;

/// <summary>Ce qu'un trait de figure sait dessiner.</summary>
public enum FigureKind
{
    /// <summary>Ligne brisée ouverte, passant par tous les points.</summary>
    Ligne,

    /// <summary>Polygone fermé.</summary>
    Forme,

    /// <summary>Cercle : centre puis rayon.</summary>
    Cercle,

    /// <summary>Ellipse : centre, demi-largeur, demi-hauteur. C'est le disque vu de biais.</summary>
    Ellipse,

    /// <summary>Arc : centre, rayon, angle de départ et amplitude, en degrés.</summary>
    Arc,

    /// <summary>Un point marqué.</summary>
    Point,

    /// <summary>Un texte centré sur sa position.</summary>
    Texte,

    /// <summary>Le petit carré de l'angle droit : sommet, puis deux points qui donnent les directions.</summary>
    AngleDroit,

    /// <summary>Segment terminé par une pointe.</summary>
    Fleche,

    /// <summary>Quadrillage léger sur toute la figure, du pas donné.</summary>
    Grille
}

public enum FigureStyle
{
    Plein,
    Pointille,

    /// <summary>Trait fin et discret : construction, quadrillage, cotes.</summary>
    Leger
}

/// <summary>Teintes disponibles, calées sur la palette du tableau.</summary>
public enum FigureTint
{
    Craie,
    Bois,
    Bon,
    Mauvais,
    Estompe
}

public sealed class FigurePart
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public FigureKind Kind { get; set; }

    /// <summary>Coordonnées à plat : x0, y0, x1, y1… Leur sens dépend du <see cref="Kind"/>.</summary>
    public double[] Points { get; set; } = Array.Empty<double>();

    public string Text { get; set; } = "";

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public FigureStyle Style { get; set; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public FigureTint Tint { get; set; }
}

/// <summary>
/// Un schéma au tableau, décrit en données plutôt qu'en objets graphiques : une figure doit
/// pouvoir dormir dans le carnet d'erreurs, repasser par le JSON, et ressortir intacte.
///
/// Le repère va de (0 ; 0) en bas à gauche à (<see cref="Width"/> ; <see cref="Height"/>) en
/// haut à droite — l'axe des ordonnées monte, comme en cours. C'est la vue qui le retourne.
/// </summary>
public sealed class Figure
{
    public double Width { get; set; } = 100;

    public double Height { get; set; } = 60;

    public List<FigurePart> Parts { get; set; } = new();

    /// <summary>Une légende sous le dessin, pour les cotes qui ne tiennent pas dedans.</summary>
    public string Caption { get; set; } = "";

    public static Figure New(double width = 100, double height = 60) => new() { Width = width, Height = height };

    private Figure Add(FigureKind kind, FigureStyle style, FigureTint tint, string text, params double[] points)
    {
        Parts.Add(new FigurePart { Kind = kind, Style = style, Tint = tint, Text = text, Points = points });
        return this;
    }

    public Figure Ligne(params double[] points) => Add(FigureKind.Ligne, FigureStyle.Plein, FigureTint.Craie, "", points);

    public Figure Trait(FigureStyle style, FigureTint tint, params double[] points) =>
        Add(FigureKind.Ligne, style, tint, "", points);

    public Figure Forme(params double[] points) => Add(FigureKind.Forme, FigureStyle.Plein, FigureTint.Craie, "", points);

    public Figure FormeStyle(FigureStyle style, FigureTint tint, params double[] points) =>
        Add(FigureKind.Forme, style, tint, "", points);

    public Figure Cercle(double cx, double cy, double r, FigureTint tint = FigureTint.Craie) =>
        Add(FigureKind.Cercle, FigureStyle.Plein, tint, "", cx, cy, r);

    public Figure Ellipse(double cx, double cy, double rx, double ry,
        FigureStyle style = FigureStyle.Plein, FigureTint tint = FigureTint.Craie) =>
        Add(FigureKind.Ellipse, style, tint, "", cx, cy, rx, ry);

    public Figure Arc(double cx, double cy, double r, double from, double sweep, FigureTint tint = FigureTint.Bois) =>
        Add(FigureKind.Arc, FigureStyle.Plein, tint, "", cx, cy, r, from, sweep);

    public Figure Point(double x, double y, FigureTint tint = FigureTint.Bois) =>
        Add(FigureKind.Point, FigureStyle.Plein, tint, "", x, y);

    public Figure Texte(double x, double y, string text, FigureTint tint = FigureTint.Craie) =>
        Add(FigureKind.Texte, FigureStyle.Plein, tint, text, x, y);

    public Figure Cote(double x, double y, string text) => Texte(x, y, text, FigureTint.Bois);

    public Figure AngleDroit(double vx, double vy, double ax, double ay, double bx, double by) =>
        Add(FigureKind.AngleDroit, FigureStyle.Plein, FigureTint.Estompe, "", vx, vy, ax, ay, bx, by);

    public Figure Fleche(double x0, double y0, double x1, double y1, FigureTint tint = FigureTint.Craie) =>
        Add(FigureKind.Fleche, FigureStyle.Plein, tint, "", x0, y0, x1, y1);

    public Figure Grille(double step) => Add(FigureKind.Grille, FigureStyle.Leger, FigureTint.Estompe, "", step);

    /// <summary>Un repère complet : quadrillage, deux axes fléchés et l'origine nommée.</summary>
    public Figure Repere(double step, double ox, double oy)
    {
        Grille(step);
        Fleche(0, oy, Width, oy, FigureTint.Estompe);
        Fleche(ox, 0, ox, Height, FigureTint.Estompe);
        return this;
    }

    public Figure Legende(string text)
    {
        Caption = text;
        return this;
    }
}
