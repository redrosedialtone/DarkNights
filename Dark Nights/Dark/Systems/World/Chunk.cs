using System.Collections;
using System.Collections.Generic;
using Nebula;
using System;
using Dark.World;

namespace Dark
{
    public interface IWorldChunk : IWorldTick
    {
        ChunkLocation Coordinates { get; }
        ITileData[,] TileData { get; }
        IWorldRegion[] Regions { get; }
        IBiome Biome { get; }

        ITileData Tile(WorldPoint Coordinates, out CbTileState cbTileState);
        ITileData TileUnsf(int _x, int _y);
        IEnumerable<ITileData> GetTiles();

        void AddRegion(IWorldRegion region);
        void ClearRegions();
    }
}

namespace Dark.World { 
    /// <summary>
    /// Collection of Tiles
    /// </summary>
    public class WorldChunk : IWorldChunk
    {
        public ITileData[,] TileData { get; private set; }

        public ChunkLocation Coordinates => coordinates;
        private readonly ChunkLocation coordinates;
        private WorldPoint _worldPos => coordinates;
        public IWorldRegion[] Regions => pathfindingRegions.ToArray();
        private List<WorldRegion> pathfindingRegions;
        public IBiome Biome { get; }

        public WorldChunk(ChunkLocation Coordinates, int CHUNK_SIZE)
        {
            coordinates = Coordinates;
            GenerateChunk(CHUNK_SIZE);
        }

        private void GenerateChunk(int CHUNK_SIZE)
        {
            TileData = new ITileData[CHUNK_SIZE, CHUNK_SIZE];
            for (int x = 0; x < CHUNK_SIZE; x++)
            {
                for (int y = 0; y < CHUNK_SIZE; y++)
                {
                    WorldPoint coordinates = new WorldPoint(_worldPos.X + x, _worldPos.Y + y);
                    var _tile = new TileData(coordinates);
                    TileData[x, y] = new TileData(coordinates);
                    //Debug.Log($"Generated Tile At {coordinates}");
                }
                pathfindingRegions = new List<WorldRegion>() { new WorldRegion(_worldPos) };
            }
        }

        public IEnumerable<ITileData> GetTiles()
        {
            for (int x = 0; x < TileData.GetLength(0); x++)
            {
                for (int y = 0; y < TileData.GetLength(1); y++)
                {
                    yield return TileData[x, y];
                }
            }
        }

        public void AddRegion(IWorldRegion Region)
        {
            pathfindingRegions.Add(Region as WorldRegion);
        }

        public void ClearRegions()
        {
            pathfindingRegions = new List<WorldRegion>()
            {
                new WorldRegion(_worldPos)
            };
        }

        public void WorldTick(float delta)
        {
            
        }

        public ITileData Tile(WorldPoint Point, out CbTileState cbTileState)
        {
            int _x = (int)MathF.Abs(_worldPos.X - Point.X);
            int _y = (int)MathF.Abs(_worldPos.Y - Point.Y);
            if (_x < 0 || _x >= TileData.GetLength(0) || _y < 0 || _y >= TileData.GetLength(1))
            {
                cbTileState = CbTileState.OutOfBounds;
                return null;
            }
            cbTileState = CbTileState.OK;
            return TileUnsf(_x, _y);
        }

        public ITileData TileUnsf(int _x, int _y) => TileData[_x, _y];
    }
}
