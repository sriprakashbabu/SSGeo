using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContinuousStream_V4 : MonoBehaviour
{
    // ... (All your variables are the same) ...
    public enum AnimationAxis { X, Y, Z }

    [Header("Object Settings")]
    public GameObject[] objectsToAnimate;

    [Header("Axis Settings")]
    public AnimationAxis animationAxis = AnimationAxis.X;
    public float startAxisValue;
    public float endAxisValue;

    [Header("Timing & Speed")]
    public float delayBetweenObjects = 0.5f;
    public float delayAtStart = 1.0f;
    public float moveSpeed = 5.0f;
    public float scaleSpeed = 2.0f;

    private List<Vector3> originalLocalPositions; // Renamed for clarity
    private List<Vector3> originalScales;
    private bool hasBeenInitialized = false;

    void OnEnable()
    {
        InitializeObjects();
        StartCoroutine(StartAnimationStream());
    }

    void OnDisable()
    {
        StopAllCoroutines();
    }

    void InitializeObjects()
    {
        if (hasBeenInitialized) return;

        originalLocalPositions = new List<Vector3>();
        originalScales = new List<Vector3>();

        foreach (GameObject obj in objectsToAnimate)
        {
            // *** KEY CHANGE ***
            // We read the LOCAL position, not the world position.
            Vector3 originalPos = obj.transform.localPosition;
            Vector3 originalScale = obj.transform.localScale;

            originalLocalPositions.Add(originalPos);
            originalScales.Add(originalScale);

            // *** KEY CHANGE ***
            // We set the LOCAL position to hide it.
            obj.transform.localPosition = GetPositionForAxis(originalPos, endAxisValue);
            obj.transform.localScale = Vector3.zero;
        }

        hasBeenInitialized = true;
    }

    IEnumerator StartAnimationStream()
    {
        foreach (GameObject obj in objectsToAnimate)
        {
            StartCoroutine(AnimateObjectLoop(obj.transform));
            yield return new WaitForSeconds(delayBetweenObjects);
        }
    }

    IEnumerator AnimateObjectLoop(Transform objTransform)
    {
        // *** KEY CHANGE ***
        int index = System.Array.IndexOf(objectsToAnimate, objTransform.gameObject);
        Vector3 originalPos = originalLocalPositions[index]; // Get original LOCAL pos
        Vector3 originalScale = originalScales[index];

        Vector3 loopStartPosition = GetPositionForAxis(originalPos, startAxisValue);
        Vector3 loopEndPosition = GetPositionForAxis(originalPos, endAxisValue);

        while (true)
        {
            // 1. Reset (using LOCAL position)
            objTransform.localPosition = loopStartPosition;
            objTransform.localScale = Vector3.zero;

            // 2. Scale up
            float scaleTimer = 0;
            float scaleDuration = 1.0f / scaleSpeed;
            if (scaleDuration < 0.01f) scaleDuration = 0.01f;

            while (scaleTimer < scaleDuration)
            {
                objTransform.localScale = Vector3.Lerp(Vector3.zero, originalScale, scaleTimer / scaleDuration);
                scaleTimer += Time.deltaTime;
                yield return null;
            }
            objTransform.localScale = originalScale;

            // 3. Wait
            yield return new WaitForSeconds(delayAtStart);

            // 4. Move (using LOCAL position)
            while (Vector3.Distance(objTransform.localPosition, loopEndPosition) > 0.01f)
            {
                // *** KEY CHANGE ***
                objTransform.localPosition = Vector3.MoveTowards(
                    objTransform.localPosition, // Move from current local
                    loopEndPosition,            // To target local
                    moveSpeed * Time.deltaTime
                );
                yield return null;
            }
            // *** KEY CHANGE ***
            objTransform.localPosition = loopEndPosition;
        }
    }

    // This helper function is already correct, as it just manipulates a Vector3
    Vector3 GetPositionForAxis(Vector3 originalPos, float axisValue)
    {
        Vector3 targetPos = originalPos; // Starts with original LOCAL pos
        switch (animationAxis)
        {
            case AnimationAxis.X: targetPos.x = axisValue; break;
            case AnimationAxis.Y: targetPos.y = axisValue; break;
            case AnimationAxis.Z: targetPos.z = axisValue; break;
        }
        return targetPos; // Returns a modified LOCAL pos
    }
}