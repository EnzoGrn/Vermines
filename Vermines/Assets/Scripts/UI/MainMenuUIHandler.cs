using TMPro;
using UnityEngine;

public class MainMenuUIHandler : MonoBehaviour {

    [Header("Panels")]
    public GameObject StartPanel;
    public GameObject SessionBrowserPanel;
    public GameObject StatusPanel;
    public GameObject CreateSessionPanels;

    [Header("New Game Session")]
    public TMP_InputField SessionNameInputField;

    private void HidePanels()
    {
        StartPanel.SetActive(false);
        SessionBrowserPanel.SetActive(false);
        StatusPanel.SetActive(false);
        CreateSessionPanels.SetActive(false);
    }

    public void OnFindGameClicked()
    {
        NetworkRunnerHandler networkRunnerHandler = FindFirstObjectByType<NetworkRunnerHandler>();

        networkRunnerHandler.OnJoinLobby();

        HidePanels();

        SessionBrowserPanel.SetActive(true);

        FindFirstObjectByType<SessionListUIHandler>(FindObjectsInactive.Include).OnLookingForGameSessions();
    }

    public void OnCreateNewGameClicked()
    {
        HidePanels();

        CreateSessionPanels.SetActive(true);
    }

    public void OnStartNewSessionClicked()
    {
        NetworkRunnerHandler networkRunnerHandler = FindFirstObjectByType<NetworkRunnerHandler>();

        networkRunnerHandler.CreateGame(SessionNameInputField.text, "Game" /* Map */);

        HidePanels();

        StatusPanel.SetActive(true);
    }

    public void OnJoiningServer()
    {
        HidePanels();

        StatusPanel.SetActive(true);
    }
}
