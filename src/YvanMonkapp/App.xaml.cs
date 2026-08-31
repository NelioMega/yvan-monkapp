using System.Diagnostics;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using YvanMonkapp.Core;
using Forms = System.Windows.Forms;

namespace YvanMonkapp;

/// <summary>
/// L'application n'a pas de fenêtre principale : elle vit dans la zone de notification,
/// et n'ouvre un popup que quand le planificateur le décide.
/// </summary>
public partial class App : Application
{
    /// <summary>Nom de l'événement partagé : sert à la fois de verrou d'instance unique et de sonnette.</summary>
    private const string SingleInstanceEvent = "YvanMonkapp.ShowDashboard";

    private AppSettings _settings = new();
    private ScoreData _score = new();
    private Scheduler? _scheduler;

    private Forms.NotifyIcon? _tray;
    private Forms.ToolStripMenuItem? _pauseItem;
    private Forms.ToolStripMenuItem? _headerItem;

    private QuizWindow? _quiz;
    private DashboardWindow? _dashboard;
    private BulletinWindow? _bulletin;
    private BitmapImage? _windowIcon;
    private EventWaitHandle? _showSignal;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        if (!ClaimSingleInstance())
        {
            Shutdown();
            return;
        }

        DispatcherUnhandledException += OnUnhandledException;

        Paths.EnsureRoot();
        _settings = Storage.LoadSettings();
        _score = Storage.LoadScore();
        Audio.EnsureAudioFiles();
        Audio.EnsureVoiceFolders();

        EnsureStartupRegistered();

        bool background = HasFlag(e.Args, "--background");
        bool askNow = HasFlag(e.Args, "--question");
        bool showBulletin = HasFlag(e.Args, "--bulletin");
        Log.Write($"démarrage ({(background ? "arrière-plan" : "manuel")})");

        BuildTray();

        _scheduler = new Scheduler(_settings)
        {
            Blocked = IsBadMoment,
            Due = OnScheduled
        };
        _scheduler.Start();

        if (!_settings.FirstRunDone) ShowWelcome();
        else if (askNow) ShowQuiz();
        else if (showBulletin) ShowBulletin();
        else if (!background) ShowDashboard();
    }

    private static bool HasFlag(IEnumerable<string> args, string flag) =>
        args.Any(a => a.Equals(flag, StringComparison.OrdinalIgnoreCase));

    /// <summary>
    /// Le démarrage avec Windows fait partie du produit : on l'inscrit au premier lancement,
    /// sans rien demander. Il reste décochable dans le tableau de bord, et un décochage tient —
    /// on ne réécrit ensuite que si l'entrée existe déjà mais vise un exe qui a bougé.
    /// </summary>
    private void EnsureStartupRegistered()
    {
        if (!_settings.FirstRunDone)
        {
            StartupManager.Set(true);
            return;
        }

        if (StartupManager.HasEntry() && !StartupManager.IsEnabled()) StartupManager.Set(true);
    }

    // --- Instance unique -----------------------------------------------------------

    /// <summary>
    /// Vrai si c'est la première instance. Les suivantes font sonner l'événement puis
    /// s'arrêtent : relancer l'exe ouvre donc le tableau de bord de l'instance en place.
    /// </summary>
    private bool ClaimSingleInstance()
    {
        try
        {
            _showSignal = new EventWaitHandle(false, EventResetMode.AutoReset, SingleInstanceEvent, out bool created);
            if (!created)
            {
                _showSignal.Set();
                _showSignal.Dispose();
                _showSignal = null;
                return false;
            }

            var listener = new Thread(WaitForShowRequests) { IsBackground = true, Name = "YvanMonkapp.Signal" };
            listener.Start();
            return true;
        }
        catch (Exception ex)
        {
            Log.Write($"verrou d'instance impossible : {ex.Message}");
            return true;
        }
    }

    private void WaitForShowRequests()
    {
        while (_showSignal is not null)
        {
            try
            {
                if (!_showSignal.WaitOne()) continue;
                Dispatcher.Invoke(ShowDashboard);
            }
            catch (Exception ex)
            {
                Log.Write($"signal d'affichage perdu : {ex.Message}");
                return;
            }
        }
    }

    // --- Zone de notification ------------------------------------------------------

    private void BuildTray()
    {
        var menu = new Forms.ContextMenuStrip();

        _headerItem = new Forms.ToolStripMenuItem("Yvan Monk'app") { Enabled = false };
        _pauseItem = new Forms.ToolStripMenuItem("Mettre en pause", null, (_, _) => TogglePause());

        menu.Items.Add(_headerItem);
        menu.Items.Add(new Forms.ToolStripSeparator());
        menu.Items.Add(new Forms.ToolStripMenuItem("Question maintenant", null, (_, _) => ShowQuiz()));
        menu.Items.Add(new Forms.ToolStripMenuItem("Tableau de bord", null, (_, _) => ShowDashboard()));
        menu.Items.Add(new Forms.ToolStripMenuItem("Mon bulletin", null, (_, _) => ShowBulletin()));
        menu.Items.Add(new Forms.ToolStripMenuItem("Dossier de voix", null, (_, _) => OpenVoiceFolder()));
        menu.Items.Add(_pauseItem);
        menu.Items.Add(new Forms.ToolStripSeparator());
        menu.Items.Add(new Forms.ToolStripMenuItem("Quitter", null, (_, _) => Quit()));
        menu.Opening += (_, _) => RefreshMenu();

        _tray = new Forms.NotifyIcon
        {
            Icon = LoadTrayIcon(),
            ContextMenuStrip = menu,
            Visible = true
        };
        _tray.DoubleClick += (_, _) => Dispatcher.Invoke(ShowDashboard);

        RefreshMenu();
    }

    private static System.Drawing.Icon LoadTrayIcon()
    {
        var uri = new Uri("pack://application:,,,/Assets/monka.ico");
        using Stream stream = GetResourceStream(uri).Stream;
        return new System.Drawing.Icon(stream, Forms.SystemInformation.SmallIconSize);
    }

    private void RefreshMenu()
    {
        if (_tray is null) return;

        var rank = Ranks.Of(_score.Points);
        string next = !_settings.Enabled ? "en pause"
            : _scheduler?.NextAt is DateTime when ? $"vers {when:HH:mm}"
            : "bientôt";

        // l'infobulle est limitée à 63 caractères par Windows
        string tip = $"Yvan Monk'app — {_score.Points} pts ({next})";
        _tray.Text = tip.Length > 63 ? tip[..63] : tip;

        if (_headerItem is not null) _headerItem.Text = $"{_score.Points} pts · {rank.Name}";
        if (_pauseItem is not null) _pauseItem.Text = _settings.Enabled ? "Mettre en pause" : "Reprendre";
    }

    private void TogglePause()
    {
        _settings.Enabled = !_settings.Enabled;
        Storage.Save(_settings);

        if (_settings.Enabled && _scheduler?.NextAt is null) _scheduler?.ScheduleNext();

        RefreshMenu();
        _dashboard?.Refresh();
    }

    // --- Fenêtres -------------------------------------------------------------------

    private BitmapImage WindowIcon =>
        _windowIcon ??= new BitmapImage(new Uri("pack://application:,,,/Assets/monka.ico"));

    /// <summary>
    /// L'échéance du planificateur : le dimanche soir, le bulletin passe avant la question.
    /// </summary>
    private void OnScheduled()
    {
        if (IsBulletinDue())
        {
            ShowBulletin();
            return;
        }

        ShowQuiz();
    }

    /// <summary>Vrai le dimanche à partir de 18 h, une seule fois par semaine.</summary>
    private bool IsBulletinDue()
    {
        var now = DateTime.Now;
        if (now.DayOfWeek != DayOfWeek.Sunday || now.Hour < 18) return false;

        return _score.LastBulletin is null || _score.LastBulletin < Bulletin.StartOfWeek(now);
    }

    private void ShowBulletin()
    {
        if (_bulletin is not null)
        {
            BringToFront(_bulletin);
            return;
        }

        try
        {
            _score.LastBulletin = DateTime.Now;
            Storage.Save(_score);

            _bulletin = new BulletinWindow(Bulletin.ForWeek(_score, DateTime.Now)) { Icon = WindowIcon };
            _bulletin.Closed += (_, _) => _bulletin = null;
            _bulletin.Show();
            BringToFront(_bulletin);
        }
        catch (Exception ex)
        {
            Log.Write($"ouverture du bulletin impossible : {ex}");
            _bulletin = null;
        }
    }

    private static void OpenVoiceFolder()
    {
        try
        {
            Audio.EnsureVoiceFolders();
            Process.Start(new ProcessStartInfo(Paths.Voices) { UseShellExecute = true });
        }
        catch (Exception ex)
        {
            Log.Write($"ouverture du dossier de voix impossible : {ex.Message}");
        }
    }

    /// <summary>Une interro surprise sur dix popups, jamais deux dans la même heure et demie.</summary>
    private const double ExamChance = 0.10;

    private static readonly TimeSpan ExamCooldown = TimeSpan.FromMinutes(90);

    private readonly Random _rng = new();

    /// <summary>
    /// Décide de ce que le popup va poser. Le carnet d'erreurs passe avant tout : une question
    /// ratée dont l'heure est venue est plus utile qu'une nouvelle.
    /// </summary>
    private QuizRun BuildRun()
    {
        if (ScoreEngine.NextDue(_score, DateTime.Now) is ReviewItem due)
        {
            return QuizRun.Single(due.ToQuestion(), fromReview: true);
        }

        int level = ScoreEngine.LevelFor(_settings, _score);

        // les chapitres ratés repassent devant : c'est là que le travail se joue
        var focus = _score.WeakTopics().ToHashSet();

        if (_settings.Exams && level >= 2 && _rng.NextDouble() < ExamChance
            && (_score.LastExam is null || DateTime.Now - _score.LastExam > ExamCooldown))
        {
            _score.LastExam = DateTime.Now;

            var questions = new List<Question>();
            for (int i = 0; i < QuizRun.ExamLength; i++)
            {
                // la dernière monte d'un cran : une interro doit finir plus haut qu'elle ne commence
                bool last = i == QuizRun.ExamLength - 1;
                int at = last ? Math.Min(level + 1, Question.MaxLevel) : level;
                questions.Add(QuestionGenerator.Next(at, focus));
            }

            Log.Write($"interro surprise, niveau {level}");
            return QuizRun.Exam(questions);
        }

        return QuizRun.Single(QuestionGenerator.Next(level, focus));
    }

    /// <summary>Vrai quand il vaut mieux ne pas interrompre : popup déjà ouvert ou plein écran.</summary>
    private bool IsBadMoment()
    {
        if (_quiz is not null) return true;
        return _settings.SkipWhenFullscreen && ForegroundWatch.IsFullscreenAppActive();
    }

    private void ShowQuiz() => ShowRun(BuildRun());

    /// <summary>
    /// Une série demandée depuis le tableau de bord. Sans chapitre, elle balaie le niveau
    /// courant ; avec un chapitre, elle se cale sur le niveau où ce chapitre se pose.
    /// </summary>
    private void StartTraining(string? topic)
    {
        if (_quiz is not null)
        {
            _quiz.Activate();
            return;
        }

        int level = ScoreEngine.LevelFor(_settings, _score);
        List<Question> questions;
        string label;

        if (string.IsNullOrEmpty(topic))
        {
            label = "ENTRAÎNEMENT";
            var focus = _score.WeakTopics().ToHashSet();
            questions = Enumerable.Range(0, QuizRun.TrainingLength)
                .Select(_ => QuestionGenerator.Next(level, focus))
                .ToList();
        }
        else
        {
            level = QuestionGenerator.LevelWith(topic, level);
            label = topic;
            questions = Enumerable.Range(0, QuizRun.TrainingLength)
                .Select(_ => QuestionGenerator.NextFrom(level, topic))
                .ToList();
        }

        Log.Write($"entraînement : {label}, niveau {level}");
        ShowRun(QuizRun.Training(questions, label));
    }

    private void ShowRun(QuizRun run)
    {
        if (_quiz is not null)
        {
            _quiz.Activate();
            return;
        }

        try
        {
            _quiz = new QuizWindow(run, _score, _settings) { Icon = WindowIcon };
            _quiz.Completed += _ =>
            {
                RefreshMenu();
                if (_dashboard is not null)
                {
                    _dashboard.NextAt = _scheduler?.NextAt;
                    _dashboard.Refresh();
                }
            };
            _quiz.Closed += (_, _) =>
            {
                _quiz = null;
                RefreshMenu();
            };
            _quiz.Show();
        }
        catch (Exception ex)
        {
            Log.Write($"ouverture du popup impossible : {ex}");
            _quiz = null;
        }
    }

    private void ShowDashboard()
    {
        if (_dashboard is not null)
        {
            _dashboard.NextAt = _scheduler?.NextAt;
            _dashboard.Refresh();
            BringToFront(_dashboard);
            return;
        }

        _dashboard = new DashboardWindow(_settings, _score)
        {
            Icon = WindowIcon,
            NextAt = _scheduler?.NextAt
        };
        _dashboard.AskNowRequested += ShowQuiz;
        _dashboard.TrainingRequested += StartTraining;
        _dashboard.SettingsChanged += OnSettingsChanged;
        _dashboard.ScoreReset += ResetScore;
        _dashboard.BulletinRequested += ShowBulletin;
        _dashboard.Closed += (_, _) => _dashboard = null;
        _dashboard.Refresh();
        _dashboard.Show();
        BringToFront(_dashboard);
    }

    /// <summary>
    /// Windows refuse le premier plan à un processus qui n'a pas eu d'interaction récente :
    /// le passage éclair par Topmost force la fenêtre devant sans la punaiser.
    /// </summary>
    private static void BringToFront(Window window)
    {
        if (window.WindowState == WindowState.Minimized) window.WindowState = WindowState.Normal;

        window.Activate();
        window.Topmost = true;
        window.Topmost = false;
        window.Focus();
    }

    private void ShowWelcome()
    {
        var welcome = new WelcomeWindow(_settings.Volume) { Icon = WindowIcon };
        bool accepted = welcome.ShowDialog() == true;

        _settings.FirstRunDone = true;
        Storage.Save(_settings);

        if (accepted && welcome.AskNow) ShowQuiz();
        else ShowDashboard();
    }

    private void OnSettingsChanged()
    {
        RefreshMenu();

        if (_scheduler is null) return;

        if (!_settings.Enabled) return;

        // un rendez-vous plus lointain que la nouvelle borne haute n'a plus de sens
        if (_scheduler.NextAt is null || _scheduler.NextAt > DateTime.Now.AddMinutes(_settings.ClampedMax))
        {
            _scheduler.ScheduleNext();
        }

        if (_dashboard is not null) _dashboard.NextAt = _scheduler.NextAt;
    }

    private void ResetScore()
    {
        _score.Points = 0;
        _score.BestPoints = 0;
        _score.Streak = 0;
        _score.BestStreak = 0;
        _score.Asked = 0;
        _score.Correct = 0;
        _score.Wrong = 0;
        _score.Timeout = 0;
        _score.Abandoned = 0;
        _score.TotalAnswerSeconds = 0;
        _score.LastQuestion = null;
        _score.BestDailyStreak = 0;
        _score.Hints = 0;
        _score.LastExam = null;
        _score.LastBulletin = null;
        _score.ByLevel.Clear();
        _score.ByTopic.Clear();
        _score.Days.Clear();
        _score.Review.Clear();
        _score.History.Clear();

        Storage.Save(_score);
        RefreshMenu();
        Log.Write("score remis à zéro");
    }

    private void Quit()
    {
        Storage.Save(_settings);
        Storage.Save(_score);
        Shutdown();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _scheduler?.Stop();

        if (_tray is not null)
        {
            _tray.Visible = false;
            _tray.Dispose();
            _tray = null;
        }

        _showSignal?.Dispose();
        _showSignal = null;

        base.OnExit(e);
    }

    private static void OnUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        Log.Write($"exception non gérée : {e.Exception}");
        e.Handled = true;
    }
}
