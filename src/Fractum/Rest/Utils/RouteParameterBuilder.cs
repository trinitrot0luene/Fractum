using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Fractum.Rest.Utils
{
    internal sealed class RouteParameterBuilder
    {
        private const string _paramFormat = "{0}={1}{2}";

        private const char _separatorChar = '&';

        private const char _queryChar = '?';
        private readonly Dictionary<string, string> _values;

        public RouteParameterBuilder()
        {
            _values = new Dictionary<string, string>();
        }

        public RouteParameterBuilder Add(string key, string value)
        {
            _values.Add(key, value);
            return this;
        }

        public string Build()
        {
            var sb = new StringBuilder();
            sb.Append(_queryChar);
            foreach (var kvp in _values)
                sb.Append(string.Format(_paramFormat, kvp.Key, kvp.Value,
                    kvp.Key == _values.Last().Key ? string.Empty : _separatorChar.ToString()));

            return sb.ToString();
        }
    }
}