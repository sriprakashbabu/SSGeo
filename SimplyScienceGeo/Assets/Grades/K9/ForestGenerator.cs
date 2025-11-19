using UnityEngine;
using System.Collections.Generic; // Required for Lists

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteInEditMode]
public class ForestGenerator : MonoBehaviour
{
    public enum ForestType
    {
        Mangrove,
        Montane,
        TropicalThorn,
        TropicalDeciduous,
        TropicalEvergreen
    }

    [Header("Forest Settings")]
    public ForestType forestType;
    public Vector2 areaSize = new Vector2(100, 100);

    [Header("Counts")]
    public int treeCount = 300;
    public int stoneCount = 100;
    public int bushCount = 150;

    [Header("Spacing (Overlap Prevention)")]
    [Tooltip("Minimum distance between trees")]
    public float treeSpacing = 4.0f;
    [Tooltip("Minimum distance between stones")]
    public float stoneSpacing = 2.0f;
    [Tooltip("Minimum distance between bushes")]
    public float bushSpacing = 1.5f;
    [Tooltip("How many times to try finding a spot before giving up (prevents crashing)")]
    public int maxSpawnAttempts = 10;

    [Header("Randomization")]
    public float minScale = 0.8f;
    public float maxScale = 1.2f;
    [Tooltip("How much the object can tilt on the X/Z axis")]
    public float maxTiltAngle = 5f;

    [Header("Presets")]
    public ForestPreset mangrovePreset;
    public ForestPreset montanePreset;
    public ForestPreset thornPreset;
    public ForestPreset deciduousPreset;
    public ForestPreset evergreenPreset;

    [HideInInspector] public ForestPreset activePreset;
    public Transform container;

    // A list to keep track of where we have placed objects
    private List<Vector3> occupiedPositions = new List<Vector3>();

    public void SelectPreset()
    {
        switch (forestType)
        {
            case ForestType.Mangrove: activePreset = mangrovePreset; break;
            case ForestType.Montane: activePreset = montanePreset; break;
            case ForestType.TropicalThorn: activePreset = thornPreset; break;
            case ForestType.TropicalDeciduous: activePreset = deciduousPreset; break;
            case ForestType.TropicalEvergreen: activePreset = evergreenPreset; break;
        }
    }

    public void ClearForest()
    {
        if (container != null)
        {
            // Clean up children safely
            while (container.childCount > 0)
                DestroyImmediate(container.GetChild(0).gameObject);
        }
        // Clear our memory of positions
        occupiedPositions.Clear();
    }

    public void GenerateForest()
    {
        SelectPreset();

        if (activePreset == null)
        {
            Debug.LogError("No preset assigned for this forest type!");
            return;
        }

        if (container == null)
        {
            container = new GameObject("ForestContainer").transform;
            container.SetParent(transform);
        }

        ClearForest();

        // 1. Spawn Trees (Usually largest, spawn first)
        SpawnGroup(activePreset.treePrefabs, treeCount, treeSpacing);

        // 2. Spawn Stones
        SpawnGroup(activePreset.stonePrefabs, stoneCount, stoneSpacing);

        // 3. Spawn Bushes
        SpawnGroup(activePreset.bushPrefabs, bushCount, bushSpacing);
    }

    // Generic function to spawn a group of objects
    void SpawnGroup(GameObject[] prefabs, int count, float spacing)
    {
        if (prefabs == null || prefabs.Length == 0) return;

        for (int i = 0; i < count; i++)
        {
            Vector3 pos = Vector3.zero;
            bool validPositionFound = false;

            // Try to find a position that doesn't overlap
            for (int attempt = 0; attempt < maxSpawnAttempts; attempt++)
            {
                pos = GetRandomPosition();

                // Check this position against all previously spawned objects
                if (IsPositionValid(pos, spacing))
                {
                    validPositionFound = true;
                    break;
                }
            }

            // Only spawn if we found a valid spot
            if (validPositionFound)
            {
                SpawnObject(prefabs, pos);
                occupiedPositions.Add(pos);
            }
            else
            {
                Debug.LogWarning($"Could not find a spot for object {i} after {maxSpawnAttempts} attempts. Area might be too crowded.");
            }
        }
    }

    Vector3 GetRandomPosition()
    {
        return new Vector3(
            transform.position.x + Random.Range(-areaSize.x / 2, areaSize.x / 2),
            transform.position.y,
            transform.position.z + Random.Range(-areaSize.y / 2, areaSize.y / 2)
        );
    }

    // Returns true if the position is far enough away from other objects
    bool IsPositionValid(Vector3 candidatePos, float minDistance)
    {
        foreach (Vector3 occupied in occupiedPositions)
        {
            if (Vector3.Distance(candidatePos, occupied) < minDistance)
            {
                return false; // Too close!
            }
        }
        return true;
    }

    void SpawnObject(GameObject[] list, Vector3 pos)
    {
        GameObject prefab = list[Random.Range(0, list.Length)];
        GameObject obj;

#if UNITY_EDITOR
        obj = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
#else
        obj = Instantiate(prefab);
#endif

        obj.transform.position = pos;

        // Enhanced Random Rotation
        float yRot = Random.Range(0f, 360f);
        // Random slight tilt on X and Z for realism
        float tiltX = Random.Range(-maxTiltAngle, maxTiltAngle);
        float tiltZ = Random.Range(-maxTiltAngle, maxTiltAngle);

        obj.transform.rotation = Quaternion.Euler(tiltX, yRot, tiltZ);

        // Enhanced Random Scale
        float uniformScale = Random.Range(minScale, maxScale);

        // Add a tiny bit of non-uniformity (squashing/stretching)
        float scaleVarX = Random.Range(0.95f, 1.05f);
        float scaleVarZ = Random.Range(0.95f, 1.05f);

        obj.transform.localScale = new Vector3(
            uniformScale * scaleVarX,
            uniformScale,
            uniformScale * scaleVarZ
        );

        obj.transform.SetParent(container);
    }
}

[System.Serializable]
public class ForestPreset
{
    public string presetName;
    public GameObject[] treePrefabs;
    public GameObject[] stonePrefabs;
    public GameObject[] bushPrefabs;
}