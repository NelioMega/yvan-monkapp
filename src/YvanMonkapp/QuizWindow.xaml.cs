using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using YvanMonkapp.Controls;
using YvanMonkapp.Core;

namespace YvanMonkapp;

/// <summary>Le popup : une question (ou cinq en interro), un chrono, puis la correction d'Yvan.</summary>
public partial class QuizWindow : Window
{
    private readonly QuizRun _run;
    private readonly ScoreData _score;
    private readonly AppSettings _settings;

    private readonly DispatcherTimer _countdown = new() { Interval = TimeSpan.FromMilliseconds(100) };
    private readonly DispatcherTimer _autoAdvance = new();
    private readonly Stopwatch _elapsed = new();

    private int _index;
    private int _correctInRun;
    private bool _answered;

    /// <summary>Déclenché après chaque réponse, une fois le score enregistré.</summary>
    public event Action<AnswerResult>? Completed;

    public QuizWindow(QuizRun run, ScoreData score, AppSettings settings)
    {
        InitializeComponent();

        _run = run;
        _score = score;
        _settings = settings;

        _countdown.Tick += OnCountdown;
        _autoAdvance.Tick += (_, _) => Advance();

        Loaded += OnLoaded;
        KeyDown += OnWindowKeyDown;
        Closed += (_, _) =>
        {
            _countdown.Stop();
            _autoAdvance.Stop();
            Audio.StopVoice();
        };

        LoadQuestion(first: true);
    }

    private Question Current => _run.Questions[_index];

    private bool HasMore => _index < _run.Questions.Count - 1;

    private void OnLoaded(object? sender, RoutedEventArgs e)
    {
        PlaceOnActiveScreen();

        Root.BeginAnimation(OpacityProperty, new DoubleAnimation(1, TimeSpan.FromMilliseconds(220)));
        ShakeShift.BeginAnimation(TranslateTransform.YProperty, new DoubleAnimation(0, TimeSpan.FromMilliseconds(280))
        {
            EasingFunction = new BackEase { EasingMode = EasingMode.EaseOut, Amplitude = 0.35 }
        });

        // une seule salutation, même quand cinq questions s'enchaînent
        if (_settings.PlayVoice) Audio.PlayGreeting(_settings.Volume);

        Activate();
        AnswerBox.Focus();

        _elapsed.Restart();
        _countdown.Start();
    }

    /// <summary>Remet la fenêtre en mode question, pour la première comme pour les suivantes.</summary>
    private void LoadQuestion(bool first)
    {
        _answered = false;

        PromptText.Text = Current.Prompt;
        LevelText.Text = BadgeText();
        SpeechText.Text = first
            ? _run.IsExam ? MonkaLines.ForExam() : _run.FromReview ? MonkaLines.ForReview() : MonkaLines.ForIntro()
            : MonkaLines.ForNext();

        TimerText.Text = $"{Current.Seconds} s";
        TimerArea.Visibility = Visibility.Visible;
        InputArea.Visibility = Visibility.Visible;
        ResultArea.Visibility = Visibility.Collapsed;
        HintText.Text = "Entrée pour valider · Échap pour abandonner";

        AnswerBox.Clear();
        Face.Mood = FaceMood.Neutre;
        UpdateFooter();

        if (first) return;

        // les suivantes arrivent fenêtre déjà ouverte : on relance nous-mêmes le chrono
        AnswerBox.Focus();
        _elapsed.Restart();
        _countdown.Start();
    }

    private string BadgeText()
    {
        if (_run.IsExam) return $"INTERRO · {_index + 1}/{_run.Questions.Count}";
        if (_run.FromReview) return $"RÉVISION · {Current.Topic.ToUpperInvariant()}";
        return $"NIVEAU {Current.Level} · {Current.Topic.ToUpperInvariant()}";
    }

    /// <summary>Centre le popup sur l'écran où travaille l'utilisateur, pas sur le principal.</summary>
    private void PlaceOnActiveScreen()
    {
        var screen = System.Windows.Forms.Screen.FromPoint(System.Windows.Forms.Cursor.Position);
        var area = screen.WorkingArea;

        // WorkingArea est en pixels physiques : on repasse en unités WPF
        var source = PresentationSource.FromVisual(this);
        double scaleX = source?.CompositionTarget?.TransformToDevice.M11 ?? 1;
        double scaleY = source?.CompositionTarget?.TransformToDevice.M22 ?? 1;

        Left = area.Left / scaleX + (area.Width / scaleX - ActualWidth) / 2;
        Top = area.Top / scaleY + (area.Height / scaleY - ActualHeight) / 2;
    }

    private void OnCountdown(object? sender, EventArgs e)
    {
        double left = Math.Max(0, Current.Seconds - _elapsed.Elapsed.TotalSeconds);
        double ratio = left / Current.Seconds;

        TimerText.Text = $"{Math.Ceiling(left)} s";
        TimerFill.Width = Math.Max(0, TimerTrack.ActualWidth * ratio);
        TimerFill.Background = ratio switch
        {
            < 0.2 => (Brush)FindResource("BadBrush"),
            < 0.45 => (Brush)FindResource("WarnBrush"),
            _ => (Brush)FindResource("GoodBrush")
        };

        // dernières secondes : le prof commence à froncer les sourcils
        if (!_answered && ratio < 0.2 && Face.Mood == FaceMood.Neutre) Face.Mood = FaceMood.Fache;

        if (left <= 0) Finish(AnswerOutcome.TempsEcoule, "");
    }

    private void OnTimerTrackResized(object sender, SizeChangedEventArgs e)
    {
        if (_answered) return;
        double ratio = Math.Max(0, Current.Seconds - _elapsed.Elapsed.TotalSeconds) / Current.Seconds;
        TimerFill.Width = Math.Max(0, TimerTrack.ActualWidth * ratio);
    }

    private void OnAnswerKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key is Key.Enter or Key.Return)
        {
            e.Handled = true;
            Submit();
        }
    }

    private void OnWindowKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Escape) return;

        e.Handled = true;
        if (_answered) Advance();
        else Finish(AnswerOutcome.Abandon, AnswerBox.Text);
    }

    private void OnSubmit(object sender, RoutedEventArgs e) => Submit();

    private void OnGiveUp(object sender, RoutedEventArgs e) => Finish(AnswerOutcome.Abandon, AnswerBox.Text);

    private void OnClose(object sender, RoutedEventArgs e) => Advance();

    private void OnDragArea(object sender, MouseButtonEventArgs e)
    {
        if (e.ButtonState == MouseButtonState.Pressed) DragMove();
    }

    private void Submit()
    {
        if (_answered) return;

        string given = AnswerBox.Text.Trim();
        if (given.Length == 0)
        {
            Nudge();
            return;
        }

        Finish(Answers.Matches(Current, given) ? AnswerOutcome.Correcte : AnswerOutcome.Fausse, given);
    }

    /// <summary>Passe à la question suivante de l'interro, ou ferme le popup.</summary>
    private void Advance()
    {
        _autoAdvance.Stop();

        if (!HasMore)
        {
            Close();
            return;
        }

        _index++;
        LoadQuestion(first: false);
    }

    private void Finish(AnswerOutcome outcome, string given)
    {
        if (_answered) return;
        _answered = true;

        _countdown.Stop();
        _elapsed.Stop();
        Audio.StopVoice();

        var result = ScoreEngine.Apply(_score, Current, outcome, given, _elapsed.Elapsed.TotalSeconds, _run.FromReview);
        if (outcome == AnswerOutcome.Correcte) _correctInRun++;

        int perfect = 0;
        if (!HasMore && _run.IsExam && _correctInRun == _run.Questions.Count)
        {
            perfect = _run.PerfectBonus;
            ScoreEngine.AwardBonus(_score, perfect);
        }

        Storage.Save(_score);

        ShowResult(result, given, perfect);
        Completed?.Invoke(result);

        _autoAdvance.Interval = TimeSpan.FromSeconds(outcome == AnswerOutcome.Correcte ? 6 : 10);
        _autoAdvance.Start();
    }

    private void ShowResult(AnswerResult result, string given, int perfectBonus)
    {
        bool good = result.Outcome == AnswerOutcome.Correcte;

        InputArea.Visibility = Visibility.Collapsed;
        // le chrono n'a plus rien à dire : sa piste vide ferait une barre morte
        TimerArea.Visibility = Visibility.Collapsed;
        ResultArea.Visibility = Visibility.Visible;

        CloseButton.Content = HasMore ? "Suivante" : "Compris";
        HintText.Text = HasMore ? "Échap ou Suivante pour continuer" : "Échap ou Compris pour fermer";

        Face.Mood = good
            ? result.Streak >= 4 ? FaceMood.Fier : FaceMood.Content
            : result.Outcome == AnswerOutcome.Abandon ? FaceMood.Fache : FaceMood.Furieux;

        SpeechText.Text = good ? MonkaLines.ForCorrect(result.Streak - 1) : MonkaLines.ForOutcome(result.Outcome);

        DeltaText.Text = result.Delta >= 0 ? $"+{result.Delta}" : result.Delta.ToString();
        DeltaText.Foreground = (Brush)FindResource(good ? "GoodBrush" : "BadBrush");

        VerdictText.Text = result.Outcome switch
        {
            AnswerOutcome.Correcte => $"Bonne réponse en {_elapsed.Elapsed.TotalSeconds:0.0} s",
            AnswerOutcome.Fausse => "Faux",
            AnswerOutcome.TempsEcoule => "Temps écoulé",
            _ => "Abandon"
        };

        ExpectedText.Text = good
            ? $"Réponse : {Current.Expected}"
            : given.Length > 0
                ? $"Vous avez répondu {given} — la réponse était {Current.Expected}"
                : $"La réponse était {Current.Expected}";

        ExplanationText.Text = Current.Explanation;

        var bonus = new List<string>();
        if (result.SpeedBonus > 0) bonus.Add($"vitesse +{result.SpeedBonus}");
        if (result.StreakBonus > 0) bonus.Add($"série +{result.StreakBonus}");
        if (perfectBonus > 0) bonus.Add($"interro sans faute +{perfectBonus}");
        if (result.LeftReview) bonus.Add("sortie du carnet d'erreurs");
        else if (!good && _run.FromReview) bonus.Add("elle repasse dans une heure");
        else if (!good) bonus.Add("elle reviendra dans une heure");
        if (result.RankChanged) bonus.Add(result.RankUp ? $"nouveau rang : {result.Rank.Name}" : $"rétrogradé : {result.Rank.Name}");
        BonusText.Text = string.Join("  ·  ", bonus);
        BonusText.Visibility = bonus.Count == 0 ? Visibility.Collapsed : Visibility.Visible;

        UpdateFooter();

        if (_settings.PlaySfx)
        {
            if (good) Audio.PlayCorrect(_settings.Volume);
            else Audio.PlayWrong(_settings.Volume);
        }

        if (result.RankUp && _settings.PlayVoice) Audio.PlayIntro(_settings.Volume);

        if (!good && result.Outcome != AnswerOutcome.Abandon) Shake();

        CloseButton.Focus();
    }

    private void UpdateFooter()
    {
        PointsText.Text = $"{_score.Points} pts";
        StreakText.Text = _score.Streak > 1 ? $"série de {_score.Streak}" : "";
    }

    /// <summary>Champ vide : on secoue à peine, sans compter de réponse.</summary>
    private void Nudge()
    {
        AnswerBox.Focus();
        Shake(6, 220);
    }

    private void Shake(double amplitude = 14, double milliseconds = 420)
    {
        var shake = new DoubleAnimationUsingKeyFrames { Duration = TimeSpan.FromMilliseconds(milliseconds) };
        double[] steps = { 0, 1, -0.85, 0.65, -0.45, 0.25, 0 };

        for (int i = 0; i < steps.Length; i++)
        {
            double at = milliseconds * i / (steps.Length - 1);
            shake.KeyFrames.Add(new LinearDoubleKeyFrame(steps[i] * amplitude,
                KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(at))));
        }

        ShakeShift.BeginAnimation(TranslateTransform.XProperty, shake);
    }
}
