using System.Linq;
using UnityEngine;

namespace Vermines.Characters {

    [CreateAssetMenu(fileName = "New Cultist Database", menuName = "Vermines/Characters/CultistDatabase")]
    public class CultistDatabase : ScriptableObject {

        [SerializeField]
        private Cultist[] _Cultists = new Cultist[0];

        public Cultist[] GetAllCultists() => _Cultists;

        public Cultist GetCultistByID(int id)
        {
            foreach (var cultist in _Cultists) {
                if (cultist.ID == id)
                    return cultist;
            }

            return null;
        }

        public bool IsValidCultistID(int id)
        {
            return _Cultists.Any(cultist => cultist.ID == id);
        }
    }
}
