using Microsoft.Extensions.DependencyInjection;
using System;

namespace Meta
{
    public static class DI
    {
        private static ServiceCollection services = new ServiceCollection();
        private static ServiceProvider provider;

        public static void Configure(Action<IServiceCollection> configureFn)
        {
            configureFn(services);
        }

        public static ServiceProvider Build()
        {
            return provider = services.BuildServiceProvider();
        }

        public static TService Get<TService>() => provider.GetService<TService>();
    }
}
