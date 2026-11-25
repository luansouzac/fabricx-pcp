using System.ComponentModel.DataAnnotations;
using FabricX.Domain.Enums;

namespace FabricX.Application.DTOs.Recursos;

public class AtualizarRecursoDto
{
    [Required(ErrorMessage = "O nome é obrigatório."), StringLength(100, ErrorMessage = "O nome deve ter até 100 caracteres.")]
    public string? Nome { get; set; }

    [EnumDataType(typeof(TipoRecurso))]
    public TipoRecurso Tipo { get; set; }

    [Range(double.Epsilon, 24, ErrorMessage = "A capacidade deve ser maior que zero e de no máximo 24 horas por dia.")]
    public double CapacidadeHorasDia { get; set; }
}
