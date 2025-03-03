using UnityEngine;

public class HoverPhaseLocation : MonoBehaviour
{
    private bool _Status = false;
    private BoxCollider _Collider;

    private void Awake()
    {
        _Collider = GetComponent<BoxCollider>();
    }

    private void FixedUpdate()
    {
        //if (_Status)
        //{
        //    DrawBoxColliderGizmo();
        //}
    }

    private void OnDrawGizmos()
    {
        if (_Status)
        {
            DrawBoxColliderGizmo();
        }
    }

    private void OnMouseEnter()
    {
        Debug.Log("OnMouseEnter");
        _Status = true;
    }

    private void OnMouseExit()
    {
        Debug.Log("OnMouseExit");
        _Status = false;
    }

    private void OnMouseDown()
    {
        Debug.Log("OnMouseDown Detected");
    }

    /// <summary>
    /// Draw a rectangle of the size of the Collider of the gameObject
    /// </summary>
    private void DrawBoxColliderGizmo()
    {
        if (_Collider == null) return;

        Gizmos.color = Color.green;
        Gizmos.matrix = transform.localToWorldMatrix; // Apply object transformations
        Gizmos.DrawWireCube(_Collider.center, _Collider.size);
    }
}
