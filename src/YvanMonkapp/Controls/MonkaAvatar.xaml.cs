using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace YvanMonkapp.Controls;

public enum FaceMood
{
    Neutre,
    Content,
    Fier,
    Fache,
    Furieux
}

public partial class MonkaAvatar : UserControl
{
    public static readonly DependencyProperty MoodProperty = DependencyProperty.Register(
        nameof(Mood), typeof(FaceMood), typeof(MonkaAvatar),
        new PropertyMetadata(FaceMood.Neutre, OnMoodChanged));

    public MonkaAvatar()
    {
        InitializeComponent();
        Apply(FaceMood.Neutre, animate: false);
    }

    public FaceMood Mood
    {
        get => (FaceMood)GetValue(MoodProperty);
        set => SetValue(MoodProperty, value);
    }

    private static void OnMoodChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is MonkaAvatar avatar) avatar.Apply((FaceMood)e.NewValue, animate: true);
    }

    private void Apply(FaceMood mood, bool animate)
    {
        // dark = part de photo sombre, glow = halo rouge, ring = couleur du cerclage
        (double dark, double glow, string ring) = mood switch
        {
            FaceMood.Content => (0d, 0d, "#69C97A"),
            FaceMood.Fier => (0d, 0d, "#C99A4F"),
            FaceMood.Fache => (0.65d, 0.5d, "#E8B34A"),
            FaceMood.Furieux => (1d, 1d, "#E2574C"),
            _ => (0d, 0d, "#9EB3A8")
        };

        var stroke = (Color)ColorConverter.ConvertFromString(ring);

        if (animate)
        {
            Dark.BeginAnimation(OpacityProperty, Fade(dark));
            Glow.BeginAnimation(OpacityProperty, Fade(glow));
            Ring.Stroke = new SolidColorBrush(stroke);
        }
        else
        {
            Dark.Opacity = dark;
            Glow.Opacity = glow;
            Ring.Stroke = new SolidColorBrush(stroke);
        }
    }

    private static DoubleAnimation Fade(double to) => new(to, TimeSpan.FromMilliseconds(220))
    {
        EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
    };
}
