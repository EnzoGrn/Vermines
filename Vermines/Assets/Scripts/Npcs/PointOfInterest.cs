using System.Collections.Generic;
using UnityEngine;

public class PointOfInterest : MonoBehaviour
{
    #region Exposed Properties
    [SerializeField] private List<PointOfInterestSlot> _slots;
    #endregion

    #region Public Properties
    public bool HasAvailableSlot => _slots.Exists(slot => slot.IsAvailable);
    #endregion

    private void Awake()
    {
        foreach (PointOfInterestSlot slot in _slots)
        {
            // Store it's own index in the list
            slot.Id = _slots.IndexOf(slot);
        }
    }

    /// <summary>
    /// Get the first available slot in the list of slots.
    /// </summary>
    /// <returns>PointOfInterestSlot</returns>
    public PointOfInterestSlot GetSlotDestination()
    {
        List<PointOfInterestSlot> avalableSlots = _slots.FindAll(slot => slot.IsAvailable);

        if (avalableSlots.Count == 0)
            return null;

        return avalableSlots[Random.Range(0, avalableSlots.Count)];
    }
}
