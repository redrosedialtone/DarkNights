using ColonySim.Systems.Actions;
using Dark.Entities;
using Nebula;
using Nebula.Systems;
using System.Collections;
using System.Collections.Generic;

namespace Dark
{
    public class ConstructionSystem : IManager, IEntityTaskManager
    {
        #region Static
        private static ConstructionSystem instance;
        public static ConstructionSystem Get => instance;
        private static readonly NLog.Logger log = NLog.LogManager.GetLogger("[CONSTRUCTIONSYS]");
        #endregion

        public string BuildingDef = "entity.wall";

        private WorldPoint CurrentPosition => CursorSystem.Get.currentMousePosition;
        private WorldPoint OldPosition => CursorSystem.Get.oldMousePosition;

        public bool Initialized => throw new System.NotImplementedException();

        private bool placingEntity;

        public void Init()
        {
            log.Info("> Construction System Init <");
            instance = this;
        }

        public void OnInitialized()
        {

        }

        public void Tick()
        {

        }

        private void PlaceTile()
        {
            log.Trace("Clicked::" + CurrentPosition);
            ITileData Data = CursorSystem.Get.highlightedTile;
            if (Data != null)
            {
                log.Trace($"Placing new Entity at {Data.Coordinates}..");

                IEntity EntityDef = EntitySystem.GetDef(BuildingDef);
                if (EntityDef != null)
                {
                    Task_GetEntityPlacementFlags defPlacementFlagsTask = new Task_GetEntityPlacementFlags(this);
                    EntityDef.AssignTask(defPlacementFlagsTask);

                    List<IEntity> toReplace = new List<IEntity>();
                    bool outcomeReplace = false;
                    bool outcome = true;

                    if (defPlacementFlagsTask.Completed)
                    {
                        log.Trace($"[I]Placement Flags::{defPlacementFlagsTask.PlacementFlags[0].Height},{defPlacementFlagsTask.PlacementFlags[0]._onPlacement}");

                        bool replace = false;
                        // Whether the entity thinks it can be placed
                        bool canPlace = true;
                        // Whether existing entities think it can be placed.
                        bool allowPlace = true;
                        foreach (var entity in Data.Container)
                        {
                            foreach (var placementFlag in defPlacementFlagsTask.PlacementFlags)
                            {
                                var _ret = placementFlag.OnPlacement(entity);
                                canPlace = canPlace && _ret;
                                log.Trace($"[I]CanPlace::{(canPlace ? "PASS" : "FAIL")}");
                                if(canPlace) replace = replace || (placementFlag._onPlacement & EntityHeightRule.Replace) != 0;
                            }
                            if (canPlace)
                            {
                                Task_GetEntityPlacementFlags entityPlacementFlags = new Task_GetEntityPlacementFlags(this);
                                entity.AssignTask(entityPlacementFlags);

                                if (entityPlacementFlags.Completed)
                                {
                                    foreach (var placementFlag in entityPlacementFlags.PlacementFlags)
                                    {
                                        var _ret = placementFlag.AllowPlacement(EntityDef);
                                        allowPlace = allowPlace && _ret;
                                        log.Trace($"[O]Allow Place::{(allowPlace ? "PASS" : "FAIL")}");
                                        if (!allowPlace && replace && toReplace.Contains(entity) == false)
                                        {
                                            toReplace.Add(entity);
                                        }
                                    }
                                }
                            }   
                        }
                        if (!allowPlace)
                        {
                            outcome = outcome && canPlace && replace;
                            log.Trace($"[CHECK-REPLACE?]::{(outcome ? "PASS" : "FAIL")}");
                            if (outcome) outcomeReplace = true;
                        }
                        else
                        {
                            outcome = outcome && canPlace && allowPlace;
                            log.Trace($"[CHECK]::{(outcome ? "PASS" : "FAIL")}");
                        }
                        
                    }
                    else
                    {
                        bool allowPlace = true;
                        foreach (var entity in Data.Container)
                        {
                            Task_GetEntityPlacementFlags entityPlacementFlags = new Task_GetEntityPlacementFlags(this);
                            entity.AssignTask(entityPlacementFlags);

                            if (entityPlacementFlags.Completed)
                            {
                                foreach (var placementFlag in entityPlacementFlags.PlacementFlags)
                                {
                                    var _ret = placementFlag.AllowPlacement(EntityDef);
                                    allowPlace = allowPlace && _ret;
                                    log.Trace($"[O]Placement Flags::{placementFlag}::{(allowPlace ? "PASS" : "FAIL")}");
                                }
                            }
                        }
                        outcome = outcome && allowPlace;
                    }

                    if (outcome)
                    {
                        log.Trace($"Placing Entity..");
                        IEntity newEntity = EntitySystem.Get.CreateEntity(BuildingDef);
                        

                        if (outcomeReplace && toReplace.Count > 0)
                        {
                            log.Trace($"Replacing Existing Entities..");
                            new Action_PlaceReplaceEntity(CurrentPosition, newEntity, toReplace.ToArray()).AutoExec();
                            
                        }
                        else
                        {
                            new Action_PlaceEntity(CurrentPosition, newEntity).AutoExec();
                        }
                    }
                    else
                    {
                        log.Trace("Entity Forbidden Placement!");
                    }

                    
                }
            }
            else
            {
                log.Trace("No Tile Data");
            }
        }

        private void RemoveTile()
        {
            log.Trace("Clicked::" + CurrentPosition);
            ITileData Data = CursorSystem.Get.highlightedTile;
            if (Data != null)
            {
                IEntity[] toRemove = Data.Container.Contents;
                foreach (var entity in toRemove)
                {
                    if (entity.EntityType != EntityType.FLOOR)
                    {
                        EntitySystem.Get.RemoveEntity(entity, Data);
                    }
                }
            }
        }

        public void OnDrawGizmos()
        {
            if (Initialized)
            {

            }          
        }

        public void SetConcreteWall()
        {
            BuildingDef = "entity.wall";
        }

        public void SetDoor()
        {
            BuildingDef = "entity.door";
        }

        public void SetFloor()
        {
            BuildingDef = "entity.concretefloor";
        }

        public bool TaskResponse(IEntityTaskWorker Worker, EntityTask Task)
        {
            return true;
        }
    }
}
