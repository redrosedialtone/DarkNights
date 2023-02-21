using System.Collections;
using System.Collections.Generic;
using System;
using Nebula;
using Microsoft.Xna.Framework;

namespace Dark
{
    public interface ICreatureNavigation : IWorldTick
    {
        ICreature Creature { get; }
        WorldPoint Coordinates { get; }
        Vector2 Position { get; }
        Vector2 Facing { get; }

        void SetTilePosition(WorldPoint tileCoordinates);
        void Destination(WorldPoint destination);
        void Stop(Action cbStopEvent);
        void OnTileMovement(Action<ITileData,ITileData> cbTileMovement);
        void TraversedChunk(Action<ChunkLocation, ChunkLocation> cbChunkMovement);
    }
}

namespace Dark.Creatures
{
    public class CreatureBaseNavigation : ICreatureNavigation
    {
        private static readonly NLog.Logger log = NLog.LogManager.GetLogger("[CREATURENAV]");

        public ICreature Creature { get; }
        public WorldPoint Coordinates { get; protected set; }
        public Vector2 Position { get
            {
                /*if (currentDestination != null)
                {
                    float X = Mathf.Lerp(Coordinates.X, currentDestination.Value.x, movementPercentage);
                    float Y = Mathf.Lerp(Coordinates.Y, currentDestination.Value.y, movementPercentage);
                    return new Vector2(X, Y);
                }
                else
                {
                    return new Vector2(Coordinates.X, Coordinates.Y);
                }*/
                return new Vector2(Coordinates.X, Coordinates.Y);
            } }

        public Vector2 Facing => currentFacing;
        private Vector2 currentFacing = Vector2.Zero;

        private Stack<INavNode> currentPath;
        private Vector2? currentDestination;
        private Action cbOnMovementEnd;
        private Action<ITileData, ITileData> onTileMovement;
        private Action<ChunkLocation, ChunkLocation> onChunkMovement;
        private ITileData currentTile;

        private float movementPercentage;
        Vector2 startFacing;
        private ChunkLocation _currentChunk;

        public CreatureBaseNavigation(ICreature Creature)
        {
            this.Creature = Creature;
        }

        public void SetTilePosition(WorldPoint tileCoordinates)
        {
            this.Coordinates = tileCoordinates;
            UpdateTilePosition();
        }

        private void UpdateTilePosition()
        {
            if(currentTile != null) currentTile.Creature = null;
            ITileData prv = currentTile;
            currentTile = WorldSystem.TileUnsf(Coordinates);
            currentTile.Creature = Creature;
            onTileMovement?.Invoke(prv, currentTile);
            if (_currentChunk != (ChunkLocation)Coordinates)
            {
                onChunkMovement?.Invoke(_currentChunk, (ChunkLocation)Coordinates);
                _currentChunk = Coordinates;
            }
        }

        public void Destination(WorldPoint destination)
        {
            log.Trace($"Navigating Creature to {destination}..");
            currentPath = NavigationSystem.Path(Coordinates, destination);
            INavNode nextNode = currentPath.Pop();
            currentDestination = new Vector2(nextNode.X, nextNode.Y);
            startFacing = currentFacing;
        }

        public void Stop(Action cbStopEvent)
        {
            this.cbOnMovementEnd = cbStopEvent;
            if (currentPath != null)
            {
                currentPath.Clear();
            }
            else
            {
                cbStopEvent();
            }
            
        }

        public void WorldTick(float delta)
        {
            Move(delta);
        }



        private void Move(float delta)
        {
            if (currentDestination != null)
            {
                Vector2 destination = (Vector2)currentDestination;
                if (movementPercentage > 0)
                {
                    Vector2 dir = new Vector2(destination.X - Position.X, destination.Y - Position.Y);
                    dir.Normalize();
                    currentFacing = dir;
                }
                
                float distance = MathF.Sqrt(
                    MathF.Pow(Coordinates.X-destination.X, 2)+
                    MathF.Pow(Coordinates.Y - destination.Y, 2)
                    );

                float speed = 3.5f * delta;
                float travel = speed / distance;

                movementPercentage += travel;
                if (movementPercentage >= 1)
                {
                    Coordinates = destination;

                    var nextPoint = NextDestination();
                    if (nextPoint != null)
                    {
                        currentDestination = new Vector2(nextPoint.Value.X, nextPoint.Value.Y);
                    }
                    else 
                    {
                        FinishMovement();
                    }

                    movementPercentage = 0;
                    startFacing = currentFacing;
                    UpdateTilePosition();
                }
            }
        }

        private void FinishMovement()
        {
            cbOnMovementEnd?.Invoke();
            cbOnMovementEnd = null;
            currentDestination = null;
            currentPath = null;
        }

        private WorldPoint? NextDestination()
        {
            if (currentPath != null)
            {
                if (currentPath.Count == 0)
                {
                    currentPath = null;
                    return null;
                }
                INavNode nextNode = currentPath.Pop();
                return new WorldPoint(nextNode.X, nextNode.Y);
            }
            return null;
        }

        public void OnTileMovement(Action<ITileData, ITileData> cbTileMovement)
        {
            onTileMovement += cbTileMovement;
            //TODO: Remove Function
        }

        public void TraversedChunk(Action<ChunkLocation, ChunkLocation> cbChunkMovement)
        {
            onChunkMovement += cbChunkMovement;
        }
    }
}
