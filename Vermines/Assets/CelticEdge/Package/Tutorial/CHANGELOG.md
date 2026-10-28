# Release Notes

## 0.1.0 (2026-10-20)

First version, excerpted from the Vermines tutorial.

- Features specific to Vermines have been removed from the module: action types, board commands, game setup (decks, court, gods), and stack names.
- Actions and commands: text identifiers chosen by the game (`TutorialInputGate.IsAllowed("buy", id)`, `AddGold(amount=2)`).
- Game-specific setup: `ITutorialLessonSetup`, selected in the lesson inspector.
- Generic scripted randomization: decks named by the game (`GameRandom.Shuffle(list, "deck:0", ...)`).
- Scripted agents (turn-based opponents): `ScriptedAgent`.
- New: hints (`TutorialHints`), runtime table loading (`TutorialTableLoader`), lesson list (`TutorialLessonMenu`), targets resolved on demand (`go:Name`, `path:...`) and set via code (`TutorialTargetScanner`), `TutorialManager.CompleteCurrentLesson`.
- The table also accepts English headers; importing into the editor handles appended sequences.
