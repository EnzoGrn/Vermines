using UnityEngine;

namespace Vermines.Tutorial
{
    /// Calcule le rectangle à l'écran (pixels, origine en bas à gauche) d'une cible,
    /// qu'elle soit un élément d'UI ou un objet 2D/3D du plateau. Reprend le calcul
    /// par bounds du prototype, sans avoir à préciser UI/World dans les données.
    public static class TutorialTargetLocator
    {
        private static readonly Vector3[] _corners = new Vector3[4];

        public static bool TryGetScreenRect(TutorialTarget target, Camera worldCamera, out Rect rect)
        {
            rect = default;
            if (target == null || !target.IsVisible)
                return false;

            RectTransform rt = target.HighlightRect;
            if (rt != null)
                return TryGetUIRect(rt, out rect);

            Camera cam = worldCamera != null ? worldCamera : Camera.main;
            if (cam == null)
                return false;
            if (!TryGetWorldBounds(target.transform, out Bounds bounds))
                return false;
            return TryProjectBounds(bounds, cam, out rect);
        }

        private static bool TryGetUIRect(RectTransform rt, out Rect rect)
        {
            rect = default;
            var canvas = rt.GetComponentInParent<Canvas>();
            if (canvas == null)
                return false;
            Canvas root = canvas.rootCanvas;
            Camera cam = root.renderMode == RenderMode.ScreenSpaceOverlay ? null : root.worldCamera;

            rt.GetWorldCorners(_corners);
            float minX = float.MaxValue, minY = float.MaxValue, maxX = float.MinValue, maxY = float.MinValue;
            for (int i = 0; i < 4; i++)
            {
                Vector2 sp = RectTransformUtility.WorldToScreenPoint(cam, _corners[i]);
                minX = Mathf.Min(minX, sp.x); minY = Mathf.Min(minY, sp.y);
                maxX = Mathf.Max(maxX, sp.x); maxY = Mathf.Max(maxY, sp.y);
            }
            rect = Rect.MinMaxRect(minX, minY, maxX, maxY);
            return rect.width > 0 && rect.height > 0;
        }

        /// Renderer d'abord, puis Collider, puis l'union des Renderers enfants (une
        /// carte = cadre + illustration + textes).
        private static bool TryGetWorldBounds(Transform target, out Bounds bounds)
        {
            if (target.TryGetComponent<Renderer>(out var renderer) && renderer.enabled)
            {
                bounds = renderer.bounds;
                return true;
            }
            if (target.TryGetComponent<Collider2D>(out var collider2D))
            {
                bounds = collider2D.bounds;
                return true;
            }
            if (target.TryGetComponent<Collider>(out var collider))
            {
                bounds = collider.bounds;
                return true;
            }

            var children = target.GetComponentsInChildren<Renderer>();
            bool found = false;
            bounds = default;
            foreach (var r in children)
            {
                if (!r.enabled)
                    continue;
                if (!found)
                {
                    bounds = r.bounds;
                    found = true;
                }
                else
                    bounds.Encapsulate(r.bounds);
            }
            if (found)
                return true;

            Debug.LogWarning($"[Tutoriel] Aucun Renderer/Collider sur \"{target.name}\" pour calculer la zone éclairée. Ajoutez un Collider (Is Trigger) pour une zone précise.", target);
            bounds = new Bounds(target.position, Vector3.one * 0.5f);
            return true;
        }

        private static bool TryProjectBounds(Bounds b, Camera cam, out Rect rect)
        {
            Vector3 min = b.min, max = b.max;
            float minX = float.MaxValue, minY = float.MaxValue, maxX = float.MinValue, maxY = float.MinValue;
            bool anyInFront = false;
            for (int i = 0; i < 8; i++)
            {
                var corner = new Vector3((i & 1) == 0 ? min.x : max.x, (i & 2) == 0 ? min.y : max.y, (i & 4) == 0 ? min.z : max.z);
                Vector3 sp = cam.WorldToScreenPoint(corner);
                if (sp.z < 0)
                    continue;
                anyInFront = true;
                minX = Mathf.Min(minX, sp.x); minY = Mathf.Min(minY, sp.y);
                maxX = Mathf.Max(maxX, sp.x); maxY = Mathf.Max(maxY, sp.y);
            }
            rect = anyInFront ? Rect.MinMaxRect(minX, minY, maxX, maxY) : default;
            return anyInFront;
        }
    }
}
