using System.Windows;
using System.Windows.Input;
using YvanMonkapp.Core;

namespace YvanMonkapp;

/// <summary>Écran d'accueil du premier lancement : explique la règle et propose l'autodémarrage.</summary>
public partial class WelcomeWindow : Window
{
    public WelcomeWindow(double volume)
    {
        InitializeComponent();

        // l'intro complete, une fois : c'est le seul moment ou elle a le temps de tourner
        Loaded += (_, _) => Audio.PlayIntro(volume);
        Closed += (_, _) => Audio.StopVoice();
    }

    /// <summary>Vrai si l'utilisateur veut une question immédiatement après l'accueil.</summary>
    public bool AskNow { get; private set; }

    private void OnStart(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
        Close();
    }

    private void OnTryNow(object sender, RoutedEventArgs e)
    {
        AskNow = true;
        DialogResult = true;
        Close();
    }

    private void OnDragArea(object sender, MouseButtonEventArgs e)
    {
        if (e.ButtonState == MouseButtonState.Pressed) DragMove();
    }
}
