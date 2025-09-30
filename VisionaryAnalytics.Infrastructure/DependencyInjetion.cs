using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using RabbitMQ.Client;
using VisionaryAnalytics.Application.Configuration;
using VisionaryAnalytics.Application.Interfaces;
using VisionaryAnalytics.Domain.Interfaces.Repositories;
using VisionaryAnalytics.Infrastructure.Adapters;
using VisionaryAnalytics.Infrastructure.Configuration;
using VisionaryAnalytics.Infrastructure.Data.Repositories;

namespace VisionaryAnalytics.Infrastructure
{
    public static class DependencyInjetion
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            var mongoDbSettings = new MongoDbSettings();
            configuration.GetSection("MongoDbSettings").Bind(mongoDbSettings);
            services.AddSingleton(mongoDbSettings);

            services.AddSingleton<IMongoClient>(serviceProvider => new MongoClient(mongoDbSettings.ConnectionString));
            services.AddSingleton<IMongoDatabase>(serviceProvider =>
            {
                var client = serviceProvider.GetRequiredService<IMongoClient>();
                return client.GetDatabase(mongoDbSettings.DatabaseName);
            });

            services.AddSingleton<IConnectionFactory>(sp => {
                var rabbitConfig = sp.GetRequiredService<IOptions<RabbitMQOptions>>().Value;
                var factory = new ConnectionFactory()
                {
                    HostName = rabbitConfig.HostName,
                    UserName = rabbitConfig.UserName,
                    Password = rabbitConfig.Password
                };
                return factory;
            });

            services.AddScoped<IVideoJobRepository, VideoJobRepository>();
            services.AddScoped<IArmazenamentoArquivoService, ArmazenamentoArquivoLocalService>();
            services.AddScoped<IVideoFrameService, FfmpegVideoFrameService>();
            services.AddSingleton<IProdutorMessageBroker, RabbitMqProducer>();
            services.AddSingleton<IConsumidorMessageBroker, RabbitMqConsumer>();

            return services;
        }
    }
}
