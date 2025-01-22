using Microsoft.Extensions.DependencyInjection;
using Microsoft.Identity.Abstractions;

public interface IAuthorizationHeaderProviderFactory
{
    IAuthorizationHeaderProvider Create();
}

public class AuthorizationHeaderProviderFactory : IAuthorizationHeaderProviderFactory
{
    private readonly IServiceProvider _serviceProvider;

    public AuthorizationHeaderProviderFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public IAuthorizationHeaderProvider Create()
    {
        return _serviceProvider.GetRequiredService<IAuthorizationHeaderProvider>();
    }
}
