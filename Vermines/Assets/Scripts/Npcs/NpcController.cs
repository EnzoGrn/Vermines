using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class NpcController : MonoBehaviour
{
    #region Exposed Fields
    [SerializeField] private NavMeshAgent _agent;
    [SerializeField] private float minIdleTime = 5f;
    [SerializeField] private float maxIdleTime = 15f;
    #endregion

    #region Private Fields
    private Vector3 _startPositon;
    private Coroutine _runningCoroutine;
    private RoutineManager _routineManager;
    private PointOfInterestSlot _currentSlot;
    #endregion

    #region Public Fields
    [HideInInspector] public bool IsRoutineRunning = false;
    #endregion

    void Start()
    {
        // Get the RoutineManager component
        _routineManager = FindFirstObjectByType<RoutineManager>();
        _startPositon = transform.position;

        _runningCoroutine = StartCoroutine(StartNpcRoutine());
    }

    void Update()
    {
    }

    /// <summary>
    /// Move the NPC to a specified position.
    /// </summary>
    /// <param name="posittion"></param>
    private void MoveTo(Vector3 posittion)
    {
        if (_agent != null)
        {
            _agent.SetDestination(posittion);
        }
    }

    /// <summary>
    /// Try to free the current slot if it exists.
    /// </summary>
    private void TryFreeSlot()
    {
        if (_currentSlot != null)
        {
            _currentSlot.Free();
            _currentSlot = null;
        }
    }

    /// <summary>
    /// Try to move the NPC to a slot position.
    /// </summary>
    private void TryMoveToSlot()
    {
        if (_currentSlot != null)
        {
            // Move to the slot position
            MoveTo(_currentSlot.SlotTransform.position);
            Debug.Log("Available slot found.");
        }
        else
        {
            MoveTo(_startPositon);
            Debug.LogWarning("No available slots found.");
        }
    }

    /// <summary>
    /// Interrupt the current routine and move the NPC to a specified position.
    /// </summary>
    /// <param name="position"></param>
    public void InterruptCoroutine(Vector3 position)
    {
        // Stop the current routine
        if (IsRoutineRunning)
        {
            IsRoutineRunning = false;
            StopCoroutine(_runningCoroutine);
            _agent.ResetPath();
            MoveTo(position);
        }
    }

    public void ResumeCoroutine()
    {
        // Resume the routine
        if (!IsRoutineRunning)
        {
            TryMoveToSlot();
            _runningCoroutine = StartCoroutine(StartNpcRoutine());
        }
    }

    #region Coroutines
    /// <summary>
    /// Start the NPC routine.
    /// </summary>
    /// <returns></returns>
    private IEnumerator StartNpcRoutine()
    {
        // Wait until the NPC reaches his destination (in case we interrupt the routine and resume it)
        while (_agent.pathPending || _agent.remainingDistance > _agent.stoppingDistance)
        {
            if (!IsRoutineRunning) yield break; // Interrupt the coroutine if the routine is stopped
            yield return null;
        }

        //Debug.Log($"Starting NPC routine: {gameObject.name}");

        // Start the routine
        IsRoutineRunning = true;
        while (IsRoutineRunning)
        {
            //yield return new WaitForSeconds(Random.Range(minIdleTime, maxIdleTime));
            float waitTime = Random.Range(minIdleTime, maxIdleTime);
            float timer = 0f;
            while (timer < waitTime)
            {
                if (!IsRoutineRunning) yield break; // check pendant l'attente
                timer += Time.deltaTime;
                yield return null;
            }

            // Free the current slot if it exists
            TryFreeSlot();

            // Get a slot from the RoutineManager
            _currentSlot = _routineManager.GetRandomDestinationSlot();
            //Debug.Log($"Slot found: {_currentSlot?.Id} by {gameObject.name}");

            TryMoveToSlot();

            // Wait until the NPC reaches the destination
            while (_agent.pathPending || _agent.remainingDistance > _agent.stoppingDistance)
            {
                if (!IsRoutineRunning) yield break; // Interrupt the coroutine if the routine is stopped
                yield return null;
            }
            //Debug.Log($"Destination reached: {_currentSlot?.Id} by {gameObject.name}");
        }
    }
    #endregion
}
