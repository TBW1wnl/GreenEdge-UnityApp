using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GeodesicSphere : MonoBehaviour
{
    public enum TerrainType
    {
        Water,
        Beach,
        Desert,
        Plains,
        Forest,
        Hills,
        City,
        Mountain,
        Cold
    }

    [SerializeField] private int subdivisions = 2;
    [SerializeField] private float radius = 5f;

    private Dictionary<int, List<int>> countries = new Dictionary<int, List<int>>();
    private Dictionary<int, int> tileToCountry = new Dictionary<int, int>();
    private Dictionary<TerrainType, Material> terrainMaterials = new Dictionary<TerrainType, Material>();
    private Dictionary<TerrainType, List<int>> terrainTriangles = new Dictionary<TerrainType, List<int>>();
    private Dictionary<TerrainType, Color> terrainColors = new Dictionary<TerrainType, Color>
    {
        { TerrainType.Water, new Color(0.2f, 0.4f, 0.8f) },      // Blue
        { TerrainType.Beach, new Color(0.93f, 0.91f, 0.67f) },   // Beige
        { TerrainType.Desert, Color.yellow },                    // Yellow
        { TerrainType.Plains, new Color(0.6f, 0.8f, 0.2f) },     // Lime
        { TerrainType.Forest, new Color(0.5f, 0.6f, 0.1f) },     // Olive
        { TerrainType.Hills, new Color(0.4f, 0.4f, 0f) },        // Dark Olive
        { TerrainType.City, Color.gray },                        // Grey
        { TerrainType.Mountain, new Color(0.5f, 0.35f, 0.2f) },  // Brown
        { TerrainType.Cold, Color.white }                        // White
    };

    private List<Vector3> vertices;
    private List<int> triangles;
    private Dictionary<int, TerrainType> vertexTerrainMap;

    void Start()
    {
        // Initialize terrain triangles lists
        foreach (TerrainType type in System.Enum.GetValues(typeof(TerrainType)))
        {
            terrainTriangles[type] = new List<int>();

            // Create material for each terrain type
            Material mat = new Material(Shader.Find("Unlit/Color"));
            if (mat == null)
            {
                Debug.LogError("Could not find shader 'Unlit/Color'");
                mat = new Material(Shader.Find("Default-Material"));
            }
            mat.color = terrainColors[type];
            terrainMaterials[type] = mat;
        }

        GenerateSphere();
        AssignBiomes();
        AssignCountries();
        CreateSubmeshes();
    }

    void GenerateSphere()
    {
        vertices = new List<Vector3>();
        triangles = new List<int>();
        vertexTerrainMap = new Dictionary<int, TerrainType>();

        // Create icosahedron (20-sided polyhedron) as the base
        CreateIcosahedron();

        // Subdivide the icosahedron faces
        for (int i = 0; i < subdivisions; i++)
        {
            SubdivideMesh();
        }

        // Project all vertices to the sphere
        for (int i = 0; i < vertices.Count; i++)
        {
            vertices[i] = vertices[i].normalized * radius;
            vertexTerrainMap[i] = TerrainType.Water;
        }
    }

    void CreateIcosahedron()
    {
        // Create the 12 vertices of the icosahedron
        float t = (1.0f + Mathf.Sqrt(5.0f)) / 2.0f;

        vertices.Add(new Vector3(-1, t, 0).normalized * radius);
        vertices.Add(new Vector3(1, t, 0).normalized * radius);
        vertices.Add(new Vector3(-1, -t, 0).normalized * radius);
        vertices.Add(new Vector3(1, -t, 0).normalized * radius);

        vertices.Add(new Vector3(0, -1, t).normalized * radius);
        vertices.Add(new Vector3(0, 1, t).normalized * radius);
        vertices.Add(new Vector3(0, -1, -t).normalized * radius);
        vertices.Add(new Vector3(0, 1, -t).normalized * radius);

        vertices.Add(new Vector3(t, 0, -1).normalized * radius);
        vertices.Add(new Vector3(t, 0, 1).normalized * radius);
        vertices.Add(new Vector3(-t, 0, -1).normalized * radius);
        vertices.Add(new Vector3(-t, 0, 1).normalized * radius);

        // Create the 20 triangles of the icosahedron
        // 5 faces around point 0
        triangles.Add(0); triangles.Add(11); triangles.Add(5);
        triangles.Add(0); triangles.Add(5); triangles.Add(1);
        triangles.Add(0); triangles.Add(1); triangles.Add(7);
        triangles.Add(0); triangles.Add(7); triangles.Add(10);
        triangles.Add(0); triangles.Add(10); triangles.Add(11);

        // 5 adjacent faces
        triangles.Add(1); triangles.Add(5); triangles.Add(9);
        triangles.Add(5); triangles.Add(11); triangles.Add(4);
        triangles.Add(11); triangles.Add(10); triangles.Add(2);
        triangles.Add(10); triangles.Add(7); triangles.Add(6);
        triangles.Add(7); triangles.Add(1); triangles.Add(8);

        // 5 faces around point 3
        triangles.Add(3); triangles.Add(9); triangles.Add(4);
        triangles.Add(3); triangles.Add(4); triangles.Add(2);
        triangles.Add(3); triangles.Add(2); triangles.Add(6);
        triangles.Add(3); triangles.Add(6); triangles.Add(8);
        triangles.Add(3); triangles.Add(8); triangles.Add(9);

        // 5 adjacent faces
        triangles.Add(4); triangles.Add(9); triangles.Add(5);
        triangles.Add(2); triangles.Add(4); triangles.Add(11);
        triangles.Add(6); triangles.Add(2); triangles.Add(10);
        triangles.Add(8); triangles.Add(6); triangles.Add(7);
        triangles.Add(9); triangles.Add(8); triangles.Add(1);
    }

    void SubdivideMesh()
    {
        List<Vector3> newVertices = new List<Vector3>();
        List<int> newTriangles = new List<int>();
        Dictionary<long, int> middlePointCache = new Dictionary<long, int>();

        // First, copy existing vertices
        for (int i = 0; i < vertices.Count; i++)
        {
            newVertices.Add(vertices[i]);
        }

        // Then iterate over all triangles and subdivide each into 4 triangles
        for (int i = 0; i < triangles.Count; i += 3)
        {
            int v1 = triangles[i];
            int v2 = triangles[i + 1];
            int v3 = triangles[i + 2];

            // Get the midpoints of each edge
            int a = GetMiddlePoint(v1, v2, middlePointCache, newVertices);
            int b = GetMiddlePoint(v2, v3, middlePointCache, newVertices);
            int c = GetMiddlePoint(v3, v1, middlePointCache, newVertices);

            // Create the 4 new triangles
            newTriangles.Add(v1); newTriangles.Add(a); newTriangles.Add(c);
            newTriangles.Add(v2); newTriangles.Add(b); newTriangles.Add(a);
            newTriangles.Add(v3); newTriangles.Add(c); newTriangles.Add(b);
            newTriangles.Add(a); newTriangles.Add(b); newTriangles.Add(c);
        }

        // Replace old vertices and triangles with new ones
        vertices = newVertices;
        triangles = newTriangles;
    }

    int GetMiddlePoint(int p1, int p2, Dictionary<long, int> cache, List<Vector3> vertices)
    {
        // First check if we already have it
        bool firstIsSmaller = p1 < p2;
        long smallerIndex = firstIsSmaller ? p1 : p2;
        long greaterIndex = firstIsSmaller ? p2 : p1;
        long key = (smallerIndex << 32) + greaterIndex;

        int ret;
        if (cache.TryGetValue(key, out ret))
        {
            return ret;
        }

        // Not in cache, calculate it
        Vector3 point1 = vertices[p1];
        Vector3 point2 = vertices[p2];
        Vector3 middle = (point1 + point2) / 2.0f;

        // Add vertex makes sure point is on unit sphere
        int i = vertices.Count;
        vertices.Add(middle.normalized * radius);

        // Store it, return index
        cache.Add(key, i);
        return i;
    }

    void AssignBiomes()
    {
        // ** Step 1: Initialize Water Map **
        for (int i = 0; i < vertices.Count; i++)
        {
            vertexTerrainMap[i] = TerrainType.Water;
        }

        // ** Step 2: Generate Snow at Poles **
        GenerateSnowCaps();

        // ** Step 3: Generate Landmasses **
        for (int i = 0; i < Random.Range(5, 10); i++)
        {
            int randomWaterTile = GetRandomTileOfType(TerrainType.Water);
            ExpandBiome(randomWaterTile, TerrainType.Plains, Random.Range(30, 100));
        }

        // ** Step 4: Convert Beaches **
        for (int i = 0; i < vertices.Count; i++)
        {
            if (vertexTerrainMap[i] != TerrainType.Water && vertexTerrainMap[i] != TerrainType.Cold)
            {
                foreach (int neighborIndex in GetVertexNeighbors(i))
                {
                    if (vertexTerrainMap[neighborIndex] == TerrainType.Water && Random.value < 0.95f)
                    {
                        vertexTerrainMap[i] = TerrainType.Beach;
                        break;
                    }
                }
            }
        }


        // ** Step 5: Generate Deserts **
        // not working currently
        //GenerateDeserts();

        GenerateForests();

        // ** Step 6: Generate Mountain Chains **
        for (int i = 0; i < vertices.Count; i++)
        {
            if (vertexTerrainMap[i] != TerrainType.Water &&
                vertexTerrainMap[i] != TerrainType.Beach &&
                vertexTerrainMap[i] != TerrainType.Cold &&
                Random.value < 0.01f)  // % chance per tile**
            {
                GenerateMountainChains();
            }
        }
        

        // ** Step 7: Scatter Cities (single-tile only) **
        for (int i = 0; i < vertices.Count; i++)
        {
            if (vertexTerrainMap[i] != TerrainType.Water &&
                vertexTerrainMap[i] != TerrainType.Beach &&
                vertexTerrainMap[i] != TerrainType.Mountain &&
                vertexTerrainMap[i] != TerrainType.Cold &&
                Random.value < 0.04f)  // % chance per tile**
            {
                vertexTerrainMap[i] = TerrainType.City;
            }
        }
    }

    void AssignCountries()
    {
        List<int> landTiles = vertexTerrainMap.Where(kv => kv.Value != TerrainType.Water).Select(kv => kv.Key).ToList();
        int numLandMasses = Mathf.Max(5, Mathf.FloorToInt(landTiles.Count / 100));
        int numCountries = Mathf.Clamp(numLandMasses * 3, 15, 30);

        List<int> countryCenters = landTiles.OrderBy(_ => Random.value).Take(numCountries).ToList();

        for (int i = 0; i < numCountries; i++)
        {
            countries[i] = new List<int>();
        }

        Queue<int> frontier = new Queue<int>();
        Dictionary<int, int> visited = new Dictionary<int, int>();

        foreach (int i in countryCenters)
        {
            frontier.Enqueue(i);
            visited[i] = countryCenters.IndexOf(i);
        }

        while (frontier.Count > 0)
        {
            int tile = frontier.Dequeue();
            int countryId = visited[tile];

            countries[countryId].Add(tile);
            tileToCountry[tile] = countryId;

            foreach (int neighbor in GetVertexNeighbors(tile))
            {
                if (!visited.ContainsKey(neighbor) && vertexTerrainMap[neighbor] != TerrainType.Water)
                {
                    visited[neighbor] = countryId;
                    frontier.Enqueue(neighbor);
                }
            }
        }
    }


    // ** Recursive Snow Expansion **
    void ExpandSnow(int tileIndex, float probability, int maxTiles)
    {
        if (maxTiles <= 0 || probability < Random.value) return;

        vertexTerrainMap[tileIndex] = TerrainType.Cold;
        maxTiles--;

        foreach (int neighbor in GetVertexNeighbors(tileIndex))
        {
            if (vertexTerrainMap[neighbor] != TerrainType.Cold)
            {
                ExpandSnow(neighbor, probability * 0.85f, maxTiles);
            }
        }
    }

    // ** Generates Snow Caps **
    void GenerateSnowCaps()
    {
        int snowLimit = vertices.Count / 12;

        foreach (int tile in GetNorthernmostTiles())
        {
            ExpandSnow(tile, 1f, snowLimit);
        }
        foreach (int tile in GetSouthernmostTiles())
        {
            ExpandSnow(tile, 1f, snowLimit);
        }
    }

    void ExpandBiome(int tileIndex, TerrainType biome, int remainingTiles)
    {
        if (remainingTiles <= 0)
        {
            return;
        }

        if (vertexTerrainMap[tileIndex] != TerrainType.Water)
        {
            return;
        }

        vertexTerrainMap[tileIndex] = biome;
        remainingTiles--;

        List<int> neighbors = GetVertexNeighbors(tileIndex);
        if (neighbors.Count == 0)
        {
            return;
        }

        // Shuffle neighbors for randomness
        neighbors = neighbors.OrderBy(n => Random.value).ToList();

        foreach (int neighbor in neighbors)
        {
            if (vertexTerrainMap[neighbor] == TerrainType.Water)
            {
                float expansionChance = Mathf.Clamp(1f - (remainingTiles / 150f), 0.5f, 0.9f);
                if (Random.value < expansionChance)
                {
                    ExpandBiome(neighbor, biome, remainingTiles);
                }
            }
        }
    }

    // ** Generate Mountain Chains with "L" or "U" Shape **
    void GenerateMountainChains()
    {
        int randomLandTile = GetRandomTileOfType(TerrainType.Plains, TerrainType.Forest);
        if (randomLandTile == -1)
        {
            return;
        }

        List<int> mountainChain = new List<int> { randomLandTile };
        int chainLength = Random.Range(5, 20);

        for (int i = 0; i < chainLength; i++)
        {
            int lastTile = mountainChain[mountainChain.Count - 1];
            List<int> neighbors = GetVertexNeighbors(lastTile);

            if (neighbors.Count == 0)
            {
                break;
            }

            int nextTile = neighbors[Random.Range(0, neighbors.Count)];
            if (!mountainChain.Contains(nextTile) && vertexTerrainMap[nextTile] != TerrainType.Water)
            {
                mountainChain.Add(nextTile);
            }
        }

        // **First, mark all as Hills**
        foreach (int tile in mountainChain)
        {
            vertexTerrainMap[tile] = TerrainType.Hills;
        }

        // **Now, upgrade some hills to mountains**
        UpgradeHillsToMountains();
    }

    void UpgradeHillsToMountains()
    {
        List<int> hillsToUpgrade = new List<int>();

        foreach (var tile in vertexTerrainMap.Where(t => t.Value == TerrainType.Hills).Select(t => t.Key))
        {
            List<int> neighbors = GetVertexNeighbors(tile);
            int totalNeighbors = neighbors.Count;
            int hillOrMountainCount = neighbors.Count(n => vertexTerrainMap[n] == TerrainType.Hills || vertexTerrainMap[n] == TerrainType.Mountain);

            // Upgrade if at least 2/3 of neighbors are hills/mountains
            if (totalNeighbors > 0 && (float)hillOrMountainCount / totalNeighbors >= 0.60f)
            {
                hillsToUpgrade.Add(tile);
            }
        }

        // Upgrade selected hills to mountains
        foreach (int tile in hillsToUpgrade)
        {
            vertexTerrainMap[tile] = TerrainType.Mountain;
        }
    }




    // ** Helper Functions **
    List<int> GetVertexNeighbors(int index)
    {
        List<int> neighbors = new List<int>();

        for (int i = 0; i < triangles.Count; i += 3)
        {
            if (triangles[i] == index || triangles[i + 1] == index || triangles[i + 2] == index)
            {
                neighbors.Add(triangles[i]);
                neighbors.Add(triangles[i + 1]);
                neighbors.Add(triangles[i + 2]);
            }
        }

        return neighbors.Distinct().Where(n => n != index).ToList();
    }


    int GetRandomTileOfType(params TerrainType[] types)
    {
        List<int> candidates = new List<int>();
        for (int i = 0; i < vertexTerrainMap.Count; i++)
        {
            if (types.Contains(vertexTerrainMap[i]))
            {
                candidates.Add(i);
            }
        }
        return (candidates.Count > 0) ? candidates[Random.Range(0, candidates.Count)] : -1;
    }

    List<int> GetNorthernmostTiles()
    {
        List<int> northernTiles = new List<int>();
        float maxY = float.MinValue;
        for (int i = 0; i < vertices.Count; i++)
        {
            if (vertices[i].y > maxY)
            {
                maxY = vertices[i].y;
                northernTiles.Clear();
                northernTiles.Add(i);
            }
            else if (Mathf.Abs(vertices[i].y - maxY) < 0.05f)
            {
                northernTiles.Add(i);
            }
        }
        return northernTiles;
    }

    List<int> GetSouthernmostTiles()
    {
        List<int> southernTiles = new List<int>();
        float minY = float.MaxValue;
        for (int i = 0; i < vertices.Count; i++)
        {
            if (vertices[i].y < minY)
            {
                minY = vertices[i].y;
                southernTiles.Clear();
                southernTiles.Add(i);
            }
            else if (Mathf.Abs(vertices[i].y - minY) < 0.05f)
            {
                southernTiles.Add(i);
            }
        }
        return southernTiles;
    }

    void GenerateDeserts()
    {
        int desertRegions = Random.Range(vertices.Count / 200, vertices.Count / 80);
        float maxY = radius;

        for (int j = 0; j < desertRegions; j++)
        {
            int startTile = GetRandomTileNearEquator();
            if (startTile == -1)
            {
                Debug.Log("Found no valid tiles for desert generation");
                continue;
            }

            int desertSize = Random.Range(20, 100);
            ExpandBiome(startTile, TerrainType.Desert, desertSize);
        }
    }

    void GenerateForests()
    {
        float smallForestChance = 0.18f;
        float LargeForestChance = 0.03f;

        for (int i = 0; i < vertices.Count; i++)
        {
            if (vertexTerrainMap[i] == TerrainType.Plains)
            {
                float chance = Random.value;

                if (chance < smallForestChance)
                {
                    vertexTerrainMap[i] = TerrainType.Forest;
                }
                else if (chance < smallForestChance + LargeForestChance)
                {
                    int forestSize = Random.Range(5, 25);
                    ExpandBiome(i, TerrainType.Forest, forestSize);
                }
            }
        }
    }



    // Helper function to find a good desert start tile near the equator
    int GetRandomTileNearEquator()
    {
        List<int> candidates = new List<int>();
        float maxY = radius;

        for (int i = 0; i < vertexTerrainMap.Count; i++)
        {
            if (vertexTerrainMap[i] == TerrainType.Plains) // Deserts replace plains
            {
                float normalizedLatitude = Mathf.Abs(vertices[i].y) / maxY;
                float equatorBias = Mathf.Lerp(1.0f, 0.0f, normalizedLatitude); // High at equator, low at poles

                if (Random.value < equatorBias) // Higher chance near equator
                {
                    candidates.Add(i);
                }
            }
        }

        return (candidates.Count > 0) ? candidates[Random.Range(0, candidates.Count)] : -1;
    }



    void CreateSubmeshes()
    {
        // Group triangles by terrain type
        for (int i = 0; i < triangles.Count; i += 3)
        {
            int v1 = triangles[i];
            int v2 = triangles[i + 1];
            int v3 = triangles[i + 2];

            // Get the dominant terrain type for this triangle
            Dictionary<TerrainType, int> typeCounts = new Dictionary<TerrainType, int>();

            // Count occurrences of each terrain type
            CountTerrainType(typeCounts, vertexTerrainMap[v1]);
            CountTerrainType(typeCounts, vertexTerrainMap[v2]);
            CountTerrainType(typeCounts, vertexTerrainMap[v3]);

            // Find the dominant type
            TerrainType dominantType = GetDominantType(typeCounts);

            // Add triangle to the appropriate terrain type
            terrainTriangles[dominantType].Add(v1);
            terrainTriangles[dominantType].Add(v2);
            terrainTriangles[dominantType].Add(v3);
        }

        // Create the main mesh
        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();

        // Set up submeshes
        mesh.subMeshCount = terrainTriangles.Count;
        int submeshIndex = 0;

        foreach (TerrainType type in System.Enum.GetValues(typeof(TerrainType)))
        {
            if (terrainTriangles[type].Count > 0)
            {
                mesh.SetTriangles(terrainTriangles[type].ToArray(), submeshIndex);
                submeshIndex++;
            }
        }

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        // Assign mesh to MeshFilter
        MeshFilter meshFilter = GetComponent<MeshFilter>();
        if (meshFilter == null)
        {
            meshFilter = gameObject.AddComponent<MeshFilter>();
        }
        meshFilter.mesh = mesh;

        // Create child objects for each terrain type
        CreateTerrainObjects();
    }

    void CountTerrainType(Dictionary<TerrainType, int> counts, TerrainType type)
    {
        if (counts.ContainsKey(type))
        {
            counts[type]++;
        }
        else
        {
            counts[type] = 1;
        }
    }

    TerrainType GetDominantType(Dictionary<TerrainType, int> counts)
    {
        TerrainType dominant = TerrainType.Water;
        int maxCount = 0;

        foreach (var pair in counts)
        {
            if (pair.Value > maxCount)
            {
                maxCount = pair.Value;
                dominant = pair.Key;
            }
        }

        return dominant;
    }

    void CreateTerrainObjects()
    {
        // First destroy any existing children
        while (transform.childCount > 0)
        {
            DestroyImmediate(transform.GetChild(0).gameObject);
        }

        // Get the submesh count
        MeshFilter meshFilter = GetComponent<MeshFilter>();
        int submeshCount = meshFilter.mesh.subMeshCount;
        foreach (TerrainType type in System.Enum.GetValues(typeof(TerrainType)))
        {
            if (terrainTriangles[type].Count > 0)
            {
                for (int i = 0; i < terrainTriangles[type].Count; i += 3)
                {
                    // Create a separate object for each triangle
                    GameObject tileObj = new GameObject("Tile_" + i / 3);
                    tileObj.transform.SetParent(transform, false);

                    // Add MeshFilter & MeshRenderer
                    MeshFilter filter = tileObj.AddComponent<MeshFilter>();
                    MeshRenderer renderer = tileObj.AddComponent<MeshRenderer>();
                    renderer.material = terrainMaterials[type];

                    // Create a new mesh for this tile
                    Mesh tileMesh = new Mesh();
                    tileMesh.vertices = new Vector3[]
                    {
                        vertices[terrainTriangles[type][i]],
                        vertices[terrainTriangles[type][i + 1]],
                        vertices[terrainTriangles[type][i + 2]]
                    };
                    tileMesh.triangles = new int[] { 0, 1, 2 };
                    tileMesh.RecalculateNormals();

                    filter.mesh = tileMesh;

                    // Add MeshCollider for raycasting
                    MeshCollider collider = tileObj.AddComponent<MeshCollider>();
                    collider.sharedMesh = tileMesh;
                    collider.convex = false;

                    // Attach the Tile script for interactivity
                    Tile tile = tileObj.AddComponent<Tile>();
                    tile.terrainType = type;
                    tile.countryId = tileToCountry.ContainsKey(terrainTriangles[type][i]) ? tileToCountry[terrainTriangles[type][i]] : -1;

                    AssignTileProprieties(tile);
                }
            }
        }
    }

    private void AssignTileProprieties(Tile tile)
    {
        if (tile.terrainType is TerrainType.Water)
        {
            return;
        }

        if (tile.terrainType is TerrainType.Beach)
        {
            tile.SeaInfrastructure.MaxLevel = Random.Range(3, 10);
            tile.SeaInfrastructure.Level = Random.Range(0, tile.SeaInfrastructure.MaxLevel);
        }
        else
        {
            tile.SeaInfrastructure.MaxLevel = 0;
            tile.SeaInfrastructure.Level = 0;
        }

        tile.RoadInfrastructure.MaxLevel = Random.Range(1, 10);
        tile.RoadInfrastructure.Level = Random.Range(1, tile.RoadInfrastructure.MaxLevel);

        tile.RailInfrastructure.MaxLevel = Random.Range(1, 10);
        tile.RailInfrastructure.Level = Random.Range(0, tile.RailInfrastructure.MaxLevel);

        tile.AirInfrastructure.MaxLevel = Random.Range(1, 10);
        tile.AirInfrastructure.Level = Random.Range(0, tile.AirInfrastructure.MaxLevel);

        if (tile.terrainType is TerrainType.City)
        {
            tile.population = Random.Range(10000, 1000000);
        }
        else
        {
            tile.population = Random.Range(1000, 100000);
        }
    }

    public Color GetTerrainColor(TerrainType type)
    {
        return terrainColors[type];
    }

    public int GetCountryCount()
    {
        return countries.Count;
    }

}