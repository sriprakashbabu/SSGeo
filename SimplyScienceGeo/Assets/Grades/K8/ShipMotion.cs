//using Debug = Utils.Logger; // You can keep this if you use it
using UnityEngine;

public class ShipMotion : MonoBehaviour
{
    public float bobbingSpeed = 0.18f;
    public float bobbingAmount = 0.5f;    // Vertical bobbing amplitude
    public float rollingAmount = 15.0f; // Horizontal rotation (rolling) amplitude

    // We now store the INITIAL LOCAL position and rotation
    private Vector3 initialLocalPosition;
    private Quaternion initialLocalRotation;

    void Start()
    {
        // Store the position and rotation RELATIVE TO THE PARENT
        initialLocalPosition = transform.localPosition;
        initialLocalRotation = transform.localRotation;
    }

    void Update()
    {
        float elapsedTime = Time.time;

        // --- Calculate vertical bobbing ---
        float verticalOffset = Mathf.Sin(elapsedTime * bobbingSpeed) * bobbingAmount;

        // Start from the initial LOCAL position and add the offset
        Vector3 newLocalPosition = initialLocalPosition + new Vector3(0.0f, verticalOffset, 0.0f);

        // --- Calculate horizontal rotation (rolling) ---
        float rollingOffset = Mathf.Sin(elapsedTime * bobbingSpeed) * rollingAmount;

        // Start from the initial LOCAL rotation and add the offset
        Quaternion newLocalRotation = initialLocalRotation * Quaternion.Euler(0.0f, 0.0f, rollingOffset);

        // --- Apply the motion ---
        // Set the LOCAL position and rotation
        transform.localPosition = newLocalPosition;
        transform.localRotation = newLocalRotation;
    }
}