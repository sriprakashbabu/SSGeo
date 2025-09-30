using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateObject : MonoBehaviour
{
    public Vector3 Rotate_val; // Target rotation speed (multiplied by multiplier)
    public bool local = false; // Local or world space rotation
    public bool instantRotation = true; // Toggle between instant and gradual rotation
    public float acceleration = 5f; // Acceleration factor for gradual rotation
    public float multiplier = 200f; // Multiplication factor for speed (adjustable in Inspector)
    public Vector3 currentSpeed = Vector3.zero; // Current rotation speed
    private Vector3 targetSpeed; // Target rotation speed
    private bool isRotating = true; // Toggle rotation on/off

    private Transform objTransform;

    private void Start()
    {
        targetSpeed = Rotate_val * multiplier; // Cache the target speed, scaled by the multiplier
        objTransform = transform; // Cache transform for performance
    }

    private void OnEnable()
    {
        if (!instantRotation)
        {
            currentSpeed = Vector3.zero; // Reset current speed when re-enabling the GameObject
        }
    }

    void Update()
    {
        if (!isRotating) return; // Stop updating if rotation is disabled

        if (instantRotation)
        {
            // Instant rotation: Apply full speed immediately
            RotateGameObject(Rotate_val * multiplier);
        }
        else
        {
            // Gradual rotation: Accelerate to target speed
            currentSpeed = Vector3.MoveTowards(currentSpeed, targetSpeed, acceleration * multiplier * Time.deltaTime);
            RotateGameObject(currentSpeed);
        }
    }

    private void RotateGameObject(Vector3 rotationSpeed)
    {
        if (!local)
        {
            objTransform.Rotate(rotationSpeed * Time.deltaTime, Space.World);
        }
        else
        {
            objTransform.Rotate(rotationSpeed * Time.deltaTime, Space.Self);
        }
    }

    // Public method to toggle rotation on/off
    public void ToggleRotation()
    {
        isRotating = !isRotating;
    }

   
}

