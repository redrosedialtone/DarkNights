using System.Collections;
using System.Collections.Generic;
using Nebula;

namespace Dark
{
    public interface INavNode : ICoordinate
    {
        INavEdge[] Edges { get; }
    }

    public interface INavEdge
    {
        int PathingCost { get; }
        INavNode Destination { get; }
    }

    /// <summary>
    /// Game Tile
    /// </summary>
    public interface ITileData : INavNode
    {
        WorldPoint Coordinates { get; }
        ITileContainer Container { get; }
        ICreature Creature { get; set; }

        Dictionary<NavigationMode, ITileNavData> NavData { get; set; }
        ITileVisibilityData VisibilityData { get; set; }
    }
}

namespace Dark.World {

    public class TileData : ITileData
    {
        public ITileContainer Container { get; }
        public WorldPoint Coordinates { get { return coordinates; } }
        private readonly WorldPoint coordinates;
        public int X => coordinates.X;
        public int Y => coordinates.Y;
        public ICreature Creature { get; set; }
        //TODO: Edges by mode
        public INavEdge[] Edges => NavData[NavigationMode.Walking].Edges;

        public Dictionary<NavigationMode, ITileNavData> NavData { get; set; }
        public ITileVisibilityData VisibilityData { get; set; }

        public TileData(WorldPoint Coordinate)
        { coordinates = Coordinate; Container = new TileContainer(this); }
    }
}
