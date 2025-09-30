using UnityEngine;
public class FeatureExtras : MonoBehaviour
{
    [Header("Objects to enable only when selected")]
    public GameObject[] objectsToEnable;

    [Header("Components to enable only when selected")]
    public Behaviour[] componentsToEnable;

    public void EnableExtras()
    {
        foreach (var obj in objectsToEnable)
            if (obj != null) obj.SetActive(true);

        foreach (var comp in componentsToEnable)
            if (comp != null) comp.enabled = true;
    }

    public void DisableExtras()
    {
        foreach (var obj in objectsToEnable)
            if (obj != null) obj.SetActive(false);

        foreach (var comp in componentsToEnable)
            if (comp != null) comp.enabled = false;
    }
}
