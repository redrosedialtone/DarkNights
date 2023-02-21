using Dark.Entites;
using Dark.Entities;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Dark
{
    public interface ITileContainer : IEntityTriggerSystem, IEntityModuleSearch, IEntityTaskSystem
    {
        void AddEntity(IEntity Entity);
        void RemoveEntity(IEntity Entity);
        EntityID GetEntityID(IEntity Entity);
        IEntity GetEntity(EntityID EntityID);
        IEntity GetEntity(string EntityDefName);

        bool HasEntity(EntityID EntityID);

        IEntity[] Contents { get; }
        IEntity First { get; }
        IEnumerable<IEntity> TileEntities();
        IEntity[] EntitiesInLayer(EntityLayer Layer);
        IEnumerator<IEntity> GetEnumerator();
    }
}

namespace Dark.World { 
    /// <summary>
    /// Game Tile
    /// </summary>
    public class TileContainer : ITileContainer
    {
        private SortedSet<IEntity> SortedEntities;
        private readonly ITileData TileData;

        public TileContainer(ITileData TileData)
        {
            this.TileData = TileData;
        }

        public void AddEntity(IEntity Entity)
        {
            if (SortedEntities == null) { SortedEntities = new SortedSet<IEntity>(new SortByRenderOrder()); }
            SortedEntities.Add(Entity);

            Entity.Trigger(new EntityTrigger_OnTileEnter(TileData, this));
        }

        public void RemoveEntity(IEntity Entity)
        {
            SortedEntities.Remove(Entity);
            Entity.Trigger(new EntityTrigger_OnTileExit(TileData, this));
        }

        #region Events

        public void Trigger(IEntityTrigger Event)
        {
            foreach (var entity in SortedEntities)
            {
                entity.Trigger(Event);
            }
        }

        public void AssignTask(EntityTask Task)
        {
            foreach (var entity in TileEntities())
            {
                entity.AssignTask(Task);
            }
        }

        public ModuleType FindModule<ModuleType>() where ModuleType : IEntityModule, new()
        {
            if (SortedEntities != null)
            {
                foreach (var entity in SortedEntities)
                {
                    return entity.FindModule<ModuleType>();
                }
            }
            return default;
        }

        #endregion

        #region Helpers

        public EntityID GetEntityID(IEntity Entity)
        {
            foreach (var entity in TileEntities())
            {
                if (entity == Entity)
                {
                    return entity.ID;
                }
            }
            return default;
        }

        public IEntity GetEntity(EntityID ID)
        {
            foreach (var entity in TileEntities())
            {
                if (entity.ID == ID)
                {
                    return entity;
                }
            }
            return null;
        }

        public IEntity GetEntity(string EntityDefName)
        {
            foreach (var entity in SortedEntities)
            {
                if (entity.DefName == EntityDefName)
                {
                    return entity;
                }
            }
            return null;
        }

        public bool HasEntity(EntityID ID) => GetEntity(ID) != null;

        public IEntity First => SortedEntities.First();
        public IEntity[] Contents => SortedEntities.ToArray();
        public IEnumerable<IEntity> TileEntities()
        {
            if (SortedEntities != null)
            {
                foreach (var entity in SortedEntities)
                {
                    yield return entity;
                }
            }
        }

        public IEntity[] EntitiesInLayer(EntityLayer Layer)
        {
            List<IEntity> entities = new List<IEntity>();
            foreach (var entity in SortedEntities)
            {
                if (entity.EntityGraphicsDef != null && entity.EntityGraphicsDef.Layer == Layer)
                {
                    entities.Add(entity);
                }
            }
            return entities.ToArray();
        }

        public IEnumerator<IEntity> GetEnumerator()
        {
            foreach (var entity in SortedEntities)
            {
                yield return entity;
            }
        }

        public override int GetHashCode()
        {
            return SortedEntities.GetHashCode() ^ SortedEntities.Count;
        }

        #endregion
    }

    public class SortByRenderOrder : IComparer<IEntity>
    {
        public int Compare(IEntity x, IEntity y)
        {
            if (x == null || y == null)
            {
                return x == null ? -1 : 1;
            }
            
            if (x.EntityGraphicsDef != null && y.EntityGraphicsDef != null)
            {
                int xV = (x.EntityGraphicsDef.DrawPriority+1) * (int)x.EntityGraphicsDef.Layer;
                int yV = (y.EntityGraphicsDef.DrawPriority+1) * (int)y.EntityGraphicsDef.Layer;
                return yV.CompareTo(xV);
            }
            if (x.EntityGraphicsDef == null && y.EntityGraphicsDef == null)
            {
                return 0;
            }
            return x.EntityGraphicsDef == null ? -1 : 1;
        }
    }
}
