using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Vermines.Tutorial
{
    /// Construit par code l'affichage par défaut du tutoriel : canvas, voile, bulle,
    /// fenêtre, narrateur et pointeur, déjà reliés. Utilisé par le menu de l'éditeur
    /// (Tools > Vermines > Tutoriel > Créer le système dans la scène) et par le
    /// projet de test. Volontairement sobre : à habiller avec la DA du jeu.
    public static class TutorialUIBuilder
    {
        public static readonly Color PanelColor = new Color(0.10f, 0.07f, 0.09f, 0.96f);
        public static readonly Color TextColor = new Color(0.96f, 0.92f, 0.85f);
        public static readonly Color AccentColor = new Color(0.80f, 0.58f, 0.16f);

        /// Crée un GameObject "TutorialSystem" avec le TutorialManager et son interface.
        public static TutorialManager BuildSystem(TutorialCampaignData campaign = null)
        {
            var root = new GameObject("TutorialSystem");
            root.SetActive(false); // les Awake attendent que tout soit relié
            var manager = root.AddComponent<TutorialManager>();
            var ui = BuildUI(root.transform);
            manager.SetUI(ui);
            if (campaign != null)
                manager.SetCampaign(campaign);
            root.SetActive(true);
            return manager;
        }

        public static TutorialUIController BuildUI(Transform parent)
        {
            var canvasGO = new GameObject("TutorialCanvas", typeof(RectTransform));
            canvasGO.transform.SetParent(parent, false);
            var canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 500;
            var scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            canvasGO.AddComponent<GraphicRaycaster>();

            // Voile (le matériau est retrouvé par Shader.Find au lancement si aucun n'est assigné)
            var overlayGO = NewUI("Overlay", canvasGO.transform);
            Stretch((RectTransform)overlayGO.transform);
            var overlay = overlayGO.AddComponent<TutorialSpotlightOverlay>();

            // Pointeur
            var pointerGO = NewUI("Pointer", canvasGO.transform);
            pointerGO.AddComponent<CanvasGroup>();
            var pointer = pointerGO.AddComponent<TutorialPointerView>();
            var hand = NewUI("Hand", pointerGO.transform);
            var handRT = (RectTransform)hand.transform;
            handRT.sizeDelta = new Vector2(56, 56);
            handRT.pivot = new Vector2(0f, 1f);
            var handImg = hand.AddComponent<Image>();
            handImg.color = new Color(1f, 0.85f, 0.3f);
            handImg.raycastTarget = false;
            pointer.Configure(handRT);

            var bubble = CreatePopup("Bubble", canvasGO.transform, 460, true);
            var modal = CreatePopup("Modal", canvasGO.transform, 720, false);
            var modalRT = (RectTransform)modal.transform;
            modalRT.anchorMin = modalRT.anchorMax = modalRT.pivot = new Vector2(0.5f, 0.5f);
            modalRT.anchoredPosition = Vector2.zero;
            var narrator = CreatePopup("Narrator", canvasGO.transform, 560, false);
            var narratorRT = (RectTransform)narrator.transform;
            narratorRT.anchorMin = narratorRT.anchorMax = narratorRT.pivot = new Vector2(0f, 0f);
            narratorRT.anchoredPosition = new Vector2(40, 40);

            var ui = canvasGO.AddComponent<TutorialUIController>();
            ui.Configure(canvas, overlay, bubble, modal, narrator, pointer);
            return ui;
        }

        private static TutorialPopupView CreatePopup(string name, Transform parent, float width, bool anchorToTarget)
        {
            var go = NewUI(name, parent);
            var rt = (RectTransform)go.transform;
            rt.sizeDelta = new Vector2(width, 0);
            go.AddComponent<CanvasGroup>();
            var bg = go.AddComponent<Image>();
            bg.color = PanelColor;
            var layout = go.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(24, 24, 20, 20);
            layout.spacing = 12;
            layout.childControlHeight = true;
            layout.childControlWidth = true;
            layout.childForceExpandHeight = false;
            var fitter = go.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var portrait = NewUI("Portrait", go.transform).AddComponent<Image>();
            portrait.preserveAspect = true;
            portrait.gameObject.AddComponent<LayoutElement>().preferredHeight = 96;

            var title = NewText("Title", go.transform, 28, true);
            var body = NewText("Body", go.transform, 22, false);

            var buttons = NewUI("Buttons", go.transform);
            var hl = buttons.AddComponent<HorizontalLayoutGroup>();
            hl.spacing = 12;
            hl.childAlignment = TextAnchor.MiddleRight;
            hl.childForceExpandWidth = false;
            hl.childControlWidth = true;
            hl.childControlHeight = true;
            var skip = NewButton("Skip", buttons.transform, "Passer la leçon", new Color(0.3f, 0.3f, 0.3f), 190);
            var next = NewButton("Next", buttons.transform, "Suivant", AccentColor, 150);

            RectTransform arrowRT = null;
            if (anchorToTarget)
            {
                var arrow = NewUI("Arrow", go.transform);
                arrow.AddComponent<LayoutElement>().ignoreLayout = true;
                arrowRT = (RectTransform)arrow.transform;
                arrowRT.sizeDelta = new Vector2(26, 26);
                // Losange (carré tourné de 45°) : sa moitié dépasse du bord et fait la pointe.
                var diamond = NewUI("Diamond", arrow.transform);
                var diamondRT = (RectTransform)diamond.transform;
                Stretch(diamondRT);
                diamondRT.localRotation = Quaternion.Euler(0, 0, 45);
                var arrowImg = diamond.AddComponent<Image>();
                arrowImg.color = bg.color;
                arrowImg.raycastTarget = false;
                arrow.transform.SetAsFirstSibling(); // derrière le fond du popup
            }

            var view = go.AddComponent<TutorialPopupView>();
            view.Configure(rt, title, body, portrait, next, skip, arrowRT, anchorToTarget);
            return view;
        }

        public static GameObject NewUI(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return go;
        }

        public static void Stretch(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = rt.offsetMax = Vector2.zero;
        }

        public static TMP_Text NewText(string name, Transform parent, float size, bool bold)
        {
            var t = NewUI(name, parent).AddComponent<TextMeshProUGUI>();
            t.fontSize = size;
            t.fontStyle = bold ? FontStyles.Bold : FontStyles.Normal;
            t.color = TextColor;
            t.raycastTarget = false;
            t.text = string.Empty;
            return t;
        }

        public static Button NewButton(string name, Transform parent, string label, Color color, float width)
        {
            var go = NewUI(name, parent);
            var img = go.AddComponent<Image>();
            img.color = color;
            var button = go.AddComponent<Button>();
            var le = go.AddComponent<LayoutElement>();
            le.minHeight = 48;
            le.preferredWidth = width;
            var text = NewText("Label", go.transform, 22, true);
            text.text = label;
            text.alignment = TextAlignmentOptions.Center;
            Stretch(text.rectTransform);
            return button;
        }
    }
}
