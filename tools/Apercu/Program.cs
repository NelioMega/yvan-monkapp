using System.Reflection;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using YvanMonkapp.Core;

namespace YvanMonkapp;

/// <summary>
/// Rend le popup de question en PNG, sans jamais le montrer ni toucher a AppData.
/// La fenetre est ouverte a opacite nulle : WPF la met bien en page et fait tourner ses
/// animations, mais rien n'apparait a l'ecran. On declenche ensuite la correction par
/// reflexion, parce que le chemin normal passe par la sauvegarde du score.
/// </summary>
public static class Apercu
{
    [STAThread]
    public static int Main(string[] args)
    {
        string folder = args.Length > 0 ? args[0] : Path.Combine(Path.GetTempPath(), "apercu-yvan");
        Directory.CreateDirectory(folder);

        var app = new Application { ShutdownMode = ShutdownMode.OnExplicitShutdown };
        app.Resources.MergedDictionaries.Add(new ResourceDictionary
        {
            Source = new Uri("Theme.xaml", UriKind.Relative)
        });

        try
        {
            Scene(folder, "1-question", niveau: 3, apres: null);
            Scene(folder, "2-indice", niveau: 3, apres: null, indice: true);
            Scene(folder, "3-bonne-reponse", niveau: 3, apres: 420);
            Scene(folder, "4-serie", niveau: 3, apres: 420, serie: 5, delta: 96);
            Scene(folder, "5-nouveau-rang", niveau: 5, apres: 520, serie: 7, delta: 138, rang: true);
            Scene(folder, "6-mauvaise-reponse", niveau: 4, apres: 420, bonne: false, delta: -24);

            // les questions dessinees : une par famille de figure, sans correction
            Scene(folder, "10-rectangle", niveau: 2, apres: null, motif: "rectangle");
            Scene(folder, "11-figure-composee", niveau: 2, apres: null, motif: "cette figure");
            Scene(folder, "12-angles", niveau: 2, apres: null, motif: "troisieme angle");
            Scene(folder, "13-pythagore", niveau: 3, apres: null, motif: "hypotenuse de ce");
            Scene(folder, "14-thales", niveau: 3, apres: null, motif: "AC");
            Scene(folder, "15-losange", niveau: 3, apres: null, motif: "losange");
            Scene(folder, "16-trapeze", niveau: 3, apres: null, motif: "trapeze");
            Scene(folder, "17-pave", niveau: 3, apres: null, motif: "pave droit");
            Scene(folder, "18-polygone", niveau: 3, apres: null, motif: "polygone");
            Scene(folder, "19-repere-droite", niveau: 4, apres: null, motif: "coefficient directeur de cette");
            Scene(folder, "20-parabole", niveau: 4, apres: null, motif: "sommet S");
            Scene(folder, "21-cercle-trigo", niveau: 4, apres: null, motif: "trigonometrique");
            Scene(folder, "22-cylindre", niveau: 4, apres: null, motif: "cylindre");
            Scene(folder, "23-pyramide", niveau: 4, apres: null, motif: "pyramide");
            Scene(folder, "24-venn", niveau: 4, apres: null, motif: "reunion");
            Scene(folder, "25-arbre", niveau: 4, apres: null, motif: "chemin en vert");
            Scene(folder, "26-integrale", niveau: 5, apres: null, motif: "hachuree");
            Scene(folder, "27-sphere", niveau: 5, apres: null, motif: "sphere");
            Scene(folder, "28-cone", niveau: 5, apres: null, motif: "cone");
            Scene(folder, "29-tangente", niveau: 5, apres: null, motif: "tangente tracee");
            Scene(folder, "30-complexes", niveau: 6, apres: null, motif: "affixe de M");
            Scene(folder, "31-determinant", niveau: 6, apres: null, motif: "parallelogramme construit");
            Scene(folder, "32-diagonale", niveau: 6, apres: null, motif: "diagonale de ce pave");

            Scene(folder, "33-entrainement", niveau: 3, apres: null, motif: "losange", entrainement: "Aire");

            Tableau(folder, "40-tableau-historique", "Historique");
            Tableau(folder, "41-tableau-chapitres", "Chapitres");
            Tableau(folder, "42-tableau-carnet", "Carnet");

            Console.WriteLine($"apercus ecrits dans {folder}");
            return 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"echec : {ex}");
            return 1;
        }
        finally
        {
            app.Shutdown();
        }
    }

    /// <summary>Ouvre la fenetre, la met dans l'etat demande, attend, puis l'enregistre.</summary>
    private static void Scene(string folder, string name, int niveau, int? apres, bool indice = false,
        int serie = 2, int delta = 64, bool bonne = true, bool rang = false, string? motif = null,
        string? entrainement = null)
    {
        var settings = new AppSettings { PlaySfx = false, PlayVoice = false, Volume = 0 };
        var score = new ScoreData { Points = 460, Streak = serie, BestStreak = 9 };

        var run = entrainement is null
            ? QuizRun.Single(Tirer(niveau, motif))
            : QuizRun.Training(Enumerable.Range(0, QuizRun.TrainingLength)
                .Select(i => i == 0 ? Tirer(niveau, motif) : QuestionGenerator.NextFrom(niveau, entrainement))
                .ToList(), entrainement);

        var window = new QuizWindow(run, score, settings)
        {
            Opacity = 0,
            ShowInTaskbar = false
        };

        window.Show();
        Pump(TimeSpan.FromMilliseconds(300));

        // le chrono tournerait toujours pendant la pose : on l'arrete a la main
        Field<DispatcherTimer>(window, "_countdown").Stop();

        if (indice)
        {
            Invoke(window, "RevealHint");
            Pump(TimeSpan.FromMilliseconds(120));
        }

        if (apres is int wait)
        {
            Field<bool>(window, "_answered");
            Set(window, "_answered", true);

            // une faute casse la serie : sans ca l'apercu montrerait un pied de fenetre faux
            if (!bonne) score.Streak = 0;

            var outcome = bonne ? AnswerOutcome.Correcte : AnswerOutcome.Fausse;
            var result = new AnswerResult(outcome, delta, bonne ? 18 : 0, bonne ? Math.Min(serie * 2, 30) : 0,
                score.Points + delta, bonne ? serie : 0, Ranks.Of(score.Points + delta),
                rang, rang, false, 4, false);

            score.Points += delta;
            Invoke(window, "ShowResult", result, bonne ? "" : "12", 0, score.Points - delta);
            Pump(TimeSpan.FromMilliseconds(wait));
        }

        Save(window, Path.Combine(folder, name + ".png"));
        window.Close();
        Pump(TimeSpan.FromMilliseconds(60));
    }

    /// <summary>Rend le tableau de bord, garni d'un historique credible, sur l'onglet demande.</summary>
    private static void Tableau(string folder, string name, string onglet)
    {
        var settings = new AppSettings { PlaySfx = false, PlayVoice = false, Difficulty = DifficultyMode.Auto };
        var score = Historique();

        var window = new DashboardWindow(settings, score)
        {
            Opacity = 0,
            ShowInTaskbar = false,
            NextAt = DateTime.Now.AddMinutes(7)
        };

        window.Show();
        Pump(TimeSpan.FromMilliseconds(320));

        Set(window, "_tab", onglet);
        Invoke(window, "FillList");
        Pump(TimeSpan.FromMilliseconds(220));

        Save(window, Path.Combine(folder, name + ".png"));
        window.Close();
        Pump(TimeSpan.FromMilliseconds(60));
    }

    /// <summary>Un joueur assidu depuis deux mois : de quoi remplir toutes les vues.</summary>
    private static ScoreData Historique()
    {
        var score = new ScoreData
        {
            Points = 1240,
            BestPoints = 1310,
            Streak = 4,
            BestStreak = 17,
            Asked = 418,
            Correct = 301,
            Wrong = 82,
            Timeout = 21,
            Abandoned = 14,
            TotalAnswerSeconds = 301 * 8.4,
            BestDailyStreak = 23,
            Hints = 19
        };

        var seed = new Random(7);

        for (int back = 0; back < 62; back++)
        {
            if (back % 7 == 5 && back > 10) continue;
            var day = score.Day(DateTime.Today.AddDays(-back));
            day.Asked = seed.Next(3, 12);
            day.Correct = seed.Next(1, day.Asked + 1);
            day.Delta = day.Correct * 40 - (day.Asked - day.Correct) * 18;
        }

        foreach (var (topic, asked, correct) in new[]
                 {
                     ("Fractions", 26, 11), ("Pythagore", 18, 15), ("Second degre", 22, 9),
                     ("Suites", 14, 12), ("Volume", 9, 4), ("Trigonometrie", 16, 14),
                     ("Systemes", 7, 6), ("Aire", 12, 5)
                 })
        {
            score.Topic(topic).Asked = asked;
            score.Topic(topic).Correct = correct;
        }

        for (int i = 0; i < 6; i++)
        {
            var question = QuestionGenerator.Next(3 + i % 2);
            var item = ReviewItem.From(question);
            item.Stage = i % 3;
            item.Misses = 1 + i % 3;
            item.DueAt = DateTime.Now.AddMinutes(i switch { 0 => -30, 1 => 25, 2 => 180, 3 => 1500, 4 => 3000, _ => 9000 });
            score.Review.Add(item);
        }

        for (int i = 0; i < 14; i++)
        {
            var question = QuestionGenerator.Next(3 + i % 3);
            bool ok = i % 3 != 1;
            score.Push(new HistoryEntry
            {
                Date = DateTime.Now.AddMinutes(-6 * i * i - 3),
                Level = question.Level,
                Topic = question.Topic,
                Prompt = question.Prompt,
                Expected = question.Expected,
                Given = ok ? question.Expected : "12",
                Outcome = ok ? AnswerOutcome.Correcte : AnswerOutcome.Fausse,
                Delta = ok ? question.BasePoints + 14 : -question.Level * 6,
                Seconds = 4 + i % 7
            });
        }

        return score;
    }

    /// <summary>
    /// Tire jusqu'a tomber sur la famille voulue. Le motif est compare sans accents ni
    /// casse : ce fichier s'ecrit sans accents, pas les enonces.
    /// </summary>
    private static Question Tirer(int niveau, string? motif)
    {
        if (motif is null) return QuestionGenerator.Next(niveau);

        for (int essai = 0; essai < 4000; essai++)
        {
            var question = QuestionGenerator.Next(niveau);
            if (question.Figure is not null && Plat(question.Prompt).Contains(Plat(motif))) return question;
        }

        throw new InvalidOperationException($"aucune question dessinee du niveau {niveau} ne contient \"{motif}\"");
    }

    /// <summary>Minuscules sans accents, pour comparer un motif ASCII a un enonce francais.</summary>
    private static string Plat(string text)
    {
        var sb = new System.Text.StringBuilder(text.Length);
        foreach (char c in text.Normalize(System.Text.NormalizationForm.FormD).ToLowerInvariant())
        {
            if (System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c)
                != System.Globalization.UnicodeCategory.NonSpacingMark) sb.Append(c);
        }
        return sb.ToString();
    }

    /// <summary>
    /// Enregistre le popup entier. On mesure la FENETRE et non le cadre : RenderTargetBitmap
    /// dessine dans le repere de la fenetre, donc la marge de 18 du cadre decalerait l'image
    /// et rognerait le bas.
    /// </summary>
    private static void Save(Window window, string path)
    {
        var root = (FrameworkElement)window.Content;
        int width = (int)Math.Ceiling(window.ActualWidth);
        int height = (int)Math.Ceiling(window.ActualHeight);
        if (width <= 0 || height <= 0) throw new InvalidOperationException($"{path} : la fenetre mesure 0");

        var bitmap = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
        bitmap.Render(root);

        var encoder = new PngBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(bitmap));

        using var file = File.Create(path);
        encoder.Save(file);
    }

    /// <summary>Fait tourner la boucle de messages : sans elle, aucune animation n'avance.</summary>
    private static void Pump(TimeSpan span)
    {
        var end = DateTime.Now + span;
        while (DateTime.Now < end)
        {
            Dispatcher.CurrentDispatcher.Invoke(() => { }, DispatcherPriority.Background);
            Thread.Sleep(15);
        }
    }

    private const BindingFlags Hidden = BindingFlags.Instance | BindingFlags.NonPublic;

    private static T Field<T>(object target, string name) =>
        (T)target.GetType().GetField(name, Hidden)!.GetValue(target)!;

    private static void Set(object target, string name, object value) =>
        target.GetType().GetField(name, Hidden)!.SetValue(target, value);

    private static void Invoke(object target, string name, params object[] arguments) =>
        target.GetType().GetMethod(name, Hidden)!.Invoke(target, arguments);
}
