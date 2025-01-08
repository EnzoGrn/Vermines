using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Fusion;
using System;

public class SessionInfoListUIItem : MonoBehaviour {

    public TextMeshProUGUI SessionNameText;
    public TextMeshProUGUI PlayerCountText;
    public Button          JoinButton;

    private SessionInfo _SessionInfo;

    public event Action<SessionInfo> OnJoinSession;

    public void SetInformation(SessionInfo sessionInfo)
    {
        _SessionInfo = sessionInfo;

        SessionNameText.text = sessionInfo.Name;
        PlayerCountText.text = $"{sessionInfo.PlayerCount.ToString()} / {sessionInfo.MaxPlayers.ToString()}";

        bool isJoinButtonActive = true;

        if (sessionInfo.PlayerCount >= sessionInfo.MaxPlayers)
            isJoinButtonActive = false;
        JoinButton.gameObject.SetActive(isJoinButtonActive);
    }

    public void OnClick()
    {
        OnJoinSession?.Invoke(_SessionInfo);
    }
}
