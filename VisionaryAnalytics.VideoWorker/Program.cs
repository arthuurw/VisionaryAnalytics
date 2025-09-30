using VisionaryAnalytics.Application;
using VisionaryAnalytics.Infrastructure;
using VisionaryAnalytics.Worker;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddHostedService<TrabalhadorProcessamentoQrCode>();

var host = builder.Build();
host.Run();
