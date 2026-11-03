using System.Collections.Generic;
using UnityEngine.Localization;
using System.Collections;
using UnityEngine.UI;
using UnityEngine;
using DG.Tweening;
using Fusion;

namespace Vermines.UI {

    using Vermines.Core.Player;
    using Vermines.Player;
    using Vermines.Core;

    using Text = TMPro.TMP_Text;

    /// <summary>
    /// Intro played before the first turn, dark and full screen: the title strikes the screen among the flames,
    /// one column per player (2 to 4) rises with the cultist and the nickname, the gods are revealed one after
    /// the other with their effects, then the first player is crowned.
    /// </summary>
    public class PUI_GameIntro : MonoBehaviour {

        #region Constants

        /// <summary>
        /// String table of the intro texts (French and English).
        /// </summary>
        public const string TableName = "GameIntro";

        /// <summary>
        /// Key of the title.
        /// </summary>
        public const string TitleKey = "intro.title";

        /// <summary>
        /// Key of the subtitle for <c>N</c> players is this prefix followed by <c>N</c>.
        /// </summary>
        public const string SubtitleKeyPrefix = "intro.subtitle.";

        #endregion

        #region Attributes

        /// <summary>
        /// Whole intro, faded in and out.
        /// </summary>
        [Header("References")]
        [SerializeField]
        private CanvasGroup _Root;

        /// <summary>
        /// Black layer above everything: the intro rises from the dark and goes back to it.
        /// </summary>
        [SerializeField]
        private Image _Fader;

        /// <summary>
        /// Background art, slowly zoomed during the intro.
        /// </summary>
        [SerializeField]
        private RectTransform _Background;

        /// <summary>
        /// Content shaken by the impacts (title, columns, versus).
        /// </summary>
        [SerializeField]
        private RectTransform _Content;

        /// <summary>
        /// Flames along the bottom of the screen, they rise at the start.
        /// </summary>
        [SerializeField]
        private RectTransform _Flames;

        /// <summary>
        /// Title, ornament and subtitle: struck at the center, then moved to the top.
        /// </summary>
        [SerializeField]
        private RectTransform _Header;

        /// <summary>
        /// Title of the intro.
        /// </summary>
        [SerializeField]
        private Text _Title;

        /// <summary>
        /// Ornament under the title, unrolled after the impact.
        /// </summary>
        [SerializeField]
        private RectTransform _Ornament;

        /// <summary>
        /// Subtitle, depends on the number of players.
        /// </summary>
        [SerializeField]
        private Text _Subtitle;

        /// <summary>
        /// Parent of the player columns, its center is the center of the row.
        /// </summary>
        [SerializeField]
        private RectTransform _CardsRoot;

        /// <summary>
        /// Column instantiated for each player.
        /// </summary>
        [SerializeField]
        private PUI_GameIntroCard _CardPrefab;

        /// <summary>
        /// Mark struck between the two columns of a two players game.
        /// </summary>
        [SerializeField]
        private RectTransform _Versus;

        /// <summary>
        /// Glow pulsing behind <see cref="_Versus"/>.
        /// </summary>
        [SerializeField]
        private Image _VersusGlow;

        /// <summary>
        /// Gap between two columns, in canvas units.
        /// </summary>
        [Header("Layout")]
        [SerializeField]
        private float _CardSpacing = 30f;

        /// <summary>
        /// Gap between the two columns of a two players game (room for <see cref="_Versus"/>).
        /// </summary>
        [SerializeField]
        private float _DuelSpacing = 320f;

        /// <summary>
        /// Scale of the columns for 2, 3 and 4 players.
        /// </summary>
        [SerializeField]
        private float[] _CardScaleByCount = { 1f, 1f, 0.95f };

        /// <summary>
        /// Height lost by the outer columns so the row draws an arc.
        /// </summary>
        [SerializeField]
        private float _ArcHeight = 18f;

        /// <summary>
        /// Rotation of the outer columns, in degrees.
        /// </summary>
        [SerializeField]
        private float _ArcAngle = 2.5f;

        /// <summary>
        /// Vertical offset of the row from the center of <see cref="_CardsRoot"/>.
        /// </summary>
        [SerializeField]
        private float _RowOffsetY = 0f;

        /// <summary>
        /// Distance the columns travel when they rise.
        /// </summary>
        [SerializeField]
        private float _EnterDistance = 1200f;

        /// <summary>
        /// Scale reached by the header while it vanishes, before the columns rise.
        /// </summary>
        [SerializeField]
        private float _HeaderExitScale = 1.25f;

        /// <summary>
        /// Duration of the fade from black.
        /// </summary>
        [Header("Timings (seconds)")]
        [SerializeField]
        private float _FadeInDuration = 0.8f;

        /// <summary>
        /// Longest wait, behind the black fader, for the frame rate to settle before the intro starts
        /// (the first frames upload the art and would make the tweens jump, hiding the title).
        /// </summary>
        [SerializeField]
        private float _WarmUpTimeout = 2f;

        /// <summary>
        /// Number of smooth frames in a row needed to start the intro.
        /// </summary>
        [SerializeField]
        private int _WarmUpFrames = 6;

        /// <summary>
        /// Longest frame (seconds) still considered smooth during the warm up.
        /// </summary>
        [SerializeField]
        private float _WarmUpMaxFrameTime = 0.05f;

        /// <summary>
        /// Duration of the title strike.
        /// </summary>
        [SerializeField]
        private float _TitleStrikeDuration = 0.35f;

        /// <summary>
        /// Time the title stays at the center.
        /// </summary>
        [SerializeField]
        private float _TitleHold = 1.6f;

        /// <summary>
        /// Duration of the vanishing of the header.
        /// </summary>
        [SerializeField]
        private float _HeaderExitDuration = 0.6f;

        /// <summary>
        /// Duration of the rise of one column.
        /// </summary>
        [SerializeField]
        private float _CardEnterDuration = 0.55f;

        /// <summary>
        /// Delay between the rises of two columns.
        /// </summary>
        [SerializeField]
        private float _CardStagger = 0.35f;

        /// <summary>
        /// Pause between the last rise (or the versus strike) and the first god reveal.
        /// </summary>
        [SerializeField]
        private float _GodFlipDelay = 0.4f;

        /// <summary>
        /// Duration of one god reveal.
        /// </summary>
        [SerializeField]
        private float _GodFlipDuration = 0.6f;

        /// <summary>
        /// Delay between two god reveals.
        /// </summary>
        [SerializeField]
        private float _GodFlipStagger = 0.45f;

        /// <summary>
        /// Time left to read the god effects.
        /// </summary>
        [SerializeField]
        private float _ReadTime = 2f;

        /// <summary>
        /// Reading time added for each player.
        /// </summary>
        [SerializeField]
        private float _ReadTimePerPlayer = 0.7f;

        /// <summary>
        /// Duration of the first player reveal.
        /// </summary>
        [SerializeField]
        private float _FirstPlayerDuration = 0.6f;

        /// <summary>
        /// Time the first player stays crowned before the intro leaves.
        /// </summary>
        [SerializeField]
        private float _FirstPlayerHold = 1.6f;

        /// <summary>
        /// Duration of the fade to black at the end.
        /// </summary>
        [SerializeField]
        private float _FadeToBlackDuration = 0.6f;

        /// <summary>
        /// Duration of the fade from black to the game.
        /// </summary>
        [SerializeField]
        private float _FadeOutDuration = 0.5f;

        /// <summary>
        /// Alpha of the columns of the players who do not start.
        /// </summary>
        [SerializeField]
        private float _DimAlpha = 0.45f;

        /// <summary>
        /// Strength of the shake of the big impacts (title, versus).
        /// </summary>
        [Header("Impacts")]
        [SerializeField]
        private float _StrongShake = 22f;

        /// <summary>
        /// Strength of the shake when a column lands.
        /// </summary>
        [SerializeField]
        private float _LightShake = 8f;

        /// <summary>
        /// Title of the intro.
        /// </summary>
        [Header("Texts")]
        [SerializeField]
        private string _TitleText = "Que le culte commence";

        /// <summary>
        /// Subtitle for 2, 3 and 4 players.
        /// </summary>
        [SerializeField]
        private string[] _SubtitleByCount = {
            "Deux cultes s'affrontent",
            "Trois cultes s'affrontent",
            "Quatre cultes s'affrontent"
        };

        /// <summary>
        /// True while an intro is playing (whatever its instance).
        /// </summary>
        public static bool IsPlaying { get; private set; }

        /// <summary>
        /// Number of game intros over (played or skipped) since the start of the application.
        /// Lets a waiting system (the tutorial) know when the intro of the current match is over.
        /// </summary>
        public static int FinishedCount { get; private set; }

        /// <summary>
        /// Marks the intro of the current match as over, whether it was played or not.
        /// </summary>
        public static void MarkFinished()
        {
            FinishedCount++;
        }

        /// <summary>
        /// True while this intro is playing.
        /// </summary>
        public bool IsRunning => _IsRunning;

        private readonly List<PUI_GameIntroCard> _Cards = new();

        private Sequence _Sequence;

        private Tween _VersusPulse;

        private bool _IsRunning;

        private Vector2 _HeaderOrigin;

        private Vector2 _FlamesOrigin;

        private bool _HasHeaderOrigin;

        #endregion

        #region Unity Methods

        private void Awake()
        {
            CacheHeaderOrigin();

            if (_Root != null) {
                _Root.alpha          = 0f;
                _Root.blocksRaycasts = false;
            }
        }

        private void OnDestroy()
        {
            if (_IsRunning)
                IsPlaying = false;
            _Sequence?.Kill();
            _VersusPulse?.Kill();
        }

        #endregion

        #region Play

        /// <summary>
        /// Plays the intro for <paramref name="players"/> (in turn order), the coroutine ends with the intro.
        /// </summary>
        public IEnumerator Play(IReadOnlyList<GameIntroPlayer> players)
        {
            Stop();

            if (players == null || players.Count == 0)
                yield break;
            _IsRunning = true;
            IsPlaying  = true;

            gameObject.SetActive(true);

            if (_Root != null) {
                _Root.alpha          = 1f;
                _Root.blocksRaycasts = true;
            }
            if (_Fader != null)
                _Fader.color = WithAlpha(_Fader.color, 1f);
            BuildCards(players);

            // Behind the black fader, wait for the hitches of the first frames to pass.
            float timeout = Time.unscaledTime + _WarmUpTimeout;
            int smooth = 0;

            while (smooth < _WarmUpFrames && Time.unscaledTime < timeout && _IsRunning) {
                yield return null;

                smooth = Time.unscaledDeltaTime <= _WarmUpMaxFrameTime ? smooth + 1 : 0;
            }
            if (!_IsRunning)
                yield break;
            _Sequence = BuildSequence();

            while (_IsRunning)
                yield return null;
        }

        /// <summary>
        /// Leaves the intro right away with a short fade (debug only, the player cannot skip the intro).
        /// </summary>
        public void Skip()
        {
            if (!_IsRunning)
                return;
            _Sequence?.Kill();
            _Sequence = DOTween.Sequence().SetUpdate(true).SetLink(gameObject);

            if (_Root != null)
                _Sequence.Append(_Root.DOFade(0f, 0.2f));
            _Sequence.OnComplete(Finish);
        }

        /// <summary>
        /// Stops the intro without animation.
        /// </summary>
        public void Stop()
        {
            _Sequence?.Kill();
            _Sequence = null;

            if (_Root != null)
                _Root.alpha = 0f;
            if (_IsRunning)
                Finish();
        }

        /// <summary>
        /// Duration of the whole intro for <paramref name="playerCount"/> players.
        /// </summary>
        public float GetDuration(int playerCount)
        {
            int count = Mathf.Max(1, playerCount);

            return GetOutroTime(count) + _FadeToBlackDuration + _FadeOutDuration;
        }

        private void Finish()
        {
            _IsRunning = false;
            IsPlaying  = false;

            _VersusPulse?.Kill();

            if (_Root != null)
                _Root.blocksRaycasts = false;
        }

        #endregion

        #region Game

        /// <summary>
        /// Gathers the players of the game in turn order: nickname, family, god, local and first player.
        /// </summary>
        public static List<GameIntroPlayer> CollectPlayers(GameplayMode mode, NetworkGame game, PlayerRef localPlayer)
        {
            List<GameIntroPlayer> players = new();

            if (mode == null || game == null)
                return players;
            int count = Mathf.Min(mode.TurnPlayerCount, mode.PlayerTurnOrder.Length);

            for (int i = 0; i < count; i++) {
                PlayerRef playerRef = mode.PlayerTurnOrder.Get(i);

                if (playerRef == PlayerRef.None)
                    continue;
                PlayerController player = game.GetPlayer(playerRef);

                if (player == null || player.Object == null || !player.Object.IsValid)
                    continue;
                PlayerStatistics stats = player.Statistics;

                players.Add(new GameIntroPlayer(
                    player.NetworkedNickname.Value,
                    stats.Family,
                    player.God,
                    playerRef == localPlayer,
                    i == mode.CurrentPlayerIndex
                ));
            }
            return players;
        }

        /// <summary>
        /// Plays the intro of the current game, called before the first turn is announced.
        /// </summary>
        /// <param name="waitFullDuration">
        /// Keeps waiting until the intro would have ended even if it was cut (the host, so the first turn starts at the same moment for everyone).
        /// </param>
        public static IEnumerator PlayForGame(GameplayMode mode, NetworkGame game, PlayerRef localPlayer, PUI_GameIntro prefab, bool waitFullDuration)
        {
            if (mode == null || game == null)
                yield break;
            if (prefab == null) {
                Debug.LogWarning("[GameIntro] No intro prefab assigned.");

                yield break;
            }

            // The god RPCs are sent just before the first turn: give them a moment to arrive.
            float                 timeout = Time.unscaledTime + 3f;
            List<GameIntroPlayer> players = CollectPlayers(mode, game, localPlayer);

            while (!IsComplete(players, mode) && Time.unscaledTime < timeout) {
                yield return null;

                players = CollectPlayers(mode, game, localPlayer);
            }

            if (players.Count < 2) {
                Debug.LogWarning($"[GameIntro] Skipped, only {players.Count} player(s) found.");

                yield break;
            }

            PUI_GameIntro intro = Instantiate(prefab);
            float           end = Time.unscaledTime + intro.GetDuration(players.Count);

            yield return intro.Play(players);

            if (intro != null)
                Destroy(intro.gameObject);
            while (waitFullDuration && Time.unscaledTime < end)
                yield return null;
        }

        private static bool IsComplete(List<GameIntroPlayer> players, GameplayMode mode)
        {
            if (players.Count < Mathf.Min(mode.TurnPlayerCount, mode.PlayerTurnOrder.Length))
                return false;
            foreach (GameIntroPlayer player in players) {
                if (player.God == null || string.IsNullOrEmpty(player.Nickname))
                    return false;
            }
            return true;
        }

        #endregion

        #region Build

        private void BuildCards(IReadOnlyList<GameIntroPlayer> players)
        {
            foreach (PUI_GameIntroCard card in _Cards) {
                if (card != null)
                    Destroy(card.gameObject);
            }
            _Cards.Clear();

            if (_CardPrefab == null || _CardsRoot == null) {
                Debug.LogError("[GameIntro] The column prefab or the cards root is missing.");

                return;
            }
            foreach (GameIntroPlayer player in players) {
                PUI_GameIntroCard card = Instantiate(_CardPrefab, _CardsRoot);

                card.name = $"Column_{player.Nickname}";
                card.Setup(player);

                _Cards.Add(card);
            }
        }

        private Sequence BuildSequence()
        {
            int count = _Cards.Count;
            Sequence sequence = DOTween.Sequence().SetUpdate(true).SetLink(gameObject);

            PrepareScene(count);

            // From the dark: the background appears, slowly zoomed, the flames rise.
            if (_Fader != null)
                sequence.Insert(0f, _Fader.DOFade(0f, _FadeInDuration).SetEase(Ease.InQuad));
            if (_Background != null)
                sequence.Insert(0f, _Background.DOScale(1f, GetDuration(count)).SetEase(Ease.OutSine));
            if (_Flames != null) {
                _Flames.anchoredPosition = _FlamesOrigin + Vector2.down * 260f;

                sequence.Insert(0.2f, _Flames.DOAnchorPos(_FlamesOrigin, 1.4f).SetEase(Ease.OutCubic));
            }

            // The title strikes the screen.
            float strike = _FadeInDuration * 0.6f;

            if (_Title != null) {
                sequence.Insert(strike, FadeText(_Title, 1f, _TitleStrikeDuration * 0.5f));
                sequence.Insert(strike, _Title.rectTransform.DOScale(1f, _TitleStrikeDuration).SetEase(Ease.InExpo));
            }
            float impact = strike + _TitleStrikeDuration;

            sequence.Insert(impact, Shake(_StrongShake, 0.45f));

            if (_Ornament != null)
                sequence.Insert(impact, _Ornament.DOScaleX(1f, 0.5f).SetEase(Ease.OutCubic));
            if (_Subtitle != null)
                sequence.Insert(impact + 0.3f, FadeText(_Subtitle, 1f, 0.5f));

            // The whole header vanishes into the dark to leave the stage to the columns.
            float headerExit = impact + _TitleHold;

            if (_Header != null)
                sequence.Insert(headerExit, _Header.DOScale(_HeaderExitScale, _HeaderExitDuration).SetEase(Ease.InCubic));
            if (_Title != null)
                sequence.Insert(headerExit, FadeText(_Title, 0f, _HeaderExitDuration).SetEase(Ease.InCubic));
            if (_Subtitle != null)
                sequence.Insert(headerExit, FadeText(_Subtitle, 0f, _HeaderExitDuration * 0.6f));
            if (_Ornament != null && _Ornament.TryGetComponent(out Graphic ornament))
                sequence.Insert(headerExit, ornament.DOFade(0f, _HeaderExitDuration * 0.8f));

            // Columns: each one rises heavily from below and lands with a jolt.
            float cardsStart = GetCardsStartTime();

            for (int i = 0; i < count; i++) {
                RectTransform rect = (RectTransform)_Cards[i].transform;
                CanvasGroup group  = _Cards[i].GetComponent<CanvasGroup>();

                GetSlot(i, count, out Vector2 position, out float angle, out float scale);

                rect.anchoredPosition = position + Vector2.down * _EnterDistance;
                rect.localEulerAngles = new Vector3(0f, 0f, angle);
                rect.localScale       = Vector3.one * scale;

                if (group != null)
                    group.alpha = 0f;
                float time = cardsStart + i * _CardStagger;

                sequence.Insert(time, rect.DOAnchorPos(position, _CardEnterDuration).SetEase(Ease.OutBack, 0.9f));

                if (group != null)
                    sequence.Insert(time, group.DOFade(1f, _CardEnterDuration * 0.4f));
                sequence.Insert(time + _CardEnterDuration * 0.7f, Shake(_LightShake, 0.25f));
            }

            // Two players: the versus mark strikes between the columns.
            float flipStart = GetFlipStartTime(count);

            if (_Versus != null) {
                _Versus.gameObject.SetActive(count == 2);

                if (count == 2) {
                    float versusTime = flipStart - _GodFlipDelay - 0.3f;
                    CanvasGroup versusGroup = _Versus.GetComponent<CanvasGroup>();

                    _Versus.localScale = Vector3.one * 4f;

                    if (versusGroup != null) {
                        versusGroup.alpha = 0f;

                        sequence.Insert(versusTime, versusGroup.DOFade(1f, 0.15f));
                    }
                    sequence.Insert(versusTime, _Versus.DOScale(1f, 0.3f).SetEase(Ease.InExpo));
                    sequence.Insert(versusTime + 0.3f, Shake(_StrongShake, 0.4f));
                    sequence.InsertCallback(versusTime + 0.3f, StartVersusPulse);
                }
            }

            // Gods: revealed one after the other with their effects.
            for (int i = 0; i < count; i++)
                sequence.Insert(flipStart + i * _GodFlipStagger, _Cards[i].FlipGod(_GodFlipDuration));

            // First player: crowned, the others step back.
            float firstTime = GetFirstPlayerTime(count);

            for (int i = 0; i < count; i++) {
                if (_Cards[i].Player.IsFirst)
                    sequence.Insert(firstTime, _Cards[i].RevealFirst(_FirstPlayerDuration));
                else
                    sequence.Insert(firstTime, _Cards[i].Dim(_DimAlpha, _FirstPlayerDuration));
            }

            // Back to the dark, then to the game.
            float outroTime = GetOutroTime(count);

            if (_Fader != null)
                sequence.Insert(outroTime, _Fader.DOFade(1f, _FadeToBlackDuration).SetEase(Ease.InQuad));
            if (_Root != null)
                sequence.Insert(outroTime + _FadeToBlackDuration, _Root.DOFade(0f, _FadeOutDuration).SetEase(Ease.OutQuad));
            sequence.OnComplete(Finish);

            return sequence;
        }

        private void PrepareScene(int count)
        {
            CacheHeaderOrigin();

            if (_Root != null)
                _Root.alpha = 1f;
            if (_Fader != null)
                _Fader.color = WithAlpha(_Fader.color, 1f);
            if (_Background != null)
                _Background.localScale = Vector3.one * 1.12f;
            if (_Content != null)
                _Content.anchoredPosition = Vector2.zero;
            if (_Header != null) {
                _Header.anchoredPosition = _HeaderOrigin;
                _Header.localScale       = Vector3.one;
            }
            if (_Title != null) {
                _Title.text  = Localize(TitleKey, _TitleText);
                _Title.alpha = 0f;
                _Title.rectTransform.localScale = Vector3.one * 3f;
            }
            if (_Ornament != null) {
                _Ornament.localScale = new Vector3(0f, 1f, 1f);

                if (_Ornament.TryGetComponent(out Graphic ornament))
                    ornament.color = WithAlpha(ornament.color, 1f);
            }
            if (_Subtitle != null) {
                int index = Mathf.Clamp(count - 2, 0, _SubtitleByCount.Length - 1);
                string fallback = _SubtitleByCount.Length > 0 ? _SubtitleByCount[index] : string.Empty;

                _Subtitle.text  = Localize(SubtitleKeyPrefix + Mathf.Clamp(count, 2, 4), fallback);
                _Subtitle.alpha = 0f;
            }
            if (_Versus != null)
                _Versus.gameObject.SetActive(false);
            if (_VersusGlow != null)
                _VersusGlow.color = WithAlpha(_VersusGlow.color, 0f);
        }

        private void StartVersusPulse()
        {
            if (_VersusGlow == null)
                return;
            _VersusPulse?.Kill();
            _VersusGlow.color = WithAlpha(_VersusGlow.color, 0.9f);
            _VersusPulse = _VersusGlow.DOFade(0.45f, 0.9f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine).SetUpdate(true).SetLink(gameObject);
        }

        private Tween Shake(float strength, float duration)
        {
            if (_Content == null)
                return DOTween.Sequence();
            return _Content.DOShakeAnchorPos(duration, strength, 24, 90f, false, true);
        }

        private void CacheHeaderOrigin()
        {
            if (_HasHeaderOrigin || _Header == null)
                return;
            _HasHeaderOrigin = true;
            _HeaderOrigin    = _Header.anchoredPosition;
            _FlamesOrigin    = _Flames != null ? _Flames.anchoredPosition : Vector2.zero;
        }

        /// <summary>
        /// Text of <paramref name="key"/> in the <see cref="TableName"/> table for the current language,
        /// or <paramref name="fallback"/> when the table or the entry is missing.
        /// </summary>
        public static string Localize(string key, string fallback)
        {
            try {
                string value = new LocalizedString(TableName, key).GetLocalizedString();

                if (string.IsNullOrEmpty(value) || value.StartsWith("No translation found"))
                    return fallback;
                return value;
            } catch (System.Exception exception) {
                Debug.LogWarning($"[PUI_GameIntro] Cannot localize '{key}': {exception.Message}");

                return fallback;
            }
        }

        private static Tween FadeText(Text text, float alpha, float duration)
        {
            return DOTween.To(() => text.alpha, value => text.alpha = value, alpha, duration);
        }

        private static Color WithAlpha(Color color, float alpha)
        {
            color.a = alpha;

            return color;
        }

        #endregion

        #region Layout

        /// <summary>
        /// Place of the column <paramref name="index"/> in a row of <paramref name="count"/> columns.
        /// </summary>
        private void GetSlot(int index, int count, out Vector2 position, out float angle, out float scale)
        {
            scale = GetCardScale(count);

            float width   = GetCardWidth() * scale;
            float spacing = count == 2 ? _DuelSpacing : _CardSpacing;
            float total   = count * width + (count - 1) * spacing;
            float x       = -total * 0.5f + width * 0.5f + index * (width + spacing);

            // -1 for the leftmost column, 1 for the rightmost one.
            float side = count > 1 ? (index / (float)(count - 1)) * 2f - 1f : 0f;

            position = new Vector2(x, _RowOffsetY - _ArcHeight * side * side);
            angle    = count > 2 ? -_ArcAngle * side : 0f;
        }

        private float GetCardScale(int count)
        {
            if (_CardScaleByCount == null || _CardScaleByCount.Length == 0)
                return 1f;
            return _CardScaleByCount[Mathf.Clamp(count - 2, 0, _CardScaleByCount.Length - 1)];
        }

        private float GetCardWidth()
        {
            if (_CardPrefab == null)
                return 440f;
            return ((RectTransform)_CardPrefab.transform).rect.width;
        }

        private float GetCardsStartTime()
        {
            return _FadeInDuration * 0.6f + _TitleStrikeDuration + _TitleHold + _HeaderExitDuration * 0.6f;
        }

        private float GetFlipStartTime(int count)
        {
            float landed = GetCardsStartTime() + (count - 1) * _CardStagger + _CardEnterDuration;

            // Two players: room for the versus strike.
            if (count == 2)
                landed += 0.6f;
            return landed + _GodFlipDelay;
        }

        private float GetFirstPlayerTime(int count)
        {
            float flipsEnd = GetFlipStartTime(count) + (count - 1) * _GodFlipStagger + _GodFlipDuration + 0.4f;

            return flipsEnd + _ReadTime + _ReadTimePerPlayer * count;
        }

        private float GetOutroTime(int count)
        {
            return GetFirstPlayerTime(count) + _FirstPlayerDuration + _FirstPlayerHold;
        }

        #endregion
    }
}
