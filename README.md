# Yvan Monk'app

Yvan Monka s'installe dans votre barre des tâches et vous colle un calcul de temps en temps.
Bonne réponse : des points. Mauvaise réponse : il s'énerve, et le score descend.

Application Windows en WPF (.NET 8). Aucune fenêtre principale : elle vit dans la zone de
notification et n'ouvre un popup que quand le planificateur le décide.

## Installation

```powershell
powershell -ExecutionPolicy Bypass -File tools\install.ps1
```

Le script compile, installe dans `%LOCALAPPDATA%\Programs\YvanMonkapp`, pose un raccourci dans
le menu Démarrer et lance l'application.

Au premier lancement, l'application s'inscrit d'elle-même au démarrage de Windows : c'est le
fonctionnement prévu, elle est faite pour être là sans qu'on y pense. L'interrupteur
« Démarrer avec Windows » du tableau de bord permet de le lui interdire, et ce refus tient :
elle ne se réinscrit jamais toute seule ensuite.

### Donner l'application à quelqu'un

```powershell
powershell -ExecutionPolicy Bypass -File tools\package.ps1
```

Ça produit `publish\YvanMonkapp-1.0.0-win-x64-autonome.zip` (63 Mo) : un seul .exe qui embarque .NET.
La personne dézippe, double-clique, et c'est tout — pas de runtime à installer, pas de
compilation, aucun réglage. Le même paquet est construit automatiquement à chaque tag `v*`
poussé sur GitHub.

### Antivirus

L'exe produit est analysé par Windows Defender à chaque appel de `package.ps1` : aucune
détection. L'application n'ouvre aucune connexion réseau, n'installe aucun pilote, ne pose
aucun hook clavier et n'écrit que dans son propre dossier `%LOCALAPPDATA%\YvanMonkapp` et dans
`HKCU\...\Run` si vous cochez le démarrage automatique.

Reste SmartScreen : l'exe n'est pas signé par un certificat payant, donc au premier lancement
d'un fichier téléchargé Windows peut afficher « Windows a protégé votre ordinateur ».
Ce n'est pas une détection de virus mais un manque de réputation — « Informations
complémentaires » puis « Exécuter quand même ». Un certificat de signature de code
supprimerait cet écran, rien d'autre ne le fera.

Si un antivirus tiers rouspète malgré tout sur le fichier unique — certains n'aiment pas les
exécutables qui décompressent leurs dépendances au lancement — livrez la variante en dossier :

```powershell
powershell -ExecutionPolicy Bypass -File tools\package.ps1 -Dossier
```

Pour tout retirer :

```powershell
powershell -ExecutionPolicy Bypass -File tools\uninstall.ps1
```

Ajoutez `-Purge` pour effacer aussi le score.

## Le principe

Toutes les 5 à 20 minutes par défaut, un popup s'ouvre au centre de l'écran où vous
travaillez : un énoncé, un chrono, un champ de réponse.

| Issue | Effet |
| --- | --- |
| Bonne réponse | `niveau × 10` points, plus un bonus de vitesse (jusqu'à +50 %) et un bonus de série (+2 par bonne réponse consécutive, plafonné à +20) |
| Mauvaise réponse | `niveau × 6` points en moins, la fenêtre tremble, Yvan devient rouge |
| Temps écoulé | `niveau × 8` points en moins — ignorer le popup coûte le plus cher |
| « Je sèche » | `niveau × 4` points en moins — abandonner honnêtement se paie moins cher que répondre au hasard |

Le total peut devenir négatif. C'est prévu, il y a un rang pour ça.

### Saisie

La comparaison est souple : `0,5`, `0.5`, `1/2`, ` 0,50 ` et `x=0,5` sont toutes acceptées
pour une réponse valant un demi. Les unités tapées par réflexe (`cm`, `€`, `%`) sont ignorées.

- `Entrée` valide
- `Échap` abandonne (compte comme un abandon)

## Les niveaux

Le niveau suit votre rang, ou se fige sur une valeur choisie dans le tableau de bord.

| Niveau | Chrono | Contenu |
| --- | --- | --- |
| 1 · Échauffement | 20 s | additions, tables, doubles, compléments à 100, suites |
| 2 · Collège | 25 s | multiplications posées, priorités, puissances, pourcentages, aires |
| 3 · Brevet | 35 s | PGCD, fractions, équations du premier degré, Pythagore, notation scientifique |
| 4 · Lycée | 45 s | discriminant, racines, identités remarquables, suites, trigonométrie, logarithme |
| 5 · Terminale | 60 s | dérivées, intégrales, suites géométriques, dénombrement, limites, probabilités |
| 6 · Post-bac | 75 s | déterminants, nombres complexes, dérivées composées et secondes, loi binomiale, arrangements, produit scalaire |

56 familles d'énoncés au total, toutes paramétrées au hasard. La difficulté monte avec le
score : chaque rang gagné rapproche du niveau suivant, et le dernier rang pose du post-bac.

## Les rangs

Cancre du fond de la classe → Élève en difficulté → Élève appliqué → Bon élève → Tête de
classe → Délégué de maths → Major de promo → Futur agrégé → Yvan Monka lui-même.

## Le carnet d'erreurs

Une question ratée n'est pas oubliée : elle revient **une heure plus tard**, puis **le
lendemain**, puis **la semaine suivante**. Trois succès d'affilée la sortent du carnet ; une
rechute la ramène au premier palier. Le popup l'annonce par un bandeau « RÉVISION ».

Une question du carnet passe toujours avant une question neuve : c'est ce qui transforme
l'application en outil de révision plutôt qu'en jeu de réflexes. Le carnet est plafonné à
40 questions, les plus anciennes s'effacent.

## L'interro surprise

Environ un popup sur dix enchaîne **cinq questions dans la même fenêtre**, avec un compteur
« INTERRO · 2/5 ». Chaque réponse compte normalement, et un sans-faute rapporte la moitié des
points de base en prime. Jamais deux interros dans la même heure et demie, et l'ensemble se
désactive depuis le tableau de bord.

## Le bulletin

Le dimanche à partir de 18 h, le popup laisse place au **bulletin de la semaine** : une note
sur 20 par chapitre, la tendance par rapport à la semaine précédente (▲ ▼ =), la moyenne
générale et l'appréciation d'Yvan. Le bouton « Exporter en image » enregistre la carte en PNG
dans `Images\Yvan Monk'app\`, prête à envoyer.

Consultable à tout moment : menu de la zone de notification, bouton « Bulletin » du tableau de
bord, ou `YvanMonkapp.exe --bulletin`.

## Vos propres voix

Le dossier `%LOCALAPPDATA%\YvanMonkapp\voix\` contient trois sous-dossiers :

| Dossier | Moment |
| --- | --- |
| `bonjour\` | ouverture d'une question |
| `bonne\` | réponse juste |
| `mauvaise\` | réponse fausse |

Déposez-y des `.mp3`, `.wav`, `.m4a`, `.wma` ou `.aac` : Yvan en tire un au hasard à la place
du son d'origine. Un dossier vide garde le son par défaut. Les fichiers sont relus à chaque
question, rien à redémarrer. Le menu de la zone de notification ouvre le dossier directement.

## Réglages

Clic droit sur l'icône de la zone de notification, ou double-clic pour le tableau de bord.

- démarrage avec Windows (activé dès le premier lancement)
- fréquence (bornes basse et haute, en minutes)
- difficulté : suivre le rang, ou niveau imposé
- interros surprises, à activer ou non
- la voix d'Yvan (« bonjour » à l'ouverture du popup, intro complète aux passages de rang),
  bips de correction, volume
- silence en plein écran : aucune question pendant un jeu ou une vidéo plein écran
- plage horaire calme (23 h → 9 h par défaut)
- pause complète

Quand le moment est mal choisi, la question n'est pas perdue : elle est repoussée de cinq
minutes, puis retentée.

## Ligne de commande

| Argument | Effet |
| --- | --- |
| *(aucun)* | ouvre le tableau de bord |
| `--background` | démarre discrètement dans la zone de notification (utilisé par le démarrage automatique) |
| `--question` | pose une question tout de suite |
| `--bulletin` | ouvre le bulletin de la semaine |

Relancer l'exe alors que l'application tourne déjà n'ouvre pas de seconde instance : cela
ramène le tableau de bord au premier plan.

## Fichiers

| Chemin | Contenu |
| --- | --- |
| `%LOCALAPPDATA%\YvanMonkapp\settings.json` | réglages |
| `%LOCALAPPDATA%\YvanMonkapp\score.json` | score, statistiques, carnet d'erreurs, activité quotidienne, 400 dernières questions |
| `%LOCALAPPDATA%\YvanMonkapp\intro.mp3` | l'intro, extraite de l'exe au premier lancement |
| `%LOCALAPPDATA%\YvanMonkapp\bonjour.mp3` | le « bonjour », extrait de la même façon |
| `%LOCALAPPDATA%\YvanMonkapp\voix\` | vos extraits de voix |
| `%LOCALAPPDATA%\YvanMonkapp\yvanmonkapp.log` | journal (l'app n'a pas de fenêtre pour signaler un pépin) |
| `Images\Yvan Monk'app\` | bulletins exportés en PNG |
| `HKCU\...\CurrentVersion\Run\Yvan Monk'app` | démarrage automatique, posé au premier lancement |

## Développement

```powershell
dotnet build src\YvanMonkapp\YvanMonkapp.csproj -c Release
```

Deux utilitaires hors application, dans `tools\` :

- `IconGen` regénère `Assets\monka.ico`. Les tailles ≤ 128 partent en DIB classique :
  `System.Drawing.Icon`, qui sert à l'icône de la zone de notification, ne décode pas les
  entrées compressées en PNG.
- `Selftest` tire 24 000 questions et vérifie que chacune accepte sa propre réponse, qu'elle
  reste tapable au clavier, et que le barème se comporte (répondre vite rapporte plus,
  se tromper coûte, le niveau monte avec le score sans jamais redescendre). Il couvre aussi
  le carnet d'erreurs (entrée, paliers, sortie après trois succès, rechute), la série
  quotidienne, le bonus d'interro, le calcul du bulletin et l'aller-retour du score par JSON :

```powershell
dotnet run --project tools\Selftest\Selftest.csproj -c Release
```

Le son de bonne et de mauvaise réponse n'est pas un fichier : les deux wav sont synthétisés
en mémoire au premier usage (`Core\Audio.cs`).

### Les images

`Assets\board.png` (le tableau vert) sert de fond aux trois fenêtres, posé en `Background`
d'un `Border` : c'est la seule couche qu'un `Border` rogne à son `CornerRadius`, un enfant
rectangulaire déborderait des coins arrondis.

`Assets\monka.png` et `Assets\monka-dark.png` sont les deux visages de l'avatar
(`Controls\MonkaAvatar.xaml`) : le vrai Yvan quand tout va bien, Dark Yvan Monka et son halo
rouge dès que le chrono s'épuise ou que la réponse est fausse. Les photos sont peintes dans
des `Ellipse`, ce qui les détoure en rond sans masque d'opacité.
