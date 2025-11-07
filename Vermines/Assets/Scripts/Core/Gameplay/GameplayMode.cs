using UnityEngine;
using Fusion;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Vermines.Core {

    using Vermines.CardSystem.Enumerations;
    using Vermines.Player;

    public abstract class GameplayMode : ContextBehaviour {

        public GameplayType Type = GameplayType.Standart;

        public void Activate() {}

        public void PlayerLeft(PlayerController player) {}

        #region Methods

        public override void Spawned()
        {
            Context.GameplayMode = this;
        }

        public void Initialize(string data)
        {
            Dictionary<int, CardFamily>                 temp = JsonConvert.DeserializeObject<Dictionary<int, CardFamily>>(data);
            Dictionary<PlayerRef, CardFamily> playerFamilies = new();

            foreach (var kvp in temp)
                playerFamilies[PlayerRef.FromEncoded(kvp.Key)] = kvp.Value;
            foreach (var kvp in playerFamilies)
                Debug.LogError($"{kvp.Key} chose the {kvp.Value}.");
        }

        #endregion
    }
}