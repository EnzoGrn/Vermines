public interface IDiscardPopupStrategy
{
    string GetTitle();
    string GetMessage();
    void OnConfirm();
    void OnCancel();
}
