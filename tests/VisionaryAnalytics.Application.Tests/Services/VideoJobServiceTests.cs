using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using VisionaryAnalytics.Application.Configuration;
using VisionaryAnalytics.Application.DTOs;
using VisionaryAnalytics.Application.Interfaces;
using VisionaryAnalytics.Application.Services;
using VisionaryAnalytics.Domain.Entities;
using VisionaryAnalytics.Domain.Enums;
using VisionaryAnalytics.Domain.Interfaces.Repositories;
using VisionaryAnalytics.Domain.VOs;
using Xunit;

namespace VisionaryAnalytics.Application.Tests.Services;

public class VideoJobServiceTests
{
    private readonly Mock<IValidadorArquivoService> _validadorArquivoServiceMock = new();
    private readonly Mock<IArmazenamentoArquivoService> _armazenamentoArquivoServiceMock = new();
    private readonly Mock<IVideoJobRepository> _videoJobRepositoryMock = new();
    private readonly Mock<IProdutorMessageBroker> _produtorMock = new();
    private readonly IOptions<RabbitMQOptions> _rabbitOptions = Options.Create(new RabbitMQOptions
    {
        HostName = "localhost",
        UserName = "guest",
        Password = "guest",
        Queues = new RabbitMQQueues
        {
            ExtrairFrames = "extrair-frames",
            ProcessarFrames = "processar-frames"
        }
    });

    private VideoJobService CriarServico()
    {
        return new VideoJobService(
            _validadorArquivoServiceMock.Object,
            _armazenamentoArquivoServiceMock.Object,
            _videoJobRepositoryMock.Object,
            _produtorMock.Object,
            _rabbitOptions);
    }

    [Fact]
    public async Task CriarJobAsync_DeveRetornarFalha_QuandoValidacaoFalhar()
    {
        _validadorArquivoServiceMock
            .Setup(s => s.Validar("video.mp4", ".mp4", 1024))
            .Returns(Resultado<string>.Falha("erro"));

        using var stream = new MemoryStream([1, 2, 3]);
        var servico = CriarServico();

        var resultado = await servico.CriarJobAsync(stream, "video.mp4", ".mp4", "video/mp4", 1024);

        resultado.Sucesso.Should().BeFalse();
        resultado.Mensagem.Should().Be("erro");
        _armazenamentoArquivoServiceMock.Verify(s => s.SalvarAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        _videoJobRepositoryMock.Verify(r => r.CriarAsync(It.IsAny<VideoJob>()), Times.Never);
        _produtorMock.Verify(p => p.ProduzirAsync(It.IsAny<VideoJob>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CriarJobAsync_DeveCriarJobEPublicarMensagem_QuandoValidacaoTiverSucesso()
    {
        _validadorArquivoServiceMock
            .Setup(s => s.Validar("video.mp4", ".mp4", 1024))
            .Returns(Resultado<string>.Ok());

        _armazenamentoArquivoServiceMock
            .Setup(s => s.SalvarAsync(It.IsAny<Stream>(), "video.mp4", ".mp4"))
            .ReturnsAsync("armazenado.mp4");

        VideoJob? jobPersistido = null;
        _videoJobRepositoryMock
            .Setup(r => r.CriarAsync(It.IsAny<VideoJob>()))
            .Callback<VideoJob>(job => jobPersistido = job)
            .Returns(Task.CompletedTask);

        _produtorMock
            .Setup(p => p.ProduzirAsync(It.IsAny<VideoJob>(), _rabbitOptions.Value.Queues.ExtrairFrames, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        using var stream = new MemoryStream([1, 2, 3]);
        var servico = CriarServico();

        var resultado = await servico.CriarJobAsync(stream, "video.mp4", ".mp4", "video/mp4", 1024);

        resultado.Sucesso.Should().BeTrue();
        resultado.Value.Should().NotBeNullOrWhiteSpace();

        jobPersistido.Should().NotBeNull();
        jobPersistido!.Id.Should().Be(resultado.Value);
        jobPersistido.NomeOriginalArquivo.Should().Be("video.mp4");
        jobPersistido.NomeArmazenadoArquivo.Should().Be("armazenado.mp4");
        jobPersistido.Status.Should().Be(Status.NaFila);

        _armazenamentoArquivoServiceMock.Verify(s => s.SalvarAsync(It.IsAny<Stream>(), "video.mp4", ".mp4"), Times.Once);
        _videoJobRepositoryMock.Verify(r => r.CriarAsync(It.IsAny<VideoJob>()), Times.Once);
        _produtorMock.Verify(p => p.ProduzirAsync(It.IsAny<VideoJob>(), _rabbitOptions.Value.Queues.ExtrairFrames, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ObterStatusAsync_DeveRetornarFalha_QuandoJobNaoExistir()
    {
        const string id = "123";
        _videoJobRepositoryMock.Setup(r => r.ObterPorIdAsync(id)).ReturnsAsync((VideoJob?)null);
        var servico = CriarServico();

        var resultado = await servico.ObterStatusAsync(id);

        resultado.Sucesso.Should().BeFalse();
        resultado.Mensagem.Should().Contain(id);
    }

    [Fact]
    public async Task ObterStatusAsync_DeveRetornarStatusFormatado_QuandoJobExistir()
    {
        var job = VideoJob.Criar("video.mp4", "armazenado.mp4");
        job.IniciarProcessamento();

        _videoJobRepositoryMock.Setup(r => r.ObterPorIdAsync(job.Id)).ReturnsAsync(job);
        var servico = CriarServico();

        var resultado = await servico.ObterStatusAsync(job.Id);

        resultado.Sucesso.Should().BeTrue();
        resultado.Value.Should().Be("Processando");
    }

    [Fact]
    public async Task ObterResultadosAsync_DeveRetornarFalha_QuandoJobNaoExistir()
    {
        const string id = "123";
        _videoJobRepositoryMock.Setup(r => r.ObterPorIdAsync(id)).ReturnsAsync((VideoJob?)null);
        var servico = CriarServico();

        var resultado = await servico.ObterResultadosAsync(id);

        resultado.Sucesso.Should().BeFalse();
        resultado.Mensagem.Should().Contain(id);
    }

    [Fact]
    public async Task ObterResultadosAsync_DeveRetornarListaDeQrCodes_QuandoJobExistir()
    {
        var job = VideoJob.Criar("video.mp4", "armazenado.mp4");
        var resultados = new List<QrCode>
        {
            new(TimeSpan.FromSeconds(1), "conteudo-1"),
            new(TimeSpan.FromSeconds(2), "conteudo-2")
        };
        job.ConcluirProcessamento(resultados);

        _videoJobRepositoryMock.Setup(r => r.ObterPorIdAsync(job.Id)).ReturnsAsync(job);
        var servico = CriarServico();

        var resultado = await servico.ObterResultadosAsync(job.Id);

        resultado.Sucesso.Should().BeTrue();
        resultado.Value.Should().BeEquivalentTo(resultados);
    }
}