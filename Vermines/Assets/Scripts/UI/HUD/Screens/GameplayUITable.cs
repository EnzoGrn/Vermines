using Fusion;
using System.Collections.Generic;
using UnityEngine;
using Vermines.CardSystem.Elements;
using Vermines.CardSystem.Enumerations;
using Vermines.Gameplay.Phases;
using Vermines.Gameplay.Phases.Enumerations;
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
        /// The close button.
        /// </summary>
        [InlineHelp, SerializeField]
        protected UnityEngine.UI.Button _CloseButton;

        [Header("Close View")]

        /// <summary>
        /// The discard all view.
        /// </summary>
        [InlineHelp, SerializeField]
        protected GameObject _CloseView;

        [Header("Zone Containers")]

        [SerializeField] protected Transform partisanSlotsContainer;
        [SerializeField] protected Transform equipmentSlotsContainer;

        [SerializeField] protected GameTableCardSlotPool _Pool;
        protected List<TableCardSlot> partisanSlots = new();
        protected List<TableCardSlot> equipmentSlots = new();
        [SerializeField] protected DiscardCardSlot discardSlot;

        [Header("Config")]

        [SerializeField] private int defaultPartisanSlotCount = 3;
        [SerializeField] private int defaultEquipmentSlotCount = 3;

        #endregion

        partial void AwakeUser();
        partial void InitUser();
        partial void ShowUser();
        partial void HideUser();

        #region Override Methods

        /// <summary>
        /// The Unity awake method.
        /// Calls partial method <see cref="AwakeUser"/> to be implemented on the SDK side.
        /// </summary>
        public override void Awake()
        {
            base.Awake();

            AwakeUser();

            #region Error Handling

            if (discardSlot == null)
            {
                Debug.LogErrorFormat(
                    gameObject,
                    "GameplayUITable Critical Error: Missing 'DiscardCardSlot' reference on GameObject '{0}'. This component is required to render the discard zone. Please assign a valid DiscardCardSlot in the Inspector.",
                    gameObject.name
                );
                return;
            }

            if (_CloseButton == null)
            {
                Debug.LogErrorFormat(
                    gameObject,
                    "GameplayUITable Critical Error: Missing 'Button' reference on GameObject '{0}'. This component is required to render the close button. Please assign a valid Button in the Inspector.",
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

            if (equipmentSlotsContainer == null)
            {
                Debug.LogErrorFormat(
                    gameObject,
                    "GameplayUITable Critical Error: Missing 'Transform' reference on GameObject '{0}'. This component is required to render the equipment slots. Please assign a valid Transform in the Inspector.",
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

        /// <summary>
        /// The screen init method.
        /// Calls partial method <see cref="InitUser"/> to be implemented on the SDK side.
        /// </summary>
        public override void Init()
        {
            base.Init();

            InitUser();

            ClearAllSlots();
            SetupPartisanSlots(defaultPartisanSlotCount);
            SetupEquipmentSlots(defaultEquipmentSlotCount);
            SetupDiscardZone();
            SetPartisanSlotsInteractable(true);
            SetEquipmentSlotsInteractable(false);
            SetDiscardZoneInteractable(true);

            if (_CloseView != null)
            {
                PopupConfirm popupScript = _CloseView.GetComponent<PopupConfirm>();
                popupScript.Setup(
                    "Pass the sacrifice phase?",
                    "Would you like to pass the sacrifice phase?",
                    onConfirm: () => {
                        GameEvents.OnAttemptNextPhase.Invoke();
                        popupScript.ForceClose();
                    },
                    onCancel: () => { }
                );

                popupScript.OnClosed += () =>
                {
                    _CloseView.SetActive(false);
                };
                _CloseView.SetActive(false);
            }
            else
            {
                Debug.LogErrorFormat(
                    gameObject,
                    "GameplayUIMain Critical Error: Missing 'DiscardAllView' reference on GameObject '{0}'. This component is required to render the turn button. Please assign a valid GameObject in the Inspector.",
                    gameObject.name
                );
            }
        }

        /// <summary>
        /// The screen show method.
        /// Calls partial method <see cref="ShowUser"/> to be implemented on the SDK side.
        /// Will check is the session code is compatible with the party code to toggle the session UI part.
        /// </summary>
        public override void Show()
        {
            base.Show();

            ShowUser();

            GameEvents.OnCardSacrified.AddListener(OnCardSacrified);
        }

        /// <summary>
        /// The screen hide method.
        /// Calls partial method <see cref="HideUser"/> to be implemented on the SDK side.
        /// </summary>
        public override void Hide()
        {
            base.Hide();

            HideUser();

            GameEvents.OnCardSacrified.RemoveListener(OnCardSacrified);
        }

        #endregion

        #region Methods

        private void SetupPartisanSlots(int count)
        {
            for (int i = 0; i < count; i++)
            {
                var slot = CreateSlot(i, partisanSlotsContainer, CardType.Partisan);
                partisanSlots.Add(slot);
            }
        }

        private void SetupEquipmentSlots(int count)
        {
            for (int i = 0; i < count; i++)
            {
                var slot = CreateSlot(i, equipmentSlotsContainer, CardType.Equipment);
                equipmentSlots.Add(slot);
            }
        }

        private TableCardSlot CreateSlot(int index, Transform parent, CardType acceptedType)
        {
            var slot = _Pool.GetSlot(parent);
            slot.Setup(acceptedType);
            slot.SetIndex(index);
            slot.ResetSlot();
            slot.transform.localScale = Vector3.one * 1.6f;
            return slot;
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

            foreach (var slot in equipmentSlots)
                _Pool.ReturnSlot(slot);
            equipmentSlots.Clear();

            discardSlot.ResetSlot();
        }

        public void SetPartisanSlotsInteractable(bool value)
        {
            Debug.Log($"[TableUI] Setting partisan slots interactable to {value}.");
            foreach (var slot in partisanSlots)
                slot.SetInteractable(value);
        }

        public void SetEquipmentSlotsInteractable(bool value)
        {
            Debug.Log($"[TableUI] Setting equipment slots interactable to {value}.");
            foreach (var slot in equipmentSlots)
                slot.SetInteractable(value);
        }

        public void SetDiscardZoneInteractable(bool value)
        {
            Debug.Log($"[TableUI] Setting discard zone interactable to {value}.");
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
                Debug.LogError($"[TableUI] Card {card.Data.Name} cannot be added to discard zone.");
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

        public void RemoveCardFromEquipmentSlot(int index)
        {
            if (index < 0 || index >= equipmentSlots.Count)
            {
                Debug.LogError($"[TableUI] Invalid index {index} for equipment slots.");
                return;
            }
            var slot = equipmentSlots[index];
            slot.ResetSlot();
        }

        #endregion

        #region Events

        /// <summary>
        /// Is called when the <see cref="_CloseButton"/> is pressed using SendMessage() from the UI object.
        /// </summary>
        public virtual void OnBackButtonPressed()
        {
            if (UIContextManager.Instance.HasContext())
            {
                var context = UIContextManager.Instance.CurrentContext;
                if (context is ForceDiscardContext)
                {
                    Debug.Log("[GameTableManager] Cannot close UI during a forced discard.");
                    return;
                }
            }
            if (PhaseManager.Instance.CurrentPhase == PhaseType.Sacrifice)
            {
            }
            Hide();
        }

        private void OnCardSacrified(ICard card)
        {
            if (GameManager.Instance.IsMyTurn() == false) return;
            Debug.Log($"[TableUI] Card {card.Data.Name} has been sacrificed.");

            for (int i = 0; i < partisanSlots.Count; i++)
            {
                var slot = partisanSlots[i];
                if (slot.CardDisplay && slot.CardDisplay.Card.ID == card.ID)
                {
                    RemoveCardFromPartisanSlot(i);
                    return;
                }
            }

            Debug.LogWarning($"[TableUI] Could not find slot containing card {card.Data.Name}.");
        }

        #endregion
    }
}