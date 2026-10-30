using UnityEngine.Serialization;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;

namespace Vermines.Test {

    using Vermines.CardSystem.Enumerations;
    using Vermines.CardSystem.Elements;
    using Vermines.CardSystem.Data;
    using Vermines.UI;

    /// <summary>
    /// Playground of the <see cref="PUI_MiniDeckDrawer"/> without network.
    /// A debug panel adds and removes cards in a local list, with every exit animation, and can run an automatic sequence.
    /// </summary>
    public class TestMiniDeckDrawer : MonoBehaviour {

        #region Attributes

        /// <summary>
        /// The drawer to feed.
        /// </summary>
        [SerializeField, FormerlySerializedAs("drawer")]
        private PUI_MiniDeckDrawer _Drawer;

        /// <summary>
        /// Delay between two changes of the automatic sequence.
        /// </summary>
        [SerializeField]
        private float _Delay = 1.2f;

        /// <summary>
        /// Number of partisans of the starting deck.
        /// </summary>
        [SerializeField]
        private int _StartingPartisans = 6;

        /// <summary>
        /// Number of tools of the starting deck.
        /// </summary>
        [SerializeField]
        private int _StartingTools = 2;

        /// <summary>
        /// Families used to load the cards.
        /// </summary>
        [SerializeField]
        private List<CardFamily> _Families = new() { CardFamily.Ladybug, CardFamily.Scarab };

        private readonly List<ICard> _Cards = new();

        private readonly List<ICard> _Partisans = new();

        private readonly List<ICard> _Tools = new();

        private Coroutine _AutoRoutine;

        private PUI_CardRemoveReason _NextReason;

        private GUIStyle _TitleStyle;

        #endregion

        #region Unity Methods

        private void Start()
        {
            if (_Drawer == null)
                _Drawer = FindFirstObjectByType<PUI_MiniDeckDrawer>();
            if (_Drawer == null) {
                Debug.LogError("[TestMiniDeckDrawer] No PUI_MiniDeckDrawer found in the scene.");

                enabled = false;

                return;
            }
            CardSetDatabase.Instance.Initialize(_Families, null);

            _Partisans.AddRange(CardSetDatabase.Instance.GetEveryCardWith(card => card.Data.Type == CardType.Partisan));
            _Tools.AddRange(CardSetDatabase.Instance.GetEveryCardWith(card => card.Data.Type == CardType.Tools));

            Debug.Log($"[TestMiniDeckDrawer] {_Partisans.Count} partisans and {_Tools.Count} tools loaded.");

            ResetDeck();
        }

        private void OnGUI()
        {
            _TitleStyle ??= new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold, fontSize = 14 };

            GUILayout.BeginArea(new Rect(16f, 16f, 260f, 520f), GUI.skin.box);

            GUILayout.Label("Mini deck drawer", _TitleStyle);
            GUILayout.Label($"Partisans : {CountOf(CardType.Partisan)}   Objets : {CountOf(CardType.Tools)}");
            GUILayout.Label($"Restants : {_Partisans.Count - CountOf(CardType.Partisan)} partisans, {_Tools.Count - CountOf(CardType.Tools)} objets");

            GUILayout.Space(6f);
            GUILayout.Label("Ajouter", _TitleStyle);

            if (GUILayout.Button("+ Partisan (achat)"))
                AddRandom(_Partisans);
            if (GUILayout.Button("+ Objet (achat)"))
                AddRandom(_Tools);
            if (GUILayout.Button("+ 3 cartes d'un coup")) {
                AddRandom(_Partisans, false);
                AddRandom(_Tools, false);
                AddRandom(_Partisans);
            }

            GUILayout.Space(6f);
            GUILayout.Label("Retirer une carte au hasard", _TitleStyle);

            if (GUILayout.Button("- Posée sur la table"))
                RemoveRandom(PUI_CardRemoveReason.ToTable);
            if (GUILayout.Button("- Recyclée"))
                RemoveRandom(PUI_CardRemoveReason.Recycled);
            if (GUILayout.Button("- Sacrifiée"))
                RemoveRandom(PUI_CardRemoveReason.Sacrificed);

            GUILayout.Space(6f);
            GUILayout.Label("Divers", _TitleStyle);

            if (GUILayout.Button("Remélanger (aucune animation attendue)"))
                Shuffle();
            if (GUILayout.Button(_AutoRoutine == null ? "Lancer la séquence auto" : "Arrêter la séquence auto"))
                ToggleAuto();
            if (GUILayout.Button("Deck de départ (sans animation)"))
                ResetDeck();
            if (GUILayout.Button("Vider"))
                Clear();
            GUILayout.EndArea();
        }

        #endregion

        #region Actions

        private void ResetDeck()
        {
            StopAuto();

            _Cards.Clear();

            for (int i = 0; i < _StartingPartisans && i < _Partisans.Count; i++)
                _Cards.Add(_Partisans[i]);
            for (int i = 0; i < _StartingTools && i < _Tools.Count; i++)
                _Cards.Add(_Tools[i]);
            _Drawer.Refresh(_Cards, true);
        }

        private void Clear()
        {
            StopAuto();

            _NextReason = PUI_CardRemoveReason.Recycled;

            _Cards.Clear();
            _Drawer.Refresh(_Cards, ResolveReason);
        }

        private void AddRandom(List<ICard> pool, bool refresh = true)
        {
            List<ICard> available = pool.FindAll(card => !_Cards.Contains(card));

            if (available.Count == 0) {
                Debug.LogWarning("[TestMiniDeckDrawer] No more card of this type to add.");

                return;
            }
            _Cards.Add(available[Random.Range(0, available.Count)]);

            if (refresh)
                _Drawer.Refresh(_Cards);
        }

        private void RemoveRandom(PUI_CardRemoveReason reason)
        {
            if (_Cards.Count == 0)
                return;
            _NextReason = reason;

            _Cards.RemoveAt(Random.Range(0, _Cards.Count));
            _Drawer.Refresh(_Cards, ResolveReason);
        }

        private void Shuffle()
        {
            for (int i = _Cards.Count - 1; i > 0; i--) {
                int j = Random.Range(0, i + 1);

                (_Cards[i], _Cards[j]) = (_Cards[j], _Cards[i]);
            }
            _Drawer.Refresh(_Cards);
        }

        private void ToggleAuto()
        {
            if (_AutoRoutine != null) {
                StopAuto();

                return;
            }
            _AutoRoutine = StartCoroutine(AutoRoutine());
        }

        private void StopAuto()
        {
            if (_AutoRoutine == null)
                return;
            StopCoroutine(_AutoRoutine);

            _AutoRoutine = null;
        }

        private IEnumerator AutoRoutine()
        {
            PUI_CardRemoveReason[] reasons = { PUI_CardRemoveReason.ToTable, PUI_CardRemoveReason.Recycled, PUI_CardRemoveReason.Sacrificed };

            while (true) {
                for (int i = 0; i < 3; i++) {
                    AddRandom(Random.value < 0.65f ? _Partisans : _Tools);

                    yield return new WaitForSeconds(_Delay);
                }
                for (int i = 0; i < 2; i++) {
                    RemoveRandom(reasons[Random.Range(0, reasons.Length)]);

                    yield return new WaitForSeconds(_Delay);
                }
            }
        }

        #endregion

        #region Helpers

        private PUI_CardRemoveReason ResolveReason(ICard card)
        {
            return _NextReason;
        }

        private int CountOf(CardType type)
        {
            return _Cards.FindAll(card => card.Data.Type == type).Count;
        }

        #endregion
    }
}
