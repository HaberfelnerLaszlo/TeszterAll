using System.Reflection;

using Microsoft.Identity.Abstractions;

namespace TeszterTests.Helpers
{
    /// <summary>
    /// IAuthorizationHeaderProvider-hamisítvány: minden Task&lt;string&gt;-et adó metódusra "Bearer &lt;token&gt;"-t ad.
    /// DispatchProxy, így nem függ az interfész pontos tagjaitól.
    /// </summary>
    public class FakeAuthorizationHeaderProvider : DispatchProxy
    {
        public string Token { get; set; } = "token";

        protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
        {
            if (targetMethod!.ReturnType == typeof(Task<string>)) return Respond();
            throw new NotSupportedException($"A hamis tokenszolgáltató nem támogatja: {targetMethod.Name}");
        }

        private async Task<string> Respond()
        {
            await Task.Yield();           // valódi aszinkronitás: a párhuzamos tesztek így tényleg átlapolódnak
            return "Bearer " + Token;
        }

        public static IAuthorizationHeaderProvider Create(string token)
        {
            var proxy = DispatchProxy.Create<IAuthorizationHeaderProvider, FakeAuthorizationHeaderProvider>();
            ((FakeAuthorizationHeaderProvider)(object)proxy).Token = token;
            return proxy;
        }
    }
}
