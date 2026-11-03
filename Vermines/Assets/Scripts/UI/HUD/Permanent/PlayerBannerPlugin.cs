using UnityEngine.Serialization;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;
using UnityEngine;
using Fusion;

namespace Vermines.UI.Plugin {

    using Vermines.Core.Scene;
    using Vermines.UI.Screen;
    using Vermines.Player;
    using Vermines.Core;

    /// <summary>
    /// Creates the banners of the other players in the gameplay screen, in turn order.
    /// </summary>
    /// <remarks>
    /// The banner of the local player is not created here: put a <see cref="PUI_LocalPlayerBanner"/> in the scene,
    /// it binds itself to <see cref="PlayerController.Local"/>.
    /// Every banner follows its player alone (statistics, family, deck, turn slide); the plugin only creates them,
    /// removes the ones whose player left and opens the player details on click.
    /// </remarks>
    public class PlayerBannerPlugin : GameplayScreenPlugin {

        #region Attributes

        /// <summary>
        /// Banner prefab of the other players (PlayerBanner.prefab).
        /// Can't be null.
        /// </summary>
        [InlineHelp, SerializeField]
        private PUI_PlayerBanner _BannerPrefab;

        /// <summary>
        /// Parent of the banners of the other players (a vertical layout group).
        /// Can't be null.
        /// </summary>
        [InlineHelp, SerializeField, FormerlySerializedAs("bannerParent")]
        private Transform _BannerParent;

        /// <summary>
        /// Banner of the local player, only used to open the player details on click. Found in the scene when empty.
        /// </summary>
        [InlineHelp, SerializeField]
        private PUI_LocalPlayerBanner _LocalBanner;

        /// <summary>
        /// Maximum number of banners of the other players.
        /// </summary>
        [InlineHelp, SerializeField]
        private int _MaxBanners = 3;

        /// <summary>
        /// Opens the player details when a banner is clicked.
        /// </summary>
        [InlineHelp, SerializeField]
        private bool _OpenDetailsOnClick = true;

        /// <summary>
        /// Banners of the other players, in turn order.
        /// </summary>
        public IReadOnlyList<PUI_PlayerBanner> Banners => _Banners;

        private readonly List<PUI_PlayerBanner> _Banners = new();

        private bool _PendingInit;

        private bool _Initialized;

        #endregion

        #region Override Methods

        /// <summary>
        /// Shows the plugin.
        /// </summary>
        /// <param name="screen">The parent screen that this plugin is attached to.</param>
        public override void Show(GameplayUIScreen screen)
        {
            base.Show(screen);

            if (_PendingInit) {
                _PendingInit = false;

                StartCoroutine(InitWhenLocalPlayerReady());
            }
        }

        #endregion

        #region Unity Methods

        private void Awake()
        {
            GameEvents.OnGameInitialized.AddListenerAndReplay(Init);
        }

        private void OnDestroy()
        {
            GameEvents.OnGameInitialized.RemoveListener(Init);
            GameEvents.OnGameInitialized.RemoveListener(Reorder);
        }

        private void LateUpdate()
        {
            if (!_Initialized)
                return;
            // A player who left the game: his banner goes away.
            for (int i = _Banners.Count - 1; i >= 0; i--) {
                PUI_PlayerBanner banner = _Banners[i];

                if (banner == null) {
                    _Banners.RemoveAt(i);

                    continue;
                }

                if (banner.Player != null && banner.Player.Object != null && banner.Player.Object.IsValid)
                    continue;
                _Banners.RemoveAt(i);

                Destroy(banner.gameObject);
            }
        }

        #endregion

        #region Init

        /// <summary>
        /// Creates the banners once the game is initialized.
        /// </summary>
        public void Init()
        {
            GameEvents.OnGameInitialized.RemoveListener(Init);

            if (gameObject.activeInHierarchy)
                StartCoroutine(InitWhenLocalPlayerReady());
            else
                _PendingInit = true;
        }

        private IEnumerator InitWhenLocalPlayerReady()
        {
            float timeout = 10f;
            float elapsed = 0f;

            while (PlayerController.Local == null && elapsed < timeout) {
                yield return null;

                elapsed += Time.unscaledDeltaTime;
            }
            if (PlayerController.Local == null) {
                Debug.LogError("[PlayerBannerPlugin] PlayerController.Local never became ready within timeout - player banners will not be created.");

                yield break;
            }
            CreateBanners();
            HookLocalBanner();

            // The turn order can be set after the game initialization: sort again when it is known.
            GameEvents.OnGameInitialized.AddListenerAndReplay(Reorder);

            _Initialized = true;
        }

        private void CreateBanners()
        {
            foreach (PUI_PlayerBanner banner in _Banners) {
                if (banner != null)
                    Destroy(banner.gameObject);
            }
            _Banners.Clear();

            if (_BannerPrefab == null || _BannerParent == null) {
                Debug.LogError("[PlayerBannerPlugin] The banner prefab or the banner parent is missing.", this);

                return;
            }
            SceneContext context = PlayerController.Local.Context;
            List<PlayerController> players = context.Runner.GetAllBehaviours<PlayerController>();

            SortByTurnOrder(players, context.GameplayMode, player => player.Object.InputAuthority);

            foreach (PlayerController player in players) {
                if (player == PlayerController.Local)
                    continue;
                if (_Banners.Count >= _MaxBanners) {
                    Debug.LogWarning("[PlayerBannerPlugin] Maximum number of banners reached.", this);

                    break;
                }
                CreateBanner(player);
            }
        }

        /// <summary>
        /// Creates the banner of <paramref name="player"/> at the end of the layout.
        /// </summary>
        public PUI_PlayerBanner CreateBanner(PlayerController player)
        {
            PUI_PlayerBanner banner = Instantiate(_BannerPrefab, _BannerParent);

            banner.name = $"PlayerBanner ({player.NetworkedNickname.Value})";
            banner.Bind(player);

            AddClick(banner);

            _Banners.Add(banner);

            return banner;
        }

        private void HookLocalBanner()
        {
            if (_LocalBanner == null)
                _LocalBanner = FindFirstObjectByType<PUI_LocalPlayerBanner>(FindObjectsInactive.Include);
            if (_LocalBanner != null)
                AddClick(_LocalBanner);
        }

        private void Reorder()
        {
            GameEvents.OnGameInitialized.RemoveListener(Reorder);

            if (PlayerController.Local == null || PlayerController.Local.Context == null)
                return;
            _Banners.RemoveAll(banner => banner == null || banner.Player == null);

            SortByTurnOrder(_Banners, PlayerController.Local.Context.GameplayMode, banner => banner.Player.Object.InputAuthority);

            for (int i = 0; i < _Banners.Count; i++) {
                _Banners[i].transform.SetSiblingIndex(i);
                _Banners[i].SyncTurn(true);
            }
        }

        #endregion

        #region Helpers

        private void AddClick(PUI_PlayerBanner banner)
        {
            if (!_OpenDetailsOnClick)
                return;
            if (!banner.TryGetComponent(out Button button)) {
                button = banner.gameObject.AddComponent<Button>();

                button.transition = Selectable.Transition.None;
            }
            button.onClick.AddListener(() => {
                if (banner.Player != null && _ParentScreen != null && _ParentScreen.Controller != null)
                    _ParentScreen.Controller.ShowWithParams<GameplayUIPlayerDetails, PlayerController>(banner.Player);
            });
        }

        private static void SortByTurnOrder<T>(List<T> items, GameplayMode mode, System.Func<T, PlayerRef> getRef)
        {
            if (mode == null)
                return;
            items.Sort((a, b) => IndexInTurnOrder(mode, getRef(a)).CompareTo(IndexInTurnOrder(mode, getRef(b))));
        }

        private static int IndexInTurnOrder(GameplayMode mode, PlayerRef player)
        {
            for (int i = 0; i < mode.PlayerTurnOrder.Length; i++) {
                if (mode.PlayerTurnOrder.Get(i) == player)
                    return i;
            }
            return int.MaxValue;
        }

        #endregion
    }
}
