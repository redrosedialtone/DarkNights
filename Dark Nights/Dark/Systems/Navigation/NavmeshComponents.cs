using System.Collections;
using System.Collections.Generic;

namespace Dark
{
    public enum NavigationMode
    {
        None = 0,
        Walking = 1,
        Flying = 2
    }

    //public class NavMesh : ILoggerSlave
    //{
    //    public LoggingUtility.ILogger Master => NavigationSystem.Get;
    //    public string LoggingPrefix => $"<color=yellow>[NAVMESH]</color>";

    //    private Node[,] nodes;

    //    //generate nodes for all tiles that don't have impassable terrain on them
    //    public bool GenerateNavMesh(World.GameWorld world)
    //    {
    //        this.Verbose("Generating Nodes..");
    //        nodes = new Node[world.Size.x, world.Size.y];
    //        //generate nodes for each walkable tile
    //        // TODO: this should pull the walkability and cost from the tile
    //        for (int x = 0; x < world.Size.x; x++)
    //        {
    //            for (int y = 0; y < world.Size.y; y++)
    //            {
    //                ITileData t = world.Tile(x, y);
    //                var navData = t.NavData[NavigationMode.Walking];

    //                if (navData.Traversible)
    //                {
    //                    Node n = new Node();
    //                    n.Cost = navData.Cost;
    //                    n.Position = (x, y);
    //                    nodes[x, y] = n;
    //                }
    //            }
    //        }

    //        this.Verbose("Generating Edges..");
    //        //Traverse Nodes in the NavMesh and generate Edges to all the neighbouring Nodes
    //        // Cost for Edges is calculated as the cost to LEAVE the node and not cost to enter
    //        for (int x = 0; x < nodes.GetLength(0); x++)
    //        {
    //            for (int y = 0; y < nodes.GetLength(1); y++)
    //            {
    //                if (nodes[x,y] != null)
    //                {
    //                    GenerateNode(x,y);
    //                }
    //            }
    //        }
    //        return false;
    //    }

    //    public void RefreshNode(int X, int Y)
    //    {
    //        ITileData tileData = WorldSystem.Tile(X, Y);
    //        if (tileData != null)
    //        {
    //            ITileNavData navData = tileData.NavData[NavigationMode.Walking];
    //            if (navData != null)
    //            {
    //                if (navData.Traversible)
    //                {
    //                    // Refresh the Node
    //                    this.Verbose($"Regenerating Node.. ({X},{Y})");
    //                    GenerateNode(navData, X, Y);
    //                    return;
    //                }
    //            }
    //        }
    //        this.Verbose($"Clearing Node..({X},{Y})");
    //        // If the node exists but shouldn't
    //        if (nodes[X,Y] != null)
    //        {
    //            nodes[X, Y] = null;
    //            foreach (var pos in TileManager.AdjacentCoordinates(X,Y))
    //            {
    //                this.Debug($"Clearing Refresh Node..({pos.X},{pos.Y})");
    //                if (nodes[pos.X,pos.Y] != null)
    //                {
    //                    GenerateNode(pos.X, pos.Y);
    //                }                    
    //            }
    //        }

    //    }

    //    public void GenerateNode(int X, int Y)
    //    {
    //        ITileData tileData = WorldSystem.Tile(X, Y);
    //        if (tileData != null)
    //        {
    //            ITileNavData navData = tileData.NavData[NavigationMode.Walking];
    //            if (navData != null)
    //            {
    //                GenerateNode(navData, X, Y);
    //            }
    //        }
    //    }

    //    public void GenerateNode(ITileNavData navData, int X, int Y)
    //    {
    //        this.Debug($"Generating edges for node ({X},{Y})", LoggingPriority.Low);

    //        Node node;
    //        if (nodes[X,Y] == null)
    //        {
    //            node = nodes[X, Y] = new Node();
    //        }
    //        else
    //        {
    //            node = nodes[X, Y];
    //        }

    //        Edge[] edges = new Edge[8];
    //        int index = 0;
    //        foreach (var neighbourPos in AdjacentTileData.ToCoordinate)
    //        {
    //            int _X = X + neighbourPos.x;
    //            int _Y = Y + neighbourPos.y;
    //            this.Debug($"Generating edge ({_X},{_Y})", LoggingPriority.Low);
    //            if (_X < 0 || _Y < 0 || _X >= nodes.GetLength(0) || _Y >= nodes.GetLength(1)) continue;
    //            if (nodes[_X, _Y] != null)
    //            {

    //                Edge e = new Edge
    //                {
    //                    cost = navData.Cost,
    //                    node = nodes[_X, _Y]
    //                };
    //                edges[index] = e;
    //            }
    //            index++;
    //        }

    //        node.Edges = edges;
    //        node.Position = (X, Y);
    //    }

    //    public Node this[int X, int Y] =>
    //        nodes[X,Y];

    //    public IEnumerator<Node> GetEnumerator()
    //    {
    //        foreach (var node in nodes)
    //        {
    //            yield return node;
    //        }
    //    }
    //}
}
