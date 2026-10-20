using System.Collections;
using System.Collections.Generic;

namespace Vermines.Tutorial
{
    /// Ce que le jeu doit fournir au tutoriel. C'est la seule classe à écrire côté jeu
    /// (un MonoBehaviour dans la scène de partie, glissé dans le TutorialManager ou
    /// enregistré via TutorialManager.Instance.SetBridge(this)).
    ///
    /// Toutes les méthodes sont des coroutines : le tutoriel attend la fin des
    /// animations (pioche, apparition de la cour...) avant d'afficher l'étape suivante.
    public interface ITutorialGameBridge
    {
        /// Prépare le plateau d'après la mise en place de la leçon. Au moment de
        /// l'appel, GameRandom.Current est déjà le mélange scripté : le jeu crée la
        /// partie comme d'habitude (il mélange, pioche 3 cartes...), et seuls les
        /// éléments sans équivalent normal (startingPartisans, courtOverride,
        /// victorySoulsOverride...) demandent du code spécifique.
        IEnumerator StartScriptedMatch(ScriptedMatchSetup setup);

        /// Exécute une commande de plateau (AddEloquence, GiveCardToHand...).
        IEnumerator ExecuteCommand(TutorialCommand command);

        /// Fait jouer une action à un joueur non humain (adversaire scripté).
        IEnumerator PerformScriptedAction(int playerIndex, ScriptedGameAction action);

        /// Nettoie la partie (retour au menu, écran de victoire de la leçon...).
        /// completed = vrai si la leçon a été terminée, faux si abandonnée.
        void EndScriptedMatch(bool completed);
    }

    /// Ce qui est vrai pendant une leçon. Le jeu le consulte pour savoir s'il joue une
    /// partie scriptée (par ex. pour désactiver le chat, les succès, le classement) et
    /// pour faire jouer l'adversaire :
    ///
    ///   if (TutorialSession.IsActive && TutorialSession.TryGetOpponentTurn(p, turn, out var actions))
    ///       StartCoroutine(TutorialSession.PlayScriptedTurn(bridge, p, actions));
    ///   else
    ///       ai.PlayTurn();
    public static class TutorialSession
    {
        public static bool IsActive { get; private set; }
        public static string LessonId { get; private set; }
        public static ScriptedMatchSetup Setup { get; private set; }
        public static OpponentScript Opponent { get; private set; }

        /// Vrai quand une étape a mis l'adversaire en pause (commande PauseOpponent).
        public static bool OpponentPaused { get; set; }

        internal static void Begin(string lessonId, ScriptedMatchSetup setup, OpponentScript opponent)
        {
            IsActive = true;
            LessonId = lessonId;
            Setup = setup;
            Opponent = opponent;
            OpponentPaused = false;
        }

        internal static void End()
        {
            IsActive = false;
            LessonId = null;
            Setup = null;
            Opponent = null;
            OpponentPaused = false;
        }

        public static bool TryGetOpponentTurn(int playerIndex, int turnNumber, out IReadOnlyList<ScriptedGameAction> actions)
        {
            actions = null;
            if (!IsActive || Opponent == null || Opponent.playerIndex != playerIndex)
                return false;
            if (Opponent.TryGetTurn(turnNumber, out actions))
                return true;
            if (Opponent.useAIForUnscriptedTurns)
                return false;
            actions = new[] { new ScriptedGameAction(GameActionType.EndTurn) };
            return true;
        }

        /// Joue une liste d'actions scriptées, en respectant les pauses imposées par
        /// le tutoriel (OpponentPaused) et les délais entre actions.
        public static IEnumerator PlayScriptedTurn(ITutorialGameBridge bridge, int playerIndex, IReadOnlyList<ScriptedGameAction> actions)
        {
            foreach (var action in actions)
            {
                if (action.delayBefore > 0)
                    yield return new UnityEngine.WaitForSeconds(action.delayBefore);
                // Vérifié après le délai : une pause demandée pendant ce temps est respectée.
                while (OpponentPaused)
                    yield return null;
                yield return bridge.PerformScriptedAction(playerIndex, action);
            }
        }
    }
}
