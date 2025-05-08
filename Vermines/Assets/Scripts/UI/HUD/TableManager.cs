using System.Collections.Generic;
using UnityEngine;
using Vermines.CardSystem.Enumerations;
using Vermines.CardSystem.Elements;
using Vermines.UI.Card;
using Vermines.Gameplay.Phases.Enumerations;
using Vermines.Gameplay.Phases;
using Vermines.UI.Popup;

namespace Vermines.UI.GameTable
{
    public class TableUI : MonoBehaviour
    {
        public static TableUI Instance;

        [Header("Table UI")]
        [SerializeField] private GameObject tableUIPrefab;

        [Header("Zone Containers")]
        [SerializeField] private Transform partisanSlotsContainer;
        [SerializeField] private Transform equipmentSlotsContainer;

        private List<TableCardSlot> partisanSlots = new();
        private List<TableCardSlot> equipmentSlots = new();
        [SerializeField] private DiscardCardSlot discardSlot;

        [Header("Config")]
        [SerializeField] private int defaultPartisanSlotCount = 3;
        [SerializeField] private int defaultEquipmentSlotCount = 3;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            if (tableUIPrefab == null)
            {
                Debug.LogError("[TableUI] Table UI prefab is not assigned.");
                return;
            }
            tableUIPrefab.SetActive(false);
            GameEvents.OnCardSacrified.AddListener(OnCardSacrified);
            InitializeTable();
        }

        public void InitializeTable()
        {
            ClearAllSlots();
            SetupPartisanSlots(defaultPartisanSlotCount);
            SetupEquipmentSlots(defaultEquipmentSlotCount);
            SetupDiscardZone();
            SetPartisanSlotsInteractable(true);
            SetEquipmentSlotsInteractable(false);
            SetDiscardZoneInteractable(true);
        }

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
            var slot = GameTableCardSlotPool.Instance.GetSlot(parent);
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

        public void AddCardToPartisanSlot(ICard card, int index)
        {
            if (index < 0 || index >= partisanSlots.Count)
            {
                Debug.LogError($"[TableUI] Invalid index {index} for partisan slots.");
                return;
            }
            TableCardSlot slot = partisanSlots[index];
            if (slot.CanAcceptCard(card))
            {
                Debug.Log($"[PartisanSlot] Init slot");
                slot.Init(card, true, new TableCardClickHandler(index));
            }
            else
            {
                Debug.LogError($"[TableUI] Card {card.Data.Name} cannot be added to slot {index}.");
            }
        }

        public void AddCardToEquipmentSlot(ICard card, int index)
        {
            if (index < 0 || index >= equipmentSlots.Count)
            {
                Debug.LogError($"[TableUI] Invalid index {index} for equipment slots.");
                return;
            }
            var slot = equipmentSlots[index];
            if (slot.CanAcceptCard(card))
            {
                slot.Init(card, true);
            }
            else
            {
                Debug.LogError($"[TableUI] Card {card.Data.Name} cannot be added to slot {index}.");
            }
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

        public void OpenTableUI()
        {
            Debug.Log("[TableUI] Open table");
            if (tableUIPrefab == null)
            {
                Debug.LogError("[TableUI] Table UI prefab is not assigned.");
                return;
            }
            tableUIPrefab.SetActive(true);
        }

        public void CloseTableUI()
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
                PopupManager.Instance.ShowConfirm(
                    title: "Passer la phase de sacrifice ?",
                    message: $"Souhaitez-vous finir votre phase de sacrifice ?",
                    onConfirm: () =>
                    {
                        GameEvents.OnAttemptNextPhase.Invoke();
                        PopupManager.Instance.CloseCurrentPopup();
                    },
                    onCancel: () =>
                    {
                        PopupManager.Instance.CloseCurrentPopup();
                    }
                );
            }
            if (tableUIPrefab == null)
            {
                Debug.LogError("[TableUI] Table UI prefab is not assigned.");
                return;
            }
            tableUIPrefab.SetActive(false);
        }

        public void ClearAllSlots()
        {
            foreach (var slot in partisanSlots)
                GameTableCardSlotPool.Instance.ReturnSlot(slot);
            partisanSlots.Clear();

            foreach (var slot in equipmentSlots)
                GameTableCardSlotPool.Instance.ReturnSlot(slot);
            equipmentSlots.Clear();

            discardSlot?.ResetSlot();
        }

        public void SetOnlyDiscardInteractable(bool value)
        {
            Debug.Log($"[TableUI] Setting only discard zone interactable to {value}.");
            SetPartisanSlotsInteractable(!value);
            SetEquipmentSlotsInteractable(!value);
        }

        public void EnableSacrificeMode()
        {
            Debug.Log($"[TableUI] Enabling sacrifice mode.");
            SetPartisanSlotsInteractable(true);
            SetEquipmentSlotsInteractable(false);
            SetDiscardZoneInteractable(false);
        }

        public void DisableSacrificeMode()
        {
            Debug.Log($"[TableUI] Disabling sacrifice mode.");
            SetPartisanSlotsInteractable(true);
            SetEquipmentSlotsInteractable(false);
            SetDiscardZoneInteractable(true);
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
    }
}
