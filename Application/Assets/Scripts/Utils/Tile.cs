using UnityEngine;

public class Tile : MonoBehaviour
{
    public GeodesicSphere.TerrainType terrainType;
    private Camera mainCamera;
    private Renderer tileRenderer;
    private Color originalColor;
    private Color hoverColor;

    void Start()
    {
        // Find the camera
        mainCamera = GameObject.Find("OrbitalCamera")?.GetComponent<Camera>();
        if (mainCamera == null)
        {
            Debug.LogError("OrbitalCamera not found! Make sure the name is correct.");
        }

        // Get Renderer and store the original color
        tileRenderer = GetComponent<Renderer>();
        if (tileRenderer != null)
        {
            originalColor = tileRenderer.material.color;
            hoverColor = originalColor * 1.3f; // Slightly brighter
        }
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && mainCamera != null)
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit) && hit.collider.gameObject == gameObject)
            {
                Debug.Log($"Clicked on tile! Terrain: {terrainType}");
            }
        }

        // Handle hover effect
        Ray hoverRay = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(hoverRay, out RaycastHit hoverHit))
        {
            if (hoverHit.collider.gameObject == gameObject)
            {
                SetHoverEffect(true);
            }
            else
            {
                SetHoverEffect(false);
            }
        }
        else
        {
            SetHoverEffect(false);
        }
    }

    void SetHoverEffect(bool isHovered)
    {
        if (tileRenderer != null)
        {
            tileRenderer.material.color = isHovered ? hoverColor : originalColor;
        }
    }
}
