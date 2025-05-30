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
    private Button terrainButton { get; set; }
    private Button countryButton { get; set; }
    private Button ownedButton { get; set; }

    private Button pause { get; set; }
    private Button speed0 { get; set; }
    private Button speed1 { get; set; }
    private Button speed2 { get; set; }


    private Button Menu { get; set; }

    private Label Funds;
    private Label Population;

    private GeodesicSphere geodesicSphere;
    private Dictionary<int, Color> countryColors = new Dictionary<int, Color>();
    private Dictionary<Tile, Color> originalTileColors = new Dictionary<Tile, Color>();

    // Make this static so Tile.cs can access the current view mode
    public static ViewMode CurrentViewMode { get; private set; } = ViewMode.Terrain;
    public static bool IsBuildMode { get; private set; } = false;

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
        ownedButton = root.Q<Button>("OwnedButton");

        Funds = root.Q<Label>("Funds");
        Population = root.Q<Label>("Population");

        pause = root.Q<Button>("pause");
        speed0 = root.Q<Button>("speed0");
        speed1 = root.Q<Button>("speed1");
        speed2 = root.Q<Button>("speed2");

        Menu = root.Q<Button>("Menu");

        // Check if buttons were found
        if (terrainButton is null || countryButton is null || ownedButton is null)
        {
            Debug.LogError("One or more UI Toolkit buttons not found! Make sure their IDs are TerrainButton and CountryButton in UXML.");
            return;
        }

        // Add click events
        terrainButton.clicked += () => ChangeViewMode(ViewMode.Terrain);
        countryButton.clicked += () => ChangeViewMode(ViewMode.Country);
        ownedButton.clicked += () => ProcessOwnedCities();

        pause.clicked += () => GameTimeManager.Instance.SetTimeSpeed(0);
        speed0.clicked += () => GameTimeManager.Instance.SetTimeSpeed(1);
        speed1.clicked += () => GameTimeManager.Instance.SetTimeSpeed(2);
        speed2.clicked += () => GameTimeManager.Instance.SetTimeSpeed(3);

        Menu.clicked += () => GameTimeManager.Instance.SetTimeSpeed(0);
        Menu.clicked += () => GameTimeManager.Instance.SetTimeSpeed(0);

        // Ensure AssignCountries() has been called before generating colors
        StartCoroutine(WaitForCountries());

        // Initialize with terrain view
        CurrentViewMode = ViewMode.Terrain;

        Debug.Log("UI Toolkit buttons found and events assigned!");
    }

    private void ProcessOwnedCities()
    {
        if (IsBuildMode)
        {
            HideOwnedCity();
        }
        else
        {
            ShowOwnedCity();
        }
    }

    private void ShowOwnedCity()
    {
        Tile[] tiles = Object.FindObjectsByType<Tile>(FindObjectsSortMode.None);
        foreach (Tile tile in tiles)
        {
            if (tile.tileData.IsBuild)
            {
                Renderer renderer = tile.GetComponent<Renderer>();
                if (renderer != null)
                {
                    // Store the current color before changing it, so we can reset later
                    originalTileColors[tile] = renderer.material.color;

                    // Set the new color to red
                    tile.SetBaseColor(Color.red);
                    renderer.material.color = Color.red;
                }
            }
        }
    }


    private void HideOwnedCity()
    {
        ChangeViewMode(CurrentViewMode);
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

        Population.text = WorldStateManager.Instance.WorldPopulation.ToString();
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
        //if (CurrentViewMode == mode) return;

        Debug.Log($"View mode changed: {mode}");
        CurrentViewMode = mode;

        Tile[] tiles = Object.FindObjectsByType<Tile>(FindObjectsSortMode.None);
        Debug.Log($"Found {tiles.Length} tiles to update");

        foreach (Tile tile in tiles)
        {
            Renderer renderer = tile.GetComponent<Renderer>();
            if (renderer == null) continue;

            if (mode == ViewMode.Terrain)
            {
                Color baseColor = geodesicSphere.GetTerrainColor(tile.tileData.TerrainType);
                tile.SetBaseColor(baseColor); // Tell the tile its base color has changed
                renderer.material.color = baseColor;
            }
            else if (mode == ViewMode.Country)
            {
                // Only change color if the tile has a valid country
                if (tile.tileData.CountryId >= 0 && countryColors.ContainsKey(tile.tileData.CountryId))
                {
                    Color countryColor = countryColors[tile.tileData.CountryId];
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