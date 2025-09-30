using VisionaryAnalytics.Api.Hubs;
using VisionaryAnalytics.Application;
using VisionaryAnalytics.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Adiciona serviços ao contêiner.

builder.Services.AddControllers();
// Saiba mais sobre a configuração do OpenAPI em https://aka.ms/aspnet/openapi
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddSignalR();
builder.Services.AddCors(options =>
{
    options.AddPolicy("LiberarSwagger", policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configura o pipeline de requisição HTTP.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseHttpsRedirection();

app.UseCors("LiberarSwagger");

app.UseAuthorization();

app.MapControllers();
app.MapHub<HubProcessamento>("/hubs/processamento");

app.Run();

public partial class Program { }
