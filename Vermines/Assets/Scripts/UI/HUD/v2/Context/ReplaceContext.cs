using Vermines.Player;
using Vermines.ShopSystem.Enumerations;
using Vermines.UI.Shop;
using System;
using UnityEngine;
using System.Collections.Generic;

public class ReplaceEffectContext : IUIContext
{
    private readonly Action<Dictionary<ShopType, int>> _onDone;
    public Dictionary<ShopType, int> dictShopSlot;

    public ReplaceEffectContext(Action<Dictionary<ShopType, int>> onDone)
    {
        _onDone = onDone;
    }

    public void Enter()
    {
        ReplaceEffectUI.Instance.Show(
            OnShopClicked,
            OnCourtClicked,
            Done
        );
    }

    public void Exit()
    {
        ReplaceEffectUI.Instance.Hide();
    }

    private void Done()
    {
        UIContextManager.Instance.PopContext();
        _onDone?.Invoke(dictShopSlot);
    }

    private void OnShopClicked()
    {
        ReplaceEffectUI.Instance.Hide();
        ShopUIManager.Instance.OpenShop(ShopType.Market);
        Debug.Log("[ReplaceEffect] Entering shop replace mode.");
        ShopUIManager.Instance.EnterReplaceMode(() =>
        {
            ReplaceEffectUI.Instance.SetShopDone();
        });
    }

    private void OnCourtClicked()
    {
        ReplaceEffectUI.Instance.Hide();
        ShopUIManager.Instance.OpenShop(ShopType.Courtyard);
        Debug.Log("[ReplaceEffect] Entering court replace mode.");
        ShopUIManager.Instance.EnterReplaceMode(() =>
        {
            ReplaceEffectUI.Instance.SetCourtDone();
        });
    }

    public void OnShopCardClicked(ShopType shopType, int slotId)
    {
        Debug.Log($"[ReplaceEffect] Card clicked in shop: {shopType} at slot {slotId}");
        //PlayerController.Local.OnShopReplaceCard(shopType, slotId);
        //ReplaceEffectUI.Instance.SetShopDone();
        if (dictShopSlot == null)
            dictShopSlot = new Dictionary<ShopType, int>();
        if (dictShopSlot.ContainsKey(shopType))
        {
            Debug.Log($"[ReplaceEffect] Already have a replacement stored for {shopType} at slot {dictShopSlot[shopType]}");
            return;
        }
        Debug.Log($"[ReplaceEffect] Replacing {shopType} at slot {slotId}");
        dictShopSlot.Add(shopType, slotId);

        if (shopType == ShopType.Market)
        {
            ReplaceEffectUI.Instance.SetShopDone();
        }
        else if (shopType == ShopType.Courtyard)
        {
            ReplaceEffectUI.Instance.SetCourtDone();
        }
    }
}
