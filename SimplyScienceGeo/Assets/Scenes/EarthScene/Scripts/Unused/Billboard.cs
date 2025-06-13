using UnityEngine;

[ExecuteAlways]
public class Billboard : MonoBehaviour
{
    Camera _cam;

    void OnEnable()
    {
        // grab the main camera (falls back if none assigned)
        if (_cam == null) _cam = Camera.main;
    }

    void LateUpdate()
    {
        if (_cam == null)
        {
            _cam = Camera.main;
            if (_cam == null) return;
        }

        // --- THE FIX ---
        // By swapping the two positions, we get the opposite vector,
        // which correctly points the object's -Z axis toward the camera.
        Vector3 dirFromCam = transform.position - _cam.transform.position;

        // The rest is the same, but we use the new vector
        Quaternion lookRot = Quaternion.LookRotation(dirFromCam, Vector3.up);
        transform.rotation = lookRot;
    }
}
