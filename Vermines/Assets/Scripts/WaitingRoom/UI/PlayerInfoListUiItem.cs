using UnityEngine;
using TMPro;
using Fusion;
using Vermines.Player;

public class PlayerInfoListUiItem : MonoBehaviour
{
    [Header("Ui")]
    [SerializeField] private TextMeshProUGUI _PlayerNameText;
    [SerializeField] private TextMeshProUGUI _HostText;
    [SerializeField] private TextMeshProUGUI _YouText;

    public void SetInformation(WaitingRoomPlayerData playerData, bool isMe)
    {
        _PlayerNameText.text = playerData.Nickname;
        _HostText.gameObject.SetActive(playerData.IsHost);
        _YouText.gameObject.SetActive(isMe);
    }
}
