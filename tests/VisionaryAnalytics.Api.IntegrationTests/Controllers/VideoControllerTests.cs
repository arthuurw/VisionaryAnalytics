using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Moq;
using VisionaryAnalytics.Application.DTOs;
using VisionaryAnalytics.Domain.VOs;
using Xunit;

namespace VisionaryAnalytics.Api.IntegrationTests.Controllers;

public class VideoControllerTests
{
    [Fact]
    public async Task ObterStatus_DeveRetornarNotFound_QuandoServicoFalhar()
    {
        await using var factory = new CustomWebApplicationFactory();
        factory.VideoJobServiceMock
            .Setup(s => s.ObterStatusAsync("123"))
            .ReturnsAsync(Resultado<string>.Falha("erro"));

        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/videos/123/status");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var payload = await response.Content.ReadAsStringAsync();
        payload.Should().Contain("erro");
    }

    [Fact]
    public async Task ObterStatus_DeveRetornarOk_QuandoServicoForBemSucedido()
    {
        await using var factory = new CustomWebApplicationFactory();
        factory.VideoJobServiceMock
            .Setup(s => s.ObterStatusAsync("123"))
            .ReturnsAsync(Resultado<string>.Ok(value: "Processando"));

        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/videos/123/status");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonDocument>();
        body!.RootElement.GetProperty("status").GetString().Should().Be("Processando");
    }

    [Fact]
    public async Task ObterResultados_DeveRetornarNotFound_QuandoServicoFalhar()
    {
        await using var factory = new CustomWebApplicationFactory();
        factory.VideoJobServiceMock
            .Setup(s => s.ObterResultadosAsync("123"))
            .ReturnsAsync(Resultado<List<QrCode>>.Falha("erro"));

        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/videos/123/resultados");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ObterResultados_DeveRetornarOkComConteudo_QuandoServicoForBemSucedido()
    {
        await using var factory = new CustomWebApplicationFactory();
        var resultados = new List<QrCode>
        {
            new(TimeSpan.FromSeconds(1), "conteudo-1"),
            new(TimeSpan.FromSeconds(2), "conteudo-2")
        };
        factory.VideoJobServiceMock
            .Setup(s => s.ObterResultadosAsync("123"))
            .ReturnsAsync(Resultado<List<QrCode>>.Ok(value: resultados));

        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/videos/123/resultados");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonDocument>();
        var lista = body!.RootElement.GetProperty("resultados").EnumerateArray().Select(e => e.GetProperty("conteudo").GetString()).ToList();
        lista.Should().BeEquivalentTo(new[] { "conteudo-1", "conteudo-2" });
    }

    [Fact]
    public async Task Enviar_DeveRetornarBadRequest_QuandoArquivoNaoFornecido()
    {
        await using var factory = new CustomWebApplicationFactory();
        using var client = factory.CreateClient();

        var response = await client.PostAsync("/api/videos/enviar", new MultipartFormDataContent());

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Enviar_DeveRetornarAccepted_QuandoProcessamentoForCriado()
    {
        await using var factory = new CustomWebApplicationFactory();
        factory.VideoJobServiceMock
            .Setup(s => s.CriarJobAsync(It.IsAny<Stream>(), "video.mp4", ".mp4", "video/mp4", 3))
            .ReturnsAsync(Resultado<string>.Ok(value: "abc"));

        using var client = factory.CreateClient();

        await using var fileStream = new MemoryStream([1, 2, 3]);
        using var content = new MultipartFormDataContent();
        var fileContent = new StreamContent(fileStream);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("video/mp4");
        content.Add(fileContent, "arquivo", "video.mp4");

        var response = await client.PostAsync("/api/videos/enviar", content);

        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
        var body = await response.Content.ReadFromJsonAsync<JsonDocument>();
        body!.RootElement.GetProperty("id").GetString().Should().Be("abc");
    }
}