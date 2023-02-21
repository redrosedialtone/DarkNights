using System;
using System.Collections;
using System.Collections.Generic;
using NLog;
using Nebula;

namespace Dark
{
    public enum CbTileState
    {
        OK,
        OutOfBounds
    }
}

namespace Dark.World
{
    /// <summary>
    /// Game World
    /// </summary>
    public class GameWorld
    {
        private static readonly NLog.Logger log = NLog.LogManager.GetLogger("[WORLD]");

        public static int WORLD_SEED;

        public float[] groundNoiseMap;
        public Vector2Int SizeMax;
        public Vector2Int SizeMin;
        public Vector2Int ChunkSize;

        private readonly Dictionary<int, IWorldChunk> WorldChunks;
        private Vector2Int _chunkSizeVector => new Vector2Int(WorldSystem.CHUNK_SIZE, WorldSystem.CHUNK_SIZE);

        public GameWorld(int seed, int width, int height)
        {
            float _halfWidth = width / 2;
            float _halfHeight = height / 2;
            WORLD_SEED = seed;
            WorldChunks = new Dictionary<int, IWorldChunk>();
            SizeMax = new Vector2Int((int)MathF.Ceiling(_halfWidth), (int)MathF.Ceiling(_halfHeight));
            SizeMin = new Vector2Int((int)MathF.Floor(0 - _halfWidth), (int)MathF.Floor(0 - _halfHeight));
            ChunkSize = new Vector2Int(width, height);
            log.Info($"> Generating world of Size {width * height}.. <   ");
            log.Trace($"MinSize::{SizeMin} MaxSize::{SizeMax}");
        }

        public void WorldCreation_CreateWorldChunks()
        {
            log.Info("Creating World Chunks...");
            for (int x = SizeMin.x; x <= SizeMax.x; x++)
            {
                for (int y = SizeMin.y; y <= SizeMax.y; y++)
                {
                    NewWorldChunk(new ChunkLocation(x, y));
                }
            }
        }

        public void WorldCreation_GenerateWorldChunks()
        {
            log.Info("Generating Tiles...");
            TemperateBiome biome = new TemperateBiome();
            //groundNoiseMap = NoiseMap.GenerateNoiseMap(SizeMax, 4, NoiseMap.GroundWave(seed));
            foreach (var chunk in WorldChunks)
            {
                ApplyBiome(chunk.Value, biome);
            }
            log.Info($"> Generated {WorldChunks.Count} Chunks from {SizeMin * _chunkSizeVector} to {SizeMax * _chunkSizeVector}<");
        }

        public IWorldChunk GenerateChunk(ChunkLocation chunkLoc)
        {
            log.Trace($"Generating Chunk {chunkLoc}");
            IWorldChunk newChunk = NewWorldChunk(chunkLoc);
            TemperateBiome biome = new TemperateBiome();
            ApplyBiome(newChunk, biome);
            return newChunk;
        }

        public IWorldChunk NewWorldChunk(ChunkLocation chunkLoc)
        {
            WorldPoint worldLoc = chunkLoc;
            log.Trace($"Creating Chunk{chunkLoc} At::{worldLoc}::{worldLoc.GetHashCode()}");
            IWorldChunk newChunk = ChunkObj(chunkLoc.X, chunkLoc.Y);
            WorldChunks.Add(worldLoc.GetHashCode(), newChunk);
            return newChunk;
        }

        private IWorldChunk ChunkObj(int X, int Y)
        {
            IWorldChunk Chunk = new WorldChunk(new ChunkLocation(X, Y), WorldSystem.CHUNK_SIZE);
            //this.Verbose($"Creating Chunk {Chunk.Coordinates} at {(WorldPoint)Chunk.Coordinates}");
            return Chunk;
        }

        public void ApplyBiome(IWorldChunk chunk, IBiome Biome)
        {
            foreach (var tile in chunk.GetTiles())
            {
                log.Trace($"Created Tile {tile.Coordinates} Meshes..");
                tile.NavData = new Dictionary<NavigationMode, ITileNavData> { { NavigationMode.Walking, new TileNav_Walkable() } };
                tile.VisibilityData = new TileVisibilityData(tile.Coordinates);
            }
            Biome.BiomeGeneration(chunk, WORLD_SEED + chunk.GetHashCode());
            NavigationSystem.QueueChunkNavData(chunk.Coordinates);
        }



        public IWorldChunk Chunk(ChunkLocation Coordinates, out CbTileState cbTileState)
        {
            if (WorldChunks.TryGetValue(Coordinates.GetHashCode(), out IWorldChunk val))
            {
                cbTileState = CbTileState.OK;
                return val;
            }
            cbTileState = CbTileState.OutOfBounds;
            return null;
        }

        public ITileData Tile(WorldPoint Coordinates, out CbTileState cbTileState)
        {
            var _chunk = Chunk(Coordinates, out cbTileState);
            if (cbTileState == CbTileState.OutOfBounds) return null;
            var _tile = _chunk.Tile(Coordinates, out cbTileState);
            return _tile;
        }

        public IWorldChunk ChunkUnsf(ChunkLocation Coordinates)
        {
            if (WorldChunks.TryGetValue(Coordinates.GetHashCode(), out IWorldChunk val))
            {
                return val;
            }
            log.Warn($"Could not find Chunk at::{Coordinates}");
            return null;
        }

        public ITileData TileUnsf(WorldPoint Coordinates)
        {
            var _chunk = ChunkUnsf(Coordinates);
            if (_chunk != null)
            {
                var _tile = _chunk.Tile(Coordinates, out CbTileState cbTileState);
                return _tile;
            }
            log.Warn($"Could not find Tile at::{Coordinates}");
            return null;

        }

        public ITileData TileUnsf(int X, int Y) =>
            TileUnsf(new WorldPoint(X, Y));

        #region Enumerations

        public IEnumerator<ITileData> GetEnumerator()
        {
            foreach (var tile in GetTiles())
            {
                yield return tile;
            }
        }

        public IEnumerable<WorldPoint> WorldCoordinates()
        {
            for (int x = SizeMin.x*WorldSystem.CHUNK_SIZE; x < SizeMax.x * WorldSystem.CHUNK_SIZE; x++)
            {
                for (int y = SizeMin.y * WorldSystem.CHUNK_SIZE; y < SizeMax.y * WorldSystem.CHUNK_SIZE; y++)
                {
                    yield return new WorldPoint(x, y);
                }
            }
        }

        public IEnumerable<IWorldChunk> Chunks()
        {
            foreach (var chunk in WorldChunks.Values)
            {
                yield return chunk;
            }
        }

        public IEnumerable<ITileData> GetTiles()
        {
            foreach (var chunk in Chunks())
            {
                foreach (var tile in chunk.GetTiles())
                {
                    yield return tile;
                }
            }
        }

        public IWorldChunk this[WorldPoint Coordinate]
        { get { return ChunkUnsf(Coordinate); } }

        #endregion
    }
}
