using System.ComponentModel.DataAnnotations;

namespace FabricX.Application.DTOs.Programacao;

public class AgendarEtapaDto
{
    [Range(1, int.MaxValue)]
    public int EtapaId { get; set; }

    [Required]
    public DateTime? Inicio { get; set; }

    [Range(1, int.MaxValue)]
    public int? RecursoId { get; set; }
}
