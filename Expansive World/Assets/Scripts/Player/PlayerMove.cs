using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    // Serialized Fields \\
    [Header("Components")]
    [SerializeField] private Camera cam;
    [SerializeField] private Transform chassis;

    [Header("Settings")]
    [SerializeField] private float dragThreshold = 10f;

    [Header("Keybinds")]
    [SerializeField] private KeyCode dragKey = KeyCode.Mouse0;

    // Private Fields \\
    private bool isDragLocked = false;

    private float dragHeight = 0f;

    private Vector2 lastMousePos = Vector2.zero;

    // Private Methods \\
    private void LateUpdate()
    {
        DragCycle();
    }

    private void DragCycle()
    {
        if (isDragLocked) { return; }
        if (!Input.GetKey(dragKey)) { return; }

        Vector2 mousePos = Input.mousePosition;

        if (Input.GetKeyDown(dragKey))
        {
            lastMousePos = mousePos;

            dragHeight = GetDragHeightFromScreen(mousePos);
        }

        if (mousePos == lastMousePos) { return; }

        Vector3 currentWorldPoint = GetWorldPointFromScreen(mousePos, dragHeight);
        Vector3 lastWorldPoint = GetWorldPointFromScreen(lastMousePos, dragHeight);

        Vector3 difference = currentWorldPoint - lastWorldPoint;
        difference.y = 0f;

        transform.position = transform.position - difference;

        lastMousePos = mousePos;
    }

    private Vector3 GetWorldPointFromScreen(Vector2 _screenPos, float _dragHeight)
    {
        Vector3 worldPoint = Vector3.zero;

        Ray ray = cam.ScreenPointToRay(_screenPos);
        Plane plane = new Plane(Vector3.up, Vector3.up * _dragHeight);

        if(plane.Raycast(ray, out float distance))
        {
            worldPoint = ray.GetPoint(distance);
        }

        return worldPoint;
    }

    private float GetDragHeightFromScreen(Vector2 _screenPos)
    {
        float height = 0f;

        Ray ray = cam.ScreenPointToRay(_screenPos);
        RaycastHit hit;

        if(Physics.Raycast(ray, out hit))
        {
            height = hit.point.y;
        }

        return height;
    }

    // Public Methods \\
    public void SetDragLocked(bool _isLocked)
    {
        isDragLocked = _isLocked;
    }
}
