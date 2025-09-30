using Microsoft.AspNetCore.Mvc;
using VisionaryAnalytics.Application.Interfaces;

namespace VisionaryAnalytics.Api.Controllers
{
    [Route("api/videos")]
    [ApiController]
    public class VideoController : ControllerBase
    {
        private readonly IVideoJobService _videoJobService;

        public VideoController(IVideoJobService videoJobService)
        {
            _videoJobService = videoJobService;
        }

        [HttpGet("{id}/status")]
        public async Task<IActionResult> ObterStatus(string id)
        {
            var resultado = await _videoJobService.ObterStatusAsync(id);
            if (!resultado.Sucesso)
            {
                return NotFound(resultado.Mensagem);
            }

            return Ok(new { Status = resultado.Value });
        }

        [HttpGet("{id}/resultados")]
        public async Task<IActionResult> ObterResultados(string id)
        {
            var resultado = await _videoJobService.ObterResultadosAsync(id);
            if (!resultado.Sucesso)
            {
                return NotFound(resultado.Mensagem);
            }

            return Ok(new { Resultados = resultado.Value });
        }

        [HttpPost("enviar")]
        public async Task<IActionResult> Enviar(IFormFile arquivo)
        {
            if (arquivo == null || arquivo.Length == 0)
            {
                return BadRequest("Nenhum arquivo enviado.");
            }

            using var stream = arquivo.OpenReadStream();
            var nomeArquivo = arquivo.FileName;
            var tamanhoArquivo = arquivo.Length;
            var tipoConteudo = arquivo.ContentType;
            var extensao = Path.GetExtension(nomeArquivo);

            var resultado = await _videoJobService.CriarJobAsync(stream, nomeArquivo, extensao, tipoConteudo, tamanhoArquivo);
            if (!resultado.Sucesso)
            {
                return BadRequest(resultado.Mensagem);
            }

            return Accepted(new { Id = resultado.Value });
        }
    }
}
