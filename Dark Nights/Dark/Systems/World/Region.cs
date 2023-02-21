using System.Collections;
using System.Collections.Generic;
using Nebula;

namespace Dark
{
    public interface IWorldRegion
    {
        public WorldPoint Origin { get; }
        public ChunkLocation Location { get; }
        public ITileData[] Tiles { get; }
        public WorldPoint[] Boundaries { get; }

        public void SetRegionTiles(ITileData[] Tiles, WorldPoint[] Boundaries);
    }
}

namespace Dark.World
{
    public class WorldRegion : IWorldRegion
    {
        public WorldPoint Origin { get; private set; }
        public ChunkLocation Location => Origin;
        public ITileData[] Tiles { get; private set; }
        public WorldPoint[] Boundaries { get; private set; }

        public WorldRegion(WorldPoint Origin)
        {
            this.Origin = Origin;
        }

        public void SetRegionTiles(ITileData[] Tiles, WorldPoint[] Boundaries)
        {
            this.Tiles = Tiles; this.Boundaries = Boundaries;
        }
    }
}
