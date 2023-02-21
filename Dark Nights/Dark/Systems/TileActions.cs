using Dark;
using Nebula;
using System.Collections;
using System.Collections.Generic;

namespace ColonySim.Systems.Actions
{
    public class Action_PlaceEntity : IReversibleAction
    {
        private readonly WorldPoint Coordinates;
        private readonly IEntity EntityToPlace;

        public Action_PlaceEntity(WorldPoint Coordinates, IEntity EntityToPlace)
        {
            this.Coordinates = Coordinates; this.EntityToPlace = EntityToPlace;
        }

        public bool Execute()
        {
            var _tile = WorldSystem.Tile(Coordinates, out CbTileState cbTileState);
            if (cbTileState == CbTileState.OutOfBounds) return false;
            if (EntitySystem.Get.PlaceEntity(EntityToPlace, _tile))
            {
                return true;
            }
            return false;
        }

        public bool UnExecute()
        {
            if (EntityToPlace != null)
            {
                return EntitySystem.Get.RemoveEntity(EntityToPlace, WorldSystem.TileUnsf(Coordinates));
            }
            return false;
        }
    }

    public class Action_PlaceReplaceEntity : IReversibleAction
    {
        private readonly WorldPoint Coordinates;
        private readonly IEntity EntityToPlace;
        private readonly IEntity[] EntitiesToReplace;

        public Action_PlaceReplaceEntity(WorldPoint Coordinates, IEntity EntityToPlace, IEntity[] EntitiesToReplace)
        {
            this.Coordinates = Coordinates; this.EntityToPlace = EntityToPlace;
            this.EntitiesToReplace = EntitiesToReplace;
        }

        public bool Execute()
        {
            var _tile = WorldSystem.Tile(Coordinates, out CbTileState cbTileState);
            if (cbTileState == CbTileState.OutOfBounds) return false;
            if (EntitySystem.Get.PlaceEntity(EntityToPlace, _tile))
            {
                bool success = true;
                foreach (var entity in EntitiesToReplace)
                {
                    success = success && EntitySystem.Get.RemoveEntity(entity, _tile);
                }
                return success;
            }
            return false;
        }

        public bool UnExecute()
        {
            if (EntityToPlace != null)
            {
                var _tile = WorldSystem.TileUnsf(Coordinates);
                if(EntitySystem.Get.RemoveEntity(EntityToPlace, _tile))
                {
                    bool success = true;
                    foreach (var entity in EntitiesToReplace)
                    {
                        success = success && EntitySystem.Get.PlaceEntity(entity, _tile);
                    }
                }
            }
            return false;
        }
    }
}
