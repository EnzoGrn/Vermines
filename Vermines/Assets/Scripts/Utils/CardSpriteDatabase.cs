using UnityEngine;

namespace Vermines {

    using Vermines.CardSystem.Enumerations;

    [CreateAssetMenu(fileName = "CardSpriteDatabase", menuName = "Cards/Card Sprite Database")]
    public class CardSpriteDatabase : ScriptableObject {

        [System.Serializable]
        public struct Entry {

            public CardType type;

            public Sprite sprite;
        }

        [SerializeField]
        private Entry[] _Entries;

        private Sprite[] _Cache;

        private void OnEnable()
        {
            BuildCache();
        }

        private void BuildCache()
        {
            _Cache = new Sprite[(int)CardType.Count];

            for (int i = 0; i < _Cache.Length; i++)
                _Cache[i] = _Entries[0].sprite != null ? _Entries[0].sprite : null;
            foreach (var entry in _Entries) {
                if (entry.type < 0 || entry.type >= CardType.Count)
                    continue;
                _Cache[(int)entry.type] = entry.sprite;
            }
        }

        public Sprite Get(CardType type)
        {
            if (_Cache == null || _Cache.Length == 0)
                BuildCache();
            if (type < 0 || type >= CardType.Count)
                return null;
            return _Cache[(int)type];
        }
    }
}
