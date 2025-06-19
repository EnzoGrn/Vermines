using UnityEngine;
using Vermines.Player;
using Vermines.UI.Utils;
using System.Collections;
using System.Collections.Generic;
using Vermines.UI.Screen;

namespace Vermines.UI.Plugin
{
    using static Vermines.UI.Screen.GameplayUIBook;
    using Text = TMPro.TMP_Text;
    using Button = UnityEngine.UI.Button;
    using Image = UnityEngine.UI.Image;

    /// <summary>
    /// Manages the display of the phase banner in the gameplay screen.
    /// </summary>
    public class PlayerBookTabPlugin : GameplayScreenPlugin
    {
        [SerializeField] private BookTabType _PageType;
        public BookTabType PageType => _PageType;

        [Header("Player List UI")]
        [SerializeField] private Transform playerTabContainer;
        [SerializeField] private GameObject playerTabPrefab;
        private readonly List<PlayerBookTab> _cachedTabs = new();

        public System.Action<PlayerData> OnTabClicked;

        private void Awake()
        {
            OnTabClicked += SetPlayerTabActive;
        }

        /// <summary>
        /// Show the plugin.
        /// </summary>
        /// <param name="screen">
        /// The parent screen that this plugin is attached to.
        /// </param>
        public override void Show(GameplayUIScreen screen)
        {
            base.Show(screen);
        }

        /// <summary>
        /// Hide the plugin.
        /// </summary>
        public override void Hide()
        {
            StartCoroutine(HidePlayerTabsSequentially());
            base.Hide();
        }

        public IEnumerator ShowPlayerTabsSequentially()
        {
            var players = GameDataStorage.Instance.PlayerData;
            int index = 0;

            foreach (var player in players)
            {
                PlayerBookTab tab;

                if (index < _cachedTabs.Count)
                {
                    tab = _cachedTabs[index];
                    tab.gameObject.SetActive(true);
                }
                else
                {
                    GameObject tabObj = Instantiate(playerTabPrefab, playerTabContainer);
                    tab = tabObj.GetComponentInChildren<PlayerBookTab>();
                    tab.UpdateTab(player.Value);
                    _cachedTabs.Add(tab);

                    Button button = tabObj.GetComponentInChildren<Button>();
                    button.onClick.AddListener(() => OnTabClicked?.Invoke(player.Value));
                }

                yield return tab.PlayShowAnimCoroutine();
                index++;
            }
            for (int i = index; i < _cachedTabs.Count; i++)
            {
                _cachedTabs[i].gameObject.SetActive(false);
            }
        }

        public IEnumerator HidePlayerTabsSequentially()
        {
            for (int i = _cachedTabs.Count - 1; i >= 0; i--)
            {
                var tab = _cachedTabs[i];
                if (tab.gameObject.activeSelf)
                {
                    yield return tab.PlayHideAnimation();
                }
            }
        }

        public void SetPlayerTabActive(PlayerData playerData)
        {
            GameplayUIBook bookScreen = _ParentScreen as GameplayUIBook;
            if (bookScreen != null)
            {
                bookScreen.SwitchToPage(_PageType);
            }
            else
            {
                Debug.LogError("Parent screen is not a GameplayUIBook.");
            }
            foreach (var tab in _cachedTabs)
            {
                tab.PlayActiveAnimation(tab.PlayerRef == playerData.PlayerRef);
            }
        }

        public void SetPlayerTabActive(bool isActive)
        {
            foreach (var tab in _cachedTabs)
            {
                tab.PlayActiveAnimation(isActive);
            }
        }
    }
}