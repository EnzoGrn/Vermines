using System;
using System.Collections.Generic;
using UnityEngine;

namespace Vermines.Tutorial
{
    /// Une liste d'IDs de cartes, du dessus vers le dessous de la pile.
    /// (Unity ne sait pas sérialiser une List<List<string>>, d'où cette enveloppe.)
    [Serializable]
    public class CardOrder
    {
        public List<string> cardIds = new List<string>();

        public CardOrder() { }
        public CardOrder(params string[] ids) { cardIds.AddRange(ids); }
    }

    /// Un tirage aléatoire hors mélange dont le résultat est imposé.
    [Serializable]
    public struct ScriptedRoll
    {
        [Tooltip("La raison passée à GameRandom.Range (ex: \"first_player\").")]
        public string reason;
        public int value;
    }

    [Serializable]
    public class ScriptedPlayerSetup
    {
        [Tooltip("Vrai pour le joueur, faux pour l'adversaire scripté.")]
        public bool isHuman = true;

        public string displayName;

        [Tooltip("ID du plateau cultiste (détermine la famille).")]
        public string cultistId;

        [Tooltip("ID ou nom du plateau divinité. Vide = un dieu sans pouvoir ; \"aleatoire\" = tiré au hasard comme dans une partie normale.")]
        public string godId;

        public int startingEloquence;
        public int startingSouls;

        [Tooltip("Ordre du deck au premier mélange, du dessus au dessous. Les 3 premières cartes forment la main de départ. Vide = deck de départ normal, mélangé au hasard.")]
        public List<string> deckTopToBottom = new List<string>();

        [Tooltip("Vrai = le deck est exactement deckTopToBottom, même vide (pas de deck de départ normal).")]
        public bool exactDeck;

        [Tooltip("Si renseigné, la main de départ est exactement ces cartes (pas de pioche initiale).")]
        public List<string> startingHand = new List<string>();

        [Tooltip("Ordre imposé à chaque remélange de la défausse (1er, 2e...). Au-delà, mélange aléatoire.")]
        public List<CardOrder> reshuffleOrders = new List<CardOrder>();

        [Tooltip("Partisans déjà posés dans la zone partisan au début de la leçon (leçons qui démarrent en milieu de partie).")]
        public List<string> startingPartisans = new List<string>();

        [Tooltip("Cartes déjà dans la défausse au début de la leçon.")]
        public List<string> startingDiscard = new List<string>();

        [Tooltip("Partisans déjà sacrifiés au début de la leçon (pour le bonus de famille du cultiste).")]
        public List<string> startingSacrificed = new List<string>();
    }

    /// Mise en place complète d'une micro-partie de tutoriel : remplace la seed
    /// aléatoire par un état de départ et des ordres de pioche connus à l'avance.
    [Serializable]
    public class ScriptedMatchSetup
    {
        [Tooltip("Index 0 = le joueur humain en général.")]
        public List<ScriptedPlayerSetup> players = new List<ScriptedPlayerSetup>();

        public int firstPlayerIndex;

        [Tooltip("Ordre de la pioche de cour I, du dessus au dessous. Les 3 premières sont révélées dans la cour.")]
        public List<string> courtPileI = new List<string>();

        [Tooltip("Ordre de la pioche de cour II. Les 2 premières sont révélées dans la cour.")]
        public List<string> courtPileII = new List<string>();

        [Tooltip("Si renseigné, la cour de départ est exactement cette liste (au lieu de 3 cartes I + 2 cartes II).")]
        public List<string> courtOverride = new List<string>();

        [Tooltip("Piles d'objets du marché, dans l'ordre. \"carte*N\" = N exemplaires seulement, \"*\" = tous les autres objets. Vide = marché normal.")]
        public List<string> marketPiles = new List<string>();

        [Tooltip("Résultats imposés pour GameRandom.Range (premier joueur, IA...).")]
        public List<ScriptedRoll> scriptedRolls = new List<ScriptedRoll>();

        [Tooltip("Seed des tirages non scriptés : la leçon reste identique d'une session à l'autre.")]
        public int fallbackSeed = 1;

        [Header("Règles adaptées à la leçon")]
        [Tooltip("Âmes pour déclencher la fin de partie (100 dans le jeu de base). 0 = règle normale.")]
        public int victorySoulsOverride;

        [Tooltip("Nombre max de tours de la leçon. 0 = illimité.")]
        public int maxTurns;
    }

    /// Une action de jeu imposée à l'adversaire.
    [Serializable]
    public struct ScriptedGameAction
    {
        public GameActionType action;

        [Tooltip("Carte concernée (carte jouée, achetée, sacrifiée...).")]
        public string cardId;

        [Tooltip("Pause avant l'action, pour que le joueur ait le temps de suivre.")]
        public float delayBefore;

        public ScriptedGameAction(GameActionType action, string cardId = null, float delayBefore = 0.6f)
        {
            this.action = action;
            this.cardId = cardId;
            this.delayBefore = delayBefore;
        }

        public override string ToString() => string.IsNullOrEmpty(cardId) ? action.ToString() : $"{action}:{cardId}";
    }

    [Serializable]
    public class ScriptedTurn
    {
        [Tooltip("Numéro du tour de cet adversaire (1 = son premier tour).")]
        public int turnNumber = 1;
        public List<ScriptedGameAction> actions = new List<ScriptedGameAction>();
    }

    /// Les tours de l'adversaire écrits à l'avance. Un tour absent laisse la main à
    /// l'IA normale (ou à un simple "fin de tour" si useAIForUnscriptedTurns est faux).
    [Serializable]
    public class OpponentScript
    {
        public int playerIndex = 1;
        public List<ScriptedTurn> turns = new List<ScriptedTurn>();

        [Tooltip("Faux = un tour non scripté se résume à \"fin de tour\".")]
        public bool useAIForUnscriptedTurns;

        [Tooltip("L'IA ne dépasse jamais ce nombre d'âmes (0 = pas de limite), pour qu'elle ne gagne pas la leçon.")]
        public int aiMaxSouls;

        public bool TryGetTurn(int turnNumber, out IReadOnlyList<ScriptedGameAction> actions)
        {
            foreach (var t in turns)
            {
                if (t.turnNumber == turnNumber)
                {
                    actions = t.actions;
                    return true;
                }
            }
            actions = null;
            return false;
        }
    }
}
