using System.Globalization;
using System.Text;

namespace YvanMonkapp.Core;

/// <summary>Comparaison souple entre ce que le joueur tape et la réponse attendue.</summary>
public static class Answers
{
    private const double Tolerance = 1e-6;

    /// <summary>
    /// Unités tapées par réflexe, retirées en fin de saisie. Rangées de la plus longue à la
    /// plus courte : sinon « km » se ferait rogner en « k » par la règle « m ».
    /// </summary>
    private static readonly string[] Units =
    {
        "degres", "degrés", "euros", "euro", "points", "unites", "unités", "km/h", "m/s",
        "cm2", "cm3", "cm", "mm", "dm", "km", "m2", "m3", "kg", "ml", "cl", "min", "pts",
        "%", "€", "$", "°", "m", "g", "l", "h", "s"
    };

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
            // les espaces (y compris insécables, fréquents au copier-coller) et le _ sautent,
            // comme les apostrophes de séparation des milliers et les « environ »
            if (char.IsWhiteSpace(c) || c is '_' or '\'' or '’' or '≈' or '~') continue;

            sb.Append(c switch
            {
                ',' => '.',
                // le moins mathématique et les tirets longs arrivent par copier-coller
                '−' or '–' or '—' => '-',
                '÷' or ':' => '/',
                _ => c
            });
        }

        string text = sb.ToString();

        // "x=12", "s=3,5" : on ne garde que la valeur
        int equals = text.LastIndexOf('=');
        if (equals >= 0 && equals < text.Length - 1) text = text[(equals + 1)..];

        // une saisie peut cumuler la valeur et son unité composée ("12cm2", "5km/h")
        for (int pass = 0; pass < 2; pass++)
        {
            string before = text;
            foreach (string unit in Units)
            {
                if (text.Length > unit.Length && text.EndsWith(unit, StringComparison.Ordinal))
                {
                    text = text[..^unit.Length];
                    break;
                }
            }
            if (before == text) break;
        }

        // le point final d'une phrase, et le + de "+12"
        text = text.TrimEnd('.');
        if (text.StartsWith('+')) text = text[1..];

        return text;
    }

    /// <summary>
    /// Lit un nombre déjà normalisé : décimal, fraction « a/b », ou puissance « a^b ».
    /// </summary>
    public static bool TryParse(string normalized, out double value)
    {
        value = 0;
        if (normalized.Length == 0) return false;

        int slash = normalized.IndexOf('/');
        if (slash > 0 && slash < normalized.Length - 1)
        {
            if (!TryNumber(normalized[..slash], out double top)) return false;
            if (!TryNumber(normalized[(slash + 1)..], out double bottom)) return false;
            if (Math.Abs(bottom) <= double.Epsilon) return false;

            value = top / bottom;
            return true;
        }

        return TryNumber(normalized, out value);
    }

    /// <summary>Un nombre simple, ou une puissance écrite « 2^10 » comme sur une calculatrice.</summary>
    private static bool TryNumber(string text, out double value)
    {
        value = 0;
        if (text.Length == 0) return false;

        int caret = text.IndexOf('^');
        if (caret > 0 && caret < text.Length - 1)
        {
            if (!Plain(text[..caret], out double baseValue)) return false;
            if (!Plain(text[(caret + 1)..].Trim('(', ')'), out double exponent)) return false;

            value = Math.Pow(baseValue, exponent);
            return !double.IsNaN(value) && !double.IsInfinity(value);
        }

        return Plain(text, out value);
    }

    private static bool Plain(string text, out double value) =>
        double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out value);

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
