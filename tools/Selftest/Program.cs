using YvanMonkapp.Core;

// Tire beaucoup de questions et verifie trois choses :
//  - la reponse affichee est acceptee par le correcteur,
//  - elle reste tapable (pas de decimale a rallonge),
//  - l'enonce ne contient pas de "+ 0" ni de double espace.

int perLevel = args.Length > 0 && int.TryParse(args[0], out int n) ? n : 4000;
bool showExamples = args.Contains("--exemples");
int failures = 0;
var topics = new SortedDictionary<string, int>();
var drawn = new SortedDictionary<string, int>();
var samples = new SortedDictionary<string, string>();

for (int level = 1; level <= Question.MaxLevel; level++)
{
    for (int i = 0; i < perLevel; i++)
    {
        Question q = QuestionGenerator.Next(level);
        string key = $"N{q.Level} {q.Topic}";
        topics[key] = topics.GetValueOrDefault(key) + 1;
        if (showExamples) samples[$"{key} #{topics[key] % 3}"] = $"{q.Prompt}  ->  {q.Expected}";

        if (!Answers.Matches(q, q.Expected))
        {
            Report(q, "la reponse affichee est refusee par le correcteur");
            continue;
        }

        if (q.Numeric is double value)
        {
            // le joueur tape ce qui est affiche : les deux doivent coincider
            if (!Answers.TryParse(Answers.Normalize(q.Expected), out double typed)
                || Math.Abs(typed - value) > 1e-6 * Math.Max(1, Math.Abs(value)))
            {
                Report(q, $"reponse non tapable : affiche {q.Expected} pour {value:R}");
                continue;
            }

            if (Math.Abs(value) > 1e7) Report(q, $"resultat demesure : {value:R}");
        }

        if (q.Prompt.Contains("+ 0") || q.Prompt.Contains("  ") || q.Prompt.Contains("- -"))
        {
            Report(q, "enonce mal forme");
        }

        if (q.Explanation.Length < 5) Report(q, "correction vide");

        if (q.Figure is Figure schema)
        {
            drawn[key] = drawn.GetValueOrDefault(key) + 1;
            CheckFigure(q, schema);
        }
    }
}

Console.WriteLine($"{perLevel * Question.MaxLevel} questions tirees, {topics.Count} chapitres, "
    + $"{QuestionGenerator.FamilyCount} familles d'enonces.");
foreach (var (topic, count) in topics) Console.WriteLine($"  {topic,-34} {count}");

// --exemples : trois tirages par chapitre, pour relire ce qu'Yvan pose vraiment
if (showExamples)
{
    Console.WriteLine();
    foreach (var (key, prompt) in samples) Console.WriteLine($"  {key,-38} {prompt}");
}

Console.WriteLine($"  dont {drawn.Count} chapitres avec figure.");

// Chaque niveau doit tenir son rang : assez de familles, un nom, un chrono qui monte.
int previousSeconds = 0;
for (int level = 1; level <= Question.MaxLevel; level++)
{
    if (QuestionGenerator.FamilyCountFor(level) < 8) Fail($"le niveau {level} n'a que {QuestionGenerator.FamilyCountFor(level)} familles");
    if (Question.LevelName(level) == "Bonus") Fail($"le niveau {level} n'a pas de nom");
    if (Question.SecondsFor(level) <= previousSeconds) Fail($"le chrono du niveau {level} ne monte pas");
    previousSeconds = Question.SecondsFor(level);
}

// Le bareme : une bonne reponse rapporte, tout le reste coute.
var score = new ScoreData();
var settings = new AppSettings();
var sample = QuestionGenerator.Next(3);

var fast = ScoreEngine.Apply(score, sample, AnswerOutcome.Correcte, sample.Expected, 1.0);
var slow = ScoreEngine.Apply(new ScoreData(), sample, AnswerOutcome.Correcte, sample.Expected, sample.Seconds - 0.5);
if (fast.Delta <= slow.Delta) Fail($"repondre vite ne rapporte pas plus ({fast.Delta} vs {slow.Delta})");
if (ScoreEngine.Apply(new ScoreData(), sample, AnswerOutcome.Fausse, "1", 5).Delta >= 0) Fail("une erreur ne coute rien");
if (ScoreEngine.Apply(new ScoreData(), sample, AnswerOutcome.TempsEcoule, "", 99).Delta
    >= ScoreEngine.Apply(new ScoreData(), sample, AnswerOutcome.Abandon, "", 99).Delta) Fail("ignorer le popup coute moins qu'abandonner");

// Le niveau doit suivre le rang en mode auto, et obeir en mode fixe.
score.Points = 30000;
if (ScoreEngine.LevelFor(settings, score) != Question.MaxLevel) Fail("le rang maximal ne donne pas le niveau le plus dur");
if (Ranks.After(30000) is not null) Fail("le dernier rang a une suite");
if (Ranks.After(0) is null) Fail("le rang du debut n'a pas de suite");

// la difficulte doit monter avec le score, jamais redescendre
int previous = 0;
foreach (int points in new[] { -50, 0, 100, 250, 500, 1000, 2000, 3000, 5000, 7000, 10000, 15000, 20000 })
{
    int level = ScoreEngine.LevelFor(new AppSettings(), new ScoreData { Points = points });
    if (level < previous) Fail($"le niveau redescend a {points} points ({previous} puis {level})");
    previous = level;
}
if (previous != Question.MaxLevel) Fail("la progression n'atteint pas le dernier niveau");
settings.Difficulty = DifficultyMode.Fixe;
settings.FixedLevel = 2;
if (ScoreEngine.LevelFor(settings, score) != 2) Fail("le niveau impose est ignore");
settings.FixedLevel = Question.MaxLevel;
if (ScoreEngine.LevelFor(settings, new ScoreData()) != Question.MaxLevel) Fail("le dernier niveau ne peut pas etre impose");

// Les plages horaires calmes peuvent passer minuit.
var quiet = new AppSettings { QuietHours = true, QuietFromHour = 23, QuietToHour = 9 };
if (!quiet.IsQuiet(new DateTime(2026, 1, 1, 2, 0, 0))) Fail("2 h devrait etre une heure calme");
if (quiet.IsQuiet(new DateTime(2026, 1, 1, 14, 0, 0))) Fail("14 h ne devrait pas etre une heure calme");

// Ecritures acceptees a la saisie.
var half = new Question
{
    Level = 4, Topic = "Trigonometrie", Prompt = "cos(60)", Expected = "0,5", Numeric = 0.5,
    Accepted = new[] { "1/2" }, Explanation = "cos(60) = 1/2.", Seconds = 45, BasePoints = 40
};
foreach (string ok in new[] { "0,5", "0.5", "1/2", " 0,50 ", "x=0,5" })
{
    if (!Answers.Matches(half, ok)) Fail($"saisie refusee a tort : \"{ok}\"");
}
foreach (string ko in new[] { "0,6", "2", "", "abc" })
{
    if (Answers.Matches(half, ko)) Fail($"saisie acceptee a tort : \"{ko}\"");
}

// Ecritures venues du copier-coller ou de la calculatrice.
var thousand = new Question
{
    Level = 2, Topic = "Puissance", Prompt = "2^10", Expected = "1024", Numeric = 1024,
    Explanation = "2^10 = 1024.", Seconds = 25, BasePoints = 20
};
foreach (string ok in new[] { "1024", "1 024", "2^10", "1024 cm", "x = 1024", "1024." })
{
    if (!Answers.Matches(thousand, ok)) Fail($"saisie refusee a tort : \"{ok}\"");
}

var negative = new Question
{
    Level = 3, Topic = "Relatifs", Prompt = "3 - 8", Expected = "-5", Numeric = -5,
    Explanation = "3 - 8 = -5.", Seconds = 35, BasePoints = 30
};
foreach (string ok in new[] { "-5", "−5", "-5,0", "-10/2" })
{
    if (!Answers.Matches(negative, ok)) Fail($"saisie refusee a tort : \"{ok}\"");
}
if (Answers.Matches(negative, "5")) Fail("le signe moins est ignore a la comparaison");

// L'indice : il fait gagner moins, et il laisse une trace.
var hintable = QuestionGenerator.Next(3);
int plain = ScoreEngine.Apply(new ScoreData(), hintable, AnswerOutcome.Correcte, hintable.Expected, 1).Delta;
var hintedScore = new ScoreData();
int hinted = ScoreEngine.Apply(hintedScore, hintable, AnswerOutcome.Correcte, hintable.Expected, 1, hinted: true).Delta;
if (hinted >= plain) Fail($"l'indice ne coute rien ({hinted} contre {plain})");
if (hinted <= 0) Fail("l'indice fait perdre des points sur une bonne reponse");
if (hintedScore.Hints != 1) Fail("l'indice n'est pas comptabilise");
if (!hintedScore.History[0].Hinted) Fail("l'historique oublie que l'indice a ete demande");
foreach (int level in new[] { 1, 4, 8 })
{
    if (Hints.For(QuestionGenerator.Next(level).Topic).Length < 5) Fail($"un chapitre du niveau {level} n'a pas de conseil");
}

// Les chapitres rates remontent en tete, ceux qui passent restent tranquilles.
var weak = new ScoreData();
for (int i = 0; i < 6; i++)
{
    weak.Topic("Fractions").Asked++;
    weak.Topic("Tables").Asked++;
    weak.Topic("Tables").Correct++;
}
weak.Topic("Fractions").Correct = 1;
var faibles = weak.WeakTopics();
if (!faibles.Contains("Fractions")) Fail("un chapitre rate n'est pas signale comme faible");
if (faibles.Contains("Tables")) Fail("un chapitre reussi est signale comme faible");
if (new ScoreData().WeakTopics().Count != 0) Fail("un score vierge sort des chapitres faibles");

// Le suivi par chapitre : c'est lui qui alimente les chapitres faibles et le bulletin.
var tracked = new ScoreData();
var counted = QuestionGenerator.Next(2);
ScoreEngine.Apply(tracked, counted, AnswerOutcome.Correcte, counted.Expected, 2);
ScoreEngine.Apply(tracked, counted, AnswerOutcome.Fausse, "0", 2);
if (tracked.Topic(counted.Topic).Asked != 2) Fail("les questions ne sont pas comptees par chapitre");
if (tracked.Topic(counted.Topic).Correct != 1) Fail("les reussites ne sont pas comptees par chapitre");

// Le carnet d'erreurs : une faute revient, trois succes la font sortir.
var book = new ScoreData();
var missed = QuestionGenerator.Next(2);

ScoreEngine.Apply(book, missed, AnswerOutcome.Fausse, "0", 5);
if (book.Review.Count != 1) Fail("une faute n'entre pas au carnet d'erreurs");
if (book.Review[0].DueAt <= DateTime.Now) Fail("la question ratee revient immediatement");
if (ScoreEngine.NextDue(book, DateTime.Now) is not null) Fail("le carnet propose une question avant l'heure");
if (ScoreEngine.NextDue(book, DateTime.Now.AddHours(2)) is null) Fail("le carnet ne rend pas la question due");

for (int i = 0; i < 3; i++) ScoreEngine.Apply(book, missed, AnswerOutcome.Correcte, missed.Expected, 2, fromReview: true);
if (book.Review.Count != 0) Fail("trois succes ne sortent pas la question du carnet");

ScoreEngine.Apply(book, missed, AnswerOutcome.Fausse, "0", 5);
ScoreEngine.Apply(book, missed, AnswerOutcome.Correcte, missed.Expected, 2, fromReview: true);
ScoreEngine.Apply(book, missed, AnswerOutcome.Fausse, "0", 5);
if (book.Review.Count != 1 || book.Review[0].Stage != 0) Fail("une rechute ne ramene pas au premier palier");

// Une question dessinee doit garder son schema en passant par le carnet d'erreurs.
Question? withFigure = null;
for (int i = 0; i < 3000 && withFigure is null; i++)
{
    var candidate = QuestionGenerator.Next(3);
    if (candidate.Figure is not null) withFigure = candidate;
}
if (withFigure is null) Fail("aucune question dessinee au niveau 3");
else
{
    var revived = ReviewItem.From(withFigure).ToQuestion();
    if (revived.Figure is null) Fail("la figure ne survit pas au carnet d'erreurs");
    else if (revived.Figure.Parts.Count != withFigure.Figure!.Parts.Count) Fail("la figure perd des traits au carnet");

    string drawing = System.Text.Json.JsonSerializer.Serialize(ReviewItem.From(withFigure));
    var reread = System.Text.Json.JsonSerializer.Deserialize<ReviewItem>(drawing);
    if (reread?.Figure is null) Fail("la figure ne survit pas au JSON");
    else if (reread.Figure.Parts.Count != withFigure.Figure!.Parts.Count) Fail("la figure perd des traits au JSON");
    else if (reread.Figure.Parts[0].Kind != withFigure.Figure.Parts[0].Kind) Fail("le type d'un trait change au JSON");
}

// L'aller-retour d'une question par le carnet doit conserver la reponse acceptee.
var stored = ReviewItem.From(missed).ToQuestion();
if (!Answers.Matches(stored, missed.Expected)) Fail("une question du carnet n'accepte plus sa reponse");

// La serie quotidienne : jours consecutifs avec au moins une bonne reponse.
var streaked = new ScoreData();
foreach (int back in new[] { 0, 1, 2, 4 })
{
    streaked.Day(DateTime.Today.AddDays(-back)).Correct = 1;
}
if (streaked.DailyStreak() != 3) Fail($"serie quotidienne erronee : {streaked.DailyStreak()} au lieu de 3");

var yesterdayOnly = new ScoreData();
yesterdayOnly.Day(DateTime.Today.AddDays(-1)).Correct = 1;
if (yesterdayOnly.DailyStreak() != 1) Fail("une journee en cours vide casse la serie a tort");

// Les chapitres d'un niveau, et l'entrainement qui les vise.
for (int level = 1; level <= Question.MaxLevel; level++)
{
    var chapters = QuestionGenerator.TopicsFor(level);
    if (chapters.Count == 0) { Fail($"le niveau {level} n'annonce aucun chapitre"); continue; }
    if (chapters.Distinct().Count() != chapters.Count) Fail($"le niveau {level} annonce deux fois le meme chapitre");

    foreach (string chapter in chapters)
    {
        var aimed = QuestionGenerator.NextFrom(level, chapter);
        if (aimed.Topic != chapter) Fail($"l'entrainement sur \"{chapter}\" (niveau {level}) sort du chapitre : {aimed.Topic}");
        if (QuestionGenerator.LevelWith(chapter, level) != level) Fail($"le chapitre \"{chapter}\" se perd a son propre niveau");
    }
}

// Un chapitre absent du niveau demande doit renvoyer vers un niveau qui le pose.
string lointain = QuestionGenerator.TopicsFor(8).First(t => !QuestionGenerator.TopicsFor(1).Contains(t));
int trouve = QuestionGenerator.LevelWith(lointain, 1);
if (!QuestionGenerator.TopicsFor(trouve).Contains(lointain)) Fail($"\"{lointain}\" est renvoye vers le niveau {trouve} qui ne le pose pas");

// L'entrainement : une serie, mais pas une interro — et donc pas de prime de sans-faute.
var drill = QuizRun.Training(Enumerable.Range(0, QuizRun.TrainingLength)
    .Select(_ => QuestionGenerator.Next(2)).ToList(), "Fractions");
if (!drill.IsSeries) Fail("l'entrainement ne s'enchaine pas");
if (drill.IsExam) Fail("l'entrainement se fait passer pour une interro");
if (drill.PerfectBonus != 0) Fail("l'entrainement touche la prime de l'interro");
if (drill.Label != "Fractions") Fail("l'entrainement perd le nom de son chapitre");
if (drill.Questions.Count != QuizRun.TrainingLength) Fail("l'entrainement n'a pas sa longueur");

// L'interro : cinq questions et un bonus de sans-faute.
var exam = QuizRun.Exam(Enumerable.Range(0, QuizRun.ExamLength).Select(_ => QuestionGenerator.Next(3)).ToList());
if (!exam.IsExam || !exam.IsSeries || exam.Questions.Count != 5) Fail("l'interro ne tient pas ses cinq questions");
if (exam.PerfectBonus <= 0) Fail("l'interro sans faute ne rapporte rien");
var seule = QuizRun.Single(QuestionGenerator.Next(1));
if (seule.PerfectBonus != 0) Fail("une question seule donne un bonus d'interro");
if (seule.IsSeries || seule.IsExam) Fail("une question seule se prend pour une serie");

// Le bulletin : notes par chapitre et appreciation.
var week = new ScoreData();
var monday = Bulletin.StartOfWeek(DateTime.Today);
for (int i = 0; i < 10; i++)
{
    bool ok = i % 2 == 0;
    week.Day(monday).Asked++;
    if (ok) week.Day(monday).Correct++;
    week.Push(new HistoryEntry
    {
        Date = monday.AddHours(10 + i), Level = 2, Topic = "Tables",
        Outcome = ok ? AnswerOutcome.Correcte : AnswerOutcome.Fausse
    });
}

var report = Bulletin.ForWeek(week, DateTime.Today);
if (Math.Abs(report.Note - 10) > 0.01) Fail($"moyenne du bulletin erronee : {report.Note}");
if (report.Lines.Count != 1 || report.Lines[0].Topic != "Tables") Fail("le bulletin ne regroupe pas par chapitre");
if (report.Appreciation.Length < 10) Fail("le bulletin sort sans appreciation");
if (Bulletin.ForWeek(new ScoreData(), DateTime.Today).Appreciation.Length < 10) Fail("bulletin vide sans appreciation");

// La persistance : le score doit repasser par JSON sans rien perdre.
var options = new System.Text.Json.JsonSerializerOptions { WriteIndented = true, PropertyNameCaseInsensitive = true };
string json = System.Text.Json.JsonSerializer.Serialize(book, options);
var reloaded = System.Text.Json.JsonSerializer.Deserialize<ScoreData>(json, options);
if (reloaded is null) Fail("le score ne se relit pas");
else
{
    if (reloaded.Review.Count != book.Review.Count) Fail("le carnet d'erreurs ne survit pas a la sauvegarde");
    if (reloaded.Days.Count != book.Days.Count) Fail("les journees ne survivent pas a la sauvegarde");
    if (reloaded.ByTopic.Count != book.ByTopic.Count) Fail("le suivi par chapitre ne survit pas a la sauvegarde");
    if (reloaded.Review.Count > 0 && reloaded.Review[0].Hint != book.Review[0].Hint) Fail("l'indice ne survit pas a la sauvegarde");
    if (reloaded.Review.Count > 0 && !Answers.Matches(reloaded.Review[0].ToQuestion(), reloaded.Review[0].Expected))
    {
        Fail("une question relue depuis le disque n'accepte plus sa reponse");
    }
}

Console.WriteLine(failures == 0 ? "OK : aucun probleme." : $"{failures} probleme(s).");
return failures == 0 ? 0 : 1;

// Une figure doit etre dessinable : des points finis, assez de points pour son trait,
// et jamais de texte vide qui laisserait un blanc au tableau.
void CheckFigure(Question q, Figure schema)
{
    if (schema.Parts.Count == 0) { Report(q, "figure vide"); return; }
    if (schema.Width <= 0 || schema.Height <= 0) Report(q, "figure sans dimensions");

    foreach (var part in schema.Parts)
    {
        foreach (double value in part.Points)
        {
            if (double.IsNaN(value) || double.IsInfinity(value)) Report(q, $"coordonnee invalide dans un {part.Kind}");
            if (Math.Abs(value) > 400) Report(q, $"coordonnee hors cadre dans un {part.Kind} : {value:0.#}");
        }

        int minimum = part.Kind switch
        {
            FigureKind.Forme => 6,
            FigureKind.AngleDroit => 6,
            FigureKind.Arc => 5,
            FigureKind.Ellipse => 4,
            FigureKind.Ligne => 4,
            FigureKind.Fleche => 4,
            FigureKind.Cercle => 3,
            FigureKind.Grille => 1,
            _ => 2
        };
        if (part.Points.Length < minimum) Report(q, $"un {part.Kind} n'a que {part.Points.Length} coordonnees");

        if (part.Kind == FigureKind.Texte && part.Text.Length == 0) Report(q, "un texte de figure est vide");
    }
}

void Report(Question q, string why)
{
    Fail($"[N{q.Level} {q.Topic}] {why}  |  {q.Prompt}  -> {q.Expected}");
}

void Fail(string message)
{
    if (failures < 25) Console.WriteLine("ECHEC " + message);
    failures++;
}
