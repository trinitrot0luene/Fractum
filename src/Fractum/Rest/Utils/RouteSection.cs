using System;
using System.Collections.Generic;
using System.Text;

namespace Fractum.Rest.Utils
{
    public struct RouteSection
    {
        public static RouteSection Create(string route, bool isMajor = false)
            => new RouteSection { BaseRoute = route, IsMajor = isMajor };

        public string BaseRoute;

        public bool IsMajor;
    }
}
