using YvanMonkapp.Core;

// Tire beaucoup de questions et verifie trois choses :
//  - la reponse affichee est acceptee par le correcteur,
//  - elle reste tapable (pas de decimale a rallonge),
//  - l'enonce ne contient pas de "+ 0" ni de double espace.

int perLevel = args.Length > 0 && int.TryParse(args[0], out int n) ? n : 4000;
int failures = 0;
var topics = new SortedDictionary<string, int>();

for (int level = 1; level <= Question.MaxLevel; level++)
{
    for (int i = 0; i < perLevel; i++)
    {
        Question q = QuestionGenerator.Next(level);
        topics[$"N{q.Level} {q.Topic}"] = topics.GetValueOrDefault($"N{q.Level} {q.Topic}") + 1;

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
    }
}

Console.WriteLine($"{perLevel * Question.MaxLevel} questions tirees, {topics.Count} familles.");
foreach (var (topic, count) in topics) Console.WriteLine($"  {topic,-34} {count}");

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
score.Points = 5000;
if (ScoreEngine.LevelFor(settings, score) != Question.MaxLevel) Fail("le rang maximal ne donne pas le niveau le plus dur");

// la difficulte doit monter avec le score, jamais redescendre
int previous = 0;
foreach (int points in new[] { -50, 0, 100, 250, 500, 1000, 2000, 3000, 5000 })
{
    int level = ScoreEngine.LevelFor(new AppSettings(), new ScoreData { Points = points });
    if (level < previous) Fail($"le niveau redescend a {points} points ({previous} puis {level})");
    previous = level;
}
if (previous != Question.MaxLevel) Fail("la progression n'atteint pas le dernier niveau");
settings.Difficulty = DifficultyMode.Fixe;
settings.FixedLevel = 2;
if (ScoreEngine.LevelFor(settings, score) != 2) Fail("le niveau impose est ignore");

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

// L'interro : cinq questions et un bonus de sans-faute.
var exam = QuizRun.Exam(Enumerable.Range(0, QuizRun.ExamLength).Select(_ => QuestionGenerator.Next(3)).ToList());
if (!exam.IsExam || exam.Questions.Count != 5) Fail("l'interro ne tient pas ses cinq questions");
if (exam.PerfectBonus <= 0) Fail("l'interro sans faute ne rapporte rien");
if (QuizRun.Single(QuestionGenerator.Next(1)).PerfectBonus != 0) Fail("une question seule donne un bonus d'interro");

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
    if (reloaded.Review.Count > 0 && !Answers.Matches(reloaded.Review[0].ToQuestion(), reloaded.Review[0].Expected))
    {
        Fail("une question relue depuis le disque n'accepte plus sa reponse");
    }
}

Console.WriteLine(failures == 0 ? "OK : aucun probleme." : $"{failures} probleme(s).");
return failures == 0 ? 0 : 1;

void Report(Question q, string why)
{
    Fail($"[N{q.Level} {q.Topic}] {why}  |  {q.Prompt}  -> {q.Expected}");
}

void Fail(string message)
{
    if (failures < 25) Console.WriteLine("ECHEC " + message);
    failures++;
}
