using System.ComponentModel.DataAnnotations;

namespace FabricX.Application.DTOs.Estrutura;

public class AlterarQuantidadeItemEstruturaDto
{
    [Range(double.Epsilon, double.MaxValue)]
    public double Quantidade { get; set; }
}
