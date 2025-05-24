public interface IDiscardPopupStrategy
{
    string GetTitle();
    string GetMessage();
    string GetConfirmText();
    string GetCancelText();
    void OnConfirm();
    void OnCancel();
}
