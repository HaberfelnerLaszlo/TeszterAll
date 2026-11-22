using System.Reflection;

using TeszterLibrary.Services;

namespace TeszterTests.Helpers
{
    /// <summary>
    /// IToastService-hamisítvány (a TeszterLibrary saját, v4-stílusú toast interfésze), ami rögzíti a hívásokat ("ShowInfo:üzenet"). DispatchProxy, így nem függ az interfész
    /// pontos tagjaitól és túlterheléseitől.
    /// </summary>
    public class ToastRecorder : DispatchProxy
    {
        public List<string> Messages { get; } = new();

        protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
        {
            var message = args?.OfType<string>().FirstOrDefault();      // az első szöveges paraméter az üzenet
            lock (Messages) Messages.Add($"{targetMethod!.Name}:{message}");
            var type = targetMethod.ReturnType;
            return type != typeof(void) && type.IsValueType ? Activator.CreateInstance(type) : null;
        }

        public static IToastService Create(out ToastRecorder recorder)
        {
            var proxy = DispatchProxy.Create<IToastService, ToastRecorder>();
            recorder = (ToastRecorder)(object)proxy;
            return proxy;
        }
    }
}
