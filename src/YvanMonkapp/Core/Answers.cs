using System.Globalization;
using System.Text;

namespace YvanMonkapp.Core;

/// <summary>Comparaison souple entre ce que le joueur tape et la réponse attendue.</summary>
public static class Answers
{
    private const double Tolerance = 1e-6;

    private static readonly string[] Units = { "euros", "euro", "cm2", "cm", "m2", "m", "%", "€" };

    /// <summary>Vrai si la saisie répond à la question.</summary>
    public static bool Matches(Question question, string input)
    {
        string clean = Normalize(input);
        if (clean.Length == 0) return false;

        foreach (string accepted in question.Accepted)
        {
            if (Normalize(accepted) == clean) return true;
        }

        if (Normalize(question.Expected) == clean) return true;

        if (question.Numeric is double expected && TryParse(clean, out double given))
        {
            double scale = Math.Max(1, Math.Abs(expected));
            return Math.Abs(expected - given) <= Tolerance * scale;
        }

        return false;
    }

    /// <summary>Met la saisie sous forme canonique : minuscules, sans espace, virgule en point.</summary>
    public static string Normalize(string raw)
    {
        var sb = new StringBuilder(raw.Length);
        foreach (char c in raw.Trim().ToLowerInvariant())
        {
            // les espaces (y compris insécables, fréquents au copier-coller) et le _ sautent
            if (char.IsWhiteSpace(c) || c == '_') continue;
            sb.Append(c == ',' ? '.' : c);
        }

        string text = sb.ToString();

        // "x=12", "s=3,5" : on ne garde que la valeur
        int equals = text.LastIndexOf('=');
        if (equals >= 0 && equals < text.Length - 1) text = text[(equals + 1)..];

        // unités tapées par réflexe
        foreach (string unit in Units)
        {
            if (text.Length > unit.Length && text.EndsWith(unit, StringComparison.Ordinal))
            {
                text = text[..^unit.Length];
                break;
            }
        }

        return text;
    }

    /// <summary>Lit un nombre décimal ou une fraction "a/b" déjà normalisés.</summary>
    public static bool TryParse(string normalized, out double value)
    {
        value = 0;
        if (normalized.Length == 0) return false;

        int slash = normalized.IndexOf('/');
        if (slash > 0 && slash < normalized.Length - 1)
        {
            string top = normalized[..slash];
            string bottom = normalized[(slash + 1)..];
            if (double.TryParse(top, NumberStyles.Float, CultureInfo.InvariantCulture, out double a)
                && double.TryParse(bottom, NumberStyles.Float, CultureInfo.InvariantCulture, out double b)
                && Math.Abs(b) > double.Epsilon)
            {
                value = a / b;
                return true;
            }

            return false;
        }

        return double.TryParse(normalized, NumberStyles.Float, CultureInfo.InvariantCulture, out value);
    }

    /// <summary>Écrit un nombre à la française : virgule décimale, pas de zéros inutiles.</summary>
    public static string Format(double value)
    {
        if (Math.Abs(value - Math.Round(value)) < Tolerance)
        {
            return Math.Round(value).ToString("0", CultureInfo.InvariantCulture);
        }

        return value.ToString("0.######", CultureInfo.InvariantCulture).Replace('.', ',');
    }
}
