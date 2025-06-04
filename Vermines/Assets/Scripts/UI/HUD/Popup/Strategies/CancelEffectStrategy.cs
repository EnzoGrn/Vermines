using UnityEngine;
using UnityEngine.Localization;
using Vermines.CardSystem.Data.Effect;
using Vermines.CardSystem.Elements;
using Vermines.Player;

public class CancelEffectStrategy : IDiscardPopupStrategy
{
    public string GetTitle() =>
        new LocalizedString("EffectTable", "cancel.title").GetLocalizedString();

    public string GetMessage() => 
        new LocalizedString("EffectTable", "cancel.message").GetLocalizedString();

    public string GetConfirmText() =>
        new LocalizedString("EffectTable", "cancel.confirmText").GetLocalizedString();

    public string GetCancelText() =>
        new LocalizedString("EffectTable", "cancel.cancelText").GetLocalizedString();

    public void OnConfirm()
    {
        UIContextManager.Instance.PopContext();
    }

    public void OnCancel()
    {
    }
}
