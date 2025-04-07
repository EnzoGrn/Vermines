using UnityEngine;

public class UIBootstrapper : MonoBehaviour
{
    [SerializeField] private GameObject shopConfirmPopupPrefab;
    [SerializeField] private Transform popupContainer;

    private void Awake()
    {
        ShopConfirmPopupFactory.Init(shopConfirmPopupPrefab, popupContainer);
    }
}
