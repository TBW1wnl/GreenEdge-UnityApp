using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Text;
using static GeodesicSphere;

public class SphereExporter : MonoBehaviour
{
    public GeodesicSphere sphere;

    public string ExportToJson()
    {
        if (sphere == null)
        {
            Debug.LogError("Sphere reference is null.");
            return "{}";
        }

        StringBuilder jsonBuilder = new StringBuilder();
        jsonBuilder.Append("{ \"tiles\": [");

        List<Vector3> vertices = sphere.vertices;
        Dictionary<int, TerrainType> vertexTerrainMap = sphere.vertexTerrainMap;
        Dictionary<int, int> tileToCountry = sphere.tileToCountry;

        for (int i = 0; i < vertices.Count; i++)
        {
            TerrainType terrain = vertexTerrainMap.ContainsKey(i) ? vertexTerrainMap[i] : TerrainType.Ocean;
            int countryId = tileToCountry.ContainsKey(i) ? tileToCountry[i] : -1;

            jsonBuilder.AppendFormat(
                "{{ \"id\": {0}, \"position\": {{ \"x\": {1}, \"y\": {2}, \"z\": {3} }}, \"terrainType\": \"{4}\", \"countryId\": {5}, \"infrastructure\": {{ \"sea\": {{ \"level\": 0, \"maxLevel\": 0 }}, \"road\": {{ \"level\": 0, \"maxLevel\": 0 }}, \"rail\": {{ \"level\": 0, \"maxLevel\": 0 }}, \"air\": {{ \"level\": 0, \"maxLevel\": 0 }} }}, \"population\": 0 }}",
                i,
                vertices[i].x, vertices[i].y, vertices[i].z,
                terrain.ToString(),
                countryId
            );

            if (i < vertices.Count - 1)
            {
                jsonBuilder.Append(", ");
            }
        }

        jsonBuilder.Append("] }");
        return jsonBuilder.ToString();
    }
}

/*
{
    "tiles": [
      {
        "id": 0,
      "position": { "x": 1.23, "y": 4.56, "z": 7.89 },
      "terrainType": "Plains",
      "countryId": 3,
      "infrastructure": {
            "sea": { "level": 2, "maxLevel": 5 },
        "road": { "level": 4, "maxLevel": 8 },
        "rail": { "level": 1, "maxLevel": 6 },
        "air": { "level": 0, "maxLevel": 4 }
        },
      "population": 5000
      }
  ]
}
*/