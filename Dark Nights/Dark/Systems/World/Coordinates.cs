using System.Collections;
using System.Collections.Generic;
using System;
using Dark;
using Microsoft.Xna.Framework;

namespace Nebula
{
    public interface ICoordinate
    {
        int X { get; }
        int Y { get; }
    }

    /// <summary>
    /// A point relative to the world position.
    /// </summary>
    public struct WorldPoint : ICoordinate
    {
        public int X { get; }
        public int Y { get; }

        public WorldPoint((int X, int Y) Coordinates)
            : this(Coordinates.X, Coordinates.Y) { }
        public WorldPoint(Vector2Int Coordinates)
            : this(Coordinates.x, Coordinates.y) { }

        public WorldPoint(int X, int Y)
        { this.X = X; this.Y = Y; }

        public static implicit operator Vector2Int(WorldPoint Coordinate) =>
            new Vector2Int(Coordinate.X, Coordinate.Y);

        public static implicit operator WorldPoint(Vector2Int Coordinate) =>
            new WorldPoint((int)MathF.Floor(Coordinate.x), (int)MathF.Floor(Coordinate.y));

        public override string ToString()
        {
            return $"({X},{Y})";
        }

        public override int GetHashCode() =>
         this.X * 666 + this.Y * 1339;

        public override bool Equals(object Other)
        {
            if (Other != null && Other is WorldPoint point)
            {
                return this.X == point.X && this.Y == point.Y;
            }
            return false;          
        }

        public static WorldPoint operator +(WorldPoint a, WorldPoint b) =>
            new WorldPoint(a.X + b.X, a.Y + b.Y);

        public static bool operator == (WorldPoint a, WorldPoint b) =>
            a.Equals(b);

        public static bool operator !=(WorldPoint a, WorldPoint b) =>
            !a.Equals(b);

        public static bool operator ==(WorldPoint a, Vector2Int b) =>
            a.X == b.x && a.Y == b.y;

        public static bool operator !=(WorldPoint a, Vector2Int b) =>
            a.X != b.x || a.Y != b.y;

        public static implicit operator WorldPoint(Vector2 v)
        {
            return new WorldPoint((int)MathF.Floor(v.X), (int)MathF.Floor(v.Y));
        }

        public static explicit operator Vector2(WorldPoint v)
        {
            return new Vector2(v.X, v.Y);
        }
    }

    /// <summary>
    /// A Chunk's relative position to another.
    /// </summary>
    public struct ChunkLocation : ICoordinate
    {
        public int X { get; }
        public int Y { get; }

        public ChunkLocation(int X, int Y)
        { this.X = X; this.Y = Y; }

        public ChunkLocation((int X, int Y) Coordinates)
        { this.X = Coordinates.X; this.Y = Coordinates.Y; }

        public WorldPoint Origin =>
            new WorldPoint(X * WorldSystem.CHUNK_SIZE, Y * WorldSystem.CHUNK_SIZE);

        public WorldPoint Boundary =>
             new WorldPoint(X * WorldSystem.CHUNK_SIZE + WorldSystem.CHUNK_SIZE, Y * WorldSystem.CHUNK_SIZE + WorldSystem.CHUNK_SIZE);

        public static implicit operator WorldPoint(ChunkLocation Chunk) =>
            new WorldPoint(Chunk.X * WorldSystem.CHUNK_SIZE, Chunk.Y * WorldSystem.CHUNK_SIZE);

        public static implicit operator ChunkLocation(WorldPoint v)
        {
            int vX = (int)MathF.Floor((float)v.X / WorldSystem.CHUNK_SIZE);
            int vY = (int)MathF.Floor((float)v.Y / WorldSystem.CHUNK_SIZE);
            return new ChunkLocation(vX, vY);
        }

        public override string ToString()
        {
            return $"({X},{Y})";
        }

        public override int GetHashCode() =>
         this.X*WorldSystem.CHUNK_SIZE * 666 + this.Y * WorldSystem.CHUNK_SIZE * 1339;

        public override bool Equals(object obj)
        {
            if (obj is ChunkLocation chunk)
            {
                if (this.X == chunk.X && this.Y == chunk.Y) ;
            }
            return base.Equals(obj);
        }

        public static bool operator ==(ChunkLocation a, ChunkLocation b) =>
            a.X == b.X && a.Y == b.Y;
        public static bool operator !=(ChunkLocation a, ChunkLocation b) =>
            a.X != b.X || a.Y != b.Y;
    }

    public struct Vector2Int
    {
        public int x;
        public int y;

        public Vector2Int(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public override bool Equals(object obj)
        {
            if (obj is Vector2Int) return this.Equals((Vector2Int)obj);
            else return false;
        }

        public bool Equals(Vector2Int other)
        {
            return ((this.x == other.x) && (this.y == other.y));
        }

        public static bool operator ==(Vector2Int value1, Vector2Int value2)
        {
            return ((value1.x == value2.x) && (value1.y == value2.y));
        }

        public static Vector2Int operator *(Vector2Int value1, Vector2Int value2)
        {
            return new Vector2Int(value1.x * value2.x, value1.y * value2.y);
        }

        public static bool operator !=(Vector2Int value1, Vector2Int value2)
        {
            if (value1.x == value2.x) return value1.y != value2.y;
            return true;
        }

        public override int GetHashCode()
        {
            return (this.x.GetHashCode() + this.y.GetHashCode());
        }

        public override string ToString()
        {
            return string.Format("{{X:{0} Z:{1}}}", this.x, this.y);
        }
    }
}
