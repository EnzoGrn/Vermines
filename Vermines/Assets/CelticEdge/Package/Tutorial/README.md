# CelticEdge.Tutorial

Un module de didacticiel pour Unity, utilisable dans n'importe quel jeu. Il est extrait du didacticiel de Vermines, sans plus rien qui dépende de Vermines.

Le jeu décrit ses leçons, et le module les déroule. Une leçon est une suite d'étapes, chacune pouvant :
- afficher une bulle, une fenêtre ou un narrateur ;
- éclairer des éléments sous un voile sombre ;
- montrer le geste à faire avec une main animée.

Pendant une étape, le module limite ce que le joueur a le droit de faire et attend une action ou un clic avant de passer à la suite.

## Sommaire

1. [Ce que fait le module](#ce-que-fait-le-module)
2. [Installation](#installation)
3. [Essayer en deux minutes](#essayer-en-deux-minutes)
4. [Brancher un jeu](#brancher-un-jeu)
5. [Écrire les leçons dans un tableau](#écrire-les-leçons-dans-un-tableau)
6. [Séquences annexes et aides](#séquences-annexes-et-aides)
7. [Mise en place propre au jeu](#mise-en-place-propre-au-jeu)
8. [Hasard scripté et agents scriptés](#hasard-scripté-et-agents-scriptés)
9. [Ce qui reste dans le jeu : l'exemple de Vermines](#ce-qui-reste-dans-le-jeu--lexemple-de-vermines)
10. [L'interface](#linterface)
11. [Arborescence](#arborescence)

## Ce que fait le module

| Besoin | Dans le module |
|---|---|
| Campagne de leçons, déblocage, progression par profil | `TutorialCampaignData`, `TutorialLessonData`, `TutorialProgressService` |
| Étapes : bulle, fenêtre, narrateur, voile, pointeur | `TutorialStepData`, `TutorialUIController`, avec vos propres vues (voir [L'interface](#linterface)) |
| N'autoriser que l'action attendue | `TutorialInputGate` |
| Attendre une action du joueur | `TutorialEventBus` (le jeu publie, l'étape attend) |
| Retrouver les éléments à éclairer | `TutorialTarget`, `TutorialTargetScanner`, IDs `go:Nom` |
| Aide qui s'affiche après une erreur, puis l'étape reprend | Séquences annexes, `TutorialHints` |
| Parties identiques à chaque fois (cartes, butin, premier joueur) | `GameRandom`, `ScriptedGameRandom` |
| Adversaire dont les tours sont écrits à l'avance | `ScriptedAgent`, `TutorialSession` |
| Scénario écrit dans Excel ou Google Sheets | `TutorialTable`, `TutorialTableLoader`, import dans l'éditeur |
| Données du menu des leçons (débloquée, terminée, progression) | `TutorialManager.Campaign`, `IsUnlocked`, `Progress` |

## Installation

Le module suit le modèle des paquets Celtic Edge (`template-package`). Le dossier du dépôt contient un projet Unity, et le paquet lui-même est dans `Tutorial/Assets/Celtic-Edge/Package/Tutorial/`.

Il y a deux façons de l'utiliser dans un jeu :
- copiez ce dossier dans `Assets/Celtic-Edge/Package/Tutorial/` du jeu ;
- ou exportez-le avec *CelticEdge > Export Package*, puis importez le `.unitypackage`.

Le module a besoin d'uGUI et de TextMeshPro, qui sont inclus dans Unity 6. Il vise Unity 2021.3 et plus récent. Il compile contre Unity 6000.2, mais n'a pas encore été testé en jeu hors de Vermines.

Les assemblies sont les suivantes :
- `CelticEdge.Tutorial` : le module lui-même, dans `Scripts/` et `Resources/` ;
- `CelticEdge.Tutorial.Editor` : les fenêtres de l'éditeur ;
- `CelticEdge.Tutorial.Demo` : la démo ;
- `CelticEdge.Tutorial.Tests` : les tests EditMode.

## Essayer en deux minutes

1. Ouvrez le projet `Tutorial/` dans Unity.
2. La première fois, importez *Window > TextMeshPro > Import TMP Essential Resources*. Sans cela, les textes créés par code n'ont pas de police.
3. Ouvrez la scène `Demo/Scenes/Demo_Tutorial.unity` et lancez Play.

Dans un autre projet, posez simplement le composant `QuickStartDemo` sur un GameObject vide d'une scène vide.

L'exemple est un mini-jeu avec deux boutons, Harvest et Buy. Il contient deux leçons :
- une leçon guidée pas à pas ;
- une leçon en jeu libre, avec une aide quand il manque de l'or.

Tout est construit par code, et les étapes viennent de `Demo/Resources/quickstart.csv`. `QuickStartGame`, `QuickStartBridge` et `QuickStartDemo` montrent l'essentiel de ce qu'un jeu doit écrire. `QuickStartUI` et `QuickStartLessonMenu` construisent l'apparence de la démo : ce sont des exemples, pas une partie de la librairie.

> L'exemple crée un EventSystem avec le `StandaloneInputModule`. Si le projet n'utilise que le nouvel Input System, ajoutez d'abord votre propre EventSystem dans la scène.

## Brancher un jeu

Le jeu ne connaît le tutoriel qu'à travers cinq points.

### 1. Le pont

Le pont est la seule classe à écrire côté jeu. Il prépare la leçon, exécute les commandes du jeu et nettoie à la fin.

```csharp
public class MyTutorialBridge : TutorialGameBridge
{
    public override IEnumerator PrepareLesson(TutorialLessonData lesson)
    {
        var setup = lesson.GetSetup<MyLessonSetup>();
        yield return game.StartMatch(setup.StartingGold);
    }

    public override IEnumerator ExecuteCommand(TutorialCommand command)
    {
        if (command.Is("AddGold"))
            game.AddGold(command.GetInt("amount"));
        yield break;
    }

    public override void CleanupLesson(TutorialLessonData lesson, bool completed) => game.BackToMenu();
}
```

### 2. Demander la permission

Le jeu pose la question au moment où une action est validée. Hors leçon, tout est autorisé.

```csharp
if (!TutorialInputGate.IsAllowed("buy", item.Id))
    return;
```

### 3. Publier ce qui se passe

Les étapes attendent ces événements, colonnes `evenement` et `argument` du tableau.

```csharp
TutorialEventBus.Publish("item_bought", item.Id);
TutorialEventBus.Publish("gold_changed", null, gold);
TutorialEventBus.Publish(TutorialEvents.TurnStarted, playerIndex.ToString(), turn);
```

### 4. Les cibles

Il y a trois façons de donner un ID aux éléments qu'une étape éclaire :
- **`TutorialTarget`** sur un élément fixe, avec l'ID dans l'inspecteur ;
- **`TutorialTargetScanner`** pour les objets créés à la volée, sans toucher à leur code :
  ```csharp
  TutorialTargetScanner.AddProvider(scanner =>
  {
      foreach (var view in hand.Views)
          scanner.Bind(view.gameObject, $"hand:{view.Card.Id}");
  });
  ```
- **directement dans le tableau**, avec `go:NomDuGameObject` ou `path:Canvas/Hud/Gold`. Pour ajouter vos propres préfixes, utilisez `TutorialTargetRegistry.AddResolver("slot", i => inventory.Slot(int.Parse(i)))`.

### 5. Le hasard

Ce point n'est nécessaire que si une leçon doit imposer un tirage. Faites passer par `GameRandom` les mélanges et les tirages qui comptent :

```csharp
GameRandom.Shuffle(deck, $"deck:{playerIndex}", card => card.Id);
int first = GameRandom.Range(0, playerCount, "first_player");
```

### Lancer le tutoriel

1. Créez le système avec *Tools > CelticEdge > Tutorial > Create System In Scene* (manager, canvas, contrôleur et voile), puis glissez vos prefabs de bulle et de pointeur dans le `TutorialUIController` (voir [L'interface](#linterface)).
2. Donnez-lui une campagne. Ce peut être un asset (*Create > CelticEdge > Tutorial > Campaign*) ou un tableau chargé à l'exécution :
   ```csharp
   var manager = TutorialManager.Instance;

   manager.SetCampaign(TutorialTableLoader.LoadCampaign("Tutorial/scenario"));
   manager.Initialize(new JsonFileTutorialProgressStore(), profileId);
   manager.SetBridge(bridge);
   manager.StartNextLesson();   // ou StartLesson(lesson) depuis votre menu
   ```
3. Utilisez les autres appels selon le besoin :
   - `CompleteCurrentLesson()` valide la leçon tout de suite, par exemple quand l'objectif est atteint en jeu libre ;
   - `FailCurrentLesson()` la rate ;
   - `AbortLesson()` la quitte ;
   - `PlayInterlude(id)` joue une séquence annexe.

## Écrire les leçons dans un tableau

Chaque ligne du tableau est une étape. Le séparateur peut être `;`, `,` ou une tabulation : il est détecté tout seul. Un modèle commenté se trouve dans `Demo/tableau_modele.csv`.

Le tableau s'utilise de deux façons :
- **à l'import dans l'éditeur** : *Tools > CelticEdge > Tutorial > Import a CSV Table* crée ou met à jour un asset par étape ;
- **à l'exécution** : `TutorialTableLoader` construit les leçons sans passer par des assets.

| Colonne (fr / en) | Valeurs |
|---|---|
| `lecon` / `lesson` | ID de la leçon. Écrivez `lecon_02/trop_cher` pour une ligne de la séquence annexe `trop_cher`. Vide = même leçon que la ligne précédente. |
| `id_etape` / `step_id` | ID stable de l'étape. C'est lui qui permet de réimporter sans perdre les portraits. |
| `style` | `bulle`, `fenetre`, `narrateur`, `aucun` |
| `titre`, `texte` / `title`, `text` | `\n` fait un retour à la ligne. |
| `cle_texte` / `text_key` | Clé de traduction, via `TutorialLocalization.Resolver`. |
| `cibles` / `targets` | IDs séparés par `\|` (4 au maximum). La première sert d'ancre à la bulle. |
| `forme`, `marge` | `rectangle` ou `cercle`, et la marge en pixels. |
| `voile`, `bloquer_clics` | `oui` / `non` |
| `pointeur`, `cible_depot` | `tapoter`, ou `glisser` avec sa cible de dépôt. |
| `placement` | `auto`, `dessus`, `dessous`, `gauche`, `droite` |
| `condition` | `bouton`, `evenement`, `clic_cible`, `delai`, `immediat` |
| `evenement`, `argument`, `nombre` | L'événement attendu, son argument (vide = n'importe lequel) et le nombre de fois. |
| `delai` | En secondes, pour la condition `delai`. |
| `actions_autorisees` | `aucune`, `toutes`, ou `buy:sword \| end_turn` (action:sujet). |
| `commandes_debut`, `commandes_fin` | `Nom(cle=valeur, ...)` séparées par `\|`. |

Les commandes intégrées sont :
- `Wait(seconds=1.5)` ;
- `HideElement(target=...)` et `ShowElement(target=...)` ;
- `PublishEvent(event=..., arg=..., value=...)` ;
- `PlayInterlude(id=...)` ;
- `PauseAgents()` et `ResumeAgents()`.

Toutes les autres commandes vont au pont du jeu.

Deux appels aident à éviter les erreurs dans le tableau :
- `TutorialTable.RegisterCommands("AddGold", ...)` fait signaler les fautes de frappe dans les commandes ;
- `TutorialTable.RegisterActionAlias("acheter", "buy")` permet d'écrire les actions dans la langue de l'équipe.

## Séquences annexes et aides

Une **séquence annexe** est une petite suite d'étapes rangée dans la leçon. Quand le jeu la demande, elle passe par-dessus l'étape en cours, puis l'étape reprend là où elle en était. Les événements reçus pendant la séquence continuent de compter pour l'étape interrompue.

Les **aides** jouent ces séquences toutes seules. Elles se règlent dans `TutorialLessonData.Hints` ou par code :

```csharp
lesson.Hints.Hints.Add(new TutorialHint("refused:buy", "not_yet", once: true));
lesson.Hints.Hints.Add(new TutorialHint("not_enough_gold", "too_expensive", cooldownPeriods: 1));
// côté jeu, pour un refus décidé par ses propres règles :
TutorialManager.Instance.Hints.Trigger("not_enough_gold");
```

Une aide peut se déclencher de trois façons :
- **sur un refus** : `refused:action` ou `refused:action:sujet` ;
- **sur un événement** : `event:id` ou `event:id:arg` ;
- **sur un texte libre** envoyé par le jeu.

Le rythme d'une aide se règle avec :
- `Once` : elle ne s'affiche qu'une fois ;
- `CooldownPeriods` : le nombre de périodes avant de pouvoir la remontrer. Une période passe à chaque `PeriodEventId`, par défaut le début du tour du joueur ;
- `CooldownSeconds` : un délai en secondes avant de pouvoir la remontrer.

`Hints.CanShow` ajoute une condition, par exemple « seulement pendant le tour du joueur ».

## Mise en place propre au jeu

Chaque jeu déclare sa propre classe de mise en place. Le champ *Setup* de la leçon la propose ensuite dans une liste, dans l'inspecteur.

```csharp
[Serializable]
public class MyLessonSetup : ITutorialLessonSetup
{
    public int          StartingGold;
    public List<string> StartingHand = new();
}
```

Le pont la lit dans `PrepareLesson` avec `lesson.GetSetup<MyLessonSetup>()`. Le reste du jeu la lit avec `TutorialSession.GetSetup<MyLessonSetup>()`.

## Hasard scripté et agents scriptés

### Hasard scripté

Le champ `Random` d'une leçon contient trois réglages :
- des **ordres**, avec l'ID de la pile et les éléments du dessus au dessous. Plusieurs ordres pour la même pile s'appliquent l'un après l'autre : premier mélange, puis premier remélange, etc. ;
- des **tirages imposés** ;
- une **seed** pour tout le reste.

Le pont peut aussi ajouter des ordres calculés avec `TutorialSession.Random.EnqueueOrder(...)`.

### Agents scriptés

Le champ `ScriptedAgents` d'une leçon décrit des adversaires dont les tours sont écrits à l'avance. Le jeu les fait jouer ainsi :

```csharp
if (TutorialSession.TryGetScriptedTurn(playerIndex, turn, out var actions))
    StartCoroutine(TutorialSession.PlayScriptedTurn(bridge, playerIndex, actions));
else
    ai.PlayTurn();
```

Un tour non écrit est joué par l'IA du jeu, ou se résume à `UnscriptedTurnAction` (par défaut `end_turn`).

## Ce qui reste dans le jeu : l'exemple de Vermines

Vermines utilise toujours sa propre copie, l'ancien module `Vermines.Tutorial`. Si Vermines passe à `CelticEdge.Tutorial`, son code se répartirait ainsi :

| Aujourd'hui dans Vermines | Avec CelticEdge.Tutorial |
|---|---|
| `ScriptedMatchSetup` (decks, cour, dieux, marché) | Une classe `VerminesLessonSetup : ITutorialLessonSetup` dans le jeu |
| Ordre des decks et de la cour | `Random.Orders` avec les piles `deck:0`, `court_1`... |
| `GameActionType` (PlayCard, BuyCard...) | Des constantes du jeu (`"play"`, `"buy"`...) |
| Commandes `AddEloquence`, `GiveCardToHand`... | Les mêmes noms, gérés par `VerminesTutorialBridge.ExecuteCommand` |
| `VerminesTutorialContent` (lecture du CSV, séquences annexes) | `TutorialTableLoader`, plus les mises en place écrites en code |
| `TutorialFreePlayCoach` : aides et recharges | `TutorialHintSet`. Les cartes proposées dans la cour restent dans le jeu. |
| `TutorialTargetBinder` | Des fournisseurs pour `TutorialTargetScanner` ; `go:Nom` est intégré |
| `TutorialLessonMenu` | Reste dans le jeu, avec les données de `TutorialManager` |

## L'interface

La librairie ne contient que ce qui ne dépend pas du style du jeu.

| Dans la librairie (`Scripts/UI/`) | Rôle |
|---|---|
| `TutorialUIController` | Affiche chaque étape : suit les cibles, choisit la vue, relaie Suivant / Passer. |
| `ITutorialHighlight`, `ITutorialPopup`, `ITutorialPointer` | Ce que le contrôleur attend d'un voile, d'une bulle et d'un pointeur. |
| `TutorialSpotlightOverlay` | Voile avec des trous, dessiné par un shader. Couleur, douceur et animation se règlent dans l'inspecteur. |
| `TutorialPopupView` | Bulle, fenêtre ou narrateur. Il ne crée rien : il remplit les textes, l'image et les boutons de **votre** prefab, et le place à côté de la cible. |
| `TutorialPointerView` | Anime le `RectTransform` que vous lui donnez (votre image de main ou de flèche). |
| `TutorialTargetLocator` | Calcule le rectangle à l'écran d'une cible UI, 2D ou 3D. |

Le contrôleur référence ses vues par leur interface : glissez n'importe quel composant qui les implémente. Pour un comportement différent (bulle animée autrement, contour au lieu d'un voile...), écrivez votre propre composant qui implémente l'interface. Chaque vue est facultative.

L'apparence de la démo (couleurs, tailles, menu des leçons) est dans `Demo/Scripts/QuickStartUI.cs` et `QuickStartLessonMenu.cs`. Ce sont des exemples à copier, pas des classes de la librairie.

## Arborescence

```
Tutorial/Assets/Celtic-Edge/Package/Tutorial/
  CelticEdge.Tutorial.asmdef, README.md, CHANGELOG.md
  Scripts/        le module
    Core/         événements, filtre d'actions, cibles, textes
    Data/         campagne, leçon, étape, mise en place
    Flow/         TutorialManager, pont, session
    Hints/        aides
    Scripting/    commandes, hasard, agents scriptés
    Authoring/    tableau CSV
    Progress/     sauvegarde de la progression
    UI/           contrôleur, interfaces des vues et leurs implémentations par défaut
  Resources/      shader du voile
  Editor/         éditeur de leçons, import CSV, création du système
  Demo/           scène et scripts de la démo (dont son interface), modèle de tableau
  Tests/EditMode/ tests
```
