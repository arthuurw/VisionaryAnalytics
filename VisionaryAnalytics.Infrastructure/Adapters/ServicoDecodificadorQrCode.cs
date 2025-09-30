using Microsoft.Extensions.Logging;
using VisionaryAnalytics.Application.Interfaces;
using VisionaryAnalytics.Domain.VOs;
using SkiaSharp;
using ZXing;
using ZXing.Common;

namespace VisionaryAnalytics.Infrastructure.Adapters
{
    public class ServicoDecodificadorQrCode : IServicoDecodificadorQrCode
    {
        private readonly ILogger<ServicoDecodificadorQrCode> _logger;
        private readonly BarcodeReaderGeneric _reader;

        public ServicoDecodificadorQrCode(ILogger<ServicoDecodificadorQrCode> logger)
        {
            _logger = logger;
            _reader = new BarcodeReaderGeneric
            {
                AutoRotate = true,
                Options = new DecodingOptions
                {
                    TryHarder = true,
                    PossibleFormats = new List<BarcodeFormat> { BarcodeFormat.QR_CODE }
                }
            };
        }

        public Task<QrCode?> DecodificarAsync(Frame frame, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                using var stream = File.OpenRead(frame.Caminho);
                using var bitmap = SKBitmap.Decode(stream);

                if (bitmap == null)
                {
                    _logger.LogWarning("Não foi possível carregar o frame {FramePath} para decodificação.", frame.Caminho);
                    return Task.FromResult<QrCode?>(null);
                }

                // Convert SKBitmap to byte[] in RGB format
                var pixels = bitmap.Bytes;
                var width = bitmap.Width;
                var height = bitmap.Height;

                var resultado = _reader.Decode(pixels, width, height, ZXing.RGBLuminanceSource.BitmapFormat.RGB32);
                
                return Task.FromResult(resultado != null ? new QrCode(frame.Instante, resultado.Text) : null);
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
            {
                _logger.LogError(ex, "Erro ao ler arquivo do frame {FramePath}.", frame.Caminho);
                return Task.FromResult<QrCode?>(null);
            }
        }
    }
}
