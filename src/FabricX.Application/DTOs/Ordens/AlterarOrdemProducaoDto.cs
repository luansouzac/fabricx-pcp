using System.ComponentModel.DataAnnotations;
using FabricX.Domain.Enums;

namespace FabricX.Application.DTOs.Ordens;

public class AlterarOrdemProducaoDto
{
    [Range(double.Epsilon, double.MaxValue)]
    public double Quantidade { get; set; }

    [Required]
    public DateOnly? PrazoEntrega { get; set; }

    [EnumDataType(typeof(Prioridade))]
    public Prioridade Prioridade { get; set; } = Prioridade.Media;
}
