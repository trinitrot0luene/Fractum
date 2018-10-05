using System;
using System.Collections.Generic;
using System.Linq;

namespace Fractum.Rest.Utils
{
    public class RouteBuilder
    {
        public RouteBuilder()
        {
            RouteObjects = new List<(RouteSection, object)>();
        }

        private List<(RouteSection, object)> RouteObjects { get; }

        /// <summary>
        ///     Add a new section to the route being built.
        /// </summary>
        /// <param name="routePart">The section to be added.</param>
        /// <param name="parameter">The parameter to be inserted into the section.</param>
        /// <returns></returns>
        public RouteBuilder WithPart(RouteSection routePart, object parameter = null)
        {
            RouteObjects.Add((routePart, parameter));

            return this;
        }

        /// <summary>
        ///     Get the route portion required to generate the bucket ID for this request.
        /// </summary>
        /// <returns></returns>
        public string GetBucketRoute()
        {
            (RouteSection, object)? majorParam = RouteObjects.FirstOrDefault(r => r.Item1.IsMajor);
            return string.Concat(majorParam?.Item1.BaseRoute,
                string.Join("", RouteObjects.Skip(1).Select(r => string.Format(r.Item1.BaseRoute, r.Item2))));
        }

        /// <summary>
        ///     Build the request URI from all supplied route parts.
        /// </summary>
        /// <returns>The target request URI.</returns>
        public Uri Build()
        {
            var urlString = string.Concat(Consts.API_BASE_URL,
                string.Join("", RouteObjects.Select(r => string.Format(r.Item1.BaseRoute, r.Item2))));
            if (urlString.EndsWith("/"))
                urlString = urlString.Substring(0, urlString.Length - 1);
            return new Uri(urlString);
        }
    }
}