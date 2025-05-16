using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
public class RadialLayout : LayoutGroup
{
    public float fDistance;
    [Range(0f, 360f)]
    public float totalAngle = 180f;
    public float startAngle = 0f;

    public RectTransform parentOverride;
    public enum LayoutOrigin { Center, Left, Right }
    public LayoutOrigin layoutOrigin = LayoutOrigin.Center;

    public List<RectTransform> items = new List<RectTransform>();

    protected override void OnEnable()
    {
        base.OnEnable();
        if (fDistance == 0f)
            ResetDistance();
        CalculateRadial();
    }

    public override void SetLayoutHorizontal() { }
    public override void SetLayoutVertical() { }

    public override void CalculateLayoutInputVertical() => CalculateRadial();
    public override void CalculateLayoutInputHorizontal() => CalculateRadial();

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();

        if (!Application.isPlaying)
        {
            EditorApplication.delayCall += () =>
            {
                if (this != null)
                    CalculateRadial();
            };
        }
    }
#endif

    [ContextMenu("Reset Distance")]
    public void ResetDistance()
    {
        RectTransform reference = parentOverride != null ? parentOverride : rectTransform;
        float radius = Mathf.Min(reference.rect.width, reference.rect.height);
        fDistance = radius;
    }

    void CalculateRadial()
    {
        m_Tracker.Clear();
        if (items == null || items.Count == 0)
            return;

        RectTransform centerReference = parentOverride != null ? parentOverride : rectTransform;

        float angleOffset = items.Count > 1 ? totalAngle / (items.Count - 1) : 0f;
        float baseAngle = GetStartAngle();

        Vector3 centerWorldPos = centerReference.TransformPoint(centerReference.rect.center);

        for (int i = 0; i < items.Count; i++)
        {
            RectTransform child = items[i];
            if (child == null) continue;

            m_Tracker.Add(this, child,
                DrivenTransformProperties.Anchors |
                DrivenTransformProperties.AnchoredPosition |
                DrivenTransformProperties.Pivot);

            float angle = baseAngle + i * angleOffset;

            Vector3 localOffset = new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad), 0f) * fDistance;
            Vector3 worldTargetPos = centerWorldPos + localOffset;

            Vector3 localTargetPos = rectTransform.InverseTransformPoint(worldTargetPos);
            child.localPosition = localTargetPos;

            child.anchorMin = child.anchorMax = child.pivot = new Vector2(0.5f, 0.5f);
        }
    }


    float GetStartAngle()
    {
        switch (layoutOrigin)
        {
            case LayoutOrigin.Center:
                return startAngle - totalAngle / 2f;
            case LayoutOrigin.Left:
                return startAngle;
            case LayoutOrigin.Right:
                return startAngle - totalAngle;
            default:
                return startAngle;
        }
    }
}
