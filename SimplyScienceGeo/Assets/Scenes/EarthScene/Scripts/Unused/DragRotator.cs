using UnityEngine;

/// <summary>
/// Drag the mouse (or a finger on mobile) to rotate this object.
/// ● Horizontal drag  → Y-axis spin (left / right)
/// ● Vertical drag    → X-axis tilt (up / down)
/// Add a collider so OnMouse… callbacks fire.
/// </summary>
[RequireComponent(typeof(Collider))]
public class DragRotator : MonoBehaviour
{
    [Header("Tuning")]
    [Tooltip("Degrees per pixel of mouse movement")]
    public float sensitivity = 0.4f;

    [Tooltip("Invert horizontal drag (Mouse X)")]
    public bool invertHorizontal = false;

    [Tooltip("Invert vertical drag (Mouse Y)")]
    public bool invertVertical = true;

    [Tooltip("Enable left / right spin")]
    public bool allowHorizontal = true;

    [Tooltip("Enable up / down tilt")]
    public bool allowVertical = true;

    // ──────────────────────────────────────────────────────────────
    Vector3 _prevMousePos;

    void OnMouseDown()                // Fires when button is pressed over this collider
    {
        _prevMousePos = Input.mousePosition;
    }

    void OnMouseDrag()                // Fires every frame while the button is held
    {
        Vector3 delta = Input.mousePosition - _prevMousePos;
        _prevMousePos = Input.mousePosition;

        float dx = (invertHorizontal ? -delta.x : delta.x) * sensitivity;
        float dy = (invertVertical ? -delta.y : delta.y) * sensitivity;

        if (allowHorizontal)
            transform.Rotate(Vector3.up, dx, Space.World);   // spin around Y

        if (allowVertical)
            transform.Rotate(Vector3.right, dy, Space.World);  // tilt around X
    }
}
