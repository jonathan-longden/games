using System;

namespace PuzzleGame.Core
{
    /// <summary>
    /// Integer grid coordinate. X grows to the right, Y grows DOWNWARD
    /// (row 0 is the top line of the level text). The view layer flips Y.
    /// </summary>
    [Serializable]
    public struct GridPos : IEquatable<GridPos>
    {
        public int X;
        public int Y;

        public GridPos(int x, int y) { X = x; Y = y; }

        public static readonly GridPos Invalid = new GridPos(int.MinValue, int.MinValue);

        public bool IsValid => X != int.MinValue;

        public GridPos Step(Direction d)
        {
            switch (d)
            {
                case Direction.Up: return new GridPos(X, Y - 1);
                case Direction.Down: return new GridPos(X, Y + 1);
                case Direction.Left: return new GridPos(X - 1, Y);
                case Direction.Right: return new GridPos(X + 1, Y);
                default: return this;
            }
        }

        public bool Equals(GridPos other) => X == other.X && Y == other.Y;
        public override bool Equals(object obj) => obj is GridPos p && Equals(p);
        public override int GetHashCode() => (X * 397) ^ Y;
        public static bool operator ==(GridPos a, GridPos b) => a.Equals(b);
        public static bool operator !=(GridPos a, GridPos b) => !a.Equals(b);
        public override string ToString() => $"({X},{Y})";
    }

    public enum Direction : byte
    {
        None = 0,
        Up = 1,
        Down = 2,
        Left = 3,
        Right = 4,
    }

    public static class DirectionUtil
    {
        public static readonly Direction[] All = { Direction.Up, Direction.Down, Direction.Left, Direction.Right };

        public static Direction Opposite(this Direction d)
        {
            switch (d)
            {
                case Direction.Up: return Direction.Down;
                case Direction.Down: return Direction.Up;
                case Direction.Left: return Direction.Right;
                case Direction.Right: return Direction.Left;
                default: return Direction.None;
            }
        }

        public static char ToChar(this Direction d)
        {
            switch (d)
            {
                case Direction.Up: return 'U';
                case Direction.Down: return 'D';
                case Direction.Left: return 'L';
                case Direction.Right: return 'R';
                default: return '-';
            }
        }

        public static Direction FromChar(char c)
        {
            switch (char.ToUpperInvariant(c))
            {
                case 'U': return Direction.Up;
                case 'D': return Direction.Down;
                case 'L': return Direction.Left;
                case 'R': return Direction.Right;
                default: return Direction.None;
            }
        }
    }

    public enum Tile : byte
    {
        Void = 0,   // nothing: outside the board, impassable
        Floor = 1,
        Wall = 2,
    }
}
