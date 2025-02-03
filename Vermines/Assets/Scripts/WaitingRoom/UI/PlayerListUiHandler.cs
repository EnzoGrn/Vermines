using Fusion;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Vermines.Player;

public class PlayerListUiHandler : MonoBehaviour
{
    // Maybe use a "NetworkPrefabRef" instead of a GameObject as type
    [SerializeField] private GameObject PlayerItemListPrefab;
    [SerializeField] private GridLayoutGroup GridLayoutGroup;

    private void Awake()
    {
        ClearList();
    }
    
    public void ClearList()
    {
        // Delete all children of the vertical layout group
        foreach (Transform child in GridLayoutGroup.transform)
            Destroy(child.gameObject);
    }

    public void AddToList(WaitingRoomPlayerData playerData, bool isMe)
    {
        // Add a new item in the list
        PlayerInfoListUiItem addedSessionInfoListUIItem = Instantiate(PlayerItemListPrefab, GridLayoutGroup.transform).GetComponent<PlayerInfoListUiItem>();
        // Set player informations
        addedSessionInfoListUIItem.SetInformation(playerData, isMe);
    }

    public void RemoveFromList(PlayerRef player)
    {
        // Remove a player from the list
    }
}
