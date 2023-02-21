using Dark;
using Dark.World;
using Nebula;
using Priority_Queue;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Dark
{
    public class AStar
    {
        private static readonly NLog.Logger log = NLog.LogManager.GetLogger("[A*]");

        private readonly SimplePriorityQueue<INavNode> OpenList = new SimplePriorityQueue<INavNode>();
        private readonly List<INavNode> ClosedList = new List<INavNode>();
        private readonly Dictionary<INavNode, INavNode> Trace = new Dictionary<INavNode, INavNode>();

        private readonly Dictionary<INavNode, float> Weights = new Dictionary<INavNode, float>();
        private readonly Dictionary<INavNode, float> HeuristicScores = new Dictionary<INavNode, float>();

        private readonly WorldPoint Start;
        private readonly WorldPoint End;
        private INavNode Current;

        public AStar(WorldPoint Start, WorldPoint End)
        {
            this.Start = Start;
            this.End = End;
        }

        public Stack<INavNode> Path()
        {
            log.Trace("Generating Heuristic Path...");
            INavNode startNode = WorldSystem.Tile(Start, out CbTileState cbTileStateStart);
            INavNode endNode = WorldSystem.Tile(End, out CbTileState cbTileStateEnd);
            if (cbTileStateStart == CbTileState.OutOfBounds || cbTileStateEnd == CbTileState.OutOfBounds)
            {
                log.Warn($"Attempted to path between {Start}({cbTileStateStart} and {End}({cbTileStateEnd})");
            }

            OpenList.Enqueue(startNode, 0);
            Weights[startNode] = 0;
            HeuristicScores[startNode] = HeuristicWeight((Start.X, Start.Y), (End.X, End.Y));

            bool targetReached = false;
            int nodesTraversed = 0;
            while (OpenList.Count > 0)
            {
                nodesTraversed++;
                Current = OpenList.Dequeue();
                // We did it!
                if (Current == endNode)
                {
                    log.Trace("Located Path Target");
                    targetReached = true;
                    break;
                }

                ClosedList.Add(Current);

                foreach (var edge in Current.Edges)
                {
                    if (edge == null) continue;
                    INavNode neighbour = edge.Destination;
                    if (ClosedList.Contains(neighbour)) continue;

                    float weight = Weights[Current] + edge.PathingCost + Distance((Current.X, Current.Y), (neighbour.X, neighbour.Y));
                    bool alreadyOpen = OpenList.Contains(neighbour);
                    if (alreadyOpen && weight >= Weights[neighbour]) continue;

                    if (Trace.ContainsKey(neighbour) == false) Trace.Add(neighbour, Current);
                    else Trace[neighbour] = Current;
                    if (Weights.ContainsKey(neighbour) == false) Weights.Add(neighbour, weight);
                    else Weights[neighbour] = weight;

                    float heuristicWeight = weight + HeuristicWeight((neighbour.X, neighbour.Y), (endNode.X, endNode.Y));
                    if (HeuristicScores.ContainsKey(neighbour) == false) HeuristicScores.Add(neighbour, heuristicWeight);
                    else HeuristicScores[neighbour] = heuristicWeight;

                    if (!alreadyOpen)
                    {
                        OpenList.Enqueue(neighbour, HeuristicScores[neighbour]);
                    }
                }
            }  

            if (targetReached)
            {
                log.Trace($"Path Located ({nodesTraversed}) after {nodesTraversed} nodes.");
                Stack<INavNode> Path = new Stack<INavNode>();
                do
                {
                    Path.Push(Current);
                    Current = Trace[Current];
                } while (Current != null && Current != startNode);
                return Path;
            }
            log.Trace("No Path Found");
            return null;
        }

        private float HeuristicWeight((int X, int Y) Start, (int X, int Y) End)
        {
            return MathF.Sqrt(
                MathF.Pow(Start.X - End.X, 2) +
                MathF.Pow(Start.Y - End.Y, 2));
        }

        private float Distance((int X, int Y) Start, (int X, int Y) End)
        {
            float xDif = MathF.Abs(Start.X - End.X);
            float yDif = MathF.Abs(Start.Y - End.Y);
            if (xDif + yDif == 1) return 1f;
            else if (xDif == 1 && yDif == 1) return 1.414213562373f;
            else
            {
                return HeuristicWeight(Start, End);
            }
        }


    }
}
