using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using YvanMonkapp.Controls;
using YvanMonkapp.Core;

namespace YvanMonkapp;

/// <summary>Le bulletin hebdomadaire, façon carnet de notes, exportable en image.</summary>
public partial class BulletinWindow : Window
{
    private readonly Bulletin _bulletin;

    public BulletinWindow(Bulletin bulletin)
    {
        InitializeComponent();

        _bulletin = bulletin;

        PeriodText.Text = bulletin.Period;
        NoteText.Text = bulletin.Asked == 0
            ? "—"
            : $"{bulletin.Note.ToString("0.#", CultureInfo.InvariantCulture).Replace('.', ',')}";
        AppreciationText.Text = bulletin.Appreciation;

        LineList.ItemsSource = bulletin.Lines.Select(ToRow).ToList();
        EmptyText.Visibility = bulletin.Lines.Count == 0 ? Visibility.Visible : Visibility.Collapsed;

        Face.Mood = bulletin.Asked == 0 ? FaceMood.Neutre
            : bulletin.Note >= 16 ? FaceMood.Fier
            : bulletin.Note >= 10 ? FaceMood.Content
            : bulletin.Note >= 6 ? FaceMood.Fache
            : FaceMood.Furieux;

        var notes = new List<string>();
        if (bulletin.Asked > 0) notes.Add($"{bulletin.Correct} bonnes réponses sur {bulletin.Asked}");
        notes.Add(bulletin.Delta >= 0 ? $"+{bulletin.Delta} points sur la semaine" : $"{bulletin.Delta} points sur la semaine");
        if (bulletin.DailyStreak > 1) notes.Add($"série de {bulletin.DailyStreak} jours");
        if (bulletin.ReviewPending > 0) notes.Add($"{bulletin.ReviewPending} au carnet d'erreurs");
        FootnoteText.Text = string.Join("  ·  ", notes);
    }

    private LineRow ToRow(BulletinLine line)
    {
        var color = (Brush)FindResource(line.Note switch
        {
            >= 14 => "GoodBrush",
            >= 10 => "ChalkBrush",
            >= 8 => "WarnBrush",
            _ => "BadBrush"
        });

        var trendColor = (Brush)FindResource(line.Trend switch
        {
            "▲" => "GoodBrush",
            "▼" => "BadBrush",
            _ => "ChalkDimBrush"
        });

        string note = line.Note.ToString("0.#", CultureInfo.InvariantCulture).Replace('.', ',');

        return new LineRow(line.Topic, line.Asked.ToString(), $"{line.Accuracy * 100:0} %",
            $"{note}/20", line.Trend, color, trendColor);
    }

    private void OnExport(object sender, RoutedEventArgs e)
    {
        try
        {
            string folder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), "Yvan Monk'app");
            Directory.CreateDirectory(folder);

            string file = Path.Combine(folder, $"bulletin {_bulletin.To:yyyy-MM-dd}.png");
            File.WriteAllBytes(file, RenderCard());

            Log.Write($"bulletin exporté : {file}");

            // on ouvre l'explorateur avec le fichier déjà sélectionné
            Process.Start(new ProcessStartInfo("explorer.exe", $"/select,\"{file}\"") { UseShellExecute = true });
        }
        catch (Exception ex)
        {
            Log.Write($"export du bulletin impossible : {ex.Message}");
            MessageBox.Show(this, "Impossible d'enregistrer l'image du bulletin.",
                "Yvan Monk'app", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    /// <summary>
    /// Rend la carte en PNG, à deux fois la taille écran pour rester net une fois partagé.
    /// On passe par un DrawingVisual : rendre directement le Border embarquerait ses marges.
    /// </summary>
    private byte[] RenderCard()
    {
        const double scale = 2;

        double width = Card.ActualWidth;
        double height = Card.ActualHeight;

        var visual = new DrawingVisual();
        using (DrawingContext context = visual.RenderOpen())
        {
            context.DrawRectangle(new VisualBrush(Card), null, new Rect(0, 0, width, height));
        }

        var target = new RenderTargetBitmap(
            (int)Math.Ceiling(width * scale), (int)Math.Ceiling(height * scale),
            96 * scale, 96 * scale, PixelFormats.Pbgra32);
        target.Render(visual);

        var encoder = new PngBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(target));

        using var stream = new MemoryStream();
        encoder.Save(stream);
        return stream.ToArray();
    }

    private void OnClose(object sender, RoutedEventArgs e) => Close();

    private void OnDragArea(object sender, MouseButtonEventArgs e)
    {
        if (e.ButtonState == MouseButtonState.Pressed) DragMove();
    }

    private sealed record LineRow(string Topic, string Asked, string Success, string Note, string Trend,
        Brush Color, Brush TrendColor);
}
