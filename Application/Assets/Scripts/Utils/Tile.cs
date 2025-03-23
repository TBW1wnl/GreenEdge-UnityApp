using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Entity;

public class Tile : MonoBehaviour
{
    //public GeodesicSphere.TerrainType terrainType;
    //public string TileName = "no name";
    //public int countryId;
    //public int population = 0;

    //public Infrastructure RoadInfrastructure { get; set; } = new Infrastructure();
    //public Infrastructure RailInfrastructure { get; set; } = new Infrastructure();
    //public Infrastructure AirInfrastructure { get; set; } = new Infrastructure();
    //public Infrastructure SeaInfrastructure { get; set; } = new Infrastructure();

    public TileEntity tileData = new();

    private Camera mainCamera;
    public Renderer tileRenderer;
    private Color baseColor;
    private Color hoverColor;
    private bool isHovered = false;

    void Start()
    {
        mainCamera = GameObject.Find("OrbitalCamera")?.GetComponent<Camera>();
        if (mainCamera == null)
        {
            Debug.LogError("OrbitalCamera not found! Make sure the name is correct.");
        }

        tileRenderer = GetComponent<Renderer>();
        if (tileRenderer != null)
        {
            baseColor = tileRenderer.material.color;
            UpdateHoverColor();
        }
    }

    void Update()
    {
        if (ModalManager.isModalOpen)
        {
            return;
        }

        if (Input.GetMouseButtonDown(0) && mainCamera != null)
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit) && hit.collider.gameObject == gameObject)
            {
                OpenTilePopup();
            }
        }

        if (mainCamera == null) return;
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

    public void OpenTilePopup()
    {
        GameObject popupPrefab = TileManager.Instance.GetPopupPrefab();

        if (popupPrefab == null)
        {
            Debug.LogError("Popup prefab not available in TileManager!");
            return;
        }

        Canvas mainCanvas = FindAnyObjectByType<Canvas>();
        if (mainCanvas == null)
        {
            Debug.LogError("Canvas not found in the scene!");
            return;
        }

        GameObject popup = Instantiate(popupPrefab, mainCanvas.transform);
        ModalManager popupController = popup.GetComponent<ModalManager>();
        if (popupController != null)
        {
            popupController.Initialize(this);
        }
    }

    public void SetBaseColor(Color newBaseColor)
    {
        baseColor = newBaseColor;
        UpdateHoverColor();

        if (!isHovered && tileRenderer != null)
        {
            tileRenderer.material.color = baseColor;
        }
    }

    private void UpdateHoverColor()
    {
        hoverColor = baseColor * 1.3f;

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
