using System.ComponentModel.DataAnnotations;

namespace FabricX.Application.DTOs.Demanda;

public class CriarDemandaPrevistaDto
{
    [Range(1, int.MaxValue)]
    public int ProdutoId { get; set; }

    [Required, StringLength(7)]
    public string? Periodo { get; set; }

    [Range(double.Epsilon, double.MaxValue)]
    public double Quantidade { get; set; }
}
