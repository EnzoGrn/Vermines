using System.Linq;
using UnityEngine;

namespace Vermines.Characters {

    [CreateAssetMenu(fileName = "New God Database", menuName = "Vermines/Characters/GodDatabase")]
    public class GodDatabase : ScriptableObject {

        [SerializeField]
        private God[] _Gods = new God[0];

        public God[] GetAllGods() => _Gods;

        public God GetGodByID(int id)
        {
            foreach (var god in _Gods) {
                if (god.ID == id)
                    return god;
            }

            return null;
        }

        public bool IsValidGodID(int id)
        {
            return _Gods.Any(god => god.ID == id);
        }
    }
}
