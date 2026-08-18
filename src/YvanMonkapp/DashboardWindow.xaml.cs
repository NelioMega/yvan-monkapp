using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using YvanMonkapp.Controls;
using YvanMonkapp.Core;

namespace YvanMonkapp;

/// <summary>Tableau de bord : le score d'un côté, les réglages de l'autre.</summary>
public partial class DashboardWindow : Window
{
    private readonly AppSettings _settings;
    private readonly ScoreData _score;

    /// <summary>
    /// Vrai pendant le remplissage des contrôles : les handlers ne doivent rien enregistrer.
    /// Démarre à vrai car les Slider lèvent ValueChanged dès le chargement du XAML, alors
    /// que les autres champs nommés ne sont pas encore affectés.
    /// </summary>
    private bool _loading = true;

    public DashboardWindow(AppSettings settings, ScoreData score)
    {
        InitializeComponent();

        _settings = settings;
        _score = score;

        Refresh();
    }

    /// <summary>Demandé par le bouton « Question maintenant ».</summary>
    public event Action? AskNowRequested;

    /// <summary>Un réglage a changé : l'application doit relire la planification.</summary>
    public event Action? SettingsChanged;

    /// <summary>Le score a été remis à zéro.</summary>
    public event Action? ScoreReset;

    /// <summary>Demandé par le bouton « Bulletin ».</summary>
    public event Action? BulletinRequested;

    /// <summary>Heure du prochain passage, affichée sous l'interrupteur principal.</summary>
    public DateTime? NextAt { get; set; }

    public void Refresh()
    {
        _loading = true;

        var rank = Ranks.Of(_score.Points);
        RankText.Text = rank.Name;
        PointsText.Text = $"{_score.Points} pts";
        StreakText.Text = _score.Streak > 0 ? $"série en cours : {_score.Streak}" : "aucune série en cours";

        int next = Ranks.NextThreshold(_score.Points);
        NextRankText.Text = next == int.MaxValue
            ? "Rang maximal atteint. Il n'y a plus rien à vous apprendre."
            : $"{Math.Max(0, next - _score.Points)} points avant le rang suivant";
        SizeRankBar();

        Face.Mood = _score.Points < 0 ? FaceMood.Fache
            : _score.Streak >= 4 ? FaceMood.Fier
            : _score.Points > 0 ? FaceMood.Content
            : FaceMood.Neutre;

        AccuracyText.Text = _score.Asked == 0 ? "—" : $"{_score.Accuracy * 100:0} %";
        AskedText.Text = _score.Asked.ToString();
        BestStreakText.Text = _score.BestStreak.ToString();
        SpeedText.Text = _score.Correct == 0 ? "—" : $"{_score.AverageSeconds:0.0} s";

        HistoryList.ItemsSource = _score.History.Take(30).Select(ToRow).ToList();
        BuildCalendar();

        EnabledCheck.IsChecked = _settings.Enabled;
        ExamCheck.IsChecked = _settings.Exams;
        StartupCheck.IsChecked = StartupManager.IsEnabled();
        AutoLevelCheck.IsChecked = _settings.Difficulty == DifficultyMode.Auto;
        IntroCheck.IsChecked = _settings.PlayVoice;
        SfxCheck.IsChecked = _settings.PlaySfx;
        FullscreenCheck.IsChecked = _settings.SkipWhenFullscreen;
        QuietCheck.IsChecked = _settings.QuietHours;

        MinSlider.Value = _settings.ClampedMin;
        MaxSlider.Value = _settings.ClampedMax;
        LevelSlider.Value = Math.Clamp(_settings.FixedLevel, 1, Question.MaxLevel);
        VolumeSlider.Value = Math.Round(_settings.Volume * 100);
        QuietFromSlider.Value = Math.Clamp(_settings.QuietFromHour, 0, 23);
        QuietToSlider.Value = Math.Clamp(_settings.QuietToHour, 0, 23);

        _loading = false;

        UpdateLabels();
    }

    /// <summary>
    /// Six mois d'activité, une case par jour, les lundis sur la première ligne.
    /// L'UniformGrid se remplit ligne par ligne : on parcourt donc les jours de la semaine
    /// à l'extérieur et les semaines à l'intérieur.
    /// </summary>
    private void BuildCalendar()
    {
        const int weeks = 26;

        int streak = _score.DailyStreak();
        StreakHeaderText.Text = streak switch
        {
            0 => "AUCUNE SÉRIE EN COURS",
            1 => "SÉRIE : 1 JOUR",
            _ => $"SÉRIE : {streak} JOURS D'AFFILÉE"
        };
        StreakRecordText.Text = _score.BestDailyStreak > 0 ? $"record : {_score.BestDailyStreak} j" : "";

        // on recule jusqu'au lundi qui ouvre la fenêtre de dix semaines
        var start = DateTime.Today.AddDays(-7 * (weeks - 1));
        start = start.AddDays(-(((int)start.DayOfWeek + 6) % 7));

        CalendarGrid.Children.Clear();
        for (int row = 0; row < 7; row++)
        {
            for (int col = 0; col < weeks; col++)
            {
                var day = start.AddDays(col * 7 + row);
                CalendarGrid.Children.Add(DayCell(day));
            }
        }
    }

    private Border DayCell(DateTime day)
    {
        var stat = _score.DayOrNull(day);
        bool future = day > DateTime.Today;

        var cell = new Border
        {
            CornerRadius = new CornerRadius(3),
            Margin = new Thickness(1.5),
            Background = future ? Brushes.Transparent : ShadeFor(stat),
            ToolTip = future ? null : Describe(day, stat)
        };

        if (day == DateTime.Today)
        {
            cell.BorderThickness = new Thickness(1.5);
            cell.BorderBrush = (Brush)FindResource("WoodBrush");
        }

        return cell;
    }

    private Brush ShadeFor(DayStat? stat)
    {
        if (stat is null || stat.Asked == 0) return new SolidColorBrush(Color.FromRgb(0x12, 0x21, 0x1C));
        if (stat.Correct == 0) return (Brush)FindResource("BadBrush");

        return stat.Correct switch
        {
            <= 2 => new SolidColorBrush(Color.FromRgb(0x2F, 0x5E, 0x3A)),
            <= 5 => new SolidColorBrush(Color.FromRgb(0x45, 0x8C, 0x52)),
            _ => (Brush)FindResource("GoodBrush")
        };
    }

    private static string Describe(DateTime day, DayStat? stat)
    {
        string date = day.ToString("dddd d MMMM");
        if (stat is null || stat.Asked == 0) return $"{date} : rien";

        string delta = stat.Delta >= 0 ? $"+{stat.Delta}" : stat.Delta.ToString();
        return $"{date} : {stat.Correct}/{stat.Asked} bonnes réponses, {delta} pts";
    }

    private HistoryRow ToRow(HistoryEntry entry)
    {
        bool good = entry.Outcome == AnswerOutcome.Correcte;

        string verdict = entry.Outcome switch
        {
            AnswerOutcome.Correcte => $"{entry.Seconds:0.0} s",
            AnswerOutcome.TempsEcoule => "temps écoulé",
            AnswerOutcome.Abandon => "abandon",
            _ => $"répondu {entry.Given} au lieu de {entry.Expected}"
        };

        return new HistoryRow(
            entry.Prompt,
            $"{Ago(entry.Date)} · niveau {entry.Level} · {verdict}",
            entry.Delta >= 0 ? $"+{entry.Delta}" : entry.Delta.ToString(),
            (Brush)FindResource(good ? "GoodBrush" : "BadBrush"));
    }

    private static string Ago(DateTime when)
    {
        TimeSpan span = DateTime.Now - when;
        if (span < TimeSpan.FromMinutes(1)) return "à l'instant";
        if (span < TimeSpan.FromHours(1)) return $"il y a {(int)span.TotalMinutes} min";
        if (span < TimeSpan.FromDays(1)) return $"il y a {(int)span.TotalHours} h";
        return when.ToString("dd/MM à HH:mm");
    }

    private void UpdateLabels()
    {
        FrequencyText.Text = $"Une question toutes les {(int)MinSlider.Value} à {(int)MaxSlider.Value} minutes";
        LevelText.Text = _settings.Difficulty == DifficultyMode.Auto
            ? $"Niveau suivi : {ScoreEngine.LevelFor(_settings, _score)} · {Question.LevelName(ScoreEngine.LevelFor(_settings, _score))}"
            : $"Niveau imposé : {(int)LevelSlider.Value} · {Question.LevelName((int)LevelSlider.Value)}";
        LevelSlider.IsEnabled = _settings.Difficulty == DifficultyMode.Fixe;

        VolumeText.Text = $"Volume : {(int)VolumeSlider.Value} %";
        QuietText.Text = $"Pas de question entre {(int)QuietFromSlider.Value} h et {(int)QuietToSlider.Value} h";
        QuietFromSlider.IsEnabled = _settings.QuietHours;
        QuietToSlider.IsEnabled = _settings.QuietHours;

        NextText.Text = !_settings.Enabled
            ? "En pause : plus aucune question."
            : NextAt is DateTime when
                ? $"Prochaine question vers {when:HH:mm}."
                : "";
    }

    private void SizeRankBar()
    {
        RankFill.Width = RankTrack.ActualWidth * Ranks.Progress(_score.Points);
    }

    private void OnRankTrackResized(object sender, SizeChangedEventArgs e) => SizeRankBar();

    private void Persist()
    {
        Storage.Save(_settings);
        SettingsChanged?.Invoke();
        UpdateLabels();
    }

    private void OnSettingChanged(object sender, RoutedEventArgs e)
    {
        if (_loading) return;

        _settings.Enabled = EnabledCheck.IsChecked == true;
        _settings.Exams = ExamCheck.IsChecked == true;
        _settings.PlayVoice = IntroCheck.IsChecked == true;
        _settings.PlaySfx = SfxCheck.IsChecked == true;
        _settings.SkipWhenFullscreen = FullscreenCheck.IsChecked == true;
        _settings.QuietHours = QuietCheck.IsChecked == true;
        _settings.Difficulty = AutoLevelCheck.IsChecked == true ? DifficultyMode.Auto : DifficultyMode.Fixe;

        Persist();
    }

    private void OnStartupChanged(object sender, RoutedEventArgs e)
    {
        if (_loading) return;

        bool wanted = StartupCheck.IsChecked == true;
        if (!StartupManager.Set(wanted))
        {
            MessageBox.Show(this,
                "Impossible d'écrire le démarrage automatique dans le registre.",
                "Yvan Monk'app", MessageBoxButton.OK, MessageBoxImage.Warning);
            StartupCheck.IsChecked = StartupManager.IsEnabled();
        }
    }

    private void OnFrequencyChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (_loading) return;

        // la borne haute ne peut pas passer sous la borne basse
        if (MaxSlider.Value < MinSlider.Value)
        {
            if (ReferenceEquals(sender, MinSlider)) MaxSlider.Value = MinSlider.Value;
            else MinSlider.Value = MaxSlider.Value;
        }

        _settings.MinMinutes = (int)MinSlider.Value;
        _settings.MaxMinutes = (int)MaxSlider.Value;
        Persist();
    }

    private void OnLevelChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (_loading) return;

        _settings.FixedLevel = (int)LevelSlider.Value;
        Persist();
    }

    private void OnVolumeChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (_loading) return;

        _settings.Volume = VolumeSlider.Value / 100.0;
        Persist();
    }

    private void OnQuietChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (_loading) return;

        _settings.QuietFromHour = (int)QuietFromSlider.Value;
        _settings.QuietToHour = (int)QuietToSlider.Value;
        Persist();
    }

    private void OnAskNow(object sender, RoutedEventArgs e) => AskNowRequested?.Invoke();

    private void OnBulletin(object sender, RoutedEventArgs e) => BulletinRequested?.Invoke();

    private void OnReset(object sender, RoutedEventArgs e)
    {
        var answer = MessageBox.Show(this,
            "Effacer le score, les séries et tout l'historique ?",
            "Yvan Monk'app", MessageBoxButton.YesNo, MessageBoxImage.Question);
        if (answer != MessageBoxResult.Yes) return;

        ScoreReset?.Invoke();
        Refresh();
    }

    private void OnDragArea(object sender, MouseButtonEventArgs e)
    {
        if (e.ButtonState == MouseButtonState.Pressed) DragMove();
    }

    private void OnClose(object sender, RoutedEventArgs e) => Close();

    private sealed record HistoryRow(string Prompt, string Detail, string Delta, Brush Color);
}
