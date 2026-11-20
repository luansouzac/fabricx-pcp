using System.ComponentModel.DataAnnotations;

namespace FabricX.Application.DTOs.Integracoes;

public class ResultadoQualidadeDto
{
    [Required, StringLength(30)]
    public string? OrdemNumero { get; set; }

    [Range(double.Epsilon, double.MaxValue)]
    public double QuantidadeInspecionada { get; set; }

    [Range(0, double.MaxValue)]
    public double QuantidadeDefeituosa { get; set; }
}
