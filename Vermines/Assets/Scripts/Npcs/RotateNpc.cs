using UnityEngine;

public class RotateNpc : MonoBehaviour
{
    [SerializeField] private Camera _camera;
    
    private void LateUpdate()
    {
        Vector3 cameraPosition = _camera.transform.position;
        cameraPosition.y = gameObject.transform.position.y;

        gameObject.transform.LookAt(cameraPosition);
        transform.Rotate(0f, 180f, 0f);
    }
}
