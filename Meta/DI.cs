using System;
using Microsoft.Extensions.DependencyInjection;

namespace Meta
{
    public static class DI
    {
        private static IServiceCollection services = new ServiceCollection();
        private static IServiceProvider provider;

        public static void Configure(Action<IServiceCollection> configureFn)
        {
            configureFn(services);
        }

        public static void Build()
        {
            provider = services.BuildServiceProvider();
        }

        public static TService Get<TService>() => provider.GetService<TService>();
    }
}
