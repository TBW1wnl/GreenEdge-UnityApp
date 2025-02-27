using UnityEngine;

public class Tile : MonoBehaviour
{
    public GeodesicSphere.TerrainType terrainType;
    public int countryId;
    private Camera mainCamera;
    private Renderer tileRenderer;
    private Color baseColor; // Store the base color (terrain or country color)
    private Color hoverColor;
    private bool isHovered = false;

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
            baseColor = tileRenderer.material.color;
            UpdateHoverColor();
        }
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && mainCamera != null)
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit) && hit.collider.gameObject == gameObject)
            {
                Debug.Log($"Clicked on tile! Terrain: {terrainType}, Country ID: {countryId}");
            }
        }

        // Only handle hover if we have a camera
        if (mainCamera == null) return;

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

    // Called by UIManager to update the base color when view changes
    public void SetBaseColor(Color newBaseColor)
    {
        baseColor = newBaseColor;
        UpdateHoverColor();

        // If not currently hovered, update the actual display color
        if (!isHovered && tileRenderer != null)
        {
            tileRenderer.material.color = baseColor;
        }
    }

    private void UpdateHoverColor()
    {
        // Make hover color brighter than base color
        hoverColor = baseColor * 1.3f;

        // Ensure hover color doesn't exceed RGB(1,1,1)
        hoverColor.r = Mathf.Min(hoverColor.r, 1f);
        hoverColor.g = Mathf.Min(hoverColor.g, 1f);
        hoverColor.b = Mathf.Min(hoverColor.b, 1f);
    }

    void SetHoverEffect(bool isHovered)
    {
        this.isHovered = isHovered;

        if (tileRenderer != null)
        {
            tileRenderer.material.color = isHovered ? hoverColor : baseColor;
        }
    }
}