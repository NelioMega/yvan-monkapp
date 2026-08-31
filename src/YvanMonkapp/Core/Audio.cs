using System.Media;
using System.Reflection;
using System.Windows.Media;

namespace YvanMonkapp.Core;

/// <summary>
/// La voix d'Yvan (deux mp3 embarqués, recopiés dans AppData au premier lancement) et deux
/// bips de synthèse pour la correction, générés à la volée : aucun fichier wav à livrer.
/// </summary>
public static class Audio
{
    private const string IntroResource = "YvanMonkapp.Assets.intro.mp3";
    private const string GreetingResource = "YvanMonkapp.Assets.bonjour.mp3";

    /// <summary>Ce que MediaPlayer sait ouvrir sans codec supplémentaire.</summary>
    private static readonly string[] Extensions = { ".mp3", ".wav", ".m4a", ".wma", ".aac" };

    private static readonly Random Rng = new();

    private static MediaPlayer? _player;

    /// <summary>
    /// Les bips déjà synthétisés, rangés par « recette + volume ». Le volume est cuit dans
    /// l'onde parce que SoundPlayer, lui, n'a aucun réglage de niveau.
    /// </summary>
    private static readonly Dictionary<string, byte[]> Beeps = new();

    /// <summary>Recopie les mp3 dans AppData s'ils manquent ou si l'exe a été mis à jour.</summary>
    public static void EnsureAudioFiles()
    {
        Extract(IntroResource, Paths.Intro);
        Extract(GreetingResource, Paths.Greeting);
    }

    private static void Extract(string resource, string path)
    {
        try
        {
            Paths.EnsureRoot();

            using Stream? source = Assembly.GetExecutingAssembly().GetManifestResourceStream(resource);
            if (source is null)
            {
                Log.Write($"ressource {resource} introuvable");
                return;
            }

            var existing = new FileInfo(path);
            if (existing.Exists && existing.Length == source.Length) return;

            using var target = File.Create(path);
            source.CopyTo(target);
        }
        catch (Exception ex)
        {
            Log.Write($"extraction de {Path.GetFileName(path)} impossible : {ex.Message}");
        }
    }

    /// <summary>
    /// Crée le dossier de voix et son mode d'emploi. Tout mp3 déposé dedans remplace
    /// le son par défaut du moment correspondant.
    /// </summary>
    public static void EnsureVoiceFolders()
    {
        try
        {
            foreach (VoiceKind kind in Enum.GetValues<VoiceKind>())
            {
                Directory.CreateDirectory(Paths.VoiceFolder(kind));
            }

            string readme = Path.Combine(Paths.Voices, "mode d'emploi.txt");
            if (File.Exists(readme)) return;

            File.WriteAllText(readme, string.Join(Environment.NewLine, new[]
            {
                "Vos propres extraits de voix",
                "============================",
                "",
                "Déposez des fichiers audio dans les dossiers ci-dessous. À chaque fois,",
                "Yvan Monk'app en tire un au hasard à la place de son son habituel.",
                "",
                "  bonjour\\   joué à l'ouverture d'une question",
                "  bonne\\     joué quand la réponse est juste",
                "  mauvaise\\  joué quand elle est fausse",
                "",
                "Formats acceptés : " + string.Join(", ", Extensions),
                "",
                "Un dossier vide = le son d'origine. Rien à redémarrer, les fichiers sont",
                "relus à chaque question."
            }));
        }
        catch (Exception ex)
        {
            Log.Write($"création du dossier de voix impossible : {ex.Message}");
        }
    }

    /// <summary>Un extrait au hasard déposé par l'utilisateur, ou null si le dossier est vide.</summary>
    private static string? CustomClip(VoiceKind kind)
    {
        try
        {
            string folder = Paths.VoiceFolder(kind);
            if (!Directory.Exists(folder)) return null;

            var clips = Directory.EnumerateFiles(folder)
                .Where(file => Extensions.Contains(Path.GetExtension(file).ToLowerInvariant()))
                .ToList();

            return clips.Count == 0 ? null : clips[Rng.Next(clips.Count)];
        }
        catch (Exception ex)
        {
            Log.Write($"lecture du dossier de voix impossible : {ex.Message}");
            return null;
        }
    }

    /// <summary>Le « bonjour » d'une seconde, à l'ouverture d'un popup.</summary>
    public static void PlayGreeting(double volume) => PlayVoice(CustomClip(VoiceKind.Greeting) ?? Paths.Greeting, volume);

    /// <summary>L'intro complète : accueil et passages de rang.</summary>
    public static void PlayIntro(double volume) => PlayVoice(Paths.Intro, volume);

    /// <summary>Lance un mp3 depuis le début. À appeler sur le thread d'interface.</summary>
    private static void PlayVoice(string path, double volume)
    {
        try
        {
            if (!File.Exists(path)) EnsureAudioFiles();
            if (!File.Exists(path)) return;

            _player ??= new MediaPlayer();
            _player.Volume = Math.Clamp(volume, 0, 1);
            _player.Open(new Uri(path));
            _player.Play();
        }
        catch (Exception ex)
        {
            Log.Write($"lecture de {Path.GetFileName(path)} impossible : {ex.Message}");
        }
    }

    public static void StopVoice()
    {
        try
        {
            _player?.Stop();
        }
        catch (Exception ex)
        {
            Log.Write($"arrêt de la voix impossible : {ex.Message}");
        }
    }

    /// <summary>Le plus haut cran de série que le carillon sait encore monter.</summary>
    private const int TopStep = 10;

    /// <summary>
    /// Extrait maison s'il y en a un, sinon un carillon montant. La note finale grimpe d'un
    /// demi-ton par bonne réponse enchaînée : c'est ce qui fait entendre la série, et pas
    /// seulement la bonne réponse.
    /// </summary>
    public static void PlayCorrect(double volume, int streak = 0)
    {
        if (CustomClip(VoiceKind.Correct) is string clip)
        {
            PlayVoice(clip, volume);
            return;
        }

        int step = Math.Clamp(streak, 0, TopStep);
        double climb = Math.Pow(2, step / 12.0);

        var notes = new List<(double, double)> { (659.3 * climb, 0.06), (880.0 * climb, 0.06) };
        // à partir de quatre d'affilée, le carillon s'offre une note de plus
        if (step >= 4) notes.Add((1108.7 * climb, 0.06));
        notes.Add((1318.5 * climb, 0.20));

        Play($"ding{step}", volume, () => BuildWav(notes.ToArray(), square: false));
    }

    /// <summary>Extrait maison s'il y en a un, sinon buzzer descendant.</summary>
    public static void PlayWrong(double volume)
    {
        if (CustomClip(VoiceKind.Wrong) is string clip)
        {
            PlayVoice(clip, volume);
            return;
        }

        Play("buzz", volume, () => BuildWav(new[] { (196.0, 0.16), (147.0, 0.28) }, square: true));
    }

    /// <summary>La petite fanfare des grands moments : passage de rang, interro sans faute.</summary>
    public static void PlayFanfare(double volume) => Play("fanfare", volume, () => BuildWav(new[]
    {
        (523.3, 0.09), (659.3, 0.09), (784.0, 0.09), (1046.5, 0.11), (784.0, 0.07), (1046.5, 0.34)
    }, square: false));

    /// <summary>Joue un bip, en le synthétisant au premier passage pour ce volume-là.</summary>
    private static void Play(string name, double volume, Func<byte[]> build)
    {
        // vingt paliers suffisent : personne n'entend la différence entre 62 % et 63 %
        int level = (int)Math.Round(Math.Clamp(volume, 0, 1) * 20);
        if (level == 0) return;

        // un changement de volume périme tout le cache : garder les anciennes ondes
        // n'aurait servi qu'à empiler des mégaoctets pour un niveau qu'on a quitté
        if (level != _cachedLevel)
        {
            Beeps.Clear();
            _cachedLevel = level;
        }

        if (!Beeps.TryGetValue(name, out byte[]? wav))
        {
            _gain = level / 20.0;
            wav = build();
            Beeps[name] = wav;
        }

        PlayWav(wav);
    }

    /// <summary>Niveau appliqué par <see cref="BuildWav"/>, posé juste avant la synthèse.</summary>
    private static double _gain = 1;

    /// <summary>Palier de volume auquel les ondes en cache ont été fabriquées.</summary>
    private static int _cachedLevel = -1;

    private static void PlayWav(byte[] wav)
    {
        try
        {
            var player = new SoundPlayer(new MemoryStream(wav));
            player.Play();
        }
        catch (Exception ex)
        {
            Log.Write($"bip impossible : {ex.Message}");
        }
    }

    /// <summary>Construit un wav 16 bits mono à partir d'une suite de notes.</summary>
    private static byte[] BuildWav((double Freq, double Seconds)[] notes, bool square)
    {
        const int rate = 44100;

        int total = 0;
        foreach (var note in notes) total += (int)(note.Seconds * rate);

        using var ms = new MemoryStream();
        using var bw = new BinaryWriter(ms);

        int dataBytes = total * 2;
        bw.Write("RIFF".ToCharArray());
        bw.Write(36 + dataBytes);
        bw.Write("WAVE".ToCharArray());
        bw.Write("fmt ".ToCharArray());
        bw.Write(16);
        bw.Write((short)1);          // PCM
        bw.Write((short)1);          // mono
        bw.Write(rate);
        bw.Write(rate * 2);          // octets par seconde
        bw.Write((short)2);          // alignement
        bw.Write((short)16);         // bits par échantillon
        bw.Write("data".ToCharArray());
        bw.Write(dataBytes);

        int written = 0;
        foreach (var (freq, seconds) in notes)
        {
            int samples = (int)(seconds * rate);
            for (int i = 0; i < samples; i++)
            {
                double t = (double)i / rate;
                double wave = Math.Sin(2 * Math.PI * freq * t);
                if (square) wave = Math.Sign(wave) * 0.6;

                // fondu d'entrée et de sortie : sinon ça claque dans les enceintes
                double fade = Math.Min(1, Math.Min(i, samples - i) / (rate * 0.012));
                double envelope = fade * (1 - 0.35 * (double)i / samples);

                bw.Write((short)(wave * envelope * _gain * 11000));
                written++;
            }
        }

        // sécurité : l'en-tête annonce déjà dataBytes échantillons
        for (; written < total; written++) bw.Write((short)0);

        bw.Flush();
        return ms.ToArray();
    }
}
