using UnityEngine;

namespace Vermines.Characters {

    using Vermines.CardSystem.Enumerations;

    [CreateAssetMenu(fileName = "New Cultist", menuName = "Vermines/Characters/Cultist")]
    public class Cultist : ScriptableObject {

        public int ID = -1;

        public CardFamily family = CardFamily.None;

        public string Name = "Cultist";

        public Sprite CultistSprite;
    }
}
