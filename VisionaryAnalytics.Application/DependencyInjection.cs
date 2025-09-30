using Microsoft.Extensions.DependencyInjection;
using VisionaryAnalytics.Application.Configuration;
using VisionaryAnalytics.Application.Interfaces;
using VisionaryAnalytics.Application.Services;

namespace VisionaryAnalytics.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddOptions<RabbitMQOptions>().BindConfiguration("RabbitMQ");
            services.AddScoped<IValidadorArquivoService, ValidadorArquivoService>();
            services.AddScoped<IVideoJobService, VideoJobService>();

            return services;
        }
    }
}
