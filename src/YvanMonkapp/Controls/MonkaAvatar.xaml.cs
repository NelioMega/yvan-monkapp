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
        // dark = part de photo sombre, glow = force du halo, ring et halo partagent la teinte
        (double dark, double glow, string tint) = mood switch
        {
            FaceMood.Content => (0d, 0.55d, "#69C97A"),
            FaceMood.Fier => (0d, 0.85d, "#C99A4F"),
            FaceMood.Fache => (0.65d, 0.5d, "#E8B34A"),
            FaceMood.Furieux => (1d, 1d, "#E2574C"),
            _ => (0d, 0d, "#9EB3A8")
        };

        var color = (Color)ColorConverter.ConvertFromString(tint);
        Ring.Stroke = new SolidColorBrush(color);
        PaintGlow(color);

        if (animate)
        {
            Dark.BeginAnimation(OpacityProperty, Fade(dark));
            Glow.BeginAnimation(OpacityProperty, Fade(glow));
        }
        else
        {
            Dark.Opacity = dark;
            Glow.Opacity = glow;
        }

        if (animate && mood is FaceMood.Content or FaceMood.Fier) Jump(mood == FaceMood.Fier ? 1.16 : 1.09);
    }

    /// <summary>
    /// Repeint le dégradé du halo. Le bord reste transparent aux deux extrémités : c'est ce
    /// qui donne un anneau lumineux plutôt qu'un disque de couleur posé sur la photo.
    /// </summary>
    private void PaintGlow(Color color)
    {
        GlowInner.Color = Color.FromArgb(0, color.R, color.G, color.B);
        GlowMid.Color = Color.FromArgb(0xB0, color.R, color.G, color.B);
        GlowOuter.Color = Color.FromArgb(0, color.R, color.G, color.B);
    }

    /// <summary>Un sursaut de joie : le prof se redresse d'un coup, puis se repose.</summary>
    public void Jump(double amplitude = 1.12)
    {
        var jump = new DoubleAnimationUsingKeyFrames { Duration = TimeSpan.FromMilliseconds(520) };
        jump.KeyFrames.Add(new EasingDoubleKeyFrame(amplitude, KeyTime.FromPercent(0.28),
            new CubicEase { EasingMode = EasingMode.EaseOut }));
        jump.KeyFrames.Add(new EasingDoubleKeyFrame(1, KeyTime.FromPercent(1),
            new ElasticEase { EasingMode = EasingMode.EaseOut, Oscillations = 2, Springiness = 4 }));

        Bounce.BeginAnimation(ScaleTransform.ScaleXProperty, jump);
        Bounce.BeginAnimation(ScaleTransform.ScaleYProperty, jump);
    }

    private static DoubleAnimation Fade(double to) => new(to, TimeSpan.FromMilliseconds(220))
    {
        EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
    };
}
