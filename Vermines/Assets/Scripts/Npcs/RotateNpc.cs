using UnityEngine;

public class RotateNpc : MonoBehaviour
{
    [SerializeField] private Camera _camera;

    // https://youtube.com/shorts/KGG2V4ZkXTg?si=TnOfEIoMn83hSeWV

    private void LateUpdate()
    {
        Vector3 cameraPosition = _camera.transform.position;
        cameraPosition.y = gameObject.transform.position.y;

        gameObject.transform.LookAt(cameraPosition);
        transform.Rotate(0f, 180f, 0f);
    }
}
