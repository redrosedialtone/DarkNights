using System.Collections;
using System.Collections.Generic;
using Dark.Entities;
using Nebula;

namespace Dark
{

    public interface ITileNavData
    {
        int Cost { get; }
        bool Traversible { get; }
        INavEdge[] Edges { get; set; }

        void NavEntityAdded(IEntityNavData NavData);
        void NavEntityRemoved(IEntityNavData NavData);
    }

    public interface ITileNavData_Walk : ITileNavData
    {

    }
}

namespace Dark.World
{
    public class TileNav_Walkable : ITileNavData_Walk
    {
        public int Cost { get; private set; }
        public bool Traversible => intraversible == 0;
        public INavEdge[] Edges { get; set; }

        private int intraversible = 0;

        public void NavEntityAdded(IEntityNavData NavData)
        {
            if (NavData is IWalkNavData WalkData)
            {
                Cost += WalkData.Cost;
                if (!WalkData.Walkable) intraversible++;
            }
        }

        public void NavEntityRemoved(IEntityNavData NavData)
        {
            if (NavData is IWalkNavData WalkData)
            {
                Cost -= WalkData.Cost;
                if (!WalkData.Walkable) intraversible--;
            }
        }
    }

    public class TileEdge_Walkable : INavEdge
    {
        public int PathingCost { get; set; }

        public INavNode Destination { get; private set; }

        public TileEdge_Walkable(int PathingCost, INavNode node)
        {
            this.PathingCost = PathingCost;
            this.Destination = node;
        }
    }
}
