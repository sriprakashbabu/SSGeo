using UnityEngine;

[System.Serializable]
public class ZoomPreset
{
    public string presetName;
    public Vector2 pan;
    public float zoom;
}

[CreateAssetMenu(fileName = "ZoomPresetCollection", menuName = "ImageZoomer/Zoom Preset Collection")]
public class ZoomPresetCollection : ScriptableObject
{
    public ZoomPreset[] presets;
}
