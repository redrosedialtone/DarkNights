using System.Collections;
using System.Collections.Generic;
using System;
using Nebula.Systems;
using NLog;
using Dark.Rendering;

namespace Dark
{
    public class EntitySystem : Manager
    {
        #region Static
        private static EntitySystem instance;
        public static EntitySystem Get => instance;

        private static readonly NLog.Logger log = NLog.LogManager.GetLogger("ENTITY");
        #endregion

        private int entityCount = 0;

        public override void Init()
        {
            log.Info("> Entity System Init.. <");
            instance = this;
            base.Init();
        }

        #region Entity Manipulation

        public static IEntity GetDef(string DefName)
        {
            var entityDefs = LoadingSystem.Get.EntityDefs;
            if (entityDefs.ContainsKey(DefName))
            {
                return entityDefs[DefName];
            }
            return null;
        }

        public bool PlaceEntity(IEntity EntityData, ITileData TileData)
        {
            TileData.Container.AddEntity(EntityData);
            //WorldRenderer.SetTileDirty(TileData);
            NavigationSystem.InvalidateNavData(TileData);
            WorldSystem.InvalidateChunk(TileData.Coordinates);
            return true;
        }

        public bool RemoveEntity(IEntity EntityData, ITileData TileData)
        {
            TileData.Container.RemoveEntity(EntityData);
            //WorldRenderer.SetTileDirty(TileData);
            NavigationSystem.InvalidateNavData(TileData);
            WorldSystem.InvalidateChunk(TileData.Coordinates);
            return true;
        }

        public IEntity CreateEntity(string defName)
        {
            log.Debug($"Creating {defName}...");
            IEntity entityDef = GetDef(defName);
            if (entityDef != null)
            {
                var _newEntity = (IEntity)Activator.CreateInstance(entityDef.GetType(), new object[] { });
                _newEntity.ID = new EntityID(entityCount);
                entityCount++;
                return _newEntity;
            }
            return null;
        }

        #endregion

        public static void InvalidateEntity(IEntity Entity)
        {
            WorldRenderer.InvalidateEntity(Entity);
        }
    }
}
