namespace Fractum.Rest.Utils
{
    internal struct RouteSection
    {
        public static RouteSection Create(string route, bool isMajor = false)
            => new RouteSection {BaseRoute = route, IsMajor = isMajor};

        public string BaseRoute;

        public bool IsMajor;
    }
}