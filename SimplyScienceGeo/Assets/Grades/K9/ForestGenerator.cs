
using UnityEngine;
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

    public ForestType forestType;
    public Vector2 areaSize = new Vector2(100, 100);

    public int treeCount = 300;
    public int stoneCount = 100;
    public int bushCount = 150;

    public ForestPreset mangrovePreset;
    public ForestPreset montanePreset;
    public ForestPreset thornPreset;
    public ForestPreset deciduousPreset;
    public ForestPreset evergreenPreset;

    [HideInInspector] public ForestPreset activePreset;

    public Transform container;  // All generated objects go here

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
            while (container.childCount > 0)
                DestroyImmediate(container.GetChild(0).gameObject);
        }
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

        // Trees
        for (int i = 0; i < treeCount; i++)
            SpawnRandom(activePreset.treePrefabs);

        // Stones
        for (int i = 0; i < stoneCount; i++)
            SpawnRandom(activePreset.stonePrefabs);

        // Bushes
        for (int i = 0; i < bushCount; i++)
            SpawnRandom(activePreset.bushPrefabs);
    }

    void SpawnRandom(GameObject[] list)
    {
        if (list == null || list.Length == 0) return;

        Vector3 pos = new Vector3(
            transform.position.x + Random.Range(-areaSize.x / 2, areaSize.x / 2),
            transform.position.y,
            transform.position.z + Random.Range(-areaSize.y / 2, areaSize.y / 2)
        );

        GameObject prefab = list[Random.Range(0, list.Length)];

        GameObject obj;

        // Use PrefabUtility only in EDITOR mode
#if UNITY_EDITOR
        obj = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
#else
        obj = Instantiate(prefab);
#endif

        obj.transform.position = pos;

        // Rotation variation
        float yRot = Random.Range(0f, 360f);
        float tiltX = Random.Range(-5f, 5f);
        float tiltZ = Random.Range(-5f, 5f);
        obj.transform.rotation = Quaternion.Euler(tiltX, yRot, tiltZ);

        // Scale variation
        float baseScale = Random.Range(0.85f, 1.25f);
        float scaleX = baseScale * Random.Range(0.95f, 1.05f);
        float scaleZ = baseScale * Random.Range(0.95f, 1.05f);

        obj.transform.localScale = new Vector3(scaleX, baseScale, scaleZ);

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
