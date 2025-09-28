using UnityEngine;
using System.Collections.Generic;

public enum EarthSeason { March, June, September, December }

[System.Serializable]
public class SeasonVisuals
{
    [Header("Objects to Activate")]
    public GameObject[] objectsToActivate;

    [Tooltip("The final rotation of the Earth model when the simulation stops. Y is for spin, X is for tilt.")]
    [Header("Target Earth Rotation (Euler)")]
    public Vector3 targetRotation;
}

public class EarthSimulationController : MonoBehaviour
{
    [Header("Scene References")]
    public Transform sun;
    public Transform earth;
    public Light directionalLight;

    [Header("Simulation Settings")]
    public float orbitRadius = 10f;
    public float orbitSpeed = 10f;
    public float rotationSpeed = 50f;
    public float axialTilt = 23.5f;

    [Header("Lock Spin to Revolution")]
    public bool lockSpinToOrbit = true;
    public float spinsPerRevolution = 48f;

    [Header("Season Angles (0–360°)")]
    public float marchAngle = 0f;
    public float juneAngle = 90f;
    public float septemberAngle = 180f;
    public float decemberAngle = 270f;

    [Header("Travel Settings (to season)")]
    public float travelSpeed = 120f;

    [Header("Color Meshes & Settings")]
    public MeshRenderer[] northHemisphereMeshes;
    public MeshRenderer[] southHemisphereMeshes;
    public Color neutralColor = Color.white;
    public Color northColor = Color.yellow; // Summer color for North
    public Color southColor = Color.cyan;  // Summer color for South

    [Header("Season Visuals")]
    public GameObject[] defaultActiveObjects;
    public SeasonVisuals marchVisuals;
    public SeasonVisuals juneVisuals;
    public SeasonVisuals septemberVisuals;
    public SeasonVisuals decemberVisuals;

    // --- Internal state ---
    private float orbitAngleAbs = 0f;
    private float spinAngle = 0f;
    private float prevOrbitAngleAbs = 0f;
    private bool traveling = false;
    private bool paused = false;
    private float targetAngleAbs = 0f;
    private EarthSeason? targetSeason = null;

    void Update()
    {
        float dt = Time.deltaTime;
        prevOrbitAngleAbs = orbitAngleAbs;

        if (traveling)
        {
            // 1. Use the absolute travel speed multiplied by the direction
            float direction = Mathf.Sign(targetAngleAbs - orbitAngleAbs);
            orbitAngleAbs += Mathf.Abs(travelSpeed) * dt * direction; // FIX 1: Removed Mathf.Max and added directional logic

            // 2. Check if the current angle has passed the target angle
            if (direction > 0) // Moving forward
            {
                if (orbitAngleAbs >= targetAngleAbs - 0.01f) // FIX 2: Check for forward stop
                {
                    orbitAngleAbs = targetAngleAbs;
                    traveling = false;
                    paused = true;
                    SetSeasonVisuals(targetSeason);
                }
            }
            else // Moving backward (direction < 0)
            {
                if (orbitAngleAbs <= targetAngleAbs + 0.01f) // FIX 3: Check for backward stop
                {
                    orbitAngleAbs = targetAngleAbs;
                    traveling = false;
                    paused = true;
                    SetSeasonVisuals(targetSeason);
                }
            }
        }
        else if (!paused)
        {
            orbitAngleAbs += orbitSpeed * dt;
        }

        // Apply a smooth tilt/rotation
        if (!paused)
        {
            if (lockSpinToOrbit)
            {
                float dOrbit = orbitAngleAbs - prevOrbitAngleAbs;
                spinAngle += dOrbit * spinsPerRevolution;
            }
            else
            {
                spinAngle += rotationSpeed * dt;
            }

            // Apply tilt and spin only during movement
            Quaternion spin = Quaternion.Euler(0f, spinAngle, 0f);
            Quaternion tilt = Quaternion.Euler(axialTilt, 0f, 0f);
            earth.rotation = tilt * spin;
        }

        float orbitAngleWrapped = Mathf.Repeat(orbitAngleAbs, 360f);
        Vector3 offset = Quaternion.Euler(0f, orbitAngleWrapped, 0f) * Vector3.forward * orbitRadius;
        earth.position = sun.position + offset;

        if (directionalLight)
        {
            Vector3 dir = (earth.position - sun.position).normalized;
            directionalLight.transform.rotation = Quaternion.LookRotation(dir);
        }
    }

    // ORIGINAL (only calculates forward delta):
    // float deltaForward = Mathf.Repeat(targetWrapped - currentWrapped, 360f);

    // NEW LOGIC: Calculate both forward and backward path, and choose the shortest.
    // You must also determine the direction (forward or backward)
    public void SwitchSeason(EarthSeason season)
    {
        float targetWrapped = 0f;
        targetSeason = season;

        switch (season)
        {
            case EarthSeason.March: targetWrapped = marchAngle; break;
            case EarthSeason.June: targetWrapped = juneAngle; break;
            case EarthSeason.September: targetWrapped = septemberAngle; break;
            case EarthSeason.December: targetWrapped = decemberAngle; break;
        }

        float currentWrapped = Mathf.Repeat(orbitAngleAbs, 360f);

        // 1. Calculate the forward path (always positive)
        float deltaForward = Mathf.Repeat(targetWrapped - currentWrapped, 360f);

        // 2. Calculate the backward path (always positive distance)
        float deltaBackward = Mathf.Repeat(currentWrapped - targetWrapped, 360f);

        float travelDelta;

        // Choose the shortest path
        if (deltaForward <= deltaBackward)
        {
            travelDelta = deltaForward; // Go forward (positive rotation)
        }
        else
        {
            // Go backward (negative rotation). travelDelta becomes negative.
            travelDelta = -deltaBackward;
        }


        if (Mathf.Abs(travelDelta) < 0.01f)
        {
            traveling = false;
            paused = true;
            SetSeasonVisuals(targetSeason);
            return;
        }

        // travelDelta is now positive OR negative
        targetAngleAbs = orbitAngleAbs + travelDelta;
        traveling = true;
        paused = false;
    }

    public void BackToOrbit()
    {
        SetSeasonVisuals(null);
        traveling = false;
        paused = false;
    }

    private void SetSeasonVisuals(EarthSeason? season)
    {
        // Deactivate all season-specific objects and reset rotation
        SetActiveObjects(marchVisuals.objectsToActivate, false);
        SetActiveObjects(juneVisuals.objectsToActivate, false);
        SetActiveObjects(septemberVisuals.objectsToActivate, false);
        SetActiveObjects(decemberVisuals.objectsToActivate, false);

        // Reset colors to neutral for movement
        ApplyColorsToHemispheres(neutralColor, neutralColor);
        earth.rotation = Quaternion.identity;

        if (season == null)
        {
            SetActiveObjects(defaultActiveObjects, true);
        }
        else
        {
            SetActiveObjects(defaultActiveObjects, false);
            SeasonVisuals currentVisuals = null;

            switch (season)
            {
                case EarthSeason.March:
                    currentVisuals = marchVisuals;
                    ApplyColorsToHemispheres(neutralColor, neutralColor);
                    break;
                case EarthSeason.June:
                    currentVisuals = juneVisuals;
                    ApplyColorsToHemispheres(northColor, southColor);
                    break;
                case EarthSeason.September:
                    currentVisuals = septemberVisuals;
                    ApplyColorsToHemispheres(neutralColor, neutralColor);
                    break;
                case EarthSeason.December:
                    currentVisuals = decemberVisuals;
                    ApplyColorsToHemispheres(southColor, northColor);
                    break;
            }

            if (currentVisuals != null)
            {
                SetActiveObjects(currentVisuals.objectsToActivate, true);
                earth.rotation = Quaternion.Euler(currentVisuals.targetRotation);
            }
        }
    }

    private void ApplyColorsToHemispheres(Color north, Color south)
    {
        SetMeshColor(northHemisphereMeshes, north);
        SetMeshColor(southHemisphereMeshes, south);
    }

    private void SetMeshColor(MeshRenderer[] meshes, Color color)
    {
        foreach (var mesh in meshes)
        {
            if (mesh != null)
            {
                mesh.material.color = color;
            }
        }
    }

    private void SetActiveObjects(GameObject[] objects, bool active)
    {
        if (objects == null) return;
        foreach (var obj in objects)
        {
            if (obj != null)
                obj.SetActive(active);
        }
    }
}