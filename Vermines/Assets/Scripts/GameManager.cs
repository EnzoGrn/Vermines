using UnityEngine;
using OMGG.DesignPattern;

public class GameManager : MonoBehaviourSingleton<GameManager> {

    private byte[] _ConnectionToken;

    private void Start()
    {
        if (_ConnectionToken == null)
            _ConnectionToken = Utils.ConnectionToken.NewToken();
    }

    public void SetConnectionToken(byte[] token)
    {
        _ConnectionToken = token;
    }

    public byte[] GetConnectionToken()
    {
        return _ConnectionToken;
    }
}
