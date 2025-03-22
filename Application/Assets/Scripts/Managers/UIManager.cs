using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System.Collections;

public class UIManager : MonoBehaviour
{
    public enum ViewMode
    {
        Terrain,
        Country
    }

    public UIDocument uiDocument;
    private Button terrainButton;
    private Button countryButton;

    private Label Funds;
    private Label Population;

    private GeodesicSphere geodesicSphere;
    private ViewMode currentViewMode = ViewMode.Terrain;
    private Dictionary<int, Color> countryColors = new Dictionary<int, Color>();
    private Dictionary<Tile, Color> originalTileColors = new Dictionary<Tile, Color>();

    // Make this static so Tile.cs can access the current view mode
    public static ViewMode CurrentViewMode { get; private set; } = ViewMode.Terrain;

    void Start()
    {
        geodesicSphere = Object.FindFirstObjectByType<GeodesicSphere>();

        if (geodesicSphere == null)
        {
            Debug.LogError("GeodesicSphere not found in the scene!");
            return;
        }

        // Get UI root
        VisualElement root = uiDocument.rootVisualElement;

        // Find buttons by ID
        terrainButton = root.Q<Button>("TerrainButton");
        countryButton = root.Q<Button>("CountryButton");

        Funds = root.Q<Label>("Funds");
        Population = root.Q<Label>("Population");

        // Check if buttons were found
        if (terrainButton == null || countryButton == null)
        {
            Debug.LogError("One or more UI Toolkit buttons not found! Make sure their IDs are TerrainButton and CountryButton in UXML.");
            return;
        }

        // Add click events
        terrainButton.clicked += () => ChangeViewMode(ViewMode.Terrain);
        countryButton.clicked += () => ChangeViewMode(ViewMode.Country);

        // Ensure AssignCountries() has been called before generating colors
        StartCoroutine(WaitForCountries());

        // Initialize with terrain view
        CurrentViewMode = ViewMode.Terrain;

        Debug.Log("UI Toolkit buttons found and events assigned!");
    }

    // Wait until countries are assigned
    private IEnumerator WaitForCountries()
    {
        Debug.Log("Waiting for countries to be assigned...");

        // Wait until the countries are assigned
        while (geodesicSphere.GetCountryCount() == 0)
        {
            yield return null; // Wait for next frame
        }

        Debug.Log($"Countries assigned! Total: {geodesicSphere.GetCountryCount()}");
        GenerateCountryColors();

        // Store original colors after countries and colors are set up
        CacheOriginalTileColors();

        Population.text = geodesicSphere.Population.ToString();
    }

    // Cache original tile colors for later use
    private void CacheOriginalTileColors()
    {
        originalTileColors.Clear();
        Tile[] tiles = Object.FindObjectsByType<Tile>(FindObjectsSortMode.None);
        foreach (Tile tile in tiles)
        {
            Renderer renderer = tile.GetComponent<Renderer>();
            if (renderer != null)
            {
                originalTileColors[tile] = renderer.material.color;
                // Tell each tile what its base color is
                tile.SetBaseColor(renderer.material.color);
            }
        }
        Debug.Log($"Cached original colors for {originalTileColors.Count} tiles");
    }

    void ChangeViewMode(ViewMode mode)
    {
        if (currentViewMode == mode) return;

        Debug.Log($"View mode changed: {mode}");
        currentViewMode = mode;
        CurrentViewMode = mode; // Update static property too

        Tile[] tiles = Object.FindObjectsByType<Tile>(FindObjectsSortMode.None);
        Debug.Log($"Found {tiles.Length} tiles to update");

        foreach (Tile tile in tiles)
        {
            Renderer renderer = tile.GetComponent<Renderer>();
            if (renderer == null) continue;

            if (mode == ViewMode.Terrain)
            {
                Color baseColor = geodesicSphere.GetTerrainColor(tile.terrainType);
                tile.SetBaseColor(baseColor); // Tell the tile its base color has changed
                renderer.material.color = baseColor;
            }
            else if (mode == ViewMode.Country)
            {
                // Only change color if the tile has a valid country
                if (tile.countryId >= 0 && countryColors.ContainsKey(tile.countryId))
                {
                    Color countryColor = countryColors[tile.countryId];
                    tile.SetBaseColor(countryColor); // Tell the tile its base color has changed
                    renderer.material.color = countryColor;
                }
            }
        }
    }

    void GenerateCountryColors()
    {
        int countryCount = geodesicSphere.GetCountryCount();
        Debug.Log($"Total number of countries: {countryCount}");

        countryColors.Clear();
        for (int i = 0; i < countryCount; i++)
        {
            countryColors[i] = new Color(Random.value, Random.value, Random.value);
            Debug.Log($"Country {i} color: {countryColors[i]}");
        }
    }

    // Helper method to get country color (for Tile.cs to use)
    public Color GetCountryColor(int countryId)
    {
        if (countryColors.ContainsKey(countryId))
        {
            return countryColors[countryId];
        }
        return Color.black; // Default fallback
    }
}