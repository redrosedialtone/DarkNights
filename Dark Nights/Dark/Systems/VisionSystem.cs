using Dark.Rendering;
using Microsoft.Xna.Framework;
using Nebula;
using Nebula.Systems;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Dark
{
    public enum Visibility
    {
        Hidden = 0,
        Seen = 1,
        Visible = 2

    }

    public interface ITileVisibilityData
    {
        WorldPoint Coordinates { get; }
        Visibility PlayerVisibility { get; set; }
        List<IPlayerVisionEntity> SeenByPlayers { get; }
        bool isVisible { get; }
        float Opacity { get; set; }

        void TileVisibility();
        void Seen(IVision Creature);
        void Unseen(IVision Creature);
    }

    public class TileVisibilityData : ITileVisibilityData
    {
        private static readonly NLog.Logger log = NLog.LogManager.GetLogger("[TILEVISIBILITY]");

        public WorldPoint Coordinates { get; set; }
        public float Opacity { get; set; }
        public Visibility PlayerVisibility { get; set; } = Visibility.Hidden;
        public List<IPlayerVisionEntity> SeenByPlayers { get; private set; }
        public bool isVisible => PlayerVisibility == Visibility.Seen;

        public TileVisibilityData(WorldPoint Coordinates)
        {
            this.Coordinates = Coordinates;
        }

        public void TileVisibility()
        {
            if(SeenByPlayers != null && SeenByPlayers.Count > 0) PlayerVisibility = Visibility.Visible;
            else if (PlayerVisibility == Visibility.Visible) PlayerVisibility = Visibility.Seen;
        }

        public void Seen(IVision Creature)
        {
            if(Creature is IPlayerVisionEntity playerVision)
            {
                if (SeenByPlayers == null) SeenByPlayers = new List<IPlayerVisionEntity>() { playerVision };
                else
                {
                    if (SeenByPlayers.Contains(playerVision)) return;
                    SeenByPlayers.Add(playerVision);
                }
                playerVision.OnVisibilityUpdate(OnVisionUpdate);
                UpdateVisibilityStatus();
            }  
        }
        public void Unseen(IVision Creature)
        {
            if (Creature is IPlayerVisionEntity playerVision)
            {
                if (SeenByPlayers.Contains(playerVision)) SeenByPlayers.Remove(playerVision);
                playerVision.RemoveVisibilityUpdate(OnVisionUpdate);
                UpdateVisibilityStatus();
            }
            
        }

        public void OnVisionUpdate(IVision vision)
        {
            if (vision.VisibleTiles.Contains(this.Coordinates))
            {
                Seen(vision);
            }
            else
            {
                Unseen(vision);               
            }
        }

        private void UpdateVisibilityStatus()
        {
            var _old = PlayerVisibility;
            if(SeenByPlayers != null && SeenByPlayers.Count > 0)
            {
                PlayerVisibility = Visibility.Visible;
            }
            else if (PlayerVisibility == Visibility.Visible)
            {
                PlayerVisibility = Visibility.Seen;
            }
            if (_old != PlayerVisibility)
            {
                //WorldRenderer.SetTileDirty(Coordinates);
            }
        }
    }
}

namespace Dark
{
    public class VisionSystem : IManager
    {
        #region Static
        private static VisionSystem instance;
        public static VisionSystem Get => instance;
        private static readonly NLog.Logger log = NLog.LogManager.GetLogger("[VISIONSYS]");

        public bool Initialized => throw new System.NotImplementedException();

        private void Awake()
        {
            instance = this;
        }
        #endregion

        private bool drawSightGizmo = false;

        //private List<WorldPoint> playerVisibleTiles = new List<WorldPoint>();
        private List<IPlayerVisionEntity> playerSight = new List<IPlayerVisionEntity>();
        private Stack<IVision> visionUpdate = new Stack<IVision>();

        public void Init()
        {
            log.Info("> Sight System Init <");
            instance = this;
        }

        public void Tick()
        {
            while (visionUpdate.Count > 0)
            {
                var _update = visionUpdate.Pop();
                if (_update is IPlayerVisionEntity playerVision)
                {

                }
                if (_update.Invalidated)
                {
                    CalculateVision(_update);
                    visionUpdate.Push(_update);
                    break;
                }
                else
                {
                    UpdateTileVisibility(_update, _update.VisibleTiles);
                }
            }
        }

        public static void RequestVisionUpdate(IVision vision)
        {
            log.Trace("Vision Update Requested...");
            instance.visionUpdate.Push(vision);
        }

        private void UpdateTileVisibility(IVision source, WorldPoint[] visibleTiles)
        {
            log.Trace($"Updating Visible Tiles [{visibleTiles.Length}]");
            foreach (var pos in visibleTiles)
            {
                var tile = WorldSystem.TileUnsf(pos);
                if (tile != null)
                {
                    tile.VisibilityData.Seen(source);
                }
            }
        }

        public void CalculateVision(IVision Sight)
        {
            log.Trace($"Calculating Vision...");
            WorldPoint[] visibleTiles = VisibleTilesInCone(Sight.Coordinates, Sight.Facing, Sight.ViewDistance, Sight.FOV);
            if (Sight.RadiusViewDistance > 0)
            {
                WorldPoint[] radiusTiles = VisibleTilesInRadius(Sight.Coordinates, Sight.RadiusViewDistance);
                visibleTiles = visibleTiles.Union(radiusTiles).ToArray();
            }
            Sight.UpdateVision(visibleTiles);
        }

        public WorldPoint[] VisibleTilesInCone(WorldPoint Origin, Vector2 Direction, float Distance, float FOV)
        {
            int[] octants = FOVOctants(Direction, 90);
            log.Trace($"[FOV] Visibility from {Origin}... Dir::{Direction}, Quadrants {octants[0]} to {octants[octants.Length-1]}");
            List<WorldPoint> visibleTiles = new List<WorldPoint>();
            foreach (var octant in octants)
            {
                var _tiles = LOSOctant(Origin, octant, Distance);
                visibleTiles = visibleTiles.Union(_tiles).ToList();
            }
            log.Trace($"[FOV] [TILES::{visibleTiles.Count}]");
            return visibleTiles.ToArray();
        }

        public WorldPoint[] VisibleTilesInRadius(WorldPoint Origin, float Distance)
        {
            List<WorldPoint> visibleTiles = new List<WorldPoint>();
            for (int octant = 1; octant <= 8; octant++)
            {
                var _tiles = LOSOctant(Origin, octant, Distance);
                visibleTiles = visibleTiles.Union(_tiles).ToList();
            }
            return visibleTiles.ToArray();
        }

        // Right:: 1,2
        // Up:: 3,4
        // Left:: 5,6
        // Down:: 7,8

        private int[] FOVOctants(Vector2 dir, float fovAngle)
        {
            int quadrantNum = (int)MathF.Floor(fovAngle / 45.0f);

            int[] octants = { 1,2,3,4,5,6,7,0};
            float angle = MathF.Atan2(dir.Y, dir.X);
            int minOctant = (int)MathF.Round(8 * angle / (2 * MathF.PI) + 8) % 8;

            int[] retOctants = new int[quadrantNum];
            retOctants[0] = octants[minOctant];
            if (quadrantNum > 1)
            {
                for (int i = 0; i < quadrantNum - 1; i++)
                {
                    int _n = i % 2;
                    if (_n == 0)
                    {
                        int nextOctant = minOctant + i + 1;
                        if (nextOctant == 8) nextOctant = 0;
                        retOctants[i + 1] = octants[nextOctant];
                    }
                    else
                    {
                        int nextOctant = minOctant - (i + 1);
                        if (nextOctant == 0) nextOctant = 7;
                        retOctants[i + 1] = octants[nextOctant];

                    }
                }
            }


            return retOctants;

        }

        public WorldPoint[] LOSOctant(WorldPoint Origin, int octant, float Distance)
        {
            List<WorldPoint> visibleTiles = new List<WorldPoint>();
            var line = new ShadowLine();
            bool fullShadow = false;
            for (int row = 0; row < Distance; row++)
            {
                Vector2 pos = (Vector2)(Origin + transformOctant(row, 0, octant));
                if (OutOfRange((Vector2)Origin, pos, Distance))
                {
                    log.Trace($"Row OUTOFRANGE::{Origin},{pos},{Distance}");
                    break;
                }

                for (int col = 0; col <= row; col++)
                {
                    pos = (Vector2)Origin + transformOctant(row, col, octant);
                    if (OutOfRange((Vector2)Origin, pos, Distance))
                    {
                        log.Trace($"Col OUTOFRANGE::{Origin},{pos},{Distance}");
                        break;
                    }

                    var tile = WorldSystem.Tile(pos, out CbTileState cbTileState);
                    if (cbTileState == CbTileState.OutOfBounds) continue;
                    if (tile != null)
                    {
                        if (fullShadow)
                        {
                            log.Trace($"Full Shadow At:: {pos}");
                            //tile.VisibilityData.isVisible = false;
                        }
                        else
                        {
                            var projection = projectTile(row, col);
                            var visible = !line.isInShadow(projection);
                            if (visible) visibleTiles.Add(pos);

                            if (visible && tile.VisibilityData.Opacity == 1)
                            {
                                line.add(projection);
                                fullShadow = line.isFullShadow();
                                log.Trace($"Tile At {pos} is Opaque FullShadow:: {fullShadow}");
                                log.Trace($"Projection Is:: {projection} from {projection.startPos} to {projection.endPos}");
                            }
                        }
                    }
                }
                           
            }
            return visibleTiles.ToArray();
        }

        public void AddPlayerSight(IPlayerVisionEntity Sight)
        {
            playerSight.Add(Sight);
        }


        class Shadow
        {
            public float start;
            public float end;

            public Vector2 startPos;
            public Vector2 endPos;

            public Shadow(float start, float end, Vector2 startPos, Vector2 endPos)
            {
                this.start = start; this.end = end;
                this.startPos = startPos;
                this.endPos = endPos;
            }

            public bool contains(Shadow other)
            {
                return start <= other.start && end >= other.end;
            }

            public override string ToString() => $"{start}-{end}";
        }

        Shadow projectTile(float row, float col)
        {
            float topLeft = col / (row + 2);
            float bottomRight = (col + 1) / (row + 1);
            log.Trace($"New Projection From ({row},{col}) to (({col} / ({row}+2) = {topLeft:0.00}({col / (row + 2)}),{bottomRight:0.00})");
            return new Shadow(topLeft, bottomRight, new Vector2(col, row + 2), new Vector2(col + 1, row + 1));
        }

        class ShadowLine
        {
            List<Shadow> _shadows = new List<Shadow>();

            public bool isInShadow(Shadow projection)
            {
                foreach (var shadow in _shadows)
                {
                    if (shadow.contains(projection)) return true;
                }

                return false;
            }

            public bool isFullShadow()
            {
                return _shadows.Count == 1 &&
                    _shadows[0].start == 0 &&
                    _shadows[0].end == 1;
            }

            public void add(Shadow shadow)
            {
                // Figure out where to slot the new shadow in the list.
                var index = 0;
                for (; index < _shadows.Count; index++)
                {
                    // Stop when we hit the insertion point.
                    if (_shadows[index].start >= shadow.start) break;
                }

                Shadow overlappingPrevious = null;
                if (index > 0 && _shadows[index - 1].end > shadow.start)
                {
                    overlappingPrevious = _shadows[index - 1];
                }

                Shadow overlappingNext = null;
                if (index < _shadows.Count &&
                    _shadows[index].start < shadow.end)
                {
                    overlappingNext = _shadows[index];
                }

                // Insert and unify with overlapping shadows.
                if (overlappingNext != null)
                {
                    if (overlappingPrevious != null)
                    {
                        // Overlaps both, so unify one and delete the other.
                        overlappingPrevious.end = overlappingNext.end;
                        overlappingPrevious.endPos = overlappingNext.endPos;
                        _shadows.RemoveAt(index);
                    }
                    else
                    {
                        // Overlaps the next one, so unify it with that.
                        overlappingNext.start = shadow.start;
                        overlappingNext.startPos = shadow.startPos;
                    }
                }
                else
                {
                    if (overlappingPrevious != null)
                    {
                        // Overlaps the previous one, so unify it with that.
                        overlappingPrevious.end = shadow.end;
                        overlappingPrevious.endPos = shadow.endPos;
                    }
                    else
                    {
                        // Does not overlap anything, so insert.
                        _shadows.Insert(index, shadow);
                    }
                }
            }
        }

        private bool OutOfRange(Vector2 Origin, Vector2 Pos, float Distance)
        {
            float difX = Origin.X - Pos.X;
            float difY = Origin.Y - Pos.Y;
            return MathF.Sqrt(MathF.Pow(difX,2) + MathF.Pow(difY,2)) > Distance;
        }

        private Vector2 transformOctant(int row, int col, int octant)
        {
            switch (octant)
            {
                case 0: return new Vector2(col, -row);
                case 1: return new Vector2(row, -col);
                case 2: return new Vector2(row, col);
                case 3: return new Vector2(col, row);
                case 4: return new Vector2(-col, row);
                case 5: return new Vector2(-row, col);
                case 6: return new Vector2(-row, -col);
                case 7: return new Vector2(-col, -row);
                default: return Vector2.Zero;
            }
        }

        private WorldPoint _lastSightGizmoCoordinate;

        /*public void OnDrawGizmos()
        {
            if (Initialized)
            {
                if (drawSightGizmo)
                {
                    if (PlayerController.Get.selectedCreature != null && PlayerController.Get.selectedCreature is IPlayerVisionEntity playerSight)
                    {
                        if (playerSight.VisibleTiles != null && playerSight.VisibleTiles.Length > 0)
                        {
                            foreach (var visibleTile in playerSight.VisibleTiles)
                            {
                                var data = WorldSystem.TileUnsf(visibleTile).VisibilityData;
                                if (data.isVisible)
                                {
                                    if (data.Opacity == 0) Gizmos.color = new Color(1, 0, 1, 0.5f);
                                    else Gizmos.color = new Color(1, 0, 0, 0.5f);
                                }
                                else Gizmos.color = new Color(0.2f, 0.2f, 0.2f, 0.5f);
                                Vector3 drawPos = new Vector3(visibleTile.X + 0.5f, visibleTile.Y + 0.5f, 1.0f);
                                Gizmos.DrawCube(drawPos, new Vector3(1f, 1f, 1f));
                            }
                        }
                    }
                }
            }
        }*/

        public void OnInitialized()
        {
            throw new System.NotImplementedException();
        }
    }
}
