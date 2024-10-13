using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

namespace Vermines {

    public class Player : MonoBehaviour {

        // Player's PhotonView

        private PhotonView _PhotonView;

        // Player's stats

        public int Eloquence = 0;
        public int Souls = 0;

        // Player's Card

        public Deck HandDeck;
        public Deck Deck;
        public Deck DiscardDeck;
        public Deck SacrifiedDeck;

        // Player's Hand

        private GameObject _Hand;

        // Player's Profile

        private Config.PlayerProfile _Profile;

        private void Awake()
        {
            Init();
        }

        private void Init()
        {
            GameObject playGround = Constants.PlayGround;

            if (playGround == null)
                Debug.Assert(false, "PlayGround not found");
            HandDeck      = new();
            Deck          = new();
            DiscardDeck   = new();
            SacrifiedDeck = new();

            _PhotonView = GetComponent<PhotonView>();
            _Hand       = Constants.InstantiatePlayerView(_PhotonView.IsMine);

            _Hand.transform.SetParent(playGround.transform, false);
        }

        public void SetProfile(Config.PlayerProfile profile)
        {
            _Profile = profile;

            // -- Temporary -- //
            // Line that display the Photon player's nickname
            GameObject playerName = _Hand.transform.Find("Name").gameObject;

            playerName.GetComponent<TMPro.TextMeshProUGUI>().text = profile.Nickname;
            // -- End of temporary -- //
        }
    }
}
