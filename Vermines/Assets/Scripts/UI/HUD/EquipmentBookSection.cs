using System.Collections.Generic;
using UnityEngine;
using Vermines.CardSystem.Elements;

namespace Vermines.UI
{
    public class EquipmentBookSection : MonoBehaviour
    {
        [SerializeField] private GameObject slotPrefab;
        [SerializeField] private Transform slotContainer;

        private readonly List<GameObject> _slotPool = new();

        public void UpdateEquipment(List<ICard> playedCards)
        {
            while (_slotPool.Count < playedCards.Count)
            {
                GameObject slot = Instantiate(slotPrefab, slotContainer);
                slot.SetActive(false);
                _slotPool.Add(slot);
            }

            for (int i = 0; i < playedCards.Count; i++)
            {
                GameObject slot = _slotPool[i];
                slot.SetActive(true);

                var cardUI = slot.GetComponent<EquipmentBookSlot>();
                cardUI.Setup(playedCards[i]);
            }

            for (int i = playedCards.Count; i < _slotPool.Count; i++)
            {
                _slotPool[i].SetActive(false);
            }
        }
    }
}