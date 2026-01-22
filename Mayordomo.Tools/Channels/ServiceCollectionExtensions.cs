using Microsoft.Extensions.DependencyInjection;

namespace Mayordomo.Tools.Channels;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddChannelQueueFactory(this IServiceCollection services)
    {
        services.AddSingleton<ChannelQueueFactory>();
        return services;
    }


    public static IServiceCollection AddChannelQueueFactory(this IServiceCollection services, Action<ChannelQueueFactoryConfigurator> configure)
    {
        services.AddSingleton<ChannelQueueFactory>();

        if (configure != null)
        {
            var tempFactory = new ChannelQueueFactory();
            var configurator = new ChannelQueueFactoryConfigurator(tempFactory);
            configure(configurator);
        }

        return services;
    }


    public class ChannelQueueFactoryConfigurator
    {
        private readonly ChannelQueueFactory _factory;

        internal ChannelQueueFactoryConfigurator(ChannelQueueFactory factory)
        {
            _factory = factory;
        }

        public ChannelQueueFactoryConfigurator Configure<T>(Action<ChannelQueueFactory.ChannelQueueBuilder<T>> builderAction)
        {
            _factory.GetQueue<T>(configure: builderAction);
            return this;
        }

        public ChannelQueueFactoryConfigurator Configure<T>(string name,
            Action<ChannelQueueFactory.ChannelQueueBuilder<T>> builderAction)
        {
            _factory.GetQueue<T>(name, builderAction);
            return this;
        }
    }
}