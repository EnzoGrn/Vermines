using UnityEngine;
using Fusion;
using TMPro;
using UnityEngine.UI;

public class SessionListUIHandler : MonoBehaviour {

    public TextMeshProUGUI     StatusText;
    public GameObject          SessionItemListPrefab;
    public VerticalLayoutGroup VerticalLayoutGroup;

    private void Awake()
    {
        ClearList();
    }

    public void ClearList()
    {
        // Delete all children of the vertical layout group
        foreach (Transform child in VerticalLayoutGroup.transform)
            Destroy(child.gameObject);

        // Hide the status message
        StatusText.gameObject.SetActive(false);
    }

    public void AddToList(SessionInfo sessionInfo)
    {
        // Add a new item in the list
        SessionInfoListUIItem addedSessionInfoListUIItem = Instantiate(SessionItemListPrefab, VerticalLayoutGroup.transform).GetComponent<SessionInfoListUIItem>();

        addedSessionInfoListUIItem.SetInformation(sessionInfo);

        // Hook up the event
        addedSessionInfoListUIItem.OnJoinSession += AddedSessionInfoListUIItem_OnJoinSession;
    }

    private void AddedSessionInfoListUIItem_OnJoinSession(SessionInfo sessionInfo)
    {
        NetworkRunnerHandler networkRunnerHandler = FindFirstObjectByType<NetworkRunnerHandler>();

        networkRunnerHandler.JoinGame(sessionInfo.Name);

        MainMenuUIHandler mainMenuUIHandler = FindFirstObjectByType<MainMenuUIHandler>();

        mainMenuUIHandler.OnJoiningServer();
    }

    public void OnNoSessionsFound()
    {
        StatusText.text = "No game session found";

        StatusText.gameObject.SetActive(true);
    }

    public void OnLookingForGameSessions()
    {
        StatusText.text = "Looking for game sessions...";

        StatusText.gameObject.SetActive(true);
    }
}
