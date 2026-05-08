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
    //  TRAVELLING
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
    public float fpsHeight = 1.75f;   // hauteur des yeux du personnage
    public float fpsWalkSpeed = 3f;
    public float fpsMouseSensitivity = 2f;
    public float gravity = -9.81f;

    [Header("Bob caméra (marche)")]
    public float bobFrequency = 1.8f;    // cycles par seconde
    public float bobAmplitude = 0.04f;   // léger et discret
    public float bobSmoothing = 8f;

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

    // Travelling multi-points
    private struct TravelPoint
    {
        public Vector3 position;
        public Quaternion rotation;
    }

    private System.Collections.Generic.List<TravelPoint> _travelPoints = new System.Collections.Generic.List<TravelPoint>();
    private bool _travellingActive = false;
    private int _travelSegment = 0;   // index du segment en cours (entre point N et N+1)
    private float _travelProgress = 0f;  // progression sur le segment courant (0→1)

    // ─────────────────────────────────────────
    //  PRIVÉ — mode FPS
    // ─────────────────────────────────────────
    private bool _fpsMode = false;
    private CharacterController _cc;
    private Vector3 _fpsVelocity = Vector3.zero;  // gravité
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
        _cc.enabled = false;   // inactif par défaut (mode caméra libre)

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
        // Stoppe le travelling si en cours
        if (_travellingActive) StopTravelling();

        // Active le CharacterController et place la caméra à hauteur des yeux
        _cc.enabled = true;
        _cc.height = fpsHeight;
        _cc.center = new Vector3(0f, -0.8f, 0f);
        _cc.stepOffset = 0.35f;   // monte les marches jusqu'à ~35cm sans bloquer
        _cc.skinWidth = 0.08f;   // évite le collage contre les bords de marche
        _cc.radius = 0.3f;    // capsule assez fine pour les escaliers étroits
        _cc.slopeLimit = 89f; // permet de monter des pentes très raides (presque verticales)

        // Repose la caméra au sol sous sa position actuelle
        Vector3 pos = transform.position;
        pos.y = GetGroundY(pos);
        transform.position = pos;
        _targetPosition = pos;

        // Initialise la rotation FPS depuis la rotation courante
        _fpsYaw = transform.eulerAngles.y;
        _fpsPitch = 0f;

        // Verrouille le curseur
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        _bobTimer = 0f;
        _bobCurrentOffset = Vector3.zero;

        Debug.Log("Mode FPS activé.");
    }

    void ExitFPSMode()
    {
        _cc.enabled = false;

        // Libère le curseur
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Reprend le yaw/pitch depuis la rotation FPS
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
        HandleFPSRotation();
        HandleFPSMovement();
        HandleCameraBob();
    }

    void HandleFPSRotation()
    {
        _fpsYaw += Input.GetAxis("Mouse X") * fpsMouseSensitivity;
        _fpsPitch -= Input.GetAxis("Mouse Y") * fpsMouseSensitivity;
        _fpsPitch = Mathf.Clamp(_fpsPitch, -80f, 80f);

        // Applique la rotation sans le bob (le bob s'ajoute après)
        transform.rotation = Quaternion.Euler(_fpsPitch, _fpsYaw, 0f);
    }

    void HandleFPSMovement()
    {
        // Direction relative au yaw uniquement (pas de pitch)
        Quaternion horizontalRot = Quaternion.Euler(0f, _fpsYaw, 0f);
        Vector3 moveDir = Vector3.zero;

        if (Input.GetKey(KeyCode.W)) moveDir += horizontalRot * Vector3.forward;
        if (Input.GetKey(KeyCode.S)) moveDir += horizontalRot * Vector3.back;
        if (Input.GetKey(KeyCode.A)) moveDir += horizontalRot * Vector3.left;
        if (Input.GetKey(KeyCode.D)) moveDir += horizontalRot * Vector3.right;

        // Gravité
        if (_cc.isGrounded && _fpsVelocity.y < 0f)
            _fpsVelocity.y = -2f;

        _fpsVelocity.y += gravity * Time.deltaTime;

        Vector3 finalMove = moveDir * fpsWalkSpeed + Vector3.up * _fpsVelocity.y;
        _cc.Move(finalMove * Time.deltaTime);

        // Bob actif seulement si on se déplace et qu'on est au sol
        bool isWalking = moveDir.magnitude > 0.1f && _cc.isGrounded;
        UpdateBobTimer(isWalking);
    }

    void UpdateBobTimer(bool isWalking)
    {
        if (isWalking)
        {
            _bobTimer += Time.deltaTime * bobFrequency * Mathf.PI * 2f;
            _bobTargetOffset = new Vector3(
                Mathf.Sin(_bobTimer * 0.5f) * bobAmplitude * 0.5f,   // léger roulis horizontal
                Mathf.Sin(_bobTimer) * bobAmplitude,            // oscillation verticale
                0f
            );
        }
        else
        {
            _bobTimer = 0f;   // reset propre pour repartir du bas du cycle
            _bobTargetOffset = Vector3.zero;
        }
    }

    void HandleCameraBob()
    {
        // Lissage du bob pour éviter les à-coups
        _bobCurrentOffset = Vector3.Lerp(_bobCurrentOffset, _bobTargetOffset, Time.deltaTime * bobSmoothing);

        // Position finale = position CharacterController + offset du bob en espace local
        Vector3 basePos = transform.position;
        transform.position = basePos + transform.TransformDirection(_bobCurrentOffset);
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
        // Molette → rotation horizontale
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.01f)
            _targetYaw += scroll * scrollRotationSpeed;

        // Clic droit maintenu → rotation libre souris
        if (Input.GetMouseButton(1))
        {
            _targetYaw += Input.GetAxis("Mouse X") * mouseRotationSpeed;
            _targetPitch -= Input.GetAxis("Mouse Y") * mouseRotationSpeed;
        }

        // Flèches → rotation parfaite sur axe fixe, sans tremblement
        if (Input.GetKey(KeyCode.LeftArrow)) _targetYaw -= arrowRotationSpeed * Time.deltaTime;
        if (Input.GetKey(KeyCode.RightArrow)) _targetYaw += arrowRotationSpeed * Time.deltaTime;
        if (Input.GetKey(KeyCode.UpArrow)) _targetPitch -= arrowRotationSpeed * Time.deltaTime;
        if (Input.GetKey(KeyCode.DownArrow)) _targetPitch += arrowRotationSpeed * Time.deltaTime;

        _targetPitch = Mathf.Clamp(_targetPitch, -89f, 89f);

        // Lissage vers la cible
        _yaw = Mathf.Lerp(_yaw, _targetYaw, Time.deltaTime * arrowSmoothing);
        _pitch = Mathf.Lerp(_pitch, _targetPitch, Time.deltaTime * arrowSmoothing);

        transform.rotation = Quaternion.Euler(_pitch, _yaw, 0f);
    }

    // ═════════════════════════════════════════
    //  VITESSE
    // ═════════════════════════════════════════
    void HandleSpeedAdjustment()
    {
        // ── Numpad + / -  →  vitesse déplacement ZQSD ──
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

        // ── Numpad * / /  →  vitesse travelling ──
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

        // ── Numpad 8 / 2  →  vitesse rotation flèches ──
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
    //  TRAVELLING A → B
    // ═════════════════════════════════════════
    void HandleTravellingControls()
    {
        // U → ajouter un point à la séquence
        if (Input.GetKeyDown(KeyCode.U))
        {
            _travelPoints.Add(new TravelPoint
            {
                position = transform.position,
                rotation = transform.rotation
            });
            Debug.Log($"Travelling : point {_travelPoints.Count} posé → {transform.position}");
        }

        // Y → annuler le dernier point
        if (Input.GetKeyDown(KeyCode.Y) && _travelPoints.Count > 0)
        {
            _travelPoints.RemoveAt(_travelPoints.Count - 1);
            Debug.Log($"Travelling : dernier point supprimé ({_travelPoints.Count} restants)");
        }

        // O → vider toute la séquence
        if (Input.GetKeyDown(KeyCode.O))
        {
            _travelPoints.Clear();
            Debug.Log("Travelling : séquence réinitialisée.");
        }

        // T → lancer / stopper
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
        float total = n - 1; // nombre de segments

        // _travelProgress ici représente t global sur [0, n-1]
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

        // t global : 0 = premier point, n-1 = dernier point
        float tGlobal = _travelSegment + _travelProgress;

        // ── Position : Catmull-Rom ──
        int i = Mathf.Clamp(_travelSegment, 0, n - 1);
        float t = _travelProgress;

        // Points de contrôle avec clamp aux extrémités
        Vector3 p0 = _travelPoints[Mathf.Max(i - 1, 0)].position;
        Vector3 p1 = _travelPoints[i].position;
        Vector3 p2 = _travelPoints[Mathf.Min(i + 1, n - 1)].position;
        Vector3 p3 = _travelPoints[Mathf.Min(i + 2, n - 1)].position;

        transform.position = CatmullRom(p0, p1, p2, p3, t);

        // ── Rotation : Slerp avec correction du flip ──
        Quaternion rotFrom = _travelPoints[i].rotation;
        Quaternion rotTo = _travelPoints[Mathf.Min(i + 1, n - 1)].rotation;

        // Si le dot est négatif, les deux quaternions sont dans des hémisphères opposés
        // → on inverse rotTo pour que Slerp prenne le chemin court
        if (Quaternion.Dot(rotFrom, rotTo) < 0f)
            rotTo = new Quaternion(-rotTo.x, -rotTo.y, -rotTo.z, -rotTo.w);

        transform.rotation = Quaternion.Slerp(rotFrom, rotTo, t);
    }

    // Calcule un point sur une courbe Catmull-Rom entre p1 et p2
    // p0 et p3 sont les points de contrôle précédent/suivant
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
            GUI.Label(new Rect(10, 10, 400, 40),
                "MODE FPS  (F2 pour revenir en caméra libre)\nSouris = regarder · ZQSD = marcher",
                style);
            return;
        }

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