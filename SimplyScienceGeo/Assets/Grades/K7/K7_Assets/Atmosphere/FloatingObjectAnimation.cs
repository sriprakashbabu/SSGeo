using UnityEngine;

public class FloatingObjectAnimation : MonoBehaviour
{

    // The base range for vertical movement. The actual range will be this value plus a random amount.
    public float baseVerticalRange = 0.5f;

    // The random variation for the vertical range.
    public float randomVerticalRange = 0.2f;

    // The base speed for vertical movement.
    public float baseVerticalSpeed = 1f;

    // The random variation for the vertical speed.
    public float randomVerticalSpeed = 0.5f;

    // The base speed for rotation.
    public float baseRotationSpeed = 15f;

    // The random variation for the rotation speed.
    public float randomRotationSpeed = 5f;

    private Vector3 initialPosition;

    private float currentVerticalRange;
    private float currentVerticalSpeed;
    private float currentRotationSpeed;

    void Start()
    {
        // Store the initial position.
        initialPosition = transform.position;

        // Assign a random value to the movement and rotation parameters.
        currentVerticalRange = baseVerticalRange + Random.Range(-randomVerticalRange, randomVerticalRange);
        currentVerticalSpeed = baseVerticalSpeed + Random.Range(-randomVerticalSpeed, randomVerticalSpeed);
        currentRotationSpeed = baseRotationSpeed + Random.Range(-randomRotationSpeed, randomRotationSpeed);
    }

    void Update()
    {
        // Use the randomized values for movement and rotation.
        float newY = initialPosition.y + Mathf.Sin(Time.time * currentVerticalSpeed) * currentVerticalRange;
        transform.position = new Vector3(initialPosition.x, newY, initialPosition.z);

        transform.Rotate(Vector3.up, currentRotationSpeed * Time.deltaTime);
    }
}