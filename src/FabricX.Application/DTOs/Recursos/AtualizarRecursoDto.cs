using System.ComponentModel.DataAnnotations;
using FabricX.Domain.Enums;

namespace FabricX.Application.DTOs.Recursos;

public class AtualizarRecursoDto
{
    [Required, StringLength(100)]
    public string? Nome { get; set; }

    [EnumDataType(typeof(TipoRecurso))]
    public TipoRecurso Tipo { get; set; }

    [Range(double.Epsilon, 24)]
    public double CapacidadeHorasDia { get; set; }
}
