using UnityEngine;

/// <summary>
/// Contrôleur de caméra cinématique pour prises de trailer.
///
/// CONTRÔLES CAMÉRA LIBRE :
///   ZQSD          → déplacement relatif au yaw
///   E / A         → monter / descendre
///   Molette       → rotation horizontale
///   Clic droit    → rotation libre souris
///   Numpad + / -  → vitesse déplacement ZQSD
///   Numpad * / /  → vitesse travelling
///   Numpad 8 / 2  → vitesse rotation flèches
///   F1            → preset vue drone
///   F2            → basculer mode FPS / caméra libre
///   F3            → reset rotation
///   U             → ajouter un point au travelling
///   Y             → annuler le dernier point
///   O             → vider toute la séquence
///   T             → lancer / stopper le travelling
///
/// CONTRÔLES MODE FPS :
///   ZQSD          → marcher
///   Souris        → regarder
///   F2            → repasser en caméra libre
///   I             → ajouter un point FPS au travelling
///   K             → annuler le dernier point FPS
///   L             → vider la séquence FPS
///   G             → lancer / stopper le travelling FPS (avec bob de marche)
///
/// ROUTINE MANAGER :
///   F5 Start · F6 Stop · F7 Interrupt · F8 Resume
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class CinematicCameraController : MonoBehaviour
{
    // ─────────────────────────────────────────
    //  RÉFÉRENCES
    // ─────────────────────────────────────────
    [Header("Routine Manager")]
    public RoutineManager routineManager;

    // ─────────────────────────────────────────
    //  CAMÉRA LIBRE — DÉPLACEMENT
    // ─────────────────────────────────────────
    [Header("Caméra libre — Déplacement")]
    public float moveSpeed = 5f;
    public float minSpeed = 0.5f;
    public float maxSpeed = 30f;
    public float speedStep = 0.5f;
    public float smoothTime = 0.08f;

    // ─────────────────────────────────────────
    //  CAMÉRA LIBRE — ROTATION
    // ─────────────────────────────────────────
    [Header("Caméra libre — Rotation")]
    public float scrollRotationSpeed = 80f;
    public float mouseRotationSpeed = 3f;

    // ─────────────────────────────────────────
    //  TRAVELLING CAMÉRA LIBRE
    // ─────────────────────────────────────────
    [Header("Rotation clavier (flèches)")]
    public float arrowRotationSpeed = 45f;
    public float arrowRotationSpeedMin = 5f;
    public float arrowRotationSpeedMax = 180f;
    public float arrowRotationStep = 5f;
    public float arrowSmoothing = 10f;

    [Header("Travelling")]
    public float travellingSpeed = 2f;
    public float travellingSpeedMin = 0.5f;
    public float travellingSpeedMax = 20f;
    public float travellingStep = 0.5f;

    // ─────────────────────────────────────────
    //  MODE FPS
    // ─────────────────────────────────────────
    [Header("Mode FPS")]
    public float fpsHeight = 1.75f;
    public float fpsWalkSpeed = 3f;
    public float fpsMouseSensitivity = 2f;
    public float gravity = -9.81f;

    [Header("Bob caméra (marche)")]
    public float bobFrequency = 1.8f;
    public float bobAmplitude = 0.04f;
    public float bobSmoothing = 8f;

    // ─────────────────────────────────────────
    //  TRAVELLING FPS
    // ─────────────────────────────────────────
    [Header("Travelling FPS")]
    [Tooltip("Vitesse de déplacement lors du travelling FPS (unités/s)")]
    public float fpsTravellingSpeed = 2f;
    [Tooltip("Fréquence du bob pendant le travelling FPS (cycles/s). Laisser à 0 pour utiliser bobFrequency.")]
    public float fpsTravelBobFrequency = 0f;   // 0 = hérite de bobFrequency
    [Tooltip("Amplitude du bob pendant le travelling FPS. Laisser à 0 pour utiliser bobAmplitude.")]
    public float fpsTravelBobAmplitude = 0f;   // 0 = hérite de bobAmplitude

    // ─────────────────────────────────────────
    //  TÉLÉPORTATION
    // ─────────────────────────────────────────
    [Header("Téléportation")]
    public Transform sacrificeZone;
    public KeyCode teleportKey = KeyCode.P;

    // ─────────────────────────────────────────
    //  PRIVÉ — HUD
    // ─────────────────────────────────────────
    private bool _showHUD = true;

    // ─────────────────────────────────────────
    //  PRIVÉ — caméra libre
    // ─────────────────────────────────────────
    private Vector3 _velocity = Vector3.zero;
    private Vector3 _targetPosition;
    private float _yaw = 0f;
    private float _pitch = 0f;
    private float _targetYaw = 0f;
    private float _targetPitch = 0f;

    // ─────────────────────────────────────────
    //  PRIVÉ — travelling partagé (struct)
    // ─────────────────────────────────────────
    private struct TravelPoint
    {
        public Vector3 position;
        public Quaternion rotation;
    }

    // ── Travelling caméra libre ──
    private System.Collections.Generic.List<TravelPoint> _travelPoints =
        new System.Collections.Generic.List<TravelPoint>();
    private bool _travellingActive = false;
    private int _travelSegment = 0;
    private float _travelProgress = 0f;

    // ── Travelling FPS ──
    private System.Collections.Generic.List<TravelPoint> _fpsTravelPoints =
        new System.Collections.Generic.List<TravelPoint>();
    private bool _fpsTravellingActive = false;
    private int _fpsTravelSegment = 0;
    private float _fpsTravelProgress = 0f;

    // ─────────────────────────────────────────
    //  PRIVÉ — mode FPS
    // ─────────────────────────────────────────
    private bool _fpsMode = false;
    private CharacterController _cc;
    private Vector3 _fpsVelocity = Vector3.zero;
    private float _fpsPitch = 0f;
    private float _fpsYaw = 0f;

    // Bob
    private float _bobTimer = 0f;
    private Vector3 _bobCurrentOffset = Vector3.zero;
    private Vector3 _bobTargetOffset = Vector3.zero;

    // ─────────────────────────────────────────
    void Start()
    {
        _cc = GetComponent<CharacterController>();
        _cc.enabled = false;

        _targetPosition = transform.position;
        _yaw = transform.eulerAngles.y;
        _pitch = transform.eulerAngles.x;
        _targetYaw = _yaw;
        _targetPitch = _pitch;
    }

    void Update()
    {
        HandleModeSwitch();

        if (_fpsMode)
        {
            UpdateFPS();
        }
        else
        {
            if (_travellingActive)
                UpdateTravelling();
            else
                HandleFreeCamera();

            HandleSpeedAdjustment();
            HandlePresets();
            HandleTravellingControls();
        }

        HandleRoutineControls();
    }

    // ═════════════════════════════════════════
    //  SWITCH DE MODE
    // ═════════════════════════════════════════
    void HandleModeSwitch()
    {
        if (!Input.GetKeyDown(KeyCode.F2)) return;

        _fpsMode = !_fpsMode;

        if (_fpsMode)
            EnterFPSMode();
        else
            ExitFPSMode();
    }

    void EnterFPSMode()
    {
        if (_travellingActive) StopTravelling();

        _cc.enabled = true;
        _cc.height = fpsHeight;
        _cc.center = new Vector3(0f, -0.8f, 0f);
        _cc.stepOffset = 0.35f;
        _cc.skinWidth = 0.08f;
        _cc.radius = 0.3f;
        _cc.slopeLimit = 89f;

        Vector3 pos = transform.position;
        pos.y = GetGroundY(pos);
        transform.position = pos;
        _targetPosition = pos;

        _fpsYaw = transform.eulerAngles.y;
        _fpsPitch = 0f;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        _bobTimer = 0f;
        _bobCurrentOffset = Vector3.zero;

        Debug.Log("Mode FPS activé.");
    }

    void ExitFPSMode()
    {
        // Arrête le travelling FPS si actif
        if (_fpsTravellingActive) StopFPSTravelling();

        _cc.enabled = false;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        _yaw = _fpsYaw;
        _pitch = _fpsPitch;
        _targetYaw = _yaw;
        _targetPitch = _pitch;
        _targetPosition = transform.position;

        Debug.Log("Mode caméra libre repris.");
    }

    // ═════════════════════════════════════════
    //  MODE FPS
    // ═════════════════════════════════════════
    void UpdateFPS()
    {
        if (_fpsTravellingActive)
        {
            // Travelling FPS : la caméra suit les points, le bob simule la marche
            UpdateFPSTravelling();
            HandleCameraBob();
        }
        else
        {
            HandleFPSRotation();
            HandleFPSMovement();
            HandleCameraBob();
            HandleFPSTeleport();
        }

        // Toujours disponibles en mode FPS
        HandleFPSTravellingControls();
    }

    void HandleFPSRotation()
    {
        _fpsYaw += Input.GetAxis("Mouse X") * fpsMouseSensitivity;
        _fpsPitch -= Input.GetAxis("Mouse Y") * fpsMouseSensitivity;
        _fpsPitch = Mathf.Clamp(_fpsPitch, -80f, 80f);

        transform.rotation = Quaternion.Euler(_fpsPitch, _fpsYaw, 0f);
    }

    void HandleFPSMovement()
    {
        Quaternion horizontalRot = Quaternion.Euler(0f, _fpsYaw, 0f);
        Vector3 moveDir = Vector3.zero;

        if (Input.GetKey(KeyCode.W)) moveDir += horizontalRot * Vector3.forward;
        if (Input.GetKey(KeyCode.S)) moveDir += horizontalRot * Vector3.back;
        if (Input.GetKey(KeyCode.A)) moveDir += horizontalRot * Vector3.left;
        if (Input.GetKey(KeyCode.D)) moveDir += horizontalRot * Vector3.right;

        if (_cc.isGrounded && _fpsVelocity.y < 0f)
            _fpsVelocity.y = -2f;

        _fpsVelocity.y += gravity * Time.deltaTime;

        Vector3 finalMove = moveDir * fpsWalkSpeed + Vector3.up * _fpsVelocity.y;
        _cc.Move(finalMove * Time.deltaTime);

        bool isWalking = moveDir.magnitude > 0.1f && _cc.isGrounded;
        UpdateBobTimer(isWalking, bobFrequency, bobAmplitude);
    }

    void UpdateBobTimer(bool isWalking, float freq, float amp)
    {
        if (isWalking)
        {
            _bobTimer += Time.deltaTime * freq * Mathf.PI * 2f;
            _bobTargetOffset = new Vector3(
                Mathf.Sin(_bobTimer * 0.5f) * amp * 0.5f,
                Mathf.Sin(_bobTimer) * amp,
                0f
            );
        }
        else
        {
            _bobTimer = 0f;
            _bobTargetOffset = Vector3.zero;
        }
    }

    void HandleCameraBob()
    {
        _bobCurrentOffset = Vector3.Lerp(_bobCurrentOffset, _bobTargetOffset, Time.deltaTime * bobSmoothing);
        Vector3 basePos = transform.position;
        transform.position = basePos + transform.TransformDirection(_bobCurrentOffset);
    }

    // ═════════════════════════════════════════
    //  TRAVELLING FPS
    // ═════════════════════════════════════════

    /// <summary>
    /// Gère les raccourcis clavier du travelling FPS.
    /// I → poser un point, K → annuler dernier, L → tout effacer, G → lancer/stopper.
    /// </summary>
    void HandleFPSTravellingControls()
    {
        // I → ajouter un point FPS
        if (Input.GetKeyDown(KeyCode.I))
        {
            _fpsTravelPoints.Add(new TravelPoint
            {
                position = transform.position,
                rotation = Quaternion.Euler(_fpsPitch, _fpsYaw, 0f)
            });
            Debug.Log($"Travelling FPS : point {_fpsTravelPoints.Count} posé → {transform.position}");
        }

        // K → annuler le dernier point FPS
        if (Input.GetKeyDown(KeyCode.K) && _fpsTravelPoints.Count > 0)
        {
            _fpsTravelPoints.RemoveAt(_fpsTravelPoints.Count - 1);
            Debug.Log($"Travelling FPS : dernier point supprimé ({_fpsTravelPoints.Count} restants)");
        }

        // L → vider la séquence FPS
        if (Input.GetKeyDown(KeyCode.L))
        {
            _fpsTravelPoints.Clear();
            Debug.Log("Travelling FPS : séquence réinitialisée.");
        }

        // G → lancer / stopper
        if (Input.GetKeyDown(KeyCode.G))
        {
            if (_fpsTravellingActive)
                StopFPSTravelling();
            else if (_fpsTravelPoints.Count >= 2)
                StartFPSTravelling();
            else
                Debug.LogWarning($"Travelling FPS : il faut au moins 2 points (actuellement {_fpsTravelPoints.Count}). Ajoute des points avec I.");
        }
    }

    void StartFPSTravelling()
    {
        _fpsTravellingActive = true;
        _fpsTravelSegment = 0;
        _fpsTravelProgress = 0f;
        _bobTimer = 0f;
        _bobCurrentOffset = Vector3.zero;

        // On place la caméra sur le premier point
        transform.position = _fpsTravelPoints[0].position;
        transform.rotation = _fpsTravelPoints[0].rotation;
        _fpsYaw = _fpsTravelPoints[0].rotation.eulerAngles.y;
        _fpsPitch = _fpsTravelPoints[0].rotation.eulerAngles.x;

        // Désactiver le CharacterController pendant le travelling pour éviter les collisions
        // (la caméra suit la courbe mathématiquement, pas via physique)
        _cc.enabled = false;

        Debug.Log($"Travelling FPS démarré ({_fpsTravelPoints.Count} points, {_fpsTravelPoints.Count - 1} segments).");
    }

    void StopFPSTravelling()
    {
        _fpsTravellingActive = false;
        _bobTimer = 0f;
        _bobTargetOffset = Vector3.zero;

        // Réactive le CharacterController pour reprendre la marche normale
        _cc.enabled = true;

        // Sync de la rotation depuis le dernier état interpolé
        _fpsYaw = transform.eulerAngles.y;
        _fpsPitch = transform.eulerAngles.x;
        // Normalise le pitch dans [-180, 180]
        if (_fpsPitch > 180f) _fpsPitch -= 360f;

        Debug.Log("Travelling FPS stoppé.");
    }

    void UpdateFPSTravelling()
    {
        int n = _fpsTravelPoints.Count;
        if (n < 2) { StopFPSTravelling(); return; }

        // ── Avance sur le segment en cours ──
        float segDist = Vector3.Distance(
            _fpsTravelPoints[_fpsTravelSegment].position,
            _fpsTravelPoints[_fpsTravelSegment + 1].position
        );

        if (segDist > 0.001f)
            _fpsTravelProgress += (fpsTravellingSpeed / segDist) * Time.deltaTime;
        else
            _fpsTravelProgress = 1f;

        if (_fpsTravelProgress >= 1f)
        {
            _fpsTravelSegment++;
            _fpsTravelProgress = 0f;

            if (_fpsTravelSegment >= n - 1)
            {
                // Arrivée au dernier point
                transform.position = _fpsTravelPoints[n - 1].position;
                transform.rotation = _fpsTravelPoints[n - 1].rotation;
                StopFPSTravelling();
                return;
            }
        }

        int i = _fpsTravelSegment;
        float t = _fpsTravelProgress;

        // ── Position : Catmull-Rom ──
        Vector3 p0 = _fpsTravelPoints[Mathf.Max(i - 1, 0)].position;
        Vector3 p1 = _fpsTravelPoints[i].position;
        Vector3 p2 = _fpsTravelPoints[Mathf.Min(i + 1, n - 1)].position;
        Vector3 p3 = _fpsTravelPoints[Mathf.Min(i + 2, n - 1)].position;

        // On applique la position de base SANS le bob (le bob s'ajoute dans HandleCameraBob)
        transform.position = CatmullRom(p0, p1, p2, p3, t);

        // ── Rotation : Slerp anti-flip ──
        Quaternion rotFrom = _fpsTravelPoints[i].rotation;
        Quaternion rotTo = _fpsTravelPoints[Mathf.Min(i + 1, n - 1)].rotation;
        if (Quaternion.Dot(rotFrom, rotTo) < 0f)
            rotTo = new Quaternion(-rotTo.x, -rotTo.y, -rotTo.z, -rotTo.w);

        Quaternion interpolatedRot = Quaternion.Slerp(rotFrom, rotTo, t);
        transform.rotation = interpolatedRot;

        // Sync les angles FPS depuis la rotation interpolée (pour cohérence si on reprend le contrôle)
        Vector3 eulers = interpolatedRot.eulerAngles;
        _fpsYaw = eulers.y;
        _fpsPitch = eulers.x > 180f ? eulers.x - 360f : eulers.x;

        // ── Bob de marche continu ──
        float freq = fpsTravelBobFrequency > 0f ? fpsTravelBobFrequency : bobFrequency;
        float amp = fpsTravelBobAmplitude > 0f ? fpsTravelBobAmplitude : bobAmplitude;
        UpdateBobTimer(true, freq, amp);
    }

    // ═════════════════════════════════════════
    //  UTILITAIRE — trouver le sol sous un point
    // ═════════════════════════════════════════
    float GetGroundY(Vector3 pos)
    {
        if (Physics.Raycast(pos + Vector3.up * 50f, Vector3.down, out RaycastHit hit, 100f))
            return hit.point.y;
        return pos.y;
    }

    // ═════════════════════════════════════════
    //  CAMÉRA LIBRE
    // ═════════════════════════════════════════
    void HandleFreeCamera()
    {
        HandleMovement();
        HandleRotation();
    }

    void HandleMovement()
    {
        Quaternion horizontalRotation = Quaternion.Euler(0f, _yaw, 0f);
        Vector3 direction = Vector3.zero;

        if (Input.GetKey(KeyCode.W)) direction += horizontalRotation * Vector3.forward;
        if (Input.GetKey(KeyCode.S)) direction += horizontalRotation * Vector3.back;
        if (Input.GetKey(KeyCode.A)) direction += horizontalRotation * Vector3.left;
        if (Input.GetKey(KeyCode.D)) direction += horizontalRotation * Vector3.right;
        if (Input.GetKey(KeyCode.E)) direction += Vector3.up;
        if (Input.GetKey(KeyCode.Q)) direction += Vector3.down;

        _targetPosition += direction * moveSpeed * Time.deltaTime;

        transform.position = Vector3.SmoothDamp(
            transform.position, _targetPosition, ref _velocity, smoothTime
        );
    }

    void HandleRotation()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.01f)
            _targetYaw += scroll * scrollRotationSpeed;

        if (Input.GetMouseButton(1))
        {
            _targetYaw += Input.GetAxis("Mouse X") * mouseRotationSpeed;
            _targetPitch -= Input.GetAxis("Mouse Y") * mouseRotationSpeed;
        }

        if (Input.GetKey(KeyCode.LeftArrow)) _targetYaw -= arrowRotationSpeed * Time.deltaTime;
        if (Input.GetKey(KeyCode.RightArrow)) _targetYaw += arrowRotationSpeed * Time.deltaTime;
        if (Input.GetKey(KeyCode.UpArrow)) _targetPitch -= arrowRotationSpeed * Time.deltaTime;
        if (Input.GetKey(KeyCode.DownArrow)) _targetPitch += arrowRotationSpeed * Time.deltaTime;

        _targetPitch = Mathf.Clamp(_targetPitch, -89f, 89f);

        _yaw = Mathf.Lerp(_yaw, _targetYaw, Time.deltaTime * arrowSmoothing);
        _pitch = Mathf.Lerp(_pitch, _targetPitch, Time.deltaTime * arrowSmoothing);

        transform.rotation = Quaternion.Euler(_pitch, _yaw, 0f);
    }

    // ═════════════════════════════════════════
    //  VITESSE
    // ═════════════════════════════════════════
    void HandleSpeedAdjustment()
    {
        if (Input.GetKeyDown(KeyCode.KeypadPlus))
        {
            moveSpeed = Mathf.Clamp(moveSpeed + speedStep, minSpeed, maxSpeed);
            Debug.Log($"Vitesse déplacement : {moveSpeed:F1}");
        }
        if (Input.GetKeyDown(KeyCode.KeypadMinus))
        {
            moveSpeed = Mathf.Clamp(moveSpeed - speedStep, minSpeed, maxSpeed);
            Debug.Log($"Vitesse déplacement : {moveSpeed:F1}");
        }
        if (Input.GetKeyDown(KeyCode.KeypadMultiply))
        {
            travellingSpeed = Mathf.Clamp(travellingSpeed + travellingStep, travellingSpeedMin, travellingSpeedMax);
            Debug.Log($"Vitesse travelling : {travellingSpeed:F1}");
        }
        if (Input.GetKeyDown(KeyCode.KeypadDivide))
        {
            travellingSpeed = Mathf.Clamp(travellingSpeed - travellingStep, travellingSpeedMin, travellingSpeedMax);
            Debug.Log($"Vitesse travelling : {travellingSpeed:F1}");
        }
        if (Input.GetKeyDown(KeyCode.Keypad8))
        {
            arrowRotationSpeed = Mathf.Clamp(arrowRotationSpeed + arrowRotationStep, arrowRotationSpeedMin, arrowRotationSpeedMax);
            Debug.Log($"Vitesse rotation : {arrowRotationSpeed:F1}°/s");
        }
        if (Input.GetKeyDown(KeyCode.Keypad2))
        {
            arrowRotationSpeed = Mathf.Clamp(arrowRotationSpeed - arrowRotationStep, arrowRotationSpeedMin, arrowRotationSpeedMax);
            Debug.Log($"Vitesse rotation : {arrowRotationSpeed:F1}°/s");
        }
    }

    // ═════════════════════════════════════════
    //  PRESETS
    // ═════════════════════════════════════════
    void HandlePresets()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            _targetPitch = 80f;
            Debug.Log("Preset : vue drone");
        }
        if (Input.GetKeyDown(KeyCode.F3))
        {
            _targetPitch = 0f;
            _targetYaw = 0f;
            Debug.Log("Preset : rotation reset");
        }
    }

    // ═════════════════════════════════════════
    //  TRAVELLING CAMÉRA LIBRE
    // ═════════════════════════════════════════
    void HandleTravellingControls()
    {
        if (Input.GetKeyDown(KeyCode.U))
        {
            _travelPoints.Add(new TravelPoint
            {
                position = transform.position,
                rotation = transform.rotation
            });
            Debug.Log($"Travelling : point {_travelPoints.Count} posé → {transform.position}");
        }

        if (Input.GetKeyDown(KeyCode.Y) && _travelPoints.Count > 0)
        {
            _travelPoints.RemoveAt(_travelPoints.Count - 1);
            Debug.Log($"Travelling : dernier point supprimé ({_travelPoints.Count} restants)");
        }

        if (Input.GetKeyDown(KeyCode.O))
        {
            _travelPoints.Clear();
            Debug.Log("Travelling : séquence réinitialisée.");
        }

        if (Input.GetKeyDown(KeyCode.T))
        {
            if (_travellingActive)
                StopTravelling();
            else if (_travelPoints.Count >= 2)
                StartTravelling();
            else
                Debug.LogWarning($"Travelling : il faut au moins 2 points (actuellement {_travelPoints.Count}). Ajoute des points avec U.");
        }
    }

    void StartTravelling()
    {
        _travellingActive = true;
        _travelSegment = 0;
        _travelProgress = 0f;
        transform.position = _travelPoints[0].position;
        transform.rotation = _travelPoints[0].rotation;
        _targetPosition = _travelPoints[0].position;
        Debug.Log($"Travelling démarré ({_travelPoints.Count} points, {_travelPoints.Count - 1} segments).");
    }

    void StopTravelling()
    {
        _travellingActive = false;
        _targetPosition = transform.position;
        _yaw = transform.eulerAngles.y;
        _pitch = transform.eulerAngles.x;
        _targetYaw = _yaw;
        _targetPitch = _pitch;
        Debug.Log("Travelling stoppé.");
    }

    void UpdateTravelling()
    {
        if (_travelPoints.Count < 2) { StopTravelling(); return; }

        int n = _travelPoints.Count;

        float segmentDistance = Vector3.Distance(
            _travelPoints[_travelSegment].position,
            _travelPoints[_travelSegment + 1].position
        );

        if (segmentDistance > 0.001f)
            _travelProgress += (travellingSpeed / segmentDistance) * Time.deltaTime;
        else
            _travelProgress = 1f;

        if (_travelProgress >= 1f)
        {
            _travelSegment++;
            _travelProgress = 0f;

            if (_travelSegment >= n - 1)
            {
                transform.position = _travelPoints[n - 1].position;
                transform.rotation = _travelPoints[n - 1].rotation;
                StopTravelling();
                return;
            }
        }

        int i = _travelSegment;
        float t = _travelProgress;

        Vector3 p0 = _travelPoints[Mathf.Max(i - 1, 0)].position;
        Vector3 p1 = _travelPoints[i].position;
        Vector3 p2 = _travelPoints[Mathf.Min(i + 1, n - 1)].position;
        Vector3 p3 = _travelPoints[Mathf.Min(i + 2, n - 1)].position;

        transform.position = CatmullRom(p0, p1, p2, p3, t);

        Quaternion rotFrom = _travelPoints[i].rotation;
        Quaternion rotTo = _travelPoints[Mathf.Min(i + 1, n - 1)].rotation;

        if (Quaternion.Dot(rotFrom, rotTo) < 0f)
            rotTo = new Quaternion(-rotTo.x, -rotTo.y, -rotTo.z, -rotTo.w);

        transform.rotation = Quaternion.Slerp(rotFrom, rotTo, t);
    }

    Vector3 CatmullRom(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        float t2 = t * t;
        float t3 = t2 * t;
        return 0.5f * (
            2f * p1
            + (-p0 + p2) * t
            + (2f * p0 - 5f * p1 + 4f * p2 - p3) * t2
            + (-p0 + 3f * p1 - 3f * p2 + p3) * t3
        );
    }

    // ═════════════════════════════════════════
    //  ROUTINE MANAGER
    // ═════════════════════════════════════════
    void HandleRoutineControls()
    {
        if (routineManager == null) return;

        if (Input.GetKeyDown(KeyCode.F5)) { routineManager.StartRoutine(); Debug.Log("RoutineManager : Start"); }
        if (Input.GetKeyDown(KeyCode.F6)) { routineManager.StopRoutine(); Debug.Log("RoutineManager : Stop"); }
        if (Input.GetKeyDown(KeyCode.F7)) { routineManager.InterruptNpcRoutine(); Debug.Log("RoutineManager : Interrupt"); }
        if (Input.GetKeyDown(KeyCode.F8)) { routineManager.ResumeNpcRoutine(); Debug.Log("RoutineManager : Resume"); }
    }

    // ═════════════════════════════════════════
    //  TÉLÉPORTATION
    // ═════════════════════════════════════════
    void HandleFPSTeleport()
    {
        if (!_fpsMode) return;
        if (sacrificeZone == null) return;
        if (Input.GetKeyDown(teleportKey))
            TeleportToSacrificeZone();
    }

    void TeleportToSacrificeZone()
    {
        bool wasEnabled = _cc.enabled;
        _cc.enabled = false;

        transform.position = sacrificeZone.position;
        transform.rotation = sacrificeZone.rotation;
        _fpsVelocity = Vector3.zero;

        _cc.enabled = wasEnabled;

        Debug.Log("Téléportation -> Zone de sacrifice");
    }

    // ═════════════════════════════════════════
    //  UI DEBUG
    // ═════════════════════════════════════════
    void OnGUI()
    {
        if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.H)
        {
            _showHUD = !_showHUD;
            Event.current.Use();
        }

        if (!_showHUD) return;

        GUIStyle style = new GUIStyle(GUI.skin.label);
        style.fontSize = 14;
        style.normal.textColor = Color.white;

        if (_fpsMode)
        {
            int fpsPts = _fpsTravelPoints.Count;
            string fpsTState;
            if (_fpsTravellingActive)
                fpsTState = $"EN COURS  segment {_fpsTravelSegment + 1}/{fpsPts - 1}  ({(_fpsTravelProgress * 100f):F0}%)";
            else
                fpsTState = $"arrêté  ({fpsPts} point{(fpsPts > 1 ? "s" : "")})";

            GUI.Label(new Rect(10, 10, 700, 120), string.Join("\n", new[]
            {
                "MODE FPS  (F2 pour revenir en caméra libre)  |  Souris = regarder · ZQSD = marcher",
                $"Travelling FPS : {fpsTState}",
                $"  I ajouter un point  ·  K annuler dernier  ·  L tout effacer  ·  G lancer/stopper",
                $"  Vitesse : {fpsTravellingSpeed:F1} u/s  (modifiable dans l'Inspector)"
            }), style);

            if (sacrificeZone != null && !_fpsTravellingActive)
            {
                if (GUI.Button(new Rect(10, 140, 260, 36), $"Téléporter à la zone de sacrifice ({teleportKey})"))
                    TeleportToSacrificeZone();
            }

            return;
        }

        // ── HUD caméra libre ──
        int totalPts = _travelPoints.Count;
        string tState = _travellingActive
            ? $"EN COURS  segment {_travelSegment + 1}/{totalPts - 1}  ({(_travelProgress * 100f):F0}%)"
            : "arrêté";
        string routine = routineManager != null
            ? $"Started={routineManager.IsStarted}  Interrupted={routineManager.IsInterrupted}"
            : "aucun RoutineManager assigné";

        GUI.Label(new Rect(10, 10, 700, 180), string.Join("\n", new[]
        {
            $"Déplacement : {moveSpeed:F1}  (Numpad +/-)    |  Travelling : {travellingSpeed:F1}  (Numpad * /)    |  Rotation : {arrowRotationSpeed:F0}°/s  (Numpad 8/2)",
            $"Travelling : {tState}  →  T lancer/stopper",
            $"  Points : {totalPts}  |  U ajouter  ·  Y annuler dernier  ·  O tout effacer",
            $"Presets : F1 drone · F3 reset    F2 → Mode FPS    H → cacher",
            $"Routine : {routine}",
            $"  F5 Start · F6 Stop · F7 Interrupt · F8 Resume"
        }), style);
    }
}