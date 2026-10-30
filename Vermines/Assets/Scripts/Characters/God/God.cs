using System.Collections.Generic;
using UnityEngine;

namespace Vermines.Characters {

    using Vermines.CardSystem.Data.Effect;

    [CreateAssetMenu(fileName = "New God", menuName = "Vermines/Characters/God")]
    public class God : ScriptableObject {

        public int ID = -1;

        public string Name = "Divinity";

        public Sprite DivinitySprite;

        public List<AEffect> Effects;
    }
}
