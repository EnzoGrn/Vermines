using UnityEngine;

public class RotateObject : MonoBehaviour
{
    [SerializeField] private float rotationSpeed = 1f; // degrees per second
    [SerializeField] private Vector3 rotationAxis = Vector3.up;
    [SerializeField] private bool rotateClockwise = true;

    public bool rotate = true;

    void Start()
    {
        
    }

    void Update()
    {
        if (!rotate) return;

        transform.Rotate(rotationAxis, (rotateClockwise ? 1 : -1) * rotationSpeed * Time.deltaTime);
    }
}
