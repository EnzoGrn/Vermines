using System.Collections.Generic;
using UnityEngine;

namespace Vermines.Test {

    using Vermines.CardSystem.Enumerations;
    using Vermines.Characters;
    using Vermines.UI;

    /// <summary>
    /// Playground of the <see cref="PUI_GameIntro"/> without network.
    /// A debug panel plays the intro for 2, 3 or 4 fake players with random families and gods.
    /// </summary>
    public class TestGameIntro : MonoBehaviour {

        #region Attributes

        /// <summary>
        /// Intro to play. Spawned from <see cref="PUI_GameIntro.ResourcePath"/> when empty.
        /// </summary>
        [SerializeField]
        private PUI_GameIntro _Intro;

        [SerializeField]
        private PUI_GameIntro _Prefab;

        /// <summary>
        /// Gods drawn by the fake players.
        /// </summary>
        [SerializeField]
        private GodDatabase _Gods;

        /// <summary>
        /// Families a player can pick.
        /// </summary>
        [SerializeField]
        private List<CardFamily> _Families = new() {
            CardFamily.Cricket, CardFamily.Scarab, CardFamily.Ladybug, CardFamily.Fly
        };

        /// <summary>
        /// Nicknames given to the fake players.
        /// </summary>
        [SerializeField]
        private List<string> _Nicknames = new() {
            "Phara", "Poutrelaloutre", "Zeno", "Scarabouille"
        };

        /// <summary>
        /// Number of players of the intro played on start (0 to wait for the panel).
        /// </summary>
        [SerializeField, Range(0, 4)]
        private int _PlayOnStart = 4;

        /// <summary>
        /// Index of the local player in the turn order.
        /// </summary>
        [SerializeField, Range(0, 3)]
        private int _LocalIndex = 0;

        private int _LastCount = 4;

        #endregion

        #region Unity Methods

        private void Start()
        {
            if (_PlayOnStart >= 2)
                Play(_PlayOnStart);
        }

        private void OnGUI()
        {
            GUILayout.BeginArea(new Rect(10f, 10f, 220f, 230f), GUI.skin.box);
            GUILayout.Label("Intro de partie");

            for (int count = 2; count <= 4; count++) {
                if (GUILayout.Button($"{count} joueurs"))
                    Play(count);
            }
            if (GUILayout.Button("Rejouer"))
                Play(_LastCount);
            if (GUILayout.Button("Passer"))
                GetIntro()?.Skip();
            GUILayout.Label(PUI_GameIntro.IsPlaying ? "En cours..." : "Terminée");
            GUILayout.EndArea();
        }

        #endregion

        #region Methods

        private void Play(int count)
        {
            PUI_GameIntro intro = GetIntro();

            if (intro == null) {
                Debug.LogError($"[TestGameIntro] No intro in the scene and no prefabs given.");

                return;
            }

            _LastCount = count;

            intro.Stop();
            StartCoroutine(intro.Play(CreatePlayers(count)));
        }

        private List<GameIntroPlayer> CreatePlayers(int count)
        {
            List<CardFamily> families = new(_Families);
            List<God> gods = new(_Gods != null ? _Gods.GetAllGods() : new God[0]);
            List<GameIntroPlayer> players = new();

            Shuffle(families);
            Shuffle(gods);

            for (int i = 0; i < count; i++) {
                CardFamily family = families.Count > 0 ? families[i % families.Count] : CardFamily.None;
                God god = gods.Count > 0 ? gods[i % gods.Count] : null;
                string nickname = _Nicknames.Count > 0 ? _Nicknames[i % _Nicknames.Count] : $"Player {i + 1}";

                players.Add(new GameIntroPlayer(nickname, family, god, i == _LocalIndex, i == 0));
            }
            return players;
        }

        private PUI_GameIntro GetIntro()
        {
            if (_Intro != null)
                return _Intro;
            _Intro = Instantiate(_Prefab);

            return _Intro;
        }

        private static void Shuffle<T>(List<T> list)
        {
            for (int i = list.Count - 1; i > 0; i--) {
                int k = Random.Range(0, i + 1);

                (list[i], list[k]) = (list[k], list[i]);
            }
        }

        #endregion
    }
}
