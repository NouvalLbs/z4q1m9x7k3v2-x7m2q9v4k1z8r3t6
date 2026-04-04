namespace ProjectSMP.Plugins.GPS
{
    public enum MapNode : int
    {
        Invalid = -1
    }

    public enum Path : int
    {
        Invalid = -1
    }

    public enum Connection : int
    {
        Invalid = -1
    }

    public enum GpsError
    {
        None = 0,
        InvalidParams = -1,
        InvalidPath = -2,
        InvalidNode = -3,
        InvalidConnection = -4,
        Internal = -5
    }
}