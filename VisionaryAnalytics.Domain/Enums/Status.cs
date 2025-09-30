using System.ComponentModel.DataAnnotations;

namespace VisionaryAnalytics.Domain.Enums
{
    public enum Status
    {
        [Display(Name = "Na Fila")]
        NaFila,
        [Display(Name = "Processando")]
        Processando,
        [Display(Name = "Concluído")]
        Concluido
    }
}
