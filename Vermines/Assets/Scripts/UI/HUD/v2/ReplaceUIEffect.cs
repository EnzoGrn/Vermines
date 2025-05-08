using UnityEngine;
using UnityEngine.UI;
using System;
using Vermines.UI.Shop;

public class ReplaceEffectUI : MonoBehaviour
{
    public static ReplaceEffectUI Instance { get; private set; }

    [SerializeField] private GameObject root;
    [SerializeField] private Button shopButton;
    [SerializeField] private Button courtButton;
    [SerializeField] private Button doneButton;

    private Action _onShop;
    private Action _onCourt;
    private Action _onDone;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (root == null)
        {
            Debug.LogError("Root GameObject is not assigned.");
            return;
        }
        root.SetActive(false);

        shopButton.onClick.AddListener(() => _onShop?.Invoke());
        courtButton.onClick.AddListener(() => _onCourt?.Invoke());
        doneButton.onClick.AddListener(() => _onDone?.Invoke());
    }

    public void Show(Action onShopClicked, Action onCourtClicked, Action onDone)
    {
        _onShop = onShopClicked;
        _onCourt = onCourtClicked;
        _onDone = onDone;

        root.SetActive(true);
        ResetUI();
    }

    public void Show()
    {
        root.SetActive(true);
    }

    public void Hide()
    {
        root.SetActive(false);
    }

    public void SetShopDone()
    {
        shopButton.interactable = false;
        ShopUIManager.Instance.CloseCurrentShop();
        Show();
    }

    public void SetCourtDone()
    {
        courtButton.interactable = false;
        ShopUIManager.Instance.CloseCurrentShop();
        Show();
    }

    private void ResetUI()
    {
        shopButton.interactable = true;
        courtButton.interactable = true;
    }
}
