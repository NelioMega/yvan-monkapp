using System.Windows.Threading;

namespace YvanMonkapp.Core;

/// <summary>
/// Décide quand Yvan débarque. Un tick court plutôt qu'un long minuteur : la mise en
/// veille du PC ne décale pas l'échéance et le prochain rendez-vous reste consultable.
/// </summary>
public sealed class Scheduler
{
    private static readonly TimeSpan Tick = TimeSpan.FromSeconds(15);
    private static readonly TimeSpan Postpone = TimeSpan.FromMinutes(5);

    private readonly Random _rng = new();
    private readonly DispatcherTimer _timer;
    private readonly AppSettings _settings;

    /// <summary>Rendu vrai quand le moment est mal choisi (plein écran, popup déjà ouvert).</summary>
    public Func<bool>? Blocked { get; set; }

    /// <summary>Appelé quand une question doit être posée.</summary>
    public Action? Due { get; set; }

    public DateTime? NextAt { get; private set; }

    public Scheduler(AppSettings settings)
    {
        _settings = settings;
        _timer = new DispatcherTimer { Interval = Tick };
        _timer.Tick += OnTick;
    }

    public void Start()
    {
        ScheduleNext();
        _timer.Start();
    }

    public void Stop()
    {
        _timer.Stop();
        NextAt = null;
    }

    /// <summary>Tire un nouveau délai dans la plage réglée.</summary>
    public void ScheduleNext()
    {
        int min = _settings.ClampedMin;
        int max = _settings.ClampedMax;
        double minutes = min + _rng.NextDouble() * (max - min);

        NextAt = DateTime.Now.AddMinutes(minutes);
        Log.Write($"prochaine question vers {NextAt:HH:mm}");
    }

    /// <summary>Repousse l'échéance de quelques minutes sans en retirer une nouvelle.</summary>
    public void Postpone5() => NextAt = DateTime.Now + Postpone;

    private void OnTick(object? sender, EventArgs e)
    {
        if (!_settings.Enabled || NextAt is not DateTime due) return;
        if (DateTime.Now < due) return;

        if (_settings.IsQuiet(DateTime.Now) || Blocked?.Invoke() == true)
        {
            Postpone5();
            return;
        }

        ScheduleNext();
        Due?.Invoke();
    }
}
