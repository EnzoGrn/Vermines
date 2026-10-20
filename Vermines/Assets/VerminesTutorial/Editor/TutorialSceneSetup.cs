using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Vermines.Tutorial.EditorTools
{
    /// Crée en un clic tout l'affichage du tutoriel dans la scène ouverte : manager,
    /// canvas, voile, bulle, fenêtre, narrateur et pointeur, avec les références déjà
    /// reliées (voir TutorialUIBuilder). À habiller ensuite avec la DA du jeu.
    public static class TutorialSceneSetup
    {
        [MenuItem("GameObject/Vermines/Système de tutoriel", false, 10)]
        [MenuItem("Tools/Vermines/Tutoriel/Créer le système dans la scène")]
        public static void Create()
        {
            var manager = TutorialUIBuilder.BuildSystem();
            Undo.RegisterCreatedObjectUndo(manager.gameObject, "Créer le système de tutoriel");
            EditorUtility.SetDirty(manager);
            foreach (var c in manager.GetComponentsInChildren<MonoBehaviour>(true))
                EditorUtility.SetDirty(c);

            Debug.Log("[Tutoriel] Système créé. Pour les builds, ajoutez le shader \"" + TutorialSpotlightOverlay.ShaderName +
                      "\" aux Always Included Shaders (Project Settings > Graphics) ou assignez un matériau qui l'utilise au voile.");

#if UNITY_2023_1_OR_NEWER
            bool hasEventSystem = Object.FindFirstObjectByType<EventSystem>() != null;
#else
            bool hasEventSystem = Object.FindObjectOfType<EventSystem>() != null;
#endif
            if (!hasEventSystem)
                Debug.LogWarning("[Tutoriel] Aucun EventSystem dans la scène : ajoutez-en un (GameObject > UI > Event System) pour que les boutons répondent.");

            Selection.activeGameObject = manager.gameObject;
        }
    }
}
