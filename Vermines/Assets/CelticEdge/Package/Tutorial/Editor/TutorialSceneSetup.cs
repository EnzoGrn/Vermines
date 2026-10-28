using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEditor;
using UnityEngine;

namespace CelticEdge.Tutorial.EditorTools {

    /// <summary>
    /// Creates the tutorial system in the open scene in one click: a TutorialManager, and a tutorial canvas
    /// holding the TutorialUIController and the TutorialSpotlightOverlay (veil), already connected.
    ///
    /// No popup or pointer is created: their look belongs to your game. Make them from your own prefabs
    /// (with TutorialPopupView / TutorialPointerView, or your own components implementing ITutorialPopup / ITutorialPointer)
    /// and drag them into the TutorialUIController. The demo (QuickStartUI) shows a complete example built by code.
    /// </summary>
    public static class TutorialSceneSetup {

        [MenuItem("GameObject/CelticEdge/Tutorial System", false, 10)]
        [MenuItem("Tools/CelticEdge/Tutorial/Create System In Scene")]
        public static void Create()
        {
            var root = new GameObject("TutorialSystem");

            root.SetActive(false); // Awake waits until everything is connected.

            var manager = root.AddComponent<TutorialManager>();

            var canvasGO = new GameObject("TutorialCanvas", typeof(RectTransform));

            canvasGO.transform.SetParent(root.transform, false);

            var canvas = canvasGO.AddComponent<Canvas>();

            canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 500;

            var scaler = canvasGO.AddComponent<CanvasScaler>();

            scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight  = 0.5f;

            canvasGO.AddComponent<GraphicRaycaster>();

            var overlayGO = new GameObject("Overlay", typeof(RectTransform));
            var overlayRT = (RectTransform)overlayGO.transform;

            overlayRT.SetParent(canvasGO.transform, false);

            overlayRT.anchorMin = Vector2.zero;
            overlayRT.anchorMax = Vector2.one;
            overlayRT.offsetMin = overlayRT.offsetMax = Vector2.zero;

            var overlay = overlayGO.AddComponent<TutorialSpotlightOverlay>();
            var ui      = canvasGO.AddComponent<TutorialUIController>();

            ui.Configure(canvas, overlay, null, null, null, null);
            manager.SetUI(ui);

            root.SetActive(true);

            Undo.RegisterCreatedObjectUndo(root, "Create Tutorial System");

            foreach (var c in root.GetComponentsInChildren<MonoBehaviour>(true))
                EditorUtility.SetDirty(c);
            Debug.Log("[Tutorial] System created. Next: add your popup and pointer prefabs to the TutorialUIController, then give the manager a campaign and the game bridge (see the module README).", ui);

#if UNITY_2023_1_OR_NEWER
            bool hasEventSystem = Object.FindFirstObjectByType<EventSystem>() != null;
#else
            bool hasEventSystem = Object.FindObjectOfType<EventSystem>() != null;
#endif
            if (!hasEventSystem)
                Debug.LogWarning("[Tutorial] No EventSystem in the scene: add one (GameObject > UI > Event System) so the buttons respond.");
            Selection.activeGameObject = root;
        }
    }
}
