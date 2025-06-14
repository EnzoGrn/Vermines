using UnityEngine;

public class CameraTravelling : MonoBehaviour
{
    #region Exposed Properties
    [SerializeField] private Transform startLookAt;
    [SerializeField] private Transform endLookAt;
    [SerializeField] private float maxUnzoomingZ;
    #endregion

    private Camera _camera;
    private bool _isAnimated;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _isAnimated = false;
        _camera = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        if (!_isAnimated && _camera != null)
        {
            _isAnimated = true;
            //_camera.transform.position = new Vector3(0, 10, -10); // Position initiale de la caméra
            //_camera.transform.LookAt(Vector3.zero); // Regarde vers l'origine
            //_camera.transform.Translate(Vector3.forward * 5, Space.Self); // Avance de 5 unités
        }
    }
}
