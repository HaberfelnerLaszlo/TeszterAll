using System.Reflection;

namespace TeszterTests.Helpers
{
    /// <summary>
    /// Üres (alapértékeket adó) megvalósítás tetszőleges interfészhez, mocking-könyvtár nélkül.
    /// Azért DispatchProxy, hogy a teszt ne törjön el, ha egy interfész új taggal bővül.
    /// </summary>
    public class DefaultsProxy : DispatchProxy
    {
        protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
        {
            var type = targetMethod!.ReturnType;
            return type != typeof(void) && type.IsValueType ? Activator.CreateInstance(type) : null;
        }

        public static T Create<T>() where T : class => DispatchProxy.Create<T, DefaultsProxy>();
    }
}
