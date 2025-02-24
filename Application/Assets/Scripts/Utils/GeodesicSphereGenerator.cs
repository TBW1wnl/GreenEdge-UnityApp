using UnityEngine;
using System.Collections.Generic;

public class GeodesicSphereGenerator : MonoBehaviour
{
    public enum TerrainType
    {
        Water,
        Beach,
        Desert,
        Plains,
        Forest,
        City,
        Mountain
    }

    public static readonly Dictionary<TerrainType, Color> TerrainColors = new Dictionary<TerrainType, Color>
    {
        { TerrainType.Water, new Color(0.2f, 0.4f, 0.8f) },      // Blue
        { TerrainType.Beach, new Color(0.93f, 0.91f, 0.67f) },   // Beige
        { TerrainType.Desert, Color.yellow },                     // Yellow
        { TerrainType.Plains, new Color(0.6f, 0.8f, 0.2f) },     // Lime
        { TerrainType.Forest, new Color(0.5f, 0.6f, 0.1f) },     // Olive
        { TerrainType.City, Color.gray },                         // Grey
        { TerrainType.Mountain, new Color(0.5f, 0.35f, 0.2f) }   // Brown
    };

    [SerializeField] private int frequency = 4;
    [SerializeField] private float radius = 3f;
    [SerializeField] private bool randomizeTerrains = true;

    private List<Vector3> vertices = new List<Vector3>();
    private List<int> triangles = new List<int>();
    private Dictionary<long, int> middlePointCache = new Dictionary<long, int>();
    private List<HexTile> hexTiles = new List<HexTile>();

    [System.Serializable]
    public class HexTile
    {
        public Vector3 center;
        public List<Vector3> vertices;
        public List<HexTile> neighbors;
        public int tileIndex;
        public GameObject tileObject;
        public TerrainType terrain;
        public Material material;

        public HexTile(Vector3 center, List<Vector3> vertices, int index)
        {
            this.center = center;
            this.vertices = vertices;
            this.tileIndex = index;
            this.neighbors = new List<HexTile>();
            this.terrain = TerrainType.Plains;
        }
    }

    public class TileController : MonoBehaviour
    {
        private HexTile tileData;
        private Color baseColor;
        private Material tileMaterial;

        public void Initialize(HexTile tile)
        {
            tileData = tile;
            tileMaterial = tile.material;
            baseColor = TerrainColors[tile.terrain];
            tileMaterial.color = baseColor;
        }

        void OnMouseEnter()
        {
            if (tileMaterial != null)
            {
                tileMaterial.color = Color.Lerp(baseColor, Color.white, 0.3f);
            }
        }

        void OnMouseExit()
        {
            if (tileMaterial != null)
            {
                tileMaterial.color = baseColor;
            }
        }

        void OnMouseDown()
        {
            Debug.Log($"Tile {tileData.tileIndex} clicked! Terrain: {tileData.terrain}");
        }

        public void UpdateTerrainColor()
        {
            baseColor = TerrainColors[tileData.terrain];
            tileMaterial.color = baseColor;
        }
    }

    void Start()
    {
        GenerateGeodesicSphere();
        IdentifyHexTiles();
        if (randomizeTerrains)
        {
            GenerateRandomTerrain();
        }
        CreateTileObjects();
        CreateInnerSphere();
    }

    private void GenerateGeodesicSphere()
    {
        vertices.Clear();
        triangles.Clear();
        middlePointCache.Clear();

        // Create initial icosahedron
        float t = (1f + Mathf.Sqrt(5f)) / 2f;

        AddVertex(new Vector3(-1, t, 0));
        AddVertex(new Vector3(1, t, 0));
        AddVertex(new Vector3(-1, -t, 0));
        AddVertex(new Vector3(1, -t, 0));
        AddVertex(new Vector3(0, -1, t));
        AddVertex(new Vector3(0, 1, t));
        AddVertex(new Vector3(0, -1, -t));
        AddVertex(new Vector3(0, 1, -t));
        AddVertex(new Vector3(t, 0, -1));
        AddVertex(new Vector3(t, 0, 1));
        AddVertex(new Vector3(-t, 0, -1));
        AddVertex(new Vector3(-t, 0, 1));

        // Create initial triangles
        List<TriangleFace> faces = new List<TriangleFace>();

        // 5 faces around point 0
        faces.Add(new TriangleFace(0, 11, 5));
        faces.Add(new TriangleFace(0, 5, 1));
        faces.Add(new TriangleFace(0, 1, 7));
        faces.Add(new TriangleFace(0, 7, 10));
        faces.Add(new TriangleFace(0, 10, 11));

        // 5 adjacent faces
        faces.Add(new TriangleFace(1, 5, 9));
        faces.Add(new TriangleFace(5, 11, 4));
        faces.Add(new TriangleFace(11, 10, 2));
        faces.Add(new TriangleFace(10, 7, 6));
        faces.Add(new TriangleFace(7, 1, 8));

        // 5 faces around point 3
        faces.Add(new TriangleFace(3, 9, 4));
        faces.Add(new TriangleFace(3, 4, 2));
        faces.Add(new TriangleFace(3, 2, 6));
        faces.Add(new TriangleFace(3, 6, 8));
        faces.Add(new TriangleFace(3, 8, 9));

        // 5 adjacent faces
        faces.Add(new TriangleFace(4, 9, 5));
        faces.Add(new TriangleFace(2, 4, 11));
        faces.Add(new TriangleFace(6, 2, 10));
        faces.Add(new TriangleFace(8, 6, 7));
        faces.Add(new TriangleFace(9, 8, 1));

        // Subdivide faces
        for (int i = 0; i < frequency; i++)
        {
            List<TriangleFace> newFaces = new List<TriangleFace>();
            foreach (var tri in faces)
            {
                int a = GetMiddlePoint(tri.v1, tri.v2);
                int b = GetMiddlePoint(tri.v2, tri.v3);
                int c = GetMiddlePoint(tri.v3, tri.v1);

                newFaces.Add(new TriangleFace(tri.v1, a, c));
                newFaces.Add(new TriangleFace(tri.v2, b, a));
                newFaces.Add(new TriangleFace(tri.v3, c, b));
                newFaces.Add(new TriangleFace(a, b, c));
            }
            faces = newFaces;
        }

        // Create final mesh triangles
        foreach (var tri in faces)
        {
            triangles.Add(tri.v1);
            triangles.Add(tri.v2);
            triangles.Add(tri.v3);
        }
    }

    private int AddVertex(Vector3 p)
    {
        p = p.normalized * radius;
        vertices.Add(p);
        return vertices.Count - 1;
    }

    private int GetMiddlePoint(int p1, int p2)
    {
        long smaller = Mathf.Min(p1, p2);
        long greater = Mathf.Max(p1, p2);
        long key = (smaller << 32) + greater;

        if (middlePointCache.TryGetValue(key, out int ret))
        {
            return ret;
        }

        Vector3 point1 = vertices[p1];
        Vector3 point2 = vertices[p2];
        Vector3 middle = (point1 + point2) * 0.5f;
        middle = middle.normalized * radius;


        int i = AddVertex(middle);
        middlePointCache.Add(key, i);
        return i;
    }

    private class TriangleFace
    {
        public int v1;
        public int v2;
        public int v3;

        public TriangleFace(int v1, int v2, int v3)
        {
            this.v1 = v1;
            this.v2 = v2;
            this.v3 = v3;
        }
    }

    private void IdentifyHexTiles()
    {
        hexTiles.Clear();
        for (int i = 0; i < triangles.Count; i += 3)
        {
            Vector3 center = (vertices[triangles[i]] + vertices[triangles[i + 1]] + vertices[triangles[i + 2]]) / 3f;
            center = center.normalized * radius;

            List<Vector3> tileVertices = new List<Vector3>
            {
                vertices[triangles[i]],
                vertices[triangles[i + 1]],
                vertices[triangles[i + 2]]
            };

            HexTile newTile = new HexTile(center, tileVertices, hexTiles.Count);
            hexTiles.Add(newTile);
        }

        // Find neighbors
        for (int i = 0; i < hexTiles.Count; i++)
        {
            for (int j = i + 1; j < hexTiles.Count; j++)
            {
                if (AretilesAdjacent(hexTiles[i], hexTiles[j]))
                {
                    hexTiles[i].neighbors.Add(hexTiles[j]);
                    hexTiles[j].neighbors.Add(hexTiles[i]);
                }
            }
        }
    }

    private void GenerateRandomTerrain()
    {
        float offsetX = Random.Range(0f, 1000f);
        float offsetZ = Random.Range(0f, 1000f);

        foreach (HexTile tile in hexTiles)
        {
            float height = Mathf.PerlinNoise(
                tile.center.x * 2f + offsetX,
                tile.center.z * 2f + offsetZ
            );

            if (height < 0.3f)
                tile.terrain = TerrainType.Water;
            else if (height < 0.35f)
                tile.terrain = TerrainType.Beach;
            else if (height < 0.4f)
                tile.terrain = TerrainType.Plains;
            else if (height < 0.6f)
                tile.terrain = TerrainType.Forest;
            else if (height < 0.7f)
                tile.terrain = TerrainType.Desert;
            else
                tile.terrain = TerrainType.Mountain;

            if (Random.value < 0.05f && tile.terrain != TerrainType.Water)
            {
                tile.terrain = TerrainType.City;
            }
        }
    }

    private void CreateInnerSphere()
    {
        GameObject innerSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        innerSphere.transform.parent = this.transform;
        innerSphere.transform.position = transform.position;
        innerSphere.transform.localScale = Vector3.one * (radius * 3.96f);
        Material blackMat = new Material(Shader.Find("Unlit/Color"));
        blackMat.color = Color.black;
        blackMat.SetFloat("_Glossiness", 0f); // Supprime les reflets
        innerSphere.GetComponent<Renderer>().material = blackMat;

    }


    private void CreateTileObjects()
    {
        foreach (HexTile tile in hexTiles)
        {
            GameObject tileObj = new GameObject($"Tile_{tile.tileIndex}");
            tileObj.transform.parent = this.transform;
            tileObj.transform.position = transform.position + tile.center.normalized * radius;

            Mesh tileMesh = new Mesh();
            List<Vector3> tileVertices = new List<Vector3>();
            Vector3 centerPoint = tile.center;

            tileVertices.Add(centerPoint);
            foreach (Vector3 vertex in tile.vertices)
            {
                Vector3 direction = (vertex - centerPoint).normalized;
                float avgEdgeLength = (Vector3.Distance(tile.vertices[0], tile.vertices[1]) +
                       Vector3.Distance(tile.vertices[1], tile.vertices[2]) +
                       Vector3.Distance(tile.vertices[2], tile.vertices[0])) / 3f;

                float tileScaleFactor = avgEdgeLength * 1.15f;

                Vector3 shrunkVertex = centerPoint + direction * tileScaleFactor;


                tileVertices.Add(shrunkVertex);
            }

            List<int> tileTriangles = new List<int>();
            for (int i = 1; i < tileVertices.Count; i++)
            {
                int nextIndex = (i + 1 > tileVertices.Count - 1) ? 1 : i + 1;
                tileTriangles.Add(0);
                tileTriangles.Add(i);
                tileTriangles.Add(nextIndex);
            }

            tileMesh.vertices = tileVertices.ToArray();
            tileMesh.triangles = tileTriangles.ToArray();
            tileMesh.RecalculateNormals();
            tileMesh.RecalculateBounds();

            MeshFilter meshFilter = tileObj.AddComponent<MeshFilter>();
            meshFilter.mesh = tileMesh;

            MeshRenderer meshRenderer = tileObj.AddComponent<MeshRenderer>();
            Material material = new Material(Shader.Find("Unlit/Color"))
            {
                color = TerrainColors[tile.terrain]
            };
            meshRenderer.material = material;
            tile.material = material;

            MeshCollider collider = tileObj.AddComponent<MeshCollider>();
            collider.sharedMesh = tileMesh;

            TileController controller = tileObj.AddComponent<TileController>();
            controller.Initialize(tile);

            tile.tileObject = tileObj;
        }
    }

    private bool AretilesAdjacent(HexTile tile1, HexTile tile2)
    {
        foreach (Vector3 vertex1 in tile1.vertices)
        {
            foreach (Vector3 vertex2 in tile2.vertices)
            {
                if (Mathf.Approximately(Vector3.Distance(vertex1, vertex2), 0f))

                {
                    return true;
                }
            }
        }
        return false;
    }
}