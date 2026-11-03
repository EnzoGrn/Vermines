using System.Linq;
using UnityEngine;

namespace Vermines.Characters {

    using Vermines.CardSystem.Enumerations;

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

        /// <summary>
        /// Cultist of <paramref name="family"/>, or null when the database has none.
        /// </summary>
        public Cultist GetCultistByFamily(CardFamily family)
        {
            foreach (Cultist cultist in _Cultists) {
                if (cultist != null && cultist.family == family)
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
