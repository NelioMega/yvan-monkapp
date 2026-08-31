using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Threading;
using Shadow = System.Windows.Media.Effects.DropShadowEffect;
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
    private bool _hinted;

    private readonly Random _fx = new();

    /// <summary>Nettoyage de la couche d'effets : un seul minuteur, pas un par confetti.</summary>
    private readonly DispatcherTimer _sweep = new();

    /// <summary>Compteur de points qui grimpe au lieu de sauter d'un coup.</summary>
    private readonly DispatcherTimer _roll = new() { Interval = TimeSpan.FromMilliseconds(28) };

    private int _rollFrom;
    private int _rollTo;
    private int _rollStep;

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
        _sweep.Tick += (_, _) => ClearEffects();
        _roll.Tick += OnRollTick;

        Loaded += OnLoaded;
        KeyDown += OnWindowKeyDown;
        Closed += (_, _) =>
        {
            _countdown.Stop();
            _autoAdvance.Stop();
            _sweep.Stop();
            _roll.Stop();
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
        _hinted = false;

        PromptText.Text = Current.Prompt;
        ShowFigure();
        LevelText.Text = BadgeText();
        SpeechText.Text = first
            ? _run.Kind switch
            {
                RunKind.Interro => MonkaLines.ForExam(),
                RunKind.Entrainement => MonkaLines.ForTraining(),
                _ => _run.FromReview ? MonkaLines.ForReview() : MonkaLines.ForIntro(Current.Level)
            }
            : MonkaLines.ForNext();

        TimerText.Text = $"{Current.Seconds} s";
        TimerArea.Visibility = Visibility.Visible;
        InputArea.Visibility = Visibility.Visible;
        ResultArea.Visibility = Visibility.Collapsed;
        HintText.Text = "Entrée pour valider · F1 pour un indice · Échap pour abandonner";

        HintLine.Visibility = Visibility.Collapsed;
        HintLine.Text = "";
        HintButton.IsEnabled = Current.Hint.Length > 0;

        ClearEffects();

        AnswerBox.Clear();
        Face.Mood = FaceMood.Neutre;
        UpdateFooter();

        if (first) return;

        // les suivantes arrivent fenêtre déjà ouverte : on relance nous-mêmes le chrono
        AnswerBox.Focus();
        _elapsed.Restart();
        _countdown.Start();
    }

    /// <summary>
    /// Installe le schéma sous l'énoncé. Une question dessinée porte l'essentiel dans son
    /// dessin : on rend alors la vedette à la figure en réduisant le texte.
    /// </summary>
    private void ShowFigure()
    {
        var figure = Current.Figure;

        FigurePane.Figure = figure;
        FigurePane.Visibility = figure is null ? Visibility.Collapsed : Visibility.Visible;
        PromptText.FontSize = figure is null ? 30 : 22;
        PromptText.LineHeight = figure is null ? 38 : 28;

        string caption = figure?.Caption ?? "";
        FigureCaption.Text = caption;
        FigureCaption.Visibility = caption.Length == 0 ? Visibility.Collapsed : Visibility.Visible;
    }

    private string BadgeText()
    {
        if (_run.IsSeries)
        {
            string head = _run.Kind == RunKind.Interro ? "INTERRO" : _run.Label.ToUpperInvariant();
            return $"{head} · {_index + 1}/{_run.Questions.Count}";
        }

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
        if (e.Key == Key.F1)
        {
            e.Handled = true;
            RevealHint();
            return;
        }

        if (e.Key != Key.Escape) return;

        e.Handled = true;
        if (_answered) Advance();
        else Finish(AnswerOutcome.Abandon, AnswerBox.Text);
    }

    private void OnHint(object sender, RoutedEventArgs e) => RevealHint();

    /// <summary>
    /// Montre la méthode sans donner la réponse. Le chrono continue de tourner : l'indice
    /// coûte la moitié du gain, pas du temps.
    /// </summary>
    private void RevealHint()
    {
        if (_answered || _hinted || Current.Hint.Length == 0) return;

        _hinted = true;
        HintButton.IsEnabled = false;
        HintLine.Text = Current.Hint;
        HintLine.Visibility = Visibility.Visible;
        AnswerBox.Focus();
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

        int pointsBefore = _score.Points;
        var result = ScoreEngine.Apply(_score, Current, outcome, given, _elapsed.Elapsed.TotalSeconds,
            _run.FromReview, _hinted);
        if (outcome == AnswerOutcome.Correcte) _correctInRun++;

        int perfect = 0;
        if (!HasMore && _run.IsExam && _correctInRun == _run.Questions.Count)
        {
            perfect = _run.PerfectBonus;
            ScoreEngine.AwardBonus(_score, perfect);
        }

        Storage.Save(_score);

        ShowResult(result, given, perfect, pointsBefore);
        Completed?.Invoke(result);

        _autoAdvance.Interval = TimeSpan.FromSeconds(outcome == AnswerOutcome.Correcte ? 6 : 10);
        _autoAdvance.Start();
    }

    private void ShowResult(AnswerResult result, string given, int perfectBonus, int pointsBefore)
    {
        bool good = result.Outcome == AnswerOutcome.Correcte;

        InputArea.Visibility = Visibility.Collapsed;
        HintLine.Visibility = Visibility.Collapsed;
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
        Pop(DeltaPop, good ? 1.45 : 1.15);

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
        if (result.Hinted && good) bonus.Add("indice demandé, gain divisé par deux");
        if (perfectBonus > 0) bonus.Add($"interro sans faute +{perfectBonus}");
        if (result.LeftReview) bonus.Add("sortie du carnet d'erreurs");
        else if (!good && _run.FromReview) bonus.Add("elle repasse dans une heure");
        else if (!good) bonus.Add("elle reviendra dans une heure");
        if (result.RankChanged) bonus.Add(result.RankUp ? $"nouveau rang : {result.Rank.Name}" : $"rétrogradé : {result.Rank.Name}");
        BonusText.Text = string.Join("  ·  ", bonus);
        BonusText.Visibility = bonus.Count == 0 ? Visibility.Collapsed : Visibility.Visible;

        StreakText.Text = _score.Streak > 1 ? $"série de {_score.Streak}" : "";
        RollPoints(pointsBefore, _score.Points);
        if (_score.Streak > 1) Pop(StreakPop, 1.35);

        // le grand moment prend toute la place : la fanfare remplace le bip, elle ne s'y ajoute pas
        bool bigMoment = good && (result.RankUp || perfectBonus > 0);
        if (_settings.PlaySfx)
        {
            if (!good) Audio.PlayWrong(_settings.Volume);
            else if (bigMoment) Audio.PlayFanfare(_settings.Volume);
            else Audio.PlayCorrect(_settings.Volume, result.Streak - 1);
        }

        if (result.RankUp && _settings.PlayVoice) Audio.PlayIntro(_settings.Volume);

        Celebrate(result, perfectBonus, bigMoment);

        if (!good && result.Outcome != AnswerOutcome.Abandon) Shake();

        CloseButton.Focus();
    }

    // --- La fête ---------------------------------------------------------------------

    /// <summary>
    /// Ce qui se passe par-dessus le popup une fois la réponse tranchée : les points
    /// s'envolent, le cadre s'allume, et une bonne réponse fait pleuvoir des confettis —
    /// d'autant plus qu'elle vient de loin.
    /// </summary>
    private void Celebrate(AnswerResult result, int perfectBonus, bool bigMoment)
    {
        bool good = result.Outcome == AnswerOutcome.Correcte;

        FlyingPoints(result.Delta, good);
        FlashBorder(good ? "GoodBrush" : "BadBrush");

        if (!good) return;

        // une bonne réponse ordinaire mérite une poignée de confettis, un record en mérite une pluie
        int pieces = 14 + Math.Min(result.Streak, 8) * 3;
        if (perfectBonus > 0) pieces += 20;
        if (result.RankUp) pieces += 30;
        Confetti(pieces, bigMoment);

        // un seul bandeau à la fois, du plus rare au plus courant
        if (result.RankUp) Banner($"NOUVEAU RANG · {result.Rank.Name.ToUpperInvariant()}");
        else if (perfectBonus > 0) Banner("INTERRO SANS FAUTE");
        else if (result.LeftReview) Banner("SORTIE DU CARNET D'ERREURS");
        else if (result.Streak > 0 && result.Streak % 5 == 0) Banner($"SÉRIE DE {result.Streak} !");

        Face.Jump(bigMoment ? 1.22 : 1.1);
    }

    /// <summary>Le gain, en gros, qui monte au-dessus de l'énoncé avant de se dissiper.</summary>
    private void FlyingPoints(int delta, bool good)
    {
        var label = new TextBlock
        {
            Text = delta >= 0 ? $"+{delta}" : delta.ToString(),
            FontSize = good ? 54 : 40,
            FontWeight = FontWeights.Bold,
            Foreground = (Brush)FindResource(good ? "GoodBrush" : "BadBrush"),
            Effect = new Shadow
            {
                BlurRadius = 16, ShadowDepth = 0, Color = Colors.Black, Opacity = 0.75
            },
            Opacity = 0
        };

        var lift = new TranslateTransform();
        var swell = new ScaleTransform(0.5, 0.5);
        label.RenderTransformOrigin = new Point(0.5, 0.5);
        label.RenderTransform = new TransformGroup { Children = { swell, lift } };

        // les points partent au milieu de l'énoncé et montent : sur le panneau sombre ils
        // se lisent mieux qu'en haut, et ils ne croisent pas le bandeau des grands moments
        Place(label, 0.5, 0.52);

        var life = TimeSpan.FromMilliseconds(1150);

        var fade = new DoubleAnimationUsingKeyFrames { Duration = life };
        fade.KeyFrames.Add(new LinearDoubleKeyFrame(1, KeyTime.FromPercent(0.12)));
        fade.KeyFrames.Add(new LinearDoubleKeyFrame(1, KeyTime.FromPercent(0.55)));
        fade.KeyFrames.Add(new LinearDoubleKeyFrame(0, KeyTime.FromPercent(1)));
        label.BeginAnimation(OpacityProperty, fade);

        lift.BeginAnimation(TranslateTransform.YProperty, new DoubleAnimation(0, good ? -105 : -55, life)
        {
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
        });

        var grow = new DoubleAnimation(0.5, 1, TimeSpan.FromMilliseconds(420))
        {
            EasingFunction = new BackEase { EasingMode = EasingMode.EaseOut, Amplitude = 0.9 }
        };
        swell.BeginAnimation(ScaleTransform.ScaleXProperty, grow);
        swell.BeginAnimation(ScaleTransform.ScaleYProperty, grow);

        Sweep(life);
    }

    /// <summary>Une gerbe de papier : chaque morceau part en cloche, tourne, puis s'efface.</summary>
    private void Confetti(int pieces, bool wide)
    {
        Color[] palette =
        {
            Color.FromRgb(0x69, 0xC9, 0x7A), Color.FromRgb(0xC9, 0x9A, 0x4F),
            Color.FromRgb(0xE8, 0xB3, 0x4A), Color.FromRgb(0xF4, 0xF4, 0xEC),
            Color.FromRgb(0x8F, 0xD8, 0xB0)
        };

        var life = TimeSpan.FromMilliseconds(1500);
        double reach = wide ? 320 : 210;

        for (int i = 0; i < pieces; i++)
        {
            var piece = new Rectangle
            {
                Width = 5 + _fx.Next(4),
                Height = 9 + _fx.Next(6),
                RadiusX = 2,
                RadiusY = 2,
                Fill = new SolidColorBrush(palette[_fx.Next(palette.Length)]),
                RenderTransformOrigin = new Point(0.5, 0.5)
            };

            var spin = new RotateTransform(_fx.Next(360));
            var move = new TranslateTransform();
            piece.RenderTransform = new TransformGroup { Children = { spin, move } };

            Place(piece, 0.5, 0.44);

            // l'écart horizontal est tiré à plat, la hauteur en cloche : ça donne le jet
            double drift = (_fx.NextDouble() * 2 - 1) * reach;
            double peak = -(70 + _fx.NextDouble() * 130);

            move.BeginAnimation(TranslateTransform.XProperty, new DoubleAnimation(0, drift, life)
            {
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            });

            var arc = new DoubleAnimationUsingKeyFrames { Duration = life };
            arc.KeyFrames.Add(new EasingDoubleKeyFrame(peak, KeyTime.FromPercent(0.32),
                new QuadraticEase { EasingMode = EasingMode.EaseOut }));
            arc.KeyFrames.Add(new EasingDoubleKeyFrame(260, KeyTime.FromPercent(1),
                new QuadraticEase { EasingMode = EasingMode.EaseIn }));
            move.BeginAnimation(TranslateTransform.YProperty, arc);

            spin.BeginAnimation(RotateTransform.AngleProperty,
                new DoubleAnimation(spin.Angle, spin.Angle + _fx.Next(-720, 720), life));

            var vanish = new DoubleAnimationUsingKeyFrames { Duration = life };
            vanish.KeyFrames.Add(new LinearDoubleKeyFrame(1, KeyTime.FromPercent(0.62)));
            vanish.KeyFrames.Add(new LinearDoubleKeyFrame(0, KeyTime.FromPercent(1)));
            piece.BeginAnimation(OpacityProperty, vanish);
        }

        Sweep(life);
    }

    /// <summary>
    /// Le bandeau des grandes occasions. Il est posé sur une pastille sombre : en texte nu,
    /// il tomberait sur le nom du prof ou sur l'énoncé et deviendrait illisible.
    /// </summary>
    private void Banner(string text)
    {
        var chip = new Border
        {
            CornerRadius = new CornerRadius(11),
            Background = new SolidColorBrush(Color.FromArgb(0xE8, 0x0C, 0x17, 0x14)),
            BorderBrush = (Brush)FindResource("WoodBrush"),
            BorderThickness = new Thickness(1),
            Padding = new Thickness(15, 7, 15, 8),
            Effect = new Shadow
            {
                BlurRadius = 18, ShadowDepth = 3, Color = Colors.Black, Opacity = 0.7
            },
            Opacity = 0,
            RenderTransformOrigin = new Point(0.5, 0.5),
            Child = new TextBlock
            {
                Text = text,
                FontSize = 14,
                FontWeight = FontWeights.Bold,
                Foreground = (Brush)FindResource("WoodBrush")
            }
        };

        var swell = new ScaleTransform(0.7, 0.7);
        chip.RenderTransform = swell;

        // tout en haut : le bandeau et les points qui montent ne doivent pas se croiser
        Place(chip, 0.5, 0.085);

        var life = TimeSpan.FromMilliseconds(1900);

        var fade = new DoubleAnimationUsingKeyFrames { Duration = life };
        fade.KeyFrames.Add(new LinearDoubleKeyFrame(1, KeyTime.FromPercent(0.1)));
        fade.KeyFrames.Add(new LinearDoubleKeyFrame(1, KeyTime.FromPercent(0.7)));
        fade.KeyFrames.Add(new LinearDoubleKeyFrame(0, KeyTime.FromPercent(1)));
        chip.BeginAnimation(OpacityProperty, fade);

        var grow = new DoubleAnimation(0.7, 1, TimeSpan.FromMilliseconds(520))
        {
            EasingFunction = new BackEase { EasingMode = EasingMode.EaseOut, Amplitude = 1.1 }
        };
        swell.BeginAnimation(ScaleTransform.ScaleXProperty, grow);
        swell.BeginAnimation(ScaleTransform.ScaleYProperty, grow);

        Sweep(life);
    }

    /// <summary>
    /// Pose un élément dans la couche d'effets, centré sur une position relative. Il faut
    /// le mesurer d'abord : sans DesiredSize, impossible de le centrer sur son point.
    /// </summary>
    private void Place(FrameworkElement element, double relativeX, double relativeY)
    {
        Effects.Children.Add(element);
        element.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

        double width = Effects.ActualWidth > 0 ? Effects.ActualWidth : ActualWidth;
        double height = Effects.ActualHeight > 0 ? Effects.ActualHeight : ActualHeight;

        Canvas.SetLeft(element, width * relativeX - element.DesiredSize.Width / 2);
        Canvas.SetTop(element, height * relativeY - element.DesiredSize.Height / 2);
    }

    /// <summary>Le cadre du popup s'allume à la couleur du verdict, puis revient au bois.</summary>
    private void FlashBorder(string brushKey)
    {
        var lit = new SolidColorBrush(((SolidColorBrush)FindResource(brushKey)).Color);
        Root.BorderBrush = lit;

        lit.BeginAnimation(SolidColorBrush.ColorProperty,
            new ColorAnimation(((SolidColorBrush)FindResource("WoodBrush")).Color, TimeSpan.FromMilliseconds(950))
            {
                BeginTime = TimeSpan.FromMilliseconds(150),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            });
    }

    /// <summary>Un coup de zoom élastique, pour un texte qui vient de changer.</summary>
    private static void Pop(ScaleTransform target, double amplitude)
    {
        var pop = new DoubleAnimationUsingKeyFrames { Duration = TimeSpan.FromMilliseconds(600) };
        pop.KeyFrames.Add(new EasingDoubleKeyFrame(amplitude, KeyTime.FromPercent(0.25),
            new CubicEase { EasingMode = EasingMode.EaseOut }));
        pop.KeyFrames.Add(new EasingDoubleKeyFrame(1, KeyTime.FromPercent(1),
            new ElasticEase { EasingMode = EasingMode.EaseOut, Oscillations = 2, Springiness = 4 }));

        target.BeginAnimation(ScaleTransform.ScaleXProperty, pop);
        target.BeginAnimation(ScaleTransform.ScaleYProperty, pop);
    }

    /// <summary>Programme le nettoyage de la couche d'effets, sans jamais l'avancer.</summary>
    private void Sweep(TimeSpan life)
    {
        var margin = life + TimeSpan.FromMilliseconds(120);
        if (_sweep.IsEnabled && _sweep.Interval >= margin) return;

        _sweep.Stop();
        _sweep.Interval = margin;
        _sweep.Start();
    }

    private void ClearEffects()
    {
        _sweep.Stop();
        Effects.Children.Clear();
    }

    /// <summary>Le total du pied de fenêtre grimpe jusqu'au nouveau score au lieu d'y sauter.</summary>
    private void RollPoints(int from, int to)
    {
        _roll.Stop();

        if (from == to)
        {
            PointsText.Text = $"{to} pts";
            return;
        }

        _rollFrom = from;
        _rollTo = to;
        _rollStep = 0;
        _roll.Start();
    }

    /// <summary>Nombre de pas du compteur : dix-huit à 28 ms, soit une demi-seconde.</summary>
    private const int RollSteps = 18;

    private void OnRollTick(object? sender, EventArgs e)
    {
        _rollStep++;

        if (_rollStep >= RollSteps)
        {
            _roll.Stop();
            PointsText.Text = $"{_rollTo} pts";
            return;
        }

        // ralenti à l'arrivée : les derniers points se comptent un par un
        double progress = 1 - Math.Pow(1 - (double)_rollStep / RollSteps, 3);
        PointsText.Text = $"{(int)Math.Round(_rollFrom + (_rollTo - _rollFrom) * progress)} pts";
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
