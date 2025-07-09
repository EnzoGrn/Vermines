using UnityEngine;

public class DebugCube : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float rotationSpeed = 3f;

    float rotationX = 0f;
    float rotationY = 0f;

    void Start()
    {
        // Lock and hide the cursor when starting
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // Unlock the cursor when pressing Escape
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        // Move with WASD / Arrows
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");
        Vector3 move = new Vector3(moveX, 0, moveZ);
        transform.Translate(move * moveSpeed * Time.deltaTime, Space.Self);

        // Rotate with Mouse
        rotationX += Input.GetAxis("Mouse X") * rotationSpeed;
        rotationY -= Input.GetAxis("Mouse Y") * rotationSpeed;
        rotationY = Mathf.Clamp(rotationY, -80f, 80f);

        transform.rotation = Quaternion.Euler(rotationY, rotationX, 0f);
    }
}
