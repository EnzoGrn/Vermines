using UnityEngine;
using UnityEngine.EventSystems;

namespace Vermines.Tutorial
{
    /// À poser sur tout ce qu'une étape doit pouvoir éclairer, pointer ou masquer :
    /// bouton "Fin de tour", zone partisan, compteur d'éloquence, pile du marché,
    /// une carte précise de la main...
    ///
    /// Pour les cartes créées à la volée, le jeu donne l'ID au moment où la carte
    /// arrive dans une zone, par exemple :
    ///   target.SetTargetId($"hand:{card.Id}");   // "hand:ouvriere"
    ///   target.SetTargetId($"court:{card.Id}");  // "court:reine"
    /// Plusieurs objets peuvent porter le même ID (deux Ouvrières en main) : le
    /// tutoriel prend le premier visible.
    [DisallowMultipleComponent]
    public class TutorialTarget : MonoBehaviour, IPointerClickHandler
    {
        [Tooltip("ID référencé par les étapes (colonne \"cibles\" du tableau).")]
        [SerializeField] private string targetId;

        [Tooltip("Zone à éclairer si elle diffère de cet objet (ex: le cadre d'une carte plutôt que son ombre).")]
        [SerializeField] private RectTransform highlightRectOverride;

        [Tooltip("Publie TutorialEvents.TargetClicked avec cet ID quand l'objet est cliqué (via l'EventSystem).")]
        [SerializeField] private bool publishClicks = true;

        private CanvasGroup _canvasGroup;
        private bool _tutorialHidden;

        public string TargetId => targetId;
        public RectTransform HighlightRect => highlightRectOverride != null ? highlightRectOverride : transform as RectTransform;
        public bool IsHiddenByTutorial => _tutorialHidden;

        public bool IsVisible => isActiveAndEnabled && !_tutorialHidden;

        private void OnEnable() => TutorialTargetRegistry.Register(targetId, this);
        private void OnDisable() => TutorialTargetRegistry.Unregister(targetId, this);

        private void Reset()
        {
            if (string.IsNullOrEmpty(targetId))
                targetId = gameObject.name;
        }

        /// Change l'ID à l'exécution (cartes instanciées, déplacées d'une zone à l'autre).
        public void SetTargetId(string newId)
        {
            if (newId == targetId)
                return;
            if (isActiveAndEnabled)
                TutorialTargetRegistry.Unregister(targetId, this);
            if (_tutorialHidden)
                SetHiddenByTutorial(false);
            targetId = newId;
            if (isActiveAndEnabled)
                TutorialTargetRegistry.Register(targetId, this);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (publishClicks)
                PublishInteraction();
        }

        /// À relier à un Button.onClick, ou à appeler depuis votre propre gestion
        /// d'input (OnMouseDown, Input System...) si l'EventSystem ne voit pas l'objet.
        public void PublishInteraction() => TutorialEventBus.Publish(TutorialEvents.TargetClicked, targetId);

        /// Cache ou réaffiche l'élément pour le tutoriel, sans le désactiver (il reste
        /// enregistré et le jeu continue de le piloter). Sert à révéler l'interface
        /// petit à petit, comme dans les premières missions de Hearthstone.
        public void SetHiddenByTutorial(bool hidden)
        {
            _tutorialHidden = hidden;

            if (transform is RectTransform)
            {
                if (_canvasGroup == null && !TryGetComponent(out _canvasGroup))
                    _canvasGroup = gameObject.AddComponent<CanvasGroup>();
                _canvasGroup.alpha = hidden ? 0f : 1f;
                _canvasGroup.interactable = !hidden;
                _canvasGroup.blocksRaycasts = !hidden;
                return;
            }

            foreach (var r in GetComponentsInChildren<Renderer>(true))
                r.enabled = !hidden;
            foreach (var c in GetComponentsInChildren<Collider>(true))
                c.enabled = !hidden;
            foreach (var c in GetComponentsInChildren<Collider2D>(true))
                c.enabled = !hidden;
        }
    }
}
