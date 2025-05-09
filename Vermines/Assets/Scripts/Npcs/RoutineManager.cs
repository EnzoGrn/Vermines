using System.Collections.Generic;
using UnityEngine;

public class RoutineManager : MonoBehaviour
{
    #region Exposed Fields
    [SerializeField] private List<PointOfInterest> _pointsOfInterest;
    #endregion

    /// <summary>
    /// Get a random destination slot from the list of PointOfInterest.
    /// </summary>
    /// <returns></returns>
    public PointOfInterestSlot GetRandomDestinationSlot()
    {
        // Get a list of all POI with available POI slots
        List<PointOfInterest> validPOIs = _pointsOfInterest.FindAll(poi => poi.HasAvailableSlot);

        if (validPOIs.Count == 0)
        {
            Debug.LogWarning("No available PointOfInterest slots found.");
            return null;
        }

        // Select a random POI slot from the list of valid POIs
        int randomIndex = Random.Range(0, validPOIs.Count);
        Debug.Log($"Selected POI: {randomIndex}");

        PointOfInterest selectedPOI = validPOIs[randomIndex];

        // Get a free slot from the selected POI
        PointOfInterestSlot chosenSlot = selectedPOI.GetSlotDestination();

        // Reserve the slot
        if (chosenSlot != null)
        {
            chosenSlot.Reserve();
        }

        return chosenSlot;
    }
}
