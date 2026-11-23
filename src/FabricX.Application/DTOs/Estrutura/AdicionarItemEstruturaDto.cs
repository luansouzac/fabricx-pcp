using System.ComponentModel.DataAnnotations;

namespace FabricX.Application.DTOs.Estrutura;

public class AdicionarItemEstruturaDto
{
    [Range(1, int.MaxValue)]
    public int ComponenteId { get; set; }

    [Range(double.Epsilon, double.MaxValue)]
    public double Quantidade { get; set; }
}
