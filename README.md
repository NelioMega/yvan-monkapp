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

Ça produit `publish\YvanMonkapp-1.2.0-win-x64-autonome.zip` (63 Mo) : un seul .exe qui embarque .NET.
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
| Bonne réponse | `niveau × 10` points, plus un bonus de vitesse (jusqu'à +50 %) et un bonus de série (+2 par bonne réponse consécutive, plafonné de +14 au niveau 1 à +42 au niveau 8) |
| Mauvaise réponse | `niveau × 6` points en moins, la fenêtre tremble, Yvan devient rouge |
| Temps écoulé | `niveau × 8` points en moins — ignorer le popup coûte le plus cher |
| « Je sèche » | `niveau × 4` points en moins — abandonner honnêtement se paie moins cher que répondre au hasard |
| Bonne réponse après un indice | la moitié du gain, bonus de vitesse et de série annulés |

Le total peut devenir négatif. C'est prévu, il y a un rang pour ça.

### Quand c'est juste

Une bonne réponse ne se contente pas d'ajouter une ligne au score.

- le gain s'écrit en gros au milieu du popup et **s'envole** au-dessus de l'énoncé ;
- une **gerbe de confettis** part du centre — une poignée pour une bonne réponse ordinaire,
  une pluie pour un passage de rang ou une interro sans faute ;
- le cadre de la fenêtre **s'allume en vert** puis revient au bois ;
- le total du pied de fenêtre **grimpe** jusqu'à sa nouvelle valeur au lieu d'y sauter ;
- Yvan **sursaute** et s'entoure d'un halo vert, doré quand il est fier ;
- le carillon **monte d'un demi-ton par bonne réponse enchaînée** : on entend la série, pas
  seulement la réponse. À partir de quatre d'affilée, il s'offre une note de plus ;
- les grands moments — passage de rang, interro sans faute — remplacent le carillon par une
  **fanfare** et posent un bandeau en travers du popup. Il y en a aussi un pour une sortie du
  carnet d'erreurs et pour chaque cinquième bonne réponse d'affilée.

Une mauvaise réponse a droit au même traitement en négatif, en plus sobre : la perte s'envole
en rouge, le cadre s'allume en rouge, et la fenêtre tremble.

Tout cela suit les réglages de son : « bips de correction » décoché, ou volume à zéro, et il
ne reste que l'image.

### Saisie

La comparaison est souple : `0,5`, `0.5`, `1/2`, ` 0,50 ` et `x=0,5` sont toutes acceptées
pour une réponse valant un demi. Sont également admis, pour 1024 : `1 024` (espace de
milliers), `2^10` (écriture calculatrice), `1024 cm` et `1024.` — les unités tapées par
réflexe (`cm`, `km/h`, `€`, `%`, `kg`…) et la ponctuation finale sautent. Le moins
typographique `−` recopié depuis l'énoncé vaut le `-` du clavier.

- `Entrée` valide
- `F1` demande un indice
- `Échap` abandonne (compte comme un abandon)

### L'indice

Le bouton `?` (ou `F1`) affiche sous l'énoncé la méthode du chapitre — la formule qui manque,
jamais la réponse. Le chrono continue de tourner et la question ne rapporte plus que la moitié
de ce qu'elle valait, bonus de vitesse et de série compris. Un seul indice par question.

## Les niveaux

Le niveau suit votre rang, ou se fige sur une valeur choisie dans le tableau de bord.

| Niveau | Chrono | Contenu |
| --- | --- | --- |
| 1 · Échauffement | 20 s | additions, tables, doubles, compléments, monnaie, durées, nombres manquants |
| 2 · Collège | 25 s | multiplications posées, priorités, puissances, pourcentages, aires, relatifs, conversions, vitesses |
| 3 · Brevet | 35 s | PGCD et PPCM, fractions, équations et inéquations, Pythagore, Thalès, solides, échelles, probabilités |
| 4 · Lycée | 45 s | discriminant, identités remarquables, suites, systèmes, trigonométrie, lectures graphiques, volumes |
| 5 · Terminale | 60 s | dérivées, intégrales, suites géométriques, dénombrement, limites, logarithmes, probabilités, congruences |
| 6 · Post-bac | 75 s | déterminants, complexes, dérivées partielles, loi binomiale, séries géométriques, équations de congruence |
| 7 · Prépa | 90 s | développements limités, séries, rayon de convergence, rang et noyau, valeurs propres, indicatrice d'Euler, inverses modulaires |
| 8 · Agrégation | 110 s | groupes cycliques et diédraux, corps finis, nombres de Catalan, ζ(2), résidus, Fourier, Markov, Wilson, RSA |

**263 familles d'énoncés**, réparties sur 135 couples niveau-chapitre et toutes paramétrées au
hasard. La difficulté monte avec le score : chaque rang gagné rapproche du niveau suivant, et
le dernier rang pose de l'agrégation.

Yvan ne repose pas deux fois le même énoncé dans la foulée : les seize derniers restent en
mémoire. Et à énoncé neuf égal, il choisit d'abord un chapitre que vous ratez (voir plus bas).

### Les figures

Quarante et une familles ne posent presque rien par écrit : elles **dessinent**. Un schéma à
la craie s'affiche sous l'énoncé, et c'est lui qui porte les données.

| Niveau | Ce qui est dessiné |
| --- | --- |
| 2 | rectangles et triangles cotés, angles d'un triangle, droites sécantes, figures composées en L, cercle, diagramme en bâtons |
| 3 | Pythagore, Thalès, trapèze, parallélogramme, losange, disque, angles alternes-internes, polygone régulier, distance dans un repère, pavé droit, cube |
| 4 | coefficient directeur et ordonnée à l'origine lus sur un graphique, sommet d'une parabole, cercle trigonométrique, vecteurs, triangle 30-60-90, pyramide, cylindre, diagramme de Venn, arbre de probabilités |
| 5 | aire sous une courbe, tangente, cône, sphère, probabilités totales sur un arbre |
| 6 | plan complexe, déterminant lu comme une aire, diagonale d'un pavé, produit scalaire |
| 7 | volume d'un parallélépipède, centre de gravité, aire d'un triangle par le déterminant |

Les cotes sont dorées, les traits de construction en pointillé, les arêtes cachées en
pointillé estompé, et ce qui est demandé apparaît en vert.

### Les congruences

Le chapitre **Congruences** court sur les quatre derniers niveaux, du réflexe au théorème :

| Niveau | Ce qu'on demande |
| --- | --- |
| 5 | chiffre des unités d'une puissance, reste d'une somme ou d'un produit, représentant naturel d'un négatif |
| 6 | reste d'une grande puissance, critère de divisibilité par 9, résolution de `ax ≡ b [n]` |
| 7 | petit théorème de Fermat, théorème chinois à deux congruences, inverse modulaire, ordre multiplicatif |
| 8 | théorème de Wilson, système à trois congruences, exposant privé RSA |

## Les rangs

Cancre du fond de la classe → Élève en difficulté → Élève appliqué → Bon élève → Tête de
classe → Délégué de maths → Major de promo → Futur agrégé → Agrégé de mathématiques →
Colleur de prépa → Docteur en mathématiques → Médaille Fields du quartier → Yvan Monka
lui-même.

## Le carnet d'erreurs

Une question ratée n'est pas oubliée : elle revient **une heure plus tard**, puis **le
lendemain**, puis **la semaine suivante**. Trois succès d'affilée la sortent du carnet ; une
rechute la ramène au premier palier. Le popup l'annonce par un bandeau « RÉVISION ».

Une question du carnet passe toujours avant une question neuve : c'est ce qui transforme
l'application en outil de révision plutôt qu'en jeu de réflexes. Le carnet est plafonné à
40 questions, les plus anciennes s'effacent.

## Les chapitres faibles

En plus des questions ratées, Yvan retient la réussite **par chapitre**. Dès qu'un chapitre
compte au moins quatre questions posées et moins de deux réponses justes sur trois, il passe
devant au tirage : à énoncé neuf égal, une question de ce chapitre est préférée à une autre.

Le tableau de bord affiche la liste sous le calendrier — « Chapitres à revoir, remis en
avant : Fractions · Pythagore ». Rien à régler : elle se vide d'elle-même quand le chapitre
repasse au-dessus des deux tiers de réussite.

## S'entraîner à la demande

Yvan n'attend pas forcément son tour. Le tableau de bord ouvre une **série de dix questions**
quand vous le décidez :

- le bouton **« S'entraîner »** balaie votre niveau courant, en donnant la priorité aux
  chapitres que vous ratez ;
- dans l'onglet **« Chapitres »**, un clic sur une ligne lance les dix questions **sur ce
  chapitre-là**. Si le chapitre ne se pose pas à votre niveau — « Pythagore » quand vous en
  êtes au lycée — la série se cale sur le niveau où il tombe.

Les points comptent normalement. Seule la prime de sans-faute reste réservée à l'interro
surprise : c'est la surprise qui se paie, pas le travail.

## L'interro surprise

Environ un popup sur dix enchaîne **cinq questions dans la même fenêtre**, avec un compteur
« INTERRO · 2/5 ». Chaque réponse compte normalement, et un sans-faute rapporte la moitié des
points de base en prime. La cinquième monte d'un niveau : une interro finit toujours plus haut
qu'elle ne commence. Jamais deux interros dans la même heure et demie, et l'ensemble se
désactive depuis le tableau de bord.

## Le bulletin

Le dimanche à partir de 18 h, le popup laisse place au **bulletin de la semaine** : une note
sur 20 par chapitre, la tendance par rapport à la semaine précédente (▲ ▼ =), la moyenne
générale et l'appréciation d'Yvan. Le bouton « Exporter en image » enregistre la carte en PNG
dans `Images\Yvan Monk'app\`, prête à envoyer.

Consultable à tout moment : menu de la zone de notification, bouton « Bulletin » du tableau de
bord, ou `YvanMonkapp.exe --bulletin`.

## Le tableau de bord

À gauche : le rang et sa barre de progression, cinq chiffres (réussite, questions posées,
record de série, temps moyen, questions au carnet), six mois d'activité jour par jour, et un
panneau à trois vues.

| Vue | Ce qu'elle montre |
| --- | --- |
| Historique | les trente dernières questions, avec le temps mis et les points gagnés |
| Chapitres | tous les chapitres de votre niveau **plus ceux déjà travaillés**, les moins réussis en tête — cliquez-en un pour vous entraîner dessus |
| Carnet | les questions en attente de révision, dans l'ordre où elles reviennent |

Un chapitre déjà travaillé reste dans la liste même quand vous montez de niveau : sinon un
point faible de sixième disparaîtrait au moment précis où il faudrait le reprendre.

À droite : les réglages, et les trois boutons d'action.

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
- difficulté : suivre le rang, ou niveau imposé de 1 à 8
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
| `%LOCALAPPDATA%\YvanMonkapp\score.json` | score, statistiques par niveau et par chapitre, carnet d'erreurs, activité quotidienne, 400 dernières questions |
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

Trois utilitaires hors application, dans `tools\` :

- `IconGen` regénère `Assets\monka.ico`. Les tailles ≤ 128 partent en DIB classique :
  `System.Drawing.Icon`, qui sert à l'icône de la zone de notification, ne décode pas les
  entrées compressées en PNG.
- `Selftest` tire 32 000 questions et vérifie que chacune accepte sa propre réponse, qu'elle
  reste tapable au clavier, et que le barème se comporte (répondre vite rapporte plus,
  se tromper coûte, l'indice coûte, le niveau monte avec le score sans jamais redescendre).
  Il couvre aussi le carnet d'erreurs (entrée, paliers, sortie après trois succès, rechute),
  le suivi par chapitre et la détection des chapitres faibles, la série quotidienne, le bonus
  d'interro, le calcul du bulletin et l'aller-retour du score par JSON :

```powershell
dotnet run --project tools\Selftest\Selftest.csproj -c Release
```

  Ajoutez `--exemples` pour qu'il imprime trois tirages par chapitre : c'est la façon la plus
  rapide de relire ce qu'Yvan pose vraiment.

- `Apercu` recompile la fenêtre de question et l'**enregistre en PNG**, sans jamais l'afficher
  ni écrire dans `%LOCALAPPDATA%`. Le popup est entièrement dessiné en XAML : sans ce banc, la
  seule façon de juger la mise en page ou une animation serait d'installer l'application.

```powershell
dotnet run --project tools\Apercu\Apercu.csproj -c Release -- .\apercus
```

  Il sort six images : la question, l'indice révélé, une bonne réponse ordinaire, une série,
  un passage de rang et une faute. La fenêtre est ouverte à opacité nulle — WPF la met en page
  et fait tourner ses animations, mais rien n'apparaît à l'écran — puis `RenderTargetBitmap`
  la rasterise en pleine animation. C'est ce banc qui a montré que le bandeau des grands
  moments et les points en vol se recouvraient.

Le générateur est découpé : `Core\QuestionGenerator.cs` tient le moteur de tirage et les
petits outils (PGCD, indicatrice d'Euler, écriture des polynômes), et les
`Core\QuestionBank*.cs` ne contiennent que les modèles d'énoncés, deux à trois niveaux
chacun. `Core\QuestionBankGeo.cs` regroupe à part les familles dessinées, avec la petite
bibliothèque de solides (pavé, cylindre, cône, sphère, pyramide) et le repère qu'elles
partagent. Les conseils du bouton `?` vivent dans `Core\Hints.cs`, rangés par chapitre plutôt
que par énoncé : c'est la méthode qui manque, pas le calcul.

### Les figures, côté code

Une figure est **une donnée, pas un objet graphique** (`Core\Figure.cs`) : une liste de traits,
cercles, arcs, flèches et textes en coordonnées 0-100, l'axe des ordonnées vers le haut comme
en cours. Il le fallait pour qu'un schéma puisse dormir dans le carnet d'erreurs, repasser par
le JSON et ressortir intact — un `Path` WPF ne se sérialise pas.

`Controls\FigureView.cs` la dessine, tout en `OnRender` : un schéma n'a ni état ni
interaction, empiler des `Path` dans un `Canvas` coûterait un arbre visuel entier pour un
dessin qui ne bouge jamais. Deux choix qui comptent : le cadrage se fait sur la **boîte
englobante réelle** des traits et non sur le repère déclaré — un triangle plat n'occupe qu'un
bandeau de son repère et doit quand même remplir le panneau — et la marge autour du dessin est
comptée **en pixels**, parce que les cotes sont écrites à taille fixe et que la place qu'il
leur faut ne dépend pas du zoom.

Les sons de correction ne sont pas des fichiers : les wav sont synthétisés en mémoire au
premier usage (`Core\Audio.cs`). Il y en a trois — le carillon, le buzzer et la fanfare — et
le carillon en existe une variante par cran de série. Le volume est **cuit dans l'onde**,
parce que `SoundPlayer` n'a aucun réglage de niveau ; changer le volume vide donc le cache et
resynthétise. Le carillon monte d'un demi-ton par cran de série, dix crans au maximum.

### La couche d'effets

Confettis, points qui s'envolent et bandeaux vivent dans un `Canvas` nommé `Effects`, posé
par-dessus le popup fini et en `IsHitTestVisible="False"` : il ne participe ni à la mise en
page ni au clic. Un seul `DispatcherTimer` le vide, réglé sur la plus longue animation en
cours — pas un gestionnaire de fin par confetti. Chaque morceau part en cloche (déplacement
horizontal linéaire, vertical en deux images clés) et tourne sur lui-même.

Le halo de l'avatar (`Controls\MonkaAvatar.xaml`) est un `RadialGradientBrush` dont les trois
arrêts sont nommés : ils sont repeints à la teinte de l'humeur plutôt que d'empiler un halo
par couleur. Les deux extrémités restent transparentes, sinon on obtient un disque de couleur
posé sur la photo au lieu d'un anneau lumineux.

### Les images

`Assets\board.png` (le tableau vert) sert de fond aux trois fenêtres, posé en `Background`
d'un `Border` : c'est la seule couche qu'un `Border` rogne à son `CornerRadius`, un enfant
rectangulaire déborderait des coins arrondis.

`Assets\monka.png` et `Assets\monka-dark.png` sont les deux visages de l'avatar
(`Controls\MonkaAvatar.xaml`) : le vrai Yvan quand tout va bien, Dark Yvan Monka et son halo
rouge dès que le chrono s'épuise ou que la réponse est fausse. Les photos sont peintes dans
des `Ellipse`, ce qui les détoure en rond sans masque d'opacité.
