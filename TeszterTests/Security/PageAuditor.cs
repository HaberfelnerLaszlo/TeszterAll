using System.Reflection;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;

namespace TeszterTests.Security
{
    public sealed record PageInfo(Type Type, string[] Routes, bool HasAuthorize, bool AllowAnonymous, string[] Roles)
    {
        public string Name => Type.Name;
        public string Namespace => Type.Namespace ?? string.Empty;
    }

    /// <summary>Reflexióval összeszedi az útvonallal rendelkező (@page) komponenseket és az authorizációs attribútumaikat.</summary>
    public static class PageAuditor
    {
        public static IReadOnlyList<PageInfo> RoutablePages(params Assembly[] assemblies)
        {
            var pages = new List<PageInfo>();
            foreach (var type in assemblies.SelectMany(SafeTypes))
            {
                if (type.IsAbstract || !typeof(IComponent).IsAssignableFrom(type)) continue;

                var routes = type.GetCustomAttributes<RouteAttribute>(inherit: false).Select(r => r.Template).ToArray();
                if (routes.Length == 0) continue;

                var attributes = type.GetCustomAttributes(inherit: true);
                var authorize = attributes.OfType<IAuthorizeData>().ToArray();
                var roles = authorize
                    .Where(a => !string.IsNullOrWhiteSpace(a.Roles))
                    .SelectMany(a => a.Roles!.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))
                    .ToArray();

                pages.Add(new PageInfo(type, routes, authorize.Length > 0, attributes.OfType<IAllowAnonymous>().Any(), roles));
            }
            return pages;
        }

        private static IEnumerable<Type> SafeTypes(Assembly assembly)
        {
            try { return assembly.GetTypes(); }
            catch (ReflectionTypeLoadException e) { return e.Types.Where(t => t is not null).Select(t => t!); }
        }
    }
}
