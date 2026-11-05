using Fusion;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;
using Vermines.CardSystem.Data.Effect;
using Vermines.CardSystem.Elements;
using Vermines.CardSystem.Enumerations;
using Vermines.Core.Scene;
using Vermines.Gameplay.Cards.Effect;
using Vermines.Gameplay.Phases;
using Vermines.Gameplay.Phases.Enumerations;
using Vermines.Player;
using Vermines.UI.Card;
using Vermines.UI.GameTable;
using Vermines.UI.Popup;

namespace Vermines.UI.Screen
{
    using Text = TMPro.TMP_Text;

    public partial class GameplayUITable : GameplayUIScreen
    {
        #region Attributes

        [Header("Navigation Buttons")]

        /// <summary>
        /// Optional close button (the table can be left with the navigation bar).
        /// </summary>
        [InlineHelp, SerializeField]
        protected UnityEngine.UI.Button _CloseButton;

        [Header("Zone Containers")]

        [SerializeField] protected Transform partisanSlotsContainer;

        [SerializeField] protected GameTableCardSlotPool _Pool;
        protected List<TableCardSlot> partisanSlots = new();
        [SerializeField] protected CardSlotBase discardSlot;

        /// <summary>
        /// Play mat of the local player (family texture, turn lock, blood). Optional.
        /// </summary>
        [SerializeField] protected PUI_PlayMat _PlayMat;

        /// <summary>
        /// Size of a partisan slot on the table, in pixels. The slot keeps the card proportions and fits in this size.
        /// </summary>
        [SerializeField] protected Vector2 _SlotSize = new Vector2(200f, 275f);

        [Header("Texts")]
        [InlineHelp, SerializeField]
        protected Text tableText;

        private bool _SacrificeMode;

        #endregion

        partial void AwakeUser();
        partial void InitUser();
        partial void ShowUser();
        partial void HideUser();

        #region Override Methods

        public override void Awake()
        {
            base.Awake();

            AwakeUser();

            // Subscribed before the checks below: a missing optional reference must never cut the table off its events
            // (a sacrificed partisan stayed on the table when Awake returned early).
            GameEvents.OnCardSacrificed.AddListener(OnCardSacrificed);
            GameEvents.OnCardReborned.AddListener(OnCardReborned);
            EffectPromptState.Changed += OnPromptStateChanged;

            if (_PlayMat != null)
                _PlayMat.LockChanged += OnTableLockChanged;

            #region Error Handling

            if (discardSlot == null)
            {
                Debug.LogErrorFormat(
                    gameObject,
                    "GameplayUITable Critical Error: Missing 'CardSlotBase' reference on GameObject '{0}'. This component is required to render the discard zone. Please assign a valid DiscardCardSlot in the Inspector.",
                    gameObject.name
                );
                return;
            }

            if (partisanSlotsContainer == null)
            {
                Debug.LogErrorFormat(
                    gameObject,
                    "GameplayUITable Critical Error: Missing 'Transform' reference on GameObject '{0}'. This component is required to render the partisan slots. Please assign a valid Transform in the Inspector.",
                    gameObject.name
                );
                return;
            }

            if (_Pool == null)
            {
                Debug.LogErrorFormat(
                    gameObject,
                    "GameplayUITable Critical Error: Missing 'GameTableCardSlotPool' reference on GameObject '{0}'. This component is required to render the card slots. Please assign a valid GameTableCardSlotPool in the Inspector.",
                    gameObject.name
                );
                return;
            }

            #endregion
        }

        public override void Init()
        {
            base.Init();

            InitUser();

            ClearAllSlots();

            SetupDiscardZone();

            GameEvents.OnPlayerUpdated.AddListener(OnLocalPlayerStatsChanged);

            GameEvents.OnPhaseChanged.AddListener(OnPhaseChanged);
            GameEvents.OnDiscardShuffled.AddListener(ClearDiscard);
        }

        public override void Show()
        {
            base.Show();

            ShowUser();

            GameEvents.OnCardClicked.AddListener(OnCardClicked);

            // Catch up with a prompt that was raised before this screen was shown.
            OnPromptStateChanged();
        }

        public override void Hide()
        {
            base.Hide();

            HideUser();

            GameEvents.OnCardClicked.RemoveListener(OnCardClicked);

            // The sacrifice splashes disappear when the table closes.
            if (_PlayMat != null)
                _PlayMat.OnTableClosed();
        }

        private void OnDestroy()
        {
            GameEvents.OnCardSacrificed.RemoveListener(OnCardSacrificed);
            GameEvents.OnCardReborned.RemoveListener(OnCardReborned);
            GameEvents.OnPlayerUpdated.RemoveListener(OnLocalPlayerStatsChanged);
            GameEvents.OnPhaseChanged.RemoveListener(OnPhaseChanged);
            GameEvents.OnDiscardShuffled.RemoveListener(ClearDiscard);
            GameEvents.OnCardClicked.RemoveListener(OnCardClicked);
            EffectPromptState.Changed -= OnPromptStateChanged;

            if (_PlayMat != null)
                _PlayMat.LockChanged -= OnTableLockChanged;
        }

        #endregion

        #region Methods

        private void OnLocalPlayerStatsChanged(PlayerController player)
        {
            if (!IsLocalPlayer(player))
                return;

            OnNumberOfPartisanSlotsChanged(player.Statistics.NumberOfSlotInTable);
        }

        private void OnNumberOfPartisanSlotsChanged(int newCount)
        {
            if (newCount < 0)
                return;
            int currentCount = partisanSlots.Count;

            if (currentCount == newCount)
                return;
            if (newCount > currentCount)
            {
                for (int i = currentCount; i < newCount; i++)
                {
                    var slot = CreateSlot(i, partisanSlotsContainer, CardType.Partisan);

                    partisanSlots.Add(slot);
                }
            }
            else
            {
                for (int i = currentCount - 1; i >= newCount; i--)
                {
                    var slot = partisanSlots[i];

                    slot.ResetSlot();

                    _Pool.ReturnSlot(slot);

                    partisanSlots.RemoveAt(i);
                }
            }

            for (int i = 0; i < partisanSlots.Count; i++)
                partisanSlots[i].SetIndex(i);
        }

        private TableCardSlot CreateSlot(int index, Transform parent, CardType acceptedType)
        {
            var slot = _Pool.GetSlot(parent);
            slot.Setup(acceptedType);
            slot.SetIndex(index);
            slot.ResetSlot();
            slot.transform.localScale = Vector3.one * GetSlotScale(slot);
            return slot;
        }

        private float GetSlotScale(TableCardSlot slot)
        {
            Rect rect = ((RectTransform)slot.transform).rect;

            if (rect.width <= 0f || rect.height <= 0f)
                return 1.6f;
            return Mathf.Min(_SlotSize.x / rect.width, _SlotSize.y / rect.height);
        }

        private void SetupDiscardZone()
        {
            discardSlot.SetIndex(-1);
        }

        public void ClearAllSlots()
        {
            foreach (var slot in partisanSlots)
                _Pool.ReturnSlot(slot);
            partisanSlots.Clear();

            discardSlot.ResetSlot();
        }

        public void ClearDiscard()
        {
            discardSlot.ResetSlot();
        }

        public void SetDiscardZoneInteractable(bool value)
        {
            discardSlot.SetInteractable(value);
        }

        public void AddCardToDiscardZone(ICard card)
        {
            if (discardSlot == null)
            {
                Debug.LogError("[TableUI] Discard slot is not set up.");
                return;
            }
            if (discardSlot.CanAcceptCard(card))
            {
                discardSlot.Init(card, true);
            }
            else
            {
                Debug.LogError($"[TableUI] Card {(card != null ? card.Data.Name : "null")} cannot be added to discard zone.");
            }
        }

        public void RemoveCardFromPartisanSlot(int index)
        {
            if (index < 0 || index >= partisanSlots.Count)
            {
                Debug.LogError($"[TableUI] Invalid index {index} for partisan slots.");
                return;
            }
            var slot = partisanSlots[index];
            slot.ResetSlot();
        }

        private void OnPhaseChanged(PhaseType phase)
        {
            // A phase change always returns the table to its default mode.
            SetSacrificeMode(false);
        }

        private void OnPromptStateChanged()
        {
            SetSacrificeMode(EffectPromptState.IsActive(EffectPromptKind.Sacrifice));
        }


        /// <summary>
        /// Sacrifice is no longer a phase: it is a temporary table mode driven by
        /// the pending Sacrifice prompt (see EffectPromptState).
        /// </summary>
        public void SetSacrificeMode(bool active)
        {
            if (active && !IsMyTurn())
                return;

            _SacrificeMode = active;

            RefreshTableText();

            SetDiscardZoneInteractable(!active);
        }

        /// <summary>
        /// Title of the table: locked (not our turn), sacrifice mode, or default.
        /// </summary>
        private void RefreshTableText()
        {
            string key = _SacrificeMode ? "table.sacrifice_text" : "table.default_text";
            string localizedText = LocalizePhase(key);

            tableText.text = string.IsNullOrEmpty(localizedText) ? "Unknown" : localizedText;
        }

        private void OnTableLockChanged(bool locked)
        {
            RefreshTableText();
        }

        private string LocalizePhase(string key)
        {
            var localizedString = new LocalizedString("UIUtils", key);
            return localizedString.GetLocalizedString();
        }

        private bool IsMyTurn()
        {
            return PlayerController.Local != null && PlayerController.Local.Context.GameplayMode.IsMyTurn;
        }

        private bool IsLocalPlayer(PlayerController player)
        {
            return PlayerController.Local != null && player != null
                && player.Object.InputAuthority == PlayerController.Local.Object.InputAuthority;
        }

        #endregion

        #region Events

        public virtual void OnBackButtonPressed()
        {
            if (EffectPromptState.BlockIfPending())
                return;

            Controller.Hide();
        }

        public void OnCardClicked(ICard card, int slotId)
        {
            if (card == null || !IsMyTurn())
                return;

            SceneContext context = PlayerController.Local.Context;
            PhaseManager phaseManager = context.GameplayMode.PhaseManager;

            if (EffectPromptState.IsActive(EffectPromptKind.Sacrifice))
            {
                Controller.ShowDualPopup(new SacrificeStrategy(card));

                return;
            }

            if (phaseManager.CurrentPhase == PhaseType.Gain)
            {
                foreach (AEffect effect in card.Data.Effects)
                {
                    if ((effect.Type & EffectType.Activate) == 0)
                        continue;
                    if (card.HasBeenActivatedThisTurn)
                        return;
                    if (!effect.CanBePlayed(PlayerController.Local.Object.InputAuthority, out string reason))
                    {
                        GameEvents.OnEffectSkipped.Invoke(card, reason);

                        return;
                    }

                    Controller.ShowDualPopup(new PlayCardEffectStrategy(effect, card));

                    return;
                }
            }
        }

        private void OnCardSacrificed(ICard card)
        {
            if (card == null || !IsMyTurn())
                return;

            for (int i = 0; i < partisanSlots.Count; i++)
            {
                var slot = partisanSlots[i];
                if (slot.CardDisplay && slot.CardDisplay.Card.ID == card.ID)
                {
                    if (_PlayMat != null)
                        _PlayMat.PlaySacrificeSplash(slot.transform as RectTransform);

                    RemoveCardFromPartisanSlot(i);
                    return;
                }
            }

            Debug.LogWarning($"[TableUI] Could not find slot containing card {card.Data.Name}.");
        }

        private void OnCardReborned(ICard card)
        {
            if (card == null || !IsMyTurn())
                return;

            for (int i = 0; i < partisanSlots.Count; i++)
            {
                var slot = partisanSlots[i];
                if (slot.CardDisplay == null || !slot.CardDisplay.gameObject.activeSelf)
                {
                    slot.Init(card, true, new TableCardClickHandler(slot.GetIndex()));
                    return;
                }
            }
            Debug.LogWarning($"[TableUI] No empty partisan slot available for card {card.Data.Name}.");
        }

        #endregion
    }
}
