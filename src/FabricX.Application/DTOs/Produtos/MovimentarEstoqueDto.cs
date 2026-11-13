using System.ComponentModel.DataAnnotations;
using FabricX.Domain.Enums;

namespace FabricX.Application.DTOs.Produtos;

public class MovimentarEstoqueDto
{
    [Required, EnumDataType(typeof(TipoMovimentacao))]
    public TipoMovimentacao Tipo { get; set; }

    [Range(double.Epsilon, double.MaxValue)]
    public double Quantidade { get; set; }

    [StringLength(200)]
    public string? Motivo { get; set; }
}
