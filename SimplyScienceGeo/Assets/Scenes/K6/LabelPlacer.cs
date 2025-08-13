using UnityEngine;
using UnityEngine.UI;

public class LabelPlacer : MonoBehaviour
{
    [Header("Globe and Camera")]
    public GameObject globe;
    public Camera mainCamera;

    [Header("Label Prefabs")]
    public GameObject labelPrefab; // A UI Text or TextMeshPro prefab
    public GameObject linePrefab; // A prefab with a LineRenderer component

    [Header("Label Settings")]
    public string labelText = "Country";
    public Vector3 labelOffset = new Vector3(0, 20, 0);

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit))
            {
                if (hit.collider.gameObject == globe)
                {
                    PlaceLabel(hit.point, hit.normal);
                }
            }
        }
    }

    private void PlaceLabel(Vector3 position, Vector3 normal)
    {
        // Instantiate the label
        GameObject newLabel = Instantiate(labelPrefab, transform);
        newLabel.GetComponentInChildren<Text>().text = labelText;

        // Position the label in screen space
        Vector3 screenPos = mainCamera.WorldToScreenPoint(position);
        newLabel.transform.position = screenPos + labelOffset;

        // Instantiate and draw the line
        GameObject newLine = Instantiate(linePrefab, transform);
        LineRenderer lineRenderer = newLine.GetComponent<LineRenderer>();

        lineRenderer.SetPosition(0, position);
        lineRenderer.SetPosition(1, mainCamera.ScreenToWorldPoint(new Vector3(newLabel.transform.position.x, newLabel.transform.position.y, mainCamera.nearClipPlane + 1f)));
    }
}