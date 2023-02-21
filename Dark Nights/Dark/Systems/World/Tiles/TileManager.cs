using System.Collections;
using System.Collections.Generic;
using Nebula;
using Nebula.Systems;
using NLog;

namespace Dark.World
{
    public class AdjacentTileData
    {
        public WorldPoint Origin;
        public ITileData[] AdjacentTiles;

        public ITileData North => AdjacentTiles[1];
        public ITileData South => AdjacentTiles[6];
        public ITileData East => AdjacentTiles[4];
        public ITileData West => AdjacentTiles[3];
        public ITileData NEast => AdjacentTiles[2];
        public ITileData NWest => AdjacentTiles[0];
        public ITileData SEast => AdjacentTiles[7];
        public ITileData SWest => AdjacentTiles[5];

        public AdjacentTileData(WorldPoint Origin, ITileData[] AdjacencyData)
        {
            this.Origin = Origin; this.AdjacentTiles = AdjacencyData;
        }

        public IEnumerator<ITileData> GetEnumerator()
        {
            for (int i = 0; i < AdjacentTiles.Length; i++)
            {
                yield return AdjacentTiles[i];
            }
        }

        public int NeighbourCount()
        {
            int neighbours = 0;
            for (int i = 0; i < 8; i++)
            {
                if(AdjacentTiles[i] != null)
                {
                    neighbours++;
                }
            }
            return neighbours;
        }

        public static readonly Vector2Int[] ToCoordinate = new Vector2Int[]
        {
            new Vector2Int(-1,1), new Vector2Int(0,1), new Vector2Int(1,1),
            new Vector2Int(-1,0),                      new Vector2Int(1,0),
            new Vector2Int(-1,-1),new Vector2Int(0,-1), new Vector2Int(1,-1)
        };

        public static readonly string[] IndexToString = new string[]
        {
            "NW", "N", "NE",
            "W",       "E",
            "SW", "S", "SE"
        };
    }

    public class TileManager : IManager
    {
        #region Static
        private static TileManager instance;
        public static TileManager Get => instance;

        public bool Initialized => throw new System.NotImplementedException();

        private static readonly NLog.Logger log = NLog.LogManager.GetLogger("[TILEMNGR]");
        #endregion

        public void Init()
        {
            log.Info("> Tile Manager Init.. <");
            instance = this;
        }

        public static ITileData[] AdjacentTiles(ITileData Origin) =>
            AdjacentTiles(Origin.Coordinates);

        public static ITileData[] AdjacentTiles(WorldPoint Origin)
        {
            log.Trace($"Fetching Adjacent Tiles For ({Origin.X},{Origin.Y})..");
            int _X = Origin.X;
            int _Y = Origin.Y;
            ITileData[] adjacentTiles = new ITileData[8];
            int i = 0;
            for (int y = _Y + 1; y > _Y - 2; y--)
            {
                for (int x = _X - 1; x < _X + 2; x++)
                {
                    log.Trace($"Adjacency Data[{i}]:{x - _X} - {y - _Y}");
                    //If this is the origin, skip
                    if (y == _Y && x == _X)
                    {
                        continue;
                    }
                    adjacentTiles[i] = GetTileData((x, y), out CbTileState cbTileState);
                    i++;
                }
            }
            return adjacentTiles;
        }

        public static WorldPoint[] AdjacentCoordinates(int originX, int originY)
        {
            log.Trace($"Fetching Adjacent Tiles For ({originX},{originY})..");
            WorldPoint[] adjacentTiles = new WorldPoint[8];
            int i = 0;
            for (int y = originY + 1; y > originY - 2; y--)
            {
                for (int x = originX - 1; x < originX + 2; x++)
                {
                    if (x == originX && y == originY)
                    {
                        continue;
                    }
                    
                    adjacentTiles[i] = new WorldPoint(x, y);
                    i++;
                }
            }
            return adjacentTiles;
        }

        public static WorldPoint[] AdjacentCoordinates(WorldPoint Origin)
        {
            return AdjacentCoordinates(Origin.X, Origin.Y);
        }

        public static AdjacentTileData AdjacencyData(ITileData OriginData) =>
             AdjacencyData(OriginData.Coordinates);


        public static AdjacentTileData AdjacencyData(WorldPoint Origin)
        {
            ITileData[] AdjacencyData = AdjacentTiles(Origin);
            AdjacentTileData AdjacenctTiles = new AdjacentTileData(Origin, AdjacencyData);
            return AdjacenctTiles;
        }

        public ITileData GetTileRelative(WorldPoint Coordinate, int X, int Y, out CbTileState cbTileState)
        {
            WorldPoint AdjustedCoordinate = new WorldPoint(Coordinate.X + X, Coordinate.Y + Y);
            return GetTileData(AdjustedCoordinate, out cbTileState);
        }

        public static ITileData GetTileData((int X, int Y) Coordinates, out CbTileState cbTileState) => 
            WorldSystem.Tile(new WorldPoint(Coordinates), out cbTileState);

        public ITileData GetTileData(WorldPoint Coordinates, out CbTileState cbTileState) =>
            WorldSystem.Tile(Coordinates, out cbTileState);

        public void OnInitialized()
        {
            
        }

        public void Tick()
        {
            
        }
    }
}
