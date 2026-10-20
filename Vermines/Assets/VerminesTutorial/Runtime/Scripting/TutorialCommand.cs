using System;
using UnityEngine;

namespace Vermines.Tutorial
{
    /// Ce qu'une étape peut faire au plateau, à son début ou à sa fin. Les commandes
    /// d'interface (Show/HideElement, Wait) sont gérées par le tutoriel lui-même ;
    /// toutes les autres sont transmises au jeu via ITutorialGameBridge.
    public enum TutorialCommandType
    {
        // Gérées par le tutoriel
        Wait,
        HideElement,
        ShowElement,
        PublishEvent,

        // Transmises au jeu
        SetEloquence,
        AddEloquence,
        AddSouls,
        DrawCards,
        GiveCardToHand,
        PutCardOnTopOfDeck,
        AddCardToCourt,
        RemoveCardFromCourt,
        PauseOpponent,
        ResumeOpponent,
        EndCurrentTurn,
        PlayOpponentTurn,
        SetCameraFocus,
        PlaySound,
        Custom
    }

    /// Une commande et ses paramètres. Dans le tableau CSV elle s'écrit
    /// Type(clé=valeur, ...), par exemple :
    ///   AddEloquence(amount=5)
    ///   GiveCardToHand(card=ouvriere, player=0)
    ///   HideElement(target=market)
    ///   Wait(seconds=1.5)
    [Serializable]
    public class TutorialCommand
    {
        public TutorialCommandType type;

        [Tooltip("Index du joueur concerné (0 = joueur humain).")]
        public int player;

        [Tooltip("ID de carte (GiveCardToHand, AddCardToCourt...).")]
        public string card;

        [Tooltip("ID de cible (Hide/ShowElement, SetCameraFocus).")]
        public string target;

        public int amount;
        public float seconds;

        [Tooltip("Texte libre : ID d'événement (PublishEvent), nom de son, commande Custom...")]
        public string arg;

        public override string ToString()
        {
            var parts = new System.Collections.Generic.List<string>();
            if (player != 0) parts.Add($"player={player}");
            if (!string.IsNullOrEmpty(card)) parts.Add($"card={card}");
            if (!string.IsNullOrEmpty(target)) parts.Add($"target={target}");
            if (amount != 0) parts.Add($"amount={amount}");
            if (seconds != 0) parts.Add($"seconds={seconds.ToString(System.Globalization.CultureInfo.InvariantCulture)}");
            if (!string.IsNullOrEmpty(arg)) parts.Add($"arg={arg}");
            return $"{type}({string.Join(", ", parts)})";
        }

        public bool IsHandledByTutorial =>
            type == TutorialCommandType.Wait ||
            type == TutorialCommandType.HideElement ||
            type == TutorialCommandType.ShowElement ||
            type == TutorialCommandType.PublishEvent;
    }
}
