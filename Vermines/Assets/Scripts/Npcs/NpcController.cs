using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class NpcController : MonoBehaviour
{
    #region Exposed Fields
    [SerializeField] private NavMeshAgent _agent;
    [SerializeField] private float minIdleTime = 5f;
    [SerializeField] private float maxIdleTime = 15f;
    [SerializeField] private Animator _animator;
    [SerializeField] private float _arrivalThresholdInterruption = 2f; // Threshold for arrival check
    [SerializeField] private bool _reversed;
    [SerializeField] private bool _hasARoutine = true;
    [SerializeField] private string _cameraName = "Main Travelling Camera";
    #endregion

    #region Private Fields
    private bool _isFacingToTheRight = true; // Default facing direction
    private Vector3 _startPositon;
    private Coroutine _runningCoroutine;
    private RoutineManager _routineManager;
    private PointOfInterestSlot _currentSlot;
    private Camera _camera;
    
    private Vector3 _interruptPos = Vector3.zero;
    #endregion

    #region Public Fields
    [HideInInspector] public bool IsRoutineRunning = false;
    public Camera Camera => _camera;
    #endregion

    void Start()
    {
        if (_reversed)
        {
            // Reverse using scale x
            Vector3 scale = transform.localScale;
            scale.x *= -1;
            transform.localScale = scale;
        }
    }

    private void Update()
    {
        if (gameObject.activeSelf)
        {
            // Update Npc Facing direction
            UpdateNpcFacingDirection();
        }
    }

    /// <summary>
    /// Start the NPC routine if it has one.
    /// </summary>
    public void StartRoutine()
    {
        if (_hasARoutine)
        {
            // Get the RoutineManager component
            _routineManager = FindFirstObjectByType<RoutineManager>();
            _runningCoroutine = StartCoroutine(StartNpcRoutine());
        }

        _startPositon = transform.position;
    }

    public void ResetNpc()
    {
        if (_hasARoutine)
        {
            if (_currentSlot != null)
            {
                _currentSlot.Free();
                _currentSlot = null;
            }

            if (_agent.isOnNavMesh)
            {
                if (!_agent.isStopped)
                {
                    _agent.isStopped = true; // Stop the agent
                }

                if (_agent.hasPath)
                {
                    _agent.ResetPath();
                }
            }
        
            _animator.SetBool("IsWalking", false);
            _isFacingToTheRight = true; // Reset facing direction
        
            _interruptPos = Vector3.zero;
            IsRoutineRunning = false;
            _runningCoroutine = null;
        }

        transform.position = _startPositon;
    }

    private bool TryGetCamera()
    {
        Camera[] cameras = FindObjectsByType<Camera>(FindObjectsSortMode.None);

        foreach (Camera cam in cameras)
        {
            if (cam.name == _cameraName)
            {
                _camera = cam;
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Update the NPC's facing direction based on the current slot position and player position.
    /// </summary>
    private void UpdateNpcFacingDirection()
    {
        // Try get camera
        
        if (_camera == null && !TryGetCamera())
        {
            Debug.Log("[UpdateNpcFacingDirection]: Cannot find camera");
            return;
        }

        Vector3 toPlayer = (_camera.transform.position - transform.position).normalized;
        Vector3 toDestination;

        if (!_currentSlot)
        {
            toDestination = (_startPositon - transform.position).normalized;
        }
        else
        {
            toDestination = (_currentSlot.SlotTransform.position - transform.position).normalized;
        }

        // Only care about XZ axis
        toDestination.y = 0;
        toPlayer.y = 0;

        // Calculate the cross product to determine the relative position
        float cross = Vector3.Cross(toPlayer, toDestination).y;

        // If cross > 0, destination is on the left from player POV
        // If cross < 0, destination is on the right from player POV
        if (_isFacingToTheRight && cross > 0)
        {
            // Flip to the left
            Vector3 scale = transform.localScale;
            scale.x *= -1;
            transform.localScale = scale;
            _isFacingToTheRight = !_isFacingToTheRight;
        }
        else if (!_isFacingToTheRight && cross < 0)
        {
            // Flip to the right
            Vector3 scale = transform.localScale;
            scale.x *= -1;
            transform.localScale = scale;
            _isFacingToTheRight = !_isFacingToTheRight;
        }
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
            _agent.isStopped = false;
            _animator.SetBool("IsWalking", true);
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
            //Debug.Log("Available slot found.");
        }
        else
        {
            MoveTo(_startPositon);
            //Debug.LogWarning("No available slots found.");
        }
    }

    /// <summary>
    /// Interrupt the current routine and move the NPC to a specified position.
    /// </summary>
    /// <param name="position"></param>
    public void Interruption(Vector3 position)
    {
        if (!_hasARoutine)
            return;

        _interruptPos = position;

        // Start animation
        _animator.SetTrigger("Interact");
    }

    /// <summary>
    /// Interrupt the current routine and move the NPC to a specified position when the interract animation is done.
    /// </summary>
    public void OnInterruptionDone()
    {
        // TODO: (see if needed) if !IsRoutineRunning maybe call resumeCoroutine() or call a resume when leaving a place like the courtyard 

        if (IsRoutineRunning)
        {
            IsRoutineRunning = false;
            if (_runningCoroutine != null)
                StopCoroutine(_runningCoroutine);
            _agent.isStopped = true; // Stop the agent
            _agent.ResetPath();
            MoveTo(_interruptPos);

            //Debug.Log($"[OnInterruptionDone]: {gameObject.name} has been interrupted");
            StartCoroutine(WaitUntilArrived(false));
        }
    }

    /// <summary>
    /// Resume the NPC routine after an interruption.
    /// </summary>
    public void ResumeCoroutine()
    {
        if (!_hasARoutine)
            return;

        // Resume the routine
        if (!IsRoutineRunning)
        {
            TryMoveToSlot();
            _runningCoroutine = StartCoroutine(WaitUntilArrived(true));
        }
    }

    #region Coroutines
    /// <summary>
    /// Wait until the NPC arrives at the destination.
    /// </summary>
    /// <param name="restartRoutine"></param>
    /// <returns></returns>
    private IEnumerator WaitUntilArrived(bool restartRoutine)
    {
        // Wait for the path to be calculated
        while (_agent.pathPending)
        {
            yield return null;
        }

        //Debug.Log($"[WaitUntilArrived]: {gameObject.name} pathPending");

        // Wait for the agent to reach the destination

        while (_agent.remainingDistance > ((restartRoutine) ? 0f : _arrivalThresholdInterruption))
        {
            yield return null;
        }

        // Agent reached his destination
        _agent.isStopped = true;
        _agent.ResetPath();
        _animator.SetBool("IsWalking", false);

        //Debug.Log($"[WaitUntilArrived]: {gameObject.name} arrived");

        if (restartRoutine)
        {
            //Debug.Log("Restart Coroutine");
            StartCoroutine(StartNpcRoutine());
        }
    }

    /// <summary>
    /// Start the NPC routine.
    /// </summary>
    /// <returns></returns>
    private IEnumerator StartNpcRoutine()
    {
        // Start the routine
        IsRoutineRunning = true;
        while (IsRoutineRunning)
        {
            //yield return new WaitForSeconds(Random.Range(minIdleTime, maxIdleTime));
            float waitTime = Random.Range(minIdleTime, maxIdleTime);
            float timer = 0f;
            //Debug.Log($"[StartNpcRoutine]: {gameObject.name} is waiting for {waitTime} seconds");
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
                //Debug.Log("Processing in routine !");
                if (!IsRoutineRunning) yield break; // Interrupt the coroutine if the routine is stopped
                yield return null;
            }
            _animator.SetBool("IsWalking", false);
            //Debug.Log($"Destination reached: {_currentSlot?.Id} by {gameObject.name}");
        }
    }
    #endregion
}
