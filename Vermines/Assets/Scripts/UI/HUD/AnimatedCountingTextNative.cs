using TMPro;
using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class AnimatedCountingTextNative : MonoBehaviour
{
    public TextMeshProUGUI text;
    public float totalDuration = 0.5f;
    public Color changeColor = Color.yellow;
    public float sizeFactor = 1.2f;
    public float returnDuration = 0.2f;

    private int currentValue = 0;
    private Coroutine countingCoroutine;
    private Color originalColor;
    private Vector2 originalSize;
    private RectTransform rectTransform;

    void Awake()
    {
        if (text == null) text = GetComponent<TextMeshProUGUI>();
        rectTransform = GetComponent<RectTransform>();
        originalColor = text.color;
        originalSize = rectTransform.sizeDelta;
        text.text = currentValue.ToString();
    }

    public void SetValue(int newValue)
    {
        if (newValue == currentValue) return;

        if (countingCoroutine != null)
            StopCoroutine(countingCoroutine);

        countingCoroutine = StartCoroutine(CountTo(newValue));
    }

    private IEnumerator CountTo(int targetValue)
    {
        int start = currentValue;
        int direction = targetValue > start ? 1 : -1;
        int steps = Mathf.Abs(targetValue - start);
        float stepTime = totalDuration / Mathf.Max(steps, 1);

        float t = 0f;
        Vector2 targetSize = originalSize * sizeFactor;

        while (t < returnDuration / 2f)
        {
            t += Time.deltaTime;
            float progress = t / (returnDuration / 2f);
            rectTransform.sizeDelta = Vector2.Lerp(originalSize, targetSize, progress);
            text.color = Color.Lerp(originalColor, changeColor, progress);
            LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
            yield return null;
        }

        for (int i = 1; i <= steps; i++)
        {
            currentValue += direction;
            text.text = currentValue.ToString();
            text.ForceMeshUpdate();
            LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
            yield return new WaitForSeconds(stepTime);
        }

        currentValue = targetValue;
        text.text = targetValue.ToString();
        text.ForceMeshUpdate();

        t = 0f;
        while (t < returnDuration)
        {
            t += Time.deltaTime;
            float progress = t / returnDuration;
            rectTransform.sizeDelta = Vector2.Lerp(targetSize, originalSize, progress);
            text.color = Color.Lerp(changeColor, originalColor, progress);
            LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
            yield return null;
        }

        rectTransform.sizeDelta = originalSize;
        text.color = originalColor;
        LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
    }
}
