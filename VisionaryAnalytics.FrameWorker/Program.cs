using VisionaryAnalytics.Application;
using VisionaryAnalytics.FrameWorker;
using VisionaryAnalytics.Infrastructure;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<TrabalhadorExtracaoFrames>();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var host = builder.Build();
host.Run();
