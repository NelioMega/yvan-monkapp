namespace YvanMonkapp.Core;

/// <summary>
/// Le coup de pouce d'Yvan : la méthode, jamais la réponse. Il est rattaché au chapitre
/// plutôt qu'à l'énoncé — c'est la formule qui manque, pas le calcul.
/// </summary>
public static class Hints
{
    private static readonly Dictionary<string, string> ByTopic = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Addition"] = "Posez les dizaines, puis les unités.",
        ["Soustraction"] = "Passez par la dizaine ronde la plus proche.",
        ["Tables"] = "Repartez de la table que vous connaissez et ajustez.",
        ["Double"] = "Doubler, c'est multiplier par 2.",
        ["Moitié"] = "Couper en deux, c'est diviser par 2.",
        ["Complément"] = "Le complément, c'est ce qu'il faut ajouter pour tomber rond.",
        ["Somme"] = "Regroupez les termes qui tombent juste avant d'ajouter le reste.",
        ["Suite"] = "Cherchez d'abord ce qu'on ajoute (ou multiplie) d'un terme au suivant.",
        ["Nombre manquant"] = "Faites l'opération inverse de celle de l'énoncé.",
        ["Quart"] = "Le quart, c'est la moitié de la moitié.",
        ["Durée"] = "Comptez d'abord jusqu'à l'heure ronde, puis le reste.",
        ["Monnaie"] = "Comptez ce qu'il manque pour arriver au billet.",
        ["Numération"] = "Une dizaine vaut 10 unités, une centaine vaut 10 dizaines.",

        ["Multiplication"] = "Décomposez : dizaines d'abord, unités ensuite.",
        ["Division"] = "Cherchez par quel nombre il faut multiplier le diviseur.",
        ["Priorités"] = "Parenthèses, puis × et ÷, puis + et −.",
        ["Carré"] = "n² = n × n.",
        ["Puissance"] = "aⁿ, c'est a multiplié n fois par lui-même.",
        ["Pourcentage"] = "t % de N, c'est N × t ÷ 100.",
        ["Géométrie"] = "Aire d'un rectangle = L × l, périmètre = 2 × (L + l).",
        ["Aire"] = "Triangle : base × hauteur ÷ 2. Carré : côté².",
        ["Moyenne"] = "Somme des valeurs divisée par leur nombre.",
        ["Médiane"] = "Rangez les valeurs, prenez celle du milieu.",
        ["Conversion"] = "Chaque marche de l'échelle vaut un facteur 10.",
        ["Relatifs"] = "Deux signes identiques donnent +, deux signes contraires donnent −.",
        ["Volume"] = "Pavé : L × l × h. Cube : côté³.",
        ["Angles"] = "Les angles d'un triangle font 180° à eux trois.",
        ["Vitesse"] = "distance = vitesse × temps.",
        ["Décimaux"] = "Comptez les chiffres après la virgule des deux facteurs.",
        ["Multiples"] = "Le PPCM, c'est le plus petit nombre qui figure dans les deux tables.",
        ["Fraction d'une quantité"] = "a/b de N, c'est N ÷ b × a.",

        ["PGCD"] = "Algorithme d'Euclide : on remplace le grand par le reste.",
        ["Fractions"] = "Même dénominateur pour additionner ; en ligne pour multiplier.",
        ["Équation"] = "Isolez x : tout ce qui traverse le signe = change de signe.",
        ["Inéquation"] = "Isolez x, et n'oubliez pas de retourner le sens si vous divisez par un négatif.",
        ["Pythagore"] = "Dans un triangle rectangle, hypoténuse² = somme des carrés des côtés.",
        ["Thalès"] = "Les longueurs se correspondent dans le même rapport.",
        ["Puissances"] = "aᵐ × aⁿ = aᵐ⁺ⁿ, et 10⁻ⁿ = 1 ÷ 10ⁿ.",
        ["Notation scientifique"] = "L'exposant compte les rangs dont la virgule se décale.",
        ["Proportionnalité"] = "Passez par la valeur d'une seule unité.",
        ["Calcul littéral"] = "Remplacez x par sa valeur, puis calculez dans l'ordre.",
        ["Développement"] = "Chaque terme du premier facteur rencontre chaque terme du second.",
        ["Factorisation"] = "a² − b² = (a − b)(a + b).",
        ["Nombres premiers"] = "Essayez 2, 3, 5, 7, 11… dans l'ordre.",
        ["Échelle"] = "Une échelle 1/k veut dire : 1 cm sur la carte pour k cm en vrai.",
        ["Statistiques"] = "Rangez la série avant de chercher le milieu.",

        ["Second degré"] = "Δ = b² − 4ac ; somme des racines = −b/a, produit = c/a.",
        ["Racine carrée"] = "Cherchez le nombre dont le carré tombe juste.",
        ["Identités remarquables"] = "(a + b)² = a² + 2ab + b².",
        ["Suites"] = "Arithmétique : uₙ = u₀ + n·r. Géométrique : uₙ = u₀ × qⁿ.",
        ["Systèmes"] = "Additionnez ou soustrayez les deux lignes pour éliminer une inconnue.",
        ["Fonction affine"] = "Coefficient directeur = (y_B − y_A) / (x_B − x_A).",
        ["Logarithme"] = "log(10ⁿ) = n, ln(eⁿ) = n, ln(ab) = ln a + ln b.",
        ["Évolutions"] = "Une hausse de t %, c'est ×(1 + t/100) ; une baisse, ×(1 − t/100).",
        ["Trigonométrie"] = "cos(60°) = 1/2, sin(30°) = 1/2, tan(45°) = 1.",
        ["Taux d'évolution"] = "(valeur d'arrivée − valeur de départ) ÷ valeur de départ × 100.",
        ["Valeur absolue"] = "La valeur absolue est la distance à zéro : toujours positive.",
        ["Vecteurs"] = "Coordonnées de AB : x_B − x_A et y_B − y_A. Produit scalaire : xx′ + yy′.",
        ["Intervalles"] = "Comptez les bornes incluses.",
        ["Fonction inverse"] = "f(x) = k/x : divisez k par la valeur donnée.",

        ["Dérivées"] = "(xⁿ)′ = n·xⁿ⁻¹, (eᵃˣ)′ = a·eᵃˣ, (ln x)′ = 1/x.",
        ["Sommes"] = "1 + 2 + … + n = n(n+1)/2 ; somme géométrique = u₀(qⁿ⁺¹ − 1)/(q − 1).",
        ["Dénombrement"] = "C(n ; k) = n! / (k! (n−k)!), A(n ; k) = n! / (n−k)!.",
        ["Factorielle"] = "n! = 1 × 2 × … × n.",
        ["Exponentielle"] = "eᵃ × eᵇ = eᵃ⁺ᵇ, et e⁰ = 1.",
        ["Intégrales"] = "Cherchez une primitive, puis faites la différence aux deux bornes.",
        ["Probabilités"] = "Cas favorables sur cas possibles ; E(X) = np pour une binomiale.",
        ["Limites"] = "Comparez les termes de plus haut degré, ou utilisez les limites usuelles.",
        ["Tangente"] = "Le coefficient directeur de la tangente en a vaut f′(a).",
        ["Récurrence"] = "Déroulez la relation terme par terme.",

        ["Matrices"] = "det = ad − bc pour une 2×2 ; la trace est la somme de la diagonale.",
        ["Complexes"] = "|z| = √(a² + b²), et i² = −1.",
        ["Arithmétique"] = "Travaillez sur les restes plutôt que sur les nombres entiers.",
        ["Modulo"] = "Cherchez le cycle des restes, il est toujours court.",
        ["Congruences"] = "Réduisez chaque facteur modulo n avant de calculer ; les puissances tournent en boucle.",
        ["Espace"] = "Norme d'un vecteur de l'espace : √(x² + y² + z²).",
        ["Séries"] = "Une géométrique de raison q, |q| < 1, converge vers u₀/(1 − q).",
        ["Développements limités"] = "eˣ = 1 + x + x²/2 + … ; cos x = 1 − x²/2 + …",
        ["Algèbre linéaire"] = "Théorème du rang : dim ker + rang = dimension de départ.",
        ["Valeurs propres"] = "Somme des valeurs propres = trace, produit = déterminant.",
        ["Équations différentielles"] = "Les solutions de y′ = ky sont les C·eᵏˣ.",
        ["Groupes"] = "L'ordre d'un élément divise l'ordre du groupe.",
        ["Corps finis"] = "Un corps fini a p^n éléments, p premier.",
        ["Topologie"] = "Revenez à la définition, c'est plus rapide qu'il n'y paraît.",
        ["Analyse complexe"] = "∮ dz/z sur un tour vaut 2iπ.",
        ["Fourier"] = "Le coefficient a₀ est la valeur moyenne sur une période.",
        ["Transformées"] = "L(tⁿ) = n! / s^(n+1).",
        ["Markov"] = "L'état stationnaire vérifie πP = π et la somme de ses termes vaut 1."
    };

    private const string Fallback = "Reprenez l'énoncé pas à pas : la réponse est un nombre simple.";

    public static string For(string topic) => ByTopic.GetValueOrDefault(topic, Fallback);

    /// <summary>Vrai si le chapitre a son propre conseil, plutôt que le conseil passe-partout.</summary>
    public static bool Knows(string topic) => ByTopic.ContainsKey(topic);
}
