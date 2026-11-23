using System.ComponentModel.DataAnnotations;

namespace FabricX.Application.DTOs.Apontamentos;

public class RegistrarApontamentoDto
{
    [Range(0, double.MaxValue)]
    public double QuantidadeProduzida { get; set; }

    [Range(0, double.MaxValue)]
    public double QuantidadeDefeituosa { get; set; }

    [Range(double.Epsilon, double.MaxValue)]
    public double TempoGastoMin { get; set; }

    [StringLength(500)]
    public string? Observacao { get; set; }
}
