using System.Collections;
using System.Collections.Generic;
using System;
using Nebula;
using Microsoft.Xna.Framework;

namespace Dark
{
    public interface IVision
    {
        WorldPoint[] VisibleTiles { get; }
        WorldPoint Coordinates { get; }

        Vector2 Facing { get; }
        float ViewDistance { get; }
        float RadiusViewDistance { get; }
        float FOV { get; }
        bool Invalidated { get; }

        void UpdateVision(WorldPoint[] VisibleTiles);
        void OnVisibilityUpdate(Action<IVision> callback);
        void RemoveVisibilityUpdate(Action<IVision> callback);
    }

    public interface IPlayerVisionEntity : IVision
    {

    }
}

namespace Dark.Creatures
{
    public abstract class BaseCreatureSight : IVision
    {
        private static readonly NLog.Logger log = NLog.LogManager.GetLogger("[CREATURESIGHT]");

        public WorldPoint[] VisibleTiles { get; protected set; }
        public WorldPoint Coordinates => Creature.Navigation.Coordinates;
        public ICreature Creature;

        public float ViewDistance { get; protected set; }
        public float RadiusViewDistance { get; protected set; }
        public Vector2 Facing => Creature.Navigation.Facing;
        public float FOV => 135.0f;
        public bool Invalidated { get; set; }

        private Action<IVision> cbOnVisibilityUpdate;

        public void TileMovement(ITileData From, ITileData To)
        {
            if (From != To)
            {
                log.Trace("Requesting Vision Update After Tile Movement..");
                Invalidated = true;
                VisionSystem.RequestVisionUpdate(this);
            }
        }

        public void UpdateVision(WorldPoint[] VisibleTiles)
        {
            log.Trace($"CreatureVision Updated [{VisibleTiles.Length} Tiles]");
            this.VisibleTiles = VisibleTiles;
            Invalidated = false;
            cbOnVisibilityUpdate?.Invoke(this);
        }

        public void OnVisibilityUpdate(Action<IVision> callback)
        {
            cbOnVisibilityUpdate += callback;
        }

        public void RemoveVisibilityUpdate(Action<IVision> callback)
        {
            cbOnVisibilityUpdate -= callback;
        }
    }

    public class PlayerSight : BaseCreatureSight, IPlayerVisionEntity
    {
        public PlayerSight(ICreature Creature, int ViewDistance)
        {
            this.Creature = Creature; this.ViewDistance = ViewDistance;
            this.RadiusViewDistance = 1.7f;
            if (Creature.Navigation != null)
            {
                Creature.Navigation.OnTileMovement(TileMovement);
            }
            Invalidated = true;
            VisionSystem.RequestVisionUpdate(this);
        }
    }
}
