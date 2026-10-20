using System.Collections.Generic;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Vermines.Tutorial.Integration {

    using Vermines.CardSystem.Elements;
    using Vermines.Player;
    using Vermines.UI;
    using Vermines.UI.Card;
    using Vermines.UI.Plugin;
    using Vermines.UI.Popup;
    using Vermines.UI.Screen;
    using Vermines.UI.Shop;
    using Vermines.ShopSystem.Enumerations;

    /// <summary>
    /// Puts TutorialTarget components on the game's UI at runtime, so that no prefab has to be edited.
    /// IDs a step can use in its "cibles" column:
    ///   hand:operarius, table:operarius, court:latro, market:epistula_regia,
    ///   discard:operarius, card:operarius (any other card display)
    ///   table_slot:0..n, turn_button, table_button, recycle_button,
    ///   my_banner, my_eloquence, my_souls, opponent_banner, opponent_eloquence, opponent_souls,
    ///   court_button, market_button, back_button, nav:&lt;button name&gt;
    ///   effect_choice:0..n (button of an effect, by its index in the card: the dagger's sacrifice is effect_choice:0),
    ///   popup_confirm, popup_cancel (shop popup and yes/no popups), recycle_validate, recycle_cancel,
    ///   gain_summary, gain_summary_close, table_screen, table_close, discard_slot
    ///   (back_button is also the close button of the shops)
    ///   go:&lt;GameObject name&gt; (any object of the scene, found by name when the step starts)
    /// Card IDs are the card names in lowercase with '_' (see TutorialCardIds).
    /// </summary>
    public class TutorialTargetBinder : MonoBehaviour {

        #region Attributes

        [SerializeField]
        private float _ScanInterval = 0.2f;

        private float _NextScan;

        private static readonly FieldInfo BannerPlayerField    = typeof(PlayerBannerUI).GetField("_player", BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly FieldInfo BannerEloquenceField = typeof(PlayerBannerUI).GetField("eloquenceText", BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly FieldInfo BannerSoulsField     = typeof(PlayerBannerUI).GetField("soulsText", BindingFlags.Instance | BindingFlags.NonPublic);

        private static readonly FieldInfo ChoiceCardField       = typeof(GameplayUIChoiceEffect).GetField("_card", BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly FieldInfo ChoiceButtonsField    = typeof(GameplayUIChoiceEffect).GetField("_spawnedButtons", BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly FieldInfo PopupConfirmField     = typeof(CardPopupBase).GetField("confirmButton", BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly FieldInfo PopupCancelField      = typeof(CardPopupBase).GetField("cancelButton", BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly FieldInfo RecycleValidateField  = typeof(GameplayUIRecycle).GetField("validateButton", BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly FieldInfo RecycleCancelField    = typeof(GameplayUIRecycle).GetField("cancelButton", BindingFlags.Instance | BindingFlags.NonPublic);

        private readonly HashSet<string> _LoggedIds = new();

        public bool LogNewIds { get; set; }

        #endregion

        #region MonoBehaviour

        private void LateUpdate()
        {
            if (!TutorialMatch.IsInitialized || Time.unscaledTime < _NextScan)
                return;
            _NextScan = Time.unscaledTime + _ScanInterval;

            Scan();
        }

        #endregion

        #region Methods

        public void Scan()
        {
            ScanHand();
            ScanCardDisplays();
            ScanTableSlots();
            ScanBanners();
            ScanNavigation();
            ScanScreens();
            ScanEffectChoices();
        }

        /// <summary>
        /// Called when a step starts: "go:Name" targets are looked up by GameObject name.
        /// </summary>
        public void ResolveNamedTargets(IEnumerable<string> targetIds)
        {
            if (targetIds == null)
                return;
            foreach (string id in targetIds) {
                if (string.IsNullOrEmpty(id) || !id.StartsWith("go:") || TutorialTargetRegistry.Get(id) != null)
                    continue;
                string objectName = id.Substring(3);
                GameObject  found = FindByName(objectName);

                if (found != null)
                    Tag(found, id);
                else
                    Debug.LogWarning($"[Tutorial] No object named \"{objectName}\" for target \"{id}\".");
            }
        }

        private static GameObject FindByName(string objectName)
        {
            foreach (Transform t in FindObjectsByType<Transform>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)) {
                if (t.name == objectName)
                    return t.gameObject;
            }

            return null;
        }

        private void ScanHand()
        {
            PlayerController local = PlayerController.Local;
            HandManager       hand = local != null ? local.Context?.HandManager : null;

            if (hand == null || hand.HandCards == null)
                return;
            foreach (GameObject cardObject in hand.HandCards) {
                if (cardObject == null)
                    continue;
                CardDisplay display = cardObject.GetComponentInChildren<CardDisplay>(true);

                if (display != null && display.Card != null)
                    Tag(cardObject, "hand:" + TutorialCardIds.Key(display.Card));
            }
        }

        private void ScanCardDisplays()
        {
            PlayerController local = PlayerController.Local;
            HandManager       hand = local != null ? local.Context?.HandManager : null;

            ShopUIController[] shops = FindObjectsByType<ShopUIController>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);

            foreach (CardDisplay display in FindObjectsByType<CardDisplay>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)) {
                ICard card = display.Card;

                if (card == null)
                    continue;
                if (hand != null && hand.HandCards != null && IsInHand(display.transform, hand.HandCards))
                    continue;
                string key  = TutorialCardIds.Key(card);
                string zone = "card";

                // The shop slots live under ShopUIController.cardSlotRoot, which is not always a child of the plugin.
                ShopUIController shop = display.GetComponentInParent<ShopUIController>(true) ?? FindShopOf(display.transform, shops);

                if (shop != null)
                    zone = shop.ShopType == ShopType.Courtyard ? "court" : shop.ShopType == ShopType.Market ? "market" : "shop";
                else if (display.GetComponentInParent<TableCardSlot>(true) != null)
                    zone = "table";
                else if (display.GetComponentInParent<DiscardCardSlot>(true) != null)
                    zone = "discard";
                Tag(display.gameObject, zone + ":" + key);
            }
        }

        private static ShopUIController FindShopOf(Transform display, ShopUIController[] shops)
        {
            foreach (ShopUIController shop in shops) {
                if (shop != null && shop.cardSlotRoot != null && display.IsChildOf(shop.cardSlotRoot))
                    return shop;
            }

            return null;
        }

        private static bool IsInHand(Transform display, List<GameObject> handCards)
        {
            foreach (GameObject cardObject in handCards) {
                if (cardObject != null && display.IsChildOf(cardObject.transform))
                    return true;
            }

            return false;
        }

        private void ScanTableSlots()
        {
            foreach (TableCardSlot slot in FindObjectsByType<TableCardSlot>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)) {
                // The card display of a slot is tagged "table:<card>": only tag the slot when it is another object.
                if (slot.CardDisplay != null && slot.CardDisplay.gameObject == slot.gameObject)
                    continue;
                Tag(slot.gameObject, "table_slot:" + slot.GetIndex());
            }
        }

        private void ScanBanners()
        {
            PlayerController local = PlayerController.Local;

            if (local == null || BannerPlayerField == null)
                return;
            foreach (PlayerBannerUI banner in FindObjectsByType<PlayerBannerUI>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)) {
                PlayerController player = BannerPlayerField.GetValue(banner) as PlayerController;

                if (player == null)
                    continue;
                string owner = player == local ? "my" : "opponent";

                Tag(banner.gameObject, owner + "_banner");

                if (BannerEloquenceField?.GetValue(banner) is GameObject eloquence && eloquence != null)
                    Tag(eloquence, owner + "_eloquence");
                if (BannerSoulsField?.GetValue(banner) is GameObject souls && souls != null)
                    Tag(souls, owner + "_souls");
            }
        }

        private void ScanNavigation()
        {
            foreach (NavButtonPlugin nav in FindObjectsByType<NavButtonPlugin>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)) {
                foreach (Button button in nav.GetComponentsInChildren<Button>(false)) {
                    string name = TutorialCardIds.Key(button.gameObject.name);
                    string id = name.Contains("court") ? "court_button" : name.Contains("market") ? "market_button" : name.Contains("back") || name.Contains("close") ? "back_button" : "nav:" + name;

                    Tag(button.gameObject, id);
                }
            }
        }

        private void ScanScreens()
        {
            foreach (GameplayUIShop shop in FindObjectsByType<GameplayUIShop>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)) {
                if (shop.TutorialCloseButton != null)
                    Tag(shop.TutorialCloseButton, "back_button");
            }

            foreach (GameplayUIGainSummary summary in FindObjectsByType<GameplayUIGainSummary>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)) {
                Tag(summary.gameObject, "gain_summary");
                TagButton(summary.TutorialCloseButton, "gain_summary_close");
            }

            foreach (GameplayUITable table in FindObjectsByType<GameplayUITable>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)) {
                Tag(table.gameObject, "table_screen");
                TagButton(table.TutorialCloseButton, "table_close");

                if (table.TutorialDiscardSlot != null)
                    Tag(table.TutorialDiscardSlot.gameObject, "discard_slot");
            }

            foreach (GameplayUIDualButtonPopup popup in FindObjectsByType<GameplayUIDualButtonPopup>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)) {
                TagButton(popup.TutorialConfirmButton, "popup_confirm");
                TagButton(popup.TutorialCancelButton, "popup_cancel");
            }

            foreach (CardPopupBase popup in FindObjectsByType<CardPopupBase>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)) {
                TagButton(PopupConfirmField?.GetValue(popup) as Button, "popup_confirm");
                TagButton(PopupCancelField?.GetValue(popup) as Button, "popup_cancel");
            }

            foreach (GameplayUIRecycle recycle in FindObjectsByType<GameplayUIRecycle>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)) {
                TagButton(RecycleValidateField?.GetValue(recycle) as Button, "recycle_validate");
                TagButton(RecycleCancelField?.GetValue(recycle) as Button, "recycle_cancel");
            }
        }

        /// <summary>
        /// The buttons of GameplayUIChoiceEffect only exist for the effects that can be played:
        /// each button is matched to its effect by its text, so "effect_choice:0" is always the first effect of the card.
        /// </summary>
        private void ScanEffectChoices()
        {
            if (ChoiceCardField == null || ChoiceButtonsField == null)
                return;
            foreach (GameplayUIChoiceEffect screen in FindObjectsByType<GameplayUIChoiceEffect>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)) {
                ICard card = ChoiceCardField.GetValue(screen) as ICard;

                if (card?.Data?.Effects == null || ChoiceButtonsField.GetValue(screen) is not List<Button> buttons)
                    continue;
                HashSet<int> used = new();

                foreach (Button button in buttons) {
                    if (button == null)
                        continue;
                    TMP_Text label = button.GetComponentInChildren<TMP_Text>(true);
                    int index = -1;

                    for (int i = 0; i < card.Data.Effects.Count && index < 0; i++) {
                        if (!used.Contains(i) && card.Data.Effects[i] != null && label != null && card.Data.Effects[i].Description == label.text)
                            index = i;
                    }

                    // Same text twice or no text: keep the order of the card.
                    for (int i = 0; i < card.Data.Effects.Count && index < 0; i++) {
                        if (!used.Contains(i))
                            index = i;
                    }

                    if (index < 0)
                        continue;
                    used.Add(index);

                    Tag(button.gameObject, "effect_choice:" + index);
                }
            }
        }

        private void TagButton(Button button, string id)
        {
            if (button != null)
                Tag(VisibleObject(button.gameObject), id);
        }

        /// <summary>
        /// Some buttons have an empty rect and draw their visuals in children (the recycle screen's ValidateButton is 70x0,
        /// its "Button" child is scaled x2 by a layout). The highlight then goes on a helper child that covers
        /// the visible graphics only; the clicks still reach the button through those graphics.
        /// </summary>
        private static GameObject VisibleObject(GameObject target)
        {
            if (target.transform is not RectTransform rect || (rect.rect.width > 1f && rect.rect.height > 1f))
                return target;
            bool    found = false;
            Vector2 min   = Vector2.zero;
            Vector2 max   = Vector2.zero;
            Vector3[] corners = new Vector3[4];

            foreach (Graphic graphic in target.GetComponentsInChildren<Graphic>(false)) {
                if (!graphic.enabled || graphic.color.a <= 0.01f || graphic.rectTransform.rect.width <= 1f || graphic.rectTransform.rect.height <= 1f)
                    continue;
                graphic.rectTransform.GetWorldCorners(corners);

                foreach (Vector3 corner in corners) {
                    Vector2 local = rect.InverseTransformPoint(corner);

                    min   = found ? Vector2.Min(min, local) : local;
                    max   = found ? Vector2.Max(max, local) : local;
                    found = true;
                }
            }

            if (!found)
                return target;
            RectTransform helper = HighlightHelper(rect);

            helper.localPosition = (min + max) * 0.5f;
            helper.sizeDelta     = max - min;

            return helper.gameObject;
        }

        private static RectTransform HighlightHelper(RectTransform parent)
        {
            const string helperName = "TutorialHighlight";

            Transform existing = parent.Find(helperName);

            if (existing is RectTransform existingRect)
                return existingRect;
            GameObject helper = new(helperName, typeof(RectTransform), typeof(LayoutElement));

            helper.GetComponent<LayoutElement>().ignoreLayout = true;

            RectTransform helperRect = helper.GetComponent<RectTransform>();

            helperRect.SetParent(parent, false);
            helperRect.anchorMin = helperRect.anchorMax = helperRect.pivot = new Vector2(0.5f, 0.5f);

            return helperRect;
        }

        private void Tag(GameObject target, string id)
        {
            if (target == null || string.IsNullOrEmpty(id))
                return;
            if (!target.TryGetComponent(out TutorialTarget tutorialTarget))
                tutorialTarget = target.AddComponent<TutorialTarget>();
            if (tutorialTarget.TargetId == id)
                return;
            tutorialTarget.SetTargetId(id);

            if (LogNewIds && _LoggedIds.Add(id))
                Debug.Log($"[Tutorial] Target available: {id} ({target.name})", target);
        }

        #endregion
    }
}
