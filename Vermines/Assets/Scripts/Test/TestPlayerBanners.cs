using System.Collections.Generic;
using UnityEngine;

namespace Vermines.Test {

    using Vermines.CardSystem.Enumerations;
    using Vermines.CardSystem.Elements;
    using Vermines.CardSystem.Data;
    using Vermines.UI;

    /// <summary>
    /// Playground of the <see cref="PUI_PlayerBanner"/> (local and remote) without network.
    /// A debug panel picks a banner and changes its family, gauges, cards and turn.
    /// </summary>
    public class TestPlayerBanners : MonoBehaviour {

        #region Types

        /// <summary>
        /// Fake state of one banner.
        /// </summary>
        private class BannerState {

            public PUI_PlayerBanner Banner;

            public PUI_MiniDeckDrawer Drawer;

            public string Nickname;

            public int FamilyIndex;

            public int Eloquence;

            public int Souls;

            public readonly List<ICard> Cards = new();
        }

        #endregion

        #region Attributes

        /// <summary>
        /// Banners to drive, in turn order. Every banner of the scene when empty.
        /// </summary>
        [SerializeField]
        private List<PUI_PlayerBanner> _Banners = new();

        /// <summary>
        /// Families a player can pick.
        /// </summary>
        [SerializeField]
        private List<CardFamily> _Families = new() {
            CardFamily.Cricket, CardFamily.Scarab, CardFamily.Ladybug, CardFamily.Fly
        };

        /// <summary>
        /// Nicknames given to the banners.
        /// </summary>
        [SerializeField]
        private List<string> _Nicknames = new() {
            "Phara", "Poutrelaloutre", "Zeno", "Scarabouille"
        };

        /// <summary>
        /// Gives a random deck to the drawer of every banner (disable it when another test feeds the drawers).
        /// </summary>
        [SerializeField]
        private bool _FillDecks = true;

        /// <summary>
        /// Shows the turn buttons.
        /// </summary>
        [SerializeField]
        private bool _ShowTurnButtons = true;

        /// <summary>
        /// Maximum eloquence of the gauge.
        /// </summary>
        [SerializeField]
        private int _MaxEloquence = 20;

        /// <summary>
        /// Souls needed to win.
        /// </summary>
        [SerializeField]
        private int _SoulsLimit = 100;

        /// <summary>
        /// How far the souls can go over the limit (last turn), to test the overflow.
        /// </summary>
        [SerializeField]
        private int _SoulsOverflowMargin = 30;

        private readonly List<BannerState> _States = new();

        private readonly List<ICard> _Partisans = new();

        private readonly List<ICard> _Tools = new();

        private int _Selected;

        private int _Turn = -1;

        private GUIStyle _TitleStyle;

        #endregion

        #region Unity Methods

        private void Start()
        {
            if (_Banners.Count == 0)
                _Banners.AddRange(FindObjectsByType<PUI_PlayerBanner>(FindObjectsInactive.Include, FindObjectsSortMode.InstanceID));
            _Banners.RemoveAll(banner => banner == null);

            if (_Banners.Count == 0) {
                Debug.LogError("[TestPlayerBanners] No PUI_PlayerBanner found in the scene.");

                enabled = false;

                return;
            }
            if (_Families.Count == 0)
                _Families.Add(CardFamily.Cricket);
            if (_FillDecks)
                LoadCards();
            for (int i = 0; i < _Banners.Count; i++) {
                BannerState state = new() {
                    Banner      = _Banners[i],
                    Drawer      = _Banners[i].GetComponentInChildren<PUI_MiniDeckDrawer>(true),
                    Nickname    = _Nicknames.Count > 0 ? _Nicknames[i % _Nicknames.Count] : $"Player {i + 1}",
                    FamilyIndex = i % _Families.Count,
                    Eloquence   = Random.Range(0, _MaxEloquence + 1),
                    Souls       = Random.Range(0, _SoulsLimit / 2)
                };

                if (_FillDecks)
                    FillDeck(state);
                _States.Add(state);

                Apply(state, true);
                state.Banner.SetTurnActive(false, true);
            }
        }

        private void OnGUI()
        {
            if (_States.Count == 0)
                return;
            _TitleStyle ??= new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold, fontSize = 14 };

            BannerState state = _States[_Selected];

            GUILayout.BeginArea(new Rect(Screen.width - 296f, 16f, 280f, _ShowTurnButtons ? 420f + 24f * _States.Count : 300f), GUI.skin.box);

            GUILayout.Label("Bannières des joueurs", _TitleStyle);

            if (GUILayout.Button($"Bannière : {state.Nickname} ({_Selected + 1}/{_States.Count})"))
                _Selected = (_Selected + 1) % _States.Count;
            if (GUILayout.Button($"Famille : {_Families[state.FamilyIndex]} (suivante)")) {
                state.FamilyIndex = (state.FamilyIndex + 1) % _Families.Count;

                Apply(state, false);
            }

            GUILayout.BeginHorizontal();
            GUILayout.Label($"Éloquence {state.Eloquence}", GUILayout.Width(110f));

            if (GUILayout.Button("-1"))
                Change(state, -1, 0);
            if (GUILayout.Button("+1"))
                Change(state, 1, 0);
            if (GUILayout.Button("+5"))
                Change(state, 5, 0);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label($"Âmes {state.Souls}", GUILayout.Width(110f));

            if (GUILayout.Button("-4"))
                Change(state, 0, -4);
            if (GUILayout.Button("+4"))
                Change(state, 0, 4);
            if (GUILayout.Button("+10"))
                Change(state, 0, 10);
            GUILayout.EndHorizontal();

            if (GUILayout.Button("Âmes au max + 4 (débordement)"))
                Change(state, 0, _SoulsLimit + 4 - state.Souls);
            if (GUILayout.Button("Vider les jauges"))
                Change(state, -state.Eloquence, -state.Souls);
            if (_FillDecks) {
                GUILayout.BeginHorizontal();

                if (GUILayout.Button("+ Partisan"))
                    AddCard(state, _Partisans);
                if (GUILayout.Button("+ Objet"))
                    AddCard(state, _Tools);
                if (GUILayout.Button("- Carte"))
                    RemoveCard(state);
                GUILayout.EndHorizontal();
            }

            if (_ShowTurnButtons) {
                GUILayout.Space(6f);
                GUILayout.Label("Tours", _TitleStyle);

                for (int i = 0; i < _States.Count; i++) {
                    if (GUILayout.Button($"Début du tour : {_States[i].Nickname}"))
                        StartTurn(i);
                }
                if (GUILayout.Button("Tour suivant"))
                    StartTurn((_Turn + 1) % _States.Count);
                if (GUILayout.Button("Aucun tour actif"))
                    StartTurn(-1);
            }
            GUILayout.EndArea();
        }

        #endregion

        #region Banner

        private void Apply(BannerState state, bool instant)
        {
            // A networked player overrides the preview.
            if (state.Banner.Player != null)
                return;
            state.Banner.SetFamily(_Families[state.FamilyIndex]);
            state.Banner.SetNickname(state.Nickname);
            state.Banner.SetStatistics(state.Eloquence, _MaxEloquence, state.Souls, _SoulsLimit, instant);
        }

        private void Change(BannerState state, int eloquence, int souls)
        {
            state.Eloquence = Mathf.Clamp(state.Eloquence + eloquence, 0, _MaxEloquence);
            state.Souls     = Mathf.Clamp(state.Souls + souls, 0, _SoulsLimit + _SoulsOverflowMargin);

            state.Banner.SetStatistics(state.Eloquence, _MaxEloquence, state.Souls, _SoulsLimit);
        }

        private void StartTurn(int index)
        {
            _Turn = index;

            for (int i = 0; i < _States.Count; i++)
                _States[i].Banner.SetTurnActive(i == index);
        }

        #endregion

        #region Cards

        private void LoadCards()
        {
            CardSetDatabase.Instance.Initialize(new List<CardFamily>(_Families), null);

            _Partisans.AddRange(CardSetDatabase.Instance.GetEveryCardWith(card => card.Data.Type == CardType.Partisan));
            _Tools.AddRange(CardSetDatabase.Instance.GetEveryCardWith(card => card.Data.Type == CardType.Tools));
        }

        private void FillDeck(BannerState state)
        {
            int partisans = Random.Range(5, 11);
            int tools     = Random.Range(1, 5);

            for (int i = 0; i < partisans; i++)
                AddCard(state, _Partisans, false);
            for (int i = 0; i < tools; i++)
                AddCard(state, _Tools, false);
            RefreshDrawer(state, true);
        }

        private void AddCard(BannerState state, List<ICard> pool, bool refresh = true)
        {
            List<ICard> available = pool.FindAll(card => !IsUsed(card));

            if (available.Count == 0)
                return;
            state.Cards.Add(available[Random.Range(0, available.Count)]);

            if (refresh)
                RefreshDrawer(state, false);
        }

        private void RemoveCard(BannerState state)
        {
            if (state.Cards.Count == 0)
                return;
            state.Cards.RemoveAt(Random.Range(0, state.Cards.Count));

            RefreshDrawer(state, false);
        }

        private void RefreshDrawer(BannerState state, bool instant)
        {
            if (state.Drawer != null && state.Drawer.Player == null)
                state.Drawer.Refresh(state.Cards, instant);
        }

        /// <summary>
        /// A card instance can only be in one deck (the drawer keys its views by card id).
        /// </summary>
        private bool IsUsed(ICard card)
        {
            foreach (BannerState state in _States) {
                if (state.Cards.Contains(card))
                    return true;
            }
            return false;
        }

        #endregion
    }
}
