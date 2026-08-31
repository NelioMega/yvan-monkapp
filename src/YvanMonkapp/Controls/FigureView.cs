using System.Globalization;
using System.Windows;
using System.Windows.Media;
using YvanMonkapp.Core;

namespace YvanMonkapp.Controls;

/// <summary>
/// Dessine une <see cref="Figure"/> à la craie. Tout passe par <see cref="OnRender"/> :
/// un schéma n'a ni état ni interaction, empiler des Path dans un Canvas coûterait un arbre
/// visuel entier pour un dessin qui ne bouge jamais.
/// </summary>
public sealed class FigureView : FrameworkElement
{
    public static readonly DependencyProperty FigureProperty = DependencyProperty.Register(
        nameof(Figure), typeof(Figure), typeof(FigureView),
        new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

    public Figure? Figure
    {
        get => (Figure?)GetValue(FigureProperty);
        set => SetValue(FigureProperty, value);
    }

    // La palette du tableau, recopiée ici : le contrôle sert un thème unique, et aller
    // chercher des ressources à chaque trait coûterait plus que la constante.
    private static readonly Color Craie = Color.FromRgb(0xF4, 0xF4, 0xEC);
    private static readonly Color Bois = Color.FromRgb(0xC9, 0x9A, 0x4F);
    private static readonly Color Bon = Color.FromRgb(0x69, 0xC9, 0x7A);
    private static readonly Color Mauvais = Color.FromRgb(0xE2, 0x57, 0x4C);
    private static readonly Color Estompe = Color.FromRgb(0x6E, 0x87, 0x7B);

    /// <summary>
    /// Marge réservée autour du dessin, en pixels. Elle est comptée à l'écran et non en
    /// unités de figure : les cotes sont écrites à taille fixe, donc la place qu'il leur
    /// faut ne dépend pas du zoom.
    /// </summary>
    private const double Gutter = 20;

    protected override Size MeasureOverride(Size available)
    {
        if (Figure is not Figure figure) return new Size(0, 0);

        // hauteur imposée par le parent, largeur prise telle quelle : la figure se centre dedans
        double width = double.IsInfinity(available.Width) ? figure.Width : available.Width;
        double height = double.IsInfinity(available.Height) ? 150 : available.Height;
        return new Size(width, height);
    }

    protected override void OnRender(DrawingContext dc)
    {
        if (Figure is not Figure figure || figure.Parts.Count == 0) return;
        if (ActualWidth <= 0 || ActualHeight <= 0) return;

        // on cadre sur ce qui est réellement dessiné, pas sur la boîte déclarée : un triangle
        // plat n'occupe qu'un bandeau de son repère, et il doit quand même remplir le panneau
        var box = Bounds(figure);

        double usableWidth = Math.Max(10, ActualWidth - 2 * Gutter);
        double usableHeight = Math.Max(10, ActualHeight - 2 * Gutter);
        double scale = Math.Min(usableWidth / box.Width, usableHeight / box.Height);
        if (scale <= 0) return;

        double left = (ActualWidth - box.Width * scale) / 2;
        double top = (ActualHeight - box.Height * scale) / 2;

        // le repère du dessin monte, celui de l'écran descend : on retourne ici, une fois
        Point At(double x, double y) =>
            new(left + (x - box.X) * scale, top + (box.Y + box.Height - y) * scale);

        foreach (var part in figure.Parts) Draw(dc, part, At, scale, figure);
    }

    /// <summary>L'étendue réellement occupée par les traits, cercles et cotes de la figure.</summary>
    private static Rect Bounds(Figure figure)
    {
        double minX = double.MaxValue, minY = double.MaxValue;
        double maxX = double.MinValue, maxY = double.MinValue;

        void Grow(double x, double y)
        {
            if (double.IsNaN(x) || double.IsNaN(y)) return;
            minX = Math.Min(minX, x);
            minY = Math.Min(minY, y);
            maxX = Math.Max(maxX, x);
            maxY = Math.Max(maxY, y);
        }

        foreach (var part in figure.Parts)
        {
            double[] p = part.Points;

            switch (part.Kind)
            {
                case FigureKind.Cercle when p.Length >= 3:
                case FigureKind.Arc when p.Length >= 3:
                    Grow(p[0] - p[2], p[1] - p[2]);
                    Grow(p[0] + p[2], p[1] + p[2]);
                    break;

                case FigureKind.Ellipse when p.Length >= 4:
                    Grow(p[0] - p[2], p[1] - p[3]);
                    Grow(p[0] + p[2], p[1] + p[3]);
                    break;

                // le quadrillage couvre tout le repère : c'est lui qui fixe le cadre
                case FigureKind.Grille:
                    Grow(0, 0);
                    Grow(figure.Width, figure.Height);
                    break;

                default:
                    for (int i = 0; i + 1 < p.Length; i += 2) Grow(p[i], p[i + 1]);
                    break;
            }
        }

        if (minX > maxX || minY > maxY) return new Rect(0, 0, figure.Width, figure.Height);

        return new Rect(minX, minY, Math.Max(maxX - minX, 1), Math.Max(maxY - minY, 1));
    }

    private void Draw(DrawingContext dc, FigurePart part, Func<double, double, Point> at, double scale, Figure figure)
    {
        var pen = PenFor(part, scale);
        double[] p = part.Points;

        switch (part.Kind)
        {
            case FigureKind.Ligne when p.Length >= 4:
                dc.DrawGeometry(null, pen, Path(p, at, closed: false));
                break;

            case FigureKind.Forme when p.Length >= 6:
                dc.DrawGeometry(null, pen, Path(p, at, closed: true));
                break;

            case FigureKind.Cercle when p.Length >= 3:
                dc.DrawEllipse(null, pen, at(p[0], p[1]), p[2] * scale, p[2] * scale);
                break;

            case FigureKind.Ellipse when p.Length >= 4:
                dc.DrawEllipse(null, pen, at(p[0], p[1]), p[2] * scale, p[3] * scale);
                break;

            case FigureKind.Arc when p.Length >= 5:
                dc.DrawGeometry(null, pen, ArcPath(p, at, scale));
                break;

            case FigureKind.Point when p.Length >= 2:
                dc.DrawEllipse(new SolidColorBrush(ColorFor(part.Tint)), null, at(p[0], p[1]), 2.6, 2.6);
                break;

            case FigureKind.Texte when p.Length >= 2:
                DrawText(dc, part, at(p[0], p[1]));
                break;

            case FigureKind.AngleDroit when p.Length >= 6:
                DrawRightAngle(dc, pen, p, at, scale);
                break;

            case FigureKind.Fleche when p.Length >= 4:
                DrawArrow(dc, pen, at(p[0], p[1]), at(p[2], p[3]), scale);
                break;

            case FigureKind.Grille when p.Length >= 1:
                DrawGrid(dc, pen, p[0], at, figure);
                break;
        }
    }

    private static StreamGeometry Path(double[] p, Func<double, double, Point> at, bool closed)
    {
        var geometry = new StreamGeometry();
        using (var draw = geometry.Open())
        {
            draw.BeginFigure(at(p[0], p[1]), isFilled: false, isClosed: closed);
            for (int i = 2; i + 1 < p.Length; i += 2) draw.LineTo(at(p[i], p[i + 1]), isStroked: true, isSmoothJoin: false);
        }
        geometry.Freeze();
        return geometry;
    }

    private static StreamGeometry ArcPath(double[] p, Func<double, double, Point> at, double scale)
    {
        double cx = p[0], cy = p[1], r = p[2];
        double from = p[3] * Math.PI / 180, sweep = p[4] * Math.PI / 180;

        var start = at(cx + r * Math.Cos(from), cy + r * Math.Sin(from));
        var end = at(cx + r * Math.Cos(from + sweep), cy + r * Math.Sin(from + sweep));

        var geometry = new StreamGeometry();
        using (var draw = geometry.Open())
        {
            draw.BeginFigure(start, isFilled: false, isClosed: false);
            // l'axe des y est retourné à l'affichage, donc le sens de rotation l'est aussi
            draw.ArcTo(end, new Size(r * scale, r * scale), 0, Math.Abs(p[4]) > 180,
                p[4] > 0 ? SweepDirection.Counterclockwise : SweepDirection.Clockwise, true, false);
        }
        geometry.Freeze();
        return geometry;
    }

    /// <summary>Le petit carré de l'angle droit, posé sur les deux directions données.</summary>
    private static void DrawRightAngle(DrawingContext dc, Pen pen, double[] p, Func<double, double, Point> at, double scale)
    {
        var vertex = at(p[0], p[1]);
        var toA = Unit(vertex, at(p[2], p[3]));
        var toB = Unit(vertex, at(p[4], p[5]));

        double side = Math.Max(6, 5 * scale / 4);
        var a = new Point(vertex.X + toA.X * side, vertex.Y + toA.Y * side);
        var b = new Point(vertex.X + toB.X * side, vertex.Y + toB.Y * side);
        var corner = new Point(a.X + toB.X * side, a.Y + toB.Y * side);

        var geometry = new StreamGeometry();
        using (var draw = geometry.Open())
        {
            draw.BeginFigure(a, isFilled: false, isClosed: false);
            draw.LineTo(corner, true, false);
            draw.LineTo(b, true, false);
        }
        geometry.Freeze();

        dc.DrawGeometry(null, pen, geometry);
    }

    private static void DrawArrow(DrawingContext dc, Pen pen, Point from, Point to, double scale)
    {
        dc.DrawLine(pen, from, to);

        var direction = Unit(from, to);
        double head = Math.Max(5, 2.2 * scale);
        var back = new Point(to.X - direction.X * head, to.Y - direction.Y * head);
        var side = new Vector(-direction.Y, direction.X) * (head * 0.42);

        var geometry = new StreamGeometry();
        using (var draw = geometry.Open())
        {
            draw.BeginFigure(to, isFilled: true, isClosed: true);
            draw.LineTo(new Point(back.X + side.X, back.Y + side.Y), true, false);
            draw.LineTo(new Point(back.X - side.X, back.Y - side.Y), true, false);
        }
        geometry.Freeze();

        dc.DrawGeometry(pen.Brush, null, geometry);
    }

    private static void DrawGrid(DrawingContext dc, Pen pen, double step, Func<double, double, Point> at, Figure figure)
    {
        if (step <= 0) return;

        for (double x = 0; x <= figure.Width + 0.001; x += step) dc.DrawLine(pen, at(x, 0), at(x, figure.Height));
        for (double y = 0; y <= figure.Height + 0.001; y += step) dc.DrawLine(pen, at(0, y), at(figure.Width, y));
    }

    private void DrawText(DrawingContext dc, FigurePart part, Point anchor)
    {
        var text = new FormattedText(part.Text, CultureInfo.CurrentCulture, FlowDirection.LeftToRight,
            new Typeface("Segoe UI"), 13, new SolidColorBrush(ColorFor(part.Tint)),
            VisualTreeHelper.GetDpi(this).PixelsPerDip)
        {
            TextAlignment = TextAlignment.Center
        };

        // le texte est centré sur son point : c'est ce qui rend les cotes posables au jugé
        dc.DrawText(text, new Point(anchor.X, anchor.Y - text.Height / 2));
    }

    private static Pen PenFor(FigurePart part, double scale)
    {
        double thickness = part.Style == FigureStyle.Leger ? 0.9 : Math.Max(1.4, scale * 0.11);

        var pen = new Pen(new SolidColorBrush(ColorFor(part.Tint)), thickness)
        {
            StartLineCap = PenLineCap.Round,
            EndLineCap = PenLineCap.Round,
            LineJoin = PenLineJoin.Round
        };

        if (part.Style == FigureStyle.Pointille)
        {
            pen.DashStyle = new DashStyle(new double[] { 3, 2.4 }, 0);
        }

        pen.Freeze();
        return pen;
    }

    private static Color ColorFor(FigureTint tint) => tint switch
    {
        FigureTint.Bois => Bois,
        FigureTint.Bon => Bon,
        FigureTint.Mauvais => Mauvais,
        FigureTint.Estompe => Estompe,
        _ => Craie
    };

    private static Vector Unit(Point from, Point to)
    {
        var v = to - from;
        double length = v.Length;
        return length < 1e-6 ? new Vector(1, 0) : v / length;
    }
}
