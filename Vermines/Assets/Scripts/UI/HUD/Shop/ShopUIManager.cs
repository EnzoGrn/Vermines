using System;
using System.Collections.Generic;
using UnityEngine;
using Vermines.ShopSystem.Enumerations;

namespace Vermines.UI.Shop
{
    [Serializable]
    public class ShopUIConfigEntry
    {
        public ShopType shopType;
        public ShopUIConfig config;
    }

    public class ShopUIManager : MonoBehaviour
    {
        public static ShopUIManager Instance;

        [Header("Shop Prefab")]
        public GameObject shopUIPrefab;

        [Header("UI Parent")]
        public Transform uiRoot;

        [Header("Shop Configs")]
        public List<ShopUIConfigEntry> shopConfigEntries;

        private Dictionary<ShopType, ShopUIConfig> shopConfigs = new();
        private ShopUIController activeShop;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }

            Instance = this;
            InitConfigs();
        }

        private void InitConfigs()
        {
            shopConfigs.Clear();
            foreach (var entry in shopConfigEntries)
            {
                if (entry.config == null)
                {
                    Debug.LogWarning($"[ShopUIManager] Config is null for {entry.shopType}, skipped.");
                    continue;
                }

                if (!shopConfigs.ContainsKey(entry.shopType))
                {
                    shopConfigs.Add(entry.shopType, entry.config);
                }
                else
                {
                    Debug.LogWarning($"[ShopUIManager] Duplicate entry for {entry.shopType}, ignored.");
                }
            }
        }

        public void OpenShop(ShopType type)
        {
            if (activeShop != null)
            {
                Destroy(activeShop.gameObject);
                activeShop = null;
            }

            if (!shopConfigs.TryGetValue(type, out var config))
            {
                Debug.LogError($"[ShopUIManager] No config found for shop type: {type}");
                return;
            }

            GameObject go = Instantiate(shopUIPrefab, uiRoot);
            activeShop = go.GetComponent<ShopUIController>();
            activeShop.config = config;

            if (ShopManager.Instance != null)
            {
                var entries = ShopManager.Instance.GetEntries(type);
                activeShop.Init(entries);
            }
        }

        public void CloseCurrentShop()
        {
            if (activeShop != null)
            {
                activeShop.SetDialogueVisible(false);
                Destroy(activeShop.gameObject);
                activeShop = null;
            }
        }

        public void EnterReplaceMode(Action onCardReplaced)
        {
            if (activeShop != null)
            {
                activeShop.EnterReplaceMode(onCardReplaced);
                OpenShop(activeShop.config.shopType);
            }
        }

        public ShopUIController GetActiveShop() => activeShop;
    }
}
