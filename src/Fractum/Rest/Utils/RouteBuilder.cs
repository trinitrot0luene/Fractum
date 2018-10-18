using System;
using System.Collections.Generic;
using System.Linq;

namespace Fractum.Rest.Utils
{
    internal sealed class RouteBuilder
    {
        public RouteBuilder()
        {
            RouteObjects = new List<(RouteSection section, object[] parameters)>();

        }

        private List<(RouteSection section, object[] parameters)> RouteObjects { get; }

        private RouteParameterBuilder RouteParameters { get; set; }

        /// <summary>
        ///     Add a new section to the route being built.
        /// </summary>
        /// <param name="routePart">The section to be added.</param>
        /// <param name="parameters">The parameter to be inserted into the section.</param>
        /// <returns></returns>
        public RouteBuilder WithPart(RouteSection routePart, params object[] parameters)
        {
            RouteObjects.Add((routePart, parameters));

            return this;
        }

        public RouteBuilder WithParameterBuilder(RouteParameterBuilder builder)
        {
            RouteParameters = builder;

            return this;
        }

        /// <summary>
        ///     Get the route portion required to generate the bucket ID for this request.
        /// </summary>
        /// <returns></returns>
        public string GetBucketRoute()
        {
            (RouteSection section, object)? majorParam = RouteObjects.FirstOrDefault(r => r.Item1.IsMajor);
            return string.Concat(majorParam?.section.BaseRoute,
                string.Join("", RouteObjects.Skip(1).Select(r => string.Format(r.section.BaseRoute, r.parameters.Length > 0 ? r.parameters : new[] { string.Empty }))));
        }

        /// <summary>
        ///     Build the request URI from all supplied route parts.
        /// </summary>
        /// <returns>The target request URI.</returns>
        public Uri Build()
        {
            var urlString = string.Concat(Consts.API_BASE_URL,
                string.Join("", RouteObjects.Select(r => string.Format(r.section.BaseRoute, r.parameters.Length > 0 ? r.parameters : new[] {string.Empty }))));
            if (urlString.EndsWith("/"))
                urlString = urlString.Substring(0, urlString.Length - 1);

            if (RouteParameters != null)
                urlString += RouteParameters.Build();

            return new Uri(urlString);
        }
    }
}