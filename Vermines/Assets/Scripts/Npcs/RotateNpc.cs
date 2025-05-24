using UnityEngine;

public class RotateNpc : MonoBehaviour
{
    private NpcController _npcController;

    // https://youtube.com/shorts/KGG2V4ZkXTg?si=TnOfEIoMn83hSeWV

    private void Start()
    {
        _npcController = GetComponent<NpcController>();
        if (_npcController == null)
        {
            Debug.LogError("NpcController component not found on the GameObject.");
            return;
        }
    }

    private void LateUpdate()
    {
        if (_npcController.Camera == null)
            return;

        Vector3 cameraPosition = _npcController.Camera.transform.position;
        cameraPosition.y = gameObject.transform.position.y;

        gameObject.transform.LookAt(cameraPosition);
        transform.Rotate(0f, 180f, 0f);
    }
}
