using TMPro;
using UnityEngine;

public class DialogueBubble : MonoBehaviour
{
    [SerializeField] private TMP_Text textField;

    public void SetText(string text)
    {
        textField.text = text;
    }

    public void SetVisible(bool isVisible)
    {
        gameObject.SetActive(isVisible);
    }
}
