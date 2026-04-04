namespace ProjectSMP.Plugins.GPS
{
    public enum GPSError
    {
        None = 0,
        InvalidParams = -1,
        InvalidPath = -2,
        InvalidNode = -3,
        InvalidConnection = -4,
        Internal = -5
    }

    public struct MapNode
    {
        public int Value;

        public MapNode(int value)
        {
            Value = value;
        }

        public static readonly MapNode Invalid = new MapNode(-1);

        public bool IsValid => Value != -1;

        public static implicit operator int(MapNode node) => node.Value;
        public static implicit operator MapNode(int value) => new MapNode(value);

        public override string ToString() => $"MapNode({Value})";
    }

    public struct Path
    {
        public int Value;

        public Path(int value)
        {
            Value = value;
        }

        public static readonly Path Invalid = new Path(-1);

        public bool IsValid => Value != -1;

        public static implicit operator int(Path path) => path.Value;
        public static implicit operator Path(int value) => new Path(value);

        public override string ToString() => $"Path({Value})";
    }

    public struct Connection
    {
        public int Value;

        public Connection(int value)
        {
            Value = value;
        }

        public static readonly Connection Invalid = new Connection(-1);

        public bool IsValid => Value != -1;

        public static implicit operator int(Connection conn) => conn.Value;
        public static implicit operator Connection(int value) => new Connection(value);

        public override string ToString() => $"Connection({Value})";
    }
}