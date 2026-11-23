using System.ComponentModel.DataAnnotations;

namespace FabricX.Application.DTOs.Roteiro;

public class CriarEtapaDto
{
    [Range(1, int.MaxValue)]
    public int RecursoId { get; set; }

    [Range(1, int.MaxValue)]
    public int Sequencia { get; set; }

    [Required, StringLength(100)]
    public string? Nome { get; set; }

    [Range(double.Epsilon, double.MaxValue)]
    public double TempoCicloMin { get; set; }

    [Range(1, int.MaxValue)]
    public int UnidadesPorCiclo { get; set; } = 1;
}
