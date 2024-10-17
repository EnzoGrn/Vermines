using UnityEngine;

/*
 * @breif This class is used to store the settings of the network.
 * when instantiated it must be created in resources folder in network foler -> "ressources/network/". 
 */
[CreateAssetMenu(fileName = "NetworkSettings", menuName = "Network/Settings")]
public class NetworkSettings : ScriptableObject
{
    /*
     * @brief automaticallySyncScene is used to enable or disable the automatic synchronization of the scene.
     */
    public bool automaticallySyncScene = true;

    /*
     * @brief maxPlayers is used to store the maximum number of players in a room.
     */
    public int maxPlayers = 4;
    /*
     * @brief minPlayers is used to store the minimum number of players in a room.
     */
    public int minPlayers = 2;

    /*
     * @brief minPlayerNicknameLength is used to define the minimum length of the player nickname.
     */
    public int minPlayerNicknameLength = 4;
    /*
     * @brief maxPlayerNicknameLength is used to define the maximum length of the player nickname.
     */
    public int maxPlayerNicknameLength = 8;

    /*
     * @brief maxRoomCodeLength is used to define the maximum length of the room code, roomCode will automatically be generated using the max length.
     */
    public int maxRoomCodeLength = 8;
    /*
     * @brief minRoomCodeLength is used to define the minimum length of the room code.
     */
    public int minRoomCodeLength = 4;
    /*
     * @brief allowCustomRoomCodes is used to enable or disable custom room codes.
     */
    public bool allowCustomRoomCodes = true;
    /*
     * @brief defaultVisibleSetting is used to define the default visibility of the room, if false the room is "private" else "public".
     */
    public bool defaultVisibleSetting = false;

    /*
     * @brief keyStringCodeGeneration is used to store the key string to generate random codes.
     */
    public string keyStringCodeGeneration = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

    /*
     * @brief random is used to generate random numbers if needed.
     */
    public System.Random random = new();

    /*
     * @brief enableDebugLogging is used to enable or disable debug logging.
     */
    public bool enableDebugLogging = true;

    private void OnValidate()
    {
        // Ensure maxPlayers is not lower than minPlayers
        if (maxPlayers < minPlayers)
        {
            Debug.LogWarning("maxPlayers cannot be less than minPlayers! Adjusting maxPlayers to match minPlayers.");
            maxPlayers = minPlayers;
        }

        if (maxRoomCodeLength < minRoomCodeLength)
        {
            Debug.LogWarning("maxRoomCodeLength cannot be less than minRoomCodeLength! Adjusting maxRoomCodeLength to match minRoomCodeLength.");
            maxRoomCodeLength = minRoomCodeLength;
        }

        if (maxPlayerNicknameLength < minPlayerNicknameLength)
        {
            Debug.LogWarning("maxPlayerNicknameLength cannot be less than minPlayerNicknameLength! Adjusting maxPlayerNicknameLength to match minPlayerNicknameLength.");
            maxPlayerNicknameLength = minPlayerNicknameLength;
        }

        if (keyStringCodeGeneration.Length == 0)
        {
            Debug.LogWarning("keyStringCodeGeneration cannot be empty");
            keyStringCodeGeneration = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        }
    }
}
