using UnityEngine;

namespace Entity
{
    public class TileEntity
    {
        public int CountryId { get; set; }
        public string CountryName { get; set; }
        public int Population { get; set; } = 0;
        public int Price { get; set; } = 0;

        public string TileName { get; set; }

        public GeodesicSphere.TerrainType TerrainType { get; set; }
        public Infrastructure RoadInfrastructure { get; set; } = new Infrastructure();
        public Infrastructure RailInfrastructure { get; set; } = new Infrastructure();
        public Infrastructure AirInfrastructure { get; set; } = new Infrastructure();
        public Infrastructure SeaInfrastructure { get; set; } = new Infrastructure();
    }
}
