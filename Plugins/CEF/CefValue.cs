using System.Linq;

namespace ProjectSMP.Plugins.CEF
{
    public readonly struct CefValue
    {
        private const int TypeString = 0;
        private const int TypeInteger = 1;
        private const int TypeFloat = 2;

        private readonly int _type;
        private readonly object _value;

        private CefValue(int type, object value)
        {
            _type = type;
            _value = value;
        }

        public static CefValue String(string value) => new(TypeString, value);
        public static CefValue Integer(int value) => new(TypeInteger, value);
        public static CefValue Float(float value) => new(TypeFloat, value);

        internal object[] ToArgs() => new object[] { _type, _value };
    }

    public static class CefValueExtensions
    {
        public static object[] Flatten(this CefValue[] values)
            => values.SelectMany(v => v.ToArgs()).ToArray();
    }
}