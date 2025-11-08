using UnityEngine;

public class SimpleWind : MonoBehaviour
{
    // The maximum angle the plant will sway (in degrees)
    [Tooltip("Maximum rotation angle in degrees for the sway.")]
    public float maxSwayAngle = 2.0f;

    // How fast the plant sways back and forth
    [Tooltip("Speed of the swaying motion.")]
    public float swaySpeed = 1.5f;

    // A small offset value to make different plants sway slightly out of sync
    private float offset;

    private Quaternion originalRotation;

    void Start()
    {
        // Store the original rotation of the plant
        originalRotation = transform.rotation;

        // Generate a random offset so plants don't move in perfect sync
        offset = Random.Range(0f, 10f);
    }

    void Update()
    {
        // 1. Calculate the sway amount using a Sine wave.
        // The sine wave cycles smoothly between -1 and 1.
        // We multiply it by swaySpeed and add the offset.
        float swayValue = Mathf.Sin((Time.time * swaySpeed) + offset);

        // 2. Scale the value by the maxSwayAngle to get the final angle.
        float finalAngle = swayValue * maxSwayAngle;

        // 3. Create the new rotation.
        // We use the local X axis for a side-to-side (pitch) movement.
        // Quaternion.Euler creates a rotation from Euler angles (X, Y, Z).
        Quaternion windSway = Quaternion.Euler(finalAngle, 0f, 0f);

        // 4. Apply the rotation.
        // We combine the original rotation with the new wind sway rotation.
        transform.rotation = originalRotation * windSway;
    }
}