using System.Collections.Generic;
using UnityEngine;

namespace Vermines.Tutorial
{
    public enum PopupStyle
    {
        [Tooltip("Bulle accrochée à la cible, avec une flèche.")]
        Bubble,
        [Tooltip("Fenêtre centrée, pour les explications longues (règle, symbole...).")]
        Modal,
        [Tooltip("Personnage narrateur dans un coin de l'écran.")]
        Narrator,
        [Tooltip("Pas de texte : seulement l'éclairage et/ou le pointeur.")]
        None
    }

    public enum PopupPlacement { Auto, Above, Below, Left, Right }

    public enum SpotlightShape { RoundedRect, Circle }

    public enum PointerMode
    {
        None,
        [Tooltip("Une main/flèche qui tapote la cible principale.")]
        Tap,
        [Tooltip("Une main qui glisse de la cible principale vers la cible de dépôt.")]
        Drag
    }

    public enum AdvanceCondition
    {
        [Tooltip("Passe tout de suite à l'étape suivante (étape de commandes seules).")]
        Immediate,
        [Tooltip("Passe après un délai.")]
        WaitForDelay,
        [Tooltip("Attend le bouton \"Suivant\" du popup.")]
        WaitForButton,
        [Tooltip("Attend un événement de jeu publié par TutorialEventBus.")]
        WaitForEvent,
        [Tooltip("Attend un clic sur la cible principale.")]
        WaitForTargetClick
    }

    /// Une étape : ce qu'on dit, ce qu'on éclaire, ce que le joueur a le droit de
    /// faire, et ce qu'il doit faire pour avancer. Basé sur le TutorialStepData du
    /// prototype, enrichi pour Vermines.
    [CreateAssetMenu(fileName = "NewTutorialStep", menuName = "Vermines/Tutoriel/Étape")]
    public class TutorialStepData : ScriptableObject
    {
        [Tooltip("Identifiant stable de l'étape (colonne \"id_etape\" du tableau). Sert à la mise à jour depuis le CSV.")]
        public string stepId;

        [Header("Texte")]
        public PopupStyle popupStyle = PopupStyle.Bubble;
        public TutorialText title;
        public TutorialText body;
        public Sprite portrait;
        public PopupPlacement placement = PopupPlacement.Auto;

        [Header("Éclairage")]
        [Tooltip("IDs des TutorialTarget à éclairer (4 max). La première sert d'ancre au popup et au pointeur.")]
        public List<string> targetIds = new List<string>();
        public SpotlightShape shape = SpotlightShape.RoundedRect;
        [Tooltip("Marge autour de la cible, en pixels du canvas.")]
        public float padding = 16f;
        [Tooltip("Assombrit l'écran autour des cibles.")]
        public bool dimScreen = true;
        [Tooltip("Bloque les clics hors des zones éclairées.")]
        public bool blockClicksOutsideTargets = true;

        [Header("Pointeur")]
        public PointerMode pointer = PointerMode.None;
        [Tooltip("Pour PointerMode.Drag : ID de la cible où déposer.")]
        public string pointerDropTargetId;

        [Header("Progression")]
        public AdvanceCondition advanceCondition = AdvanceCondition.WaitForButton;
        [Tooltip("WaitForEvent : ID de l'événement (voir TutorialEvents).")]
        public string eventId;
        [Tooltip("WaitForEvent : argument attendu (ex: ID de carte). Vide = n'importe lequel.")]
        public string eventArg;
        [Tooltip("WaitForEvent : nombre d'occurrences à attendre.")]
        [Min(1)] public int eventCount = 1;
        [Tooltip("WaitForDelay : durée en secondes.")]
        public float delaySeconds = 2f;

        [Header("Règles pendant l'étape")]
        [Tooltip("Limite les actions du joueur à la liste ci-dessous (liste vide = aucune action).")]
        public bool restrictGameActions = true;
        public List<AllowedAction> allowedActions = new List<AllowedAction>();

        [Header("Commandes")]
        [Tooltip("Exécutées avant d'afficher l'étape.")]
        public List<TutorialCommand> onEnter = new List<TutorialCommand>();
        [Tooltip("Exécutées une fois la condition remplie.")]
        public List<TutorialCommand> onExit = new List<TutorialCommand>();

        public string MainTargetId => targetIds != null && targetIds.Count > 0 ? targetIds[0] : null;

        /// Problèmes de saisie détectables sans lancer le jeu (affichés par l'éditeur
        /// et par l'import CSV).
        public IEnumerable<string> Validate()
        {
            if (advanceCondition == AdvanceCondition.WaitForButton && popupStyle == PopupStyle.None)
                yield return "Attend le bouton \"Suivant\" mais n'affiche aucun popup : le joueur sera bloqué.";
            if (advanceCondition == AdvanceCondition.WaitForEvent && string.IsNullOrEmpty(eventId))
                yield return "Attend un événement mais aucun ID d'événement n'est renseigné.";
            if (advanceCondition == AdvanceCondition.WaitForTargetClick && string.IsNullOrEmpty(MainTargetId))
                yield return "Attend un clic sur la cible mais aucune cible n'est renseignée.";
            if (pointer == PointerMode.Drag && string.IsNullOrEmpty(pointerDropTargetId))
                yield return "Pointeur en glisser sans cible de dépôt.";
            if (pointer != PointerMode.None && string.IsNullOrEmpty(MainTargetId))
                yield return "Pointeur demandé sans cible.";
            if (targetIds != null && targetIds.Count > TutorialSpotlightOverlay.MaxHoles)
                yield return $"Plus de {TutorialSpotlightOverlay.MaxHoles} cibles : les suivantes ne seront pas éclairées.";
            if (advanceCondition == AdvanceCondition.WaitForEvent && restrictGameActions && allowedActions.Count == 0
                && eventId != TutorialEvents.TurnStarted && eventId != TutorialEvents.ButtonClicked && eventId != TutorialEvents.TargetClicked)
                yield return "Attend une action de jeu mais aucune action n'est autorisée : vérifiez \"actions autorisées\".";
            if (popupStyle != PopupStyle.None && body.IsEmpty && title.IsEmpty)
                yield return "Popup affiché sans texte.";
        }
    }
}
