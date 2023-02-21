using System;
using System.Collections;
using System.Collections.Generic;
using Nebula;

namespace Dark
{
    public interface IBiome
    {
        void BiomeGeneration(IWorldChunk Chunk, int seed);
    }
}

namespace Dark.World
{
    public abstract class BiomeDef : IBiome
    {
        public abstract void BiomeGeneration(IWorldChunk Chunk, int seed);
    }

    public class TemperateBiome : BiomeDef
    {
        public override void BiomeGeneration(IWorldChunk Chunk, int seed)
        {
            System.Random rand = new System.Random(seed);
            foreach (var tile in Chunk.GetTiles())
            {
                WorldPoint Point = tile.Coordinates;
                IEntity _entity;
                _entity = EntitySystem.Get.CreateEntity("entity.basicfloor");
                float r = rand.Next(1, 1000);
                //Debug.Log($"Generating tile {tile.Coordinates}::{tile.Container == null}");
                if (r < 10)
                {
                    IEntity _shrub = EntitySystem.Get.CreateEntity("entity.tree");
                    tile.Container.AddEntity(_shrub);
                }
                else if (r < 100)
                {
                    IEntity _shrub = EntitySystem.Get.CreateEntity("entity.shrub");
                    tile.Container.AddEntity(_shrub);
                }
                tile.Container.AddEntity(_entity);
            }
        }
    }
}
