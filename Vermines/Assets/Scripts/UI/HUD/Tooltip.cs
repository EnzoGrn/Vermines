using UnityEngine;

public class Tooltip : MonoBehaviour
{
    public GameObject objectToShow;

    private void Start()
    {
        if (objectToShow != null)
        {
            objectToShow.SetActive(false);
        }
    }

    public void Show()
    {
        if (objectToShow != null)
        {
            objectToShow.SetActive(true);
        }
    }

    public void Hide()
    {
        if (objectToShow != null)
        {
            objectToShow.SetActive(false);
        }
    }

}
