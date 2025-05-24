using log4net;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class RoutineManager : MonoBehaviour
{
    #region Exposed Fields
    [SerializeField] private List<PointOfInterest> _pointsOfInterest;
    [SerializeField] private List<NpcController> _npcs;

    [SerializeField] private float _radiusOfAreaInterruption;
    [SerializeField] private float _rateOfInterestForInterruption;
    #endregion

    #region Private Fields
    private float _percentOfNpcInterest = 0;
    private Vector3 _interruptPos = Vector3.zero;
    #endregion

    private void Start()
    {
        _percentOfNpcInterest = _npcs.Count * _rateOfInterestForInterruption;
    }

    /// <summary>
    /// Start the NPC routine if it has one.
    /// </summary>
    public void StartRoutine()
    {
        foreach (NpcController npc in _npcs)
        {
            if (npc.gameObject.activeSelf == false)
            {
                npc.gameObject.SetActive(true);
            }

            // Start the NPC routine
            if (npc.IsRoutineRunning == false)
            {
                npc.StartRoutine();
            }
        }
    }

    public void StopRoutine()
    {
        foreach (NpcController npc in _npcs)
        {
            if (npc.gameObject.activeSelf == true)
            {
                npc.gameObject.SetActive(false);
            }
            // Reset the NPC position
            npc.ResetNpc();
        }
    }

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
        //Debug.Log($"Selected POI: {randomIndex}");

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

    /// <summary>
    /// Get a random point in a radius around the center position.
    /// </summary>
    /// <param name="center"></param>
    /// <returns></returns>
    private Vector3 GetRandomPointInRadius(Vector3 center)
    {
        Vector2 randomPoint = Random.insideUnitCircle * _radiusOfAreaInterruption;
        Vector3 result = center + new Vector3(randomPoint.x, 0f, randomPoint.y);

        // On s'assure que le point est navigable (optionnel mais recommandé)
        NavMeshHit hit;
        if (NavMesh.SamplePosition(result, out hit, 2.0f, NavMesh.AllAreas))
        {
            return hit.position;
        }
        return center; // fallback au centre si aucun point n’est trouvé
    }

    /// <summary>
    /// Interrupt the current routine of some NPCs to go to a specififc destination.
    /// </summary>
    public void InterruptNpcRoutine()
    {
        _interruptPos = Vector3.zero;

        foreach (NpcController npc in _npcs)
        {
            int randomIndex = Random.Range(0, _npcs.Count);
            if (randomIndex < _percentOfNpcInterest)
            {
                Debug.Log($"Interrupting NPC: {npc.gameObject.name}");
                
                // TODO: Run the interact animation that will then start the InterruptCoroutine
                npc.Interruption(GetRandomPointInRadius(_interruptPos));
            }
        }
    }

    /// <summary>
    /// Resume the NPC routine after an interruption.
    /// </summary>
    public void ResumeNpcRoutine()
    {
        foreach (NpcController npc in _npcs)
        {
            npc.ResumeCoroutine();
        }
    }
}
