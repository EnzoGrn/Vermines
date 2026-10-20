using System;
using System.Collections.Generic;
using UnityEngine;

namespace Vermines.Tutorial
{
    /// Les actions de jeu qu'un joueur humain peut tenter. Le jeu demande au
    /// TutorialInputGate s'il a le droit de les faire avant de les exécuter.
    public enum GameActionType
    {
        PlayCard,
        BuyCard,
        RecycleCard,
        DiscardCard,
        KeepCard,
        ActivatePartisanEffect,
        SacrificePartisan,
        EndTurn,
        OpenMenu,
        Other,
        /// Passer à la phase suivante (bouton de phase). cardId = nom de la phase en cours
        /// ("Sacrifice", "Gain", "Action"...), vide = n'importe laquelle.
        NextPhase,
        /// Choisir l'un des effets d'une carte qui en propose plusieurs (ex : la dague).
        /// cardId = "carte#index" (index de l'effet dans la carte, 0 = le premier),
        /// ou juste "carte" pour n'importe lequel de ses effets.
        ChooseEffect
    }

    /// Une action autorisée pendant une étape. cardId vide = n'importe quelle carte.
    [Serializable]
    public struct AllowedAction
    {
        public GameActionType action;

        [Tooltip("Vide = n'importe quelle carte. Sinon l'ID de carte du jeu (ex: \"ouvriere\").")]
        public string cardId;

        public AllowedAction(GameActionType action, string cardId = null)
        {
            this.action = action;
            this.cardId = cardId;
        }

        public bool Allows(GameActionType requested, string requestedCardId)
        {
            if (action != requested)
                return false;
            return string.IsNullOrEmpty(cardId) || string.Equals(cardId, requestedCardId, StringComparison.OrdinalIgnoreCase);
        }

        public override string ToString() => string.IsNullOrEmpty(cardId) ? action.ToString() : $"{action}:{cardId}";
    }

    /// Filtre des actions de jeu du joueur humain pendant le tutoriel.
    ///
    /// Le voile noir bloque les clics, mais une partie de cartes a aussi des
    /// raccourcis, du glisser-déposer et des actions déclenchées par code : c'est
    /// donc la logique du jeu elle-même qui doit poser la question, au moment où
    /// l'action est validée :
    ///
    ///   if (!TutorialInputGate.IsAllowed(GameActionType.BuyCard, card.Id)) return;
    ///
    /// Hors tutoriel, ou pendant une étape sans restriction, tout est autorisé.
    /// Les actions de l'adversaire scripté ne passent pas par ce filtre.
    public static class TutorialInputGate
    {
        private static readonly List<AllowedAction> _allowed = new List<AllowedAction>();

        public static bool IsRestricted { get; private set; }

        /// Levé quand le joueur tente une action interdite, pour que l'UI puisse
        /// rappeler la consigne (faire vibrer la bulle, pulser la zone éclairée...).
        public static event Action<GameActionType, string> OnActionRefused;

        public static IReadOnlyList<AllowedAction> Allowed => _allowed;

        /// Restreint les actions à la liste donnée. Une liste vide interdit tout.
        public static void Restrict(IEnumerable<AllowedAction> allowed)
        {
            _allowed.Clear();
            if (allowed != null)
                _allowed.AddRange(allowed);
            IsRestricted = true;
        }

        public static void Release()
        {
            _allowed.Clear();
            IsRestricted = false;
        }

        public static bool IsAllowed(GameActionType action, string cardId = null)
        {
            if (!IsRestricted)
                return true;

            foreach (var a in _allowed)
                if (a.Allows(action, cardId))
                    return true;

            OnActionRefused?.Invoke(action, cardId);
            return false;
        }

        /// Comme IsAllowed, mais sans lever OnActionRefused : pour griser un bouton
        /// ou décider de l'état "jouable" d'une carte dans l'affichage.
        public static bool WouldAllow(GameActionType action, string cardId = null)
        {
            if (!IsRestricted)
                return true;
            foreach (var a in _allowed)
                if (a.Allows(action, cardId))
                    return true;
            return false;
        }
    }
}
