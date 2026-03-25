namespace ProjectSMP.Plugins.SampCEF
{
    public enum CefValueType
    {
        String = 0,
        Integer = 1,
        Float = 2
    }

    public readonly struct CefArg
    {
        public CefValueType Type { get; }
        public object Value { get; }

        private CefArg(CefValueType type, object value) { Type = type; Value = value; }

        public static CefArg Str(string v) => new(CefValueType.String, v);
        public static CefArg Int(int v) => new(CefValueType.Integer, v);
        public static CefArg Float(float v) => new(CefValueType.Float, v);
    }
}