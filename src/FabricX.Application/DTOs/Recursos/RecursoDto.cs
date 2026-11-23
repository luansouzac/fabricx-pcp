using FabricX.Domain.Entities;
using FabricX.Domain.Enums;

namespace FabricX.Application.DTOs.Recursos;

public class RecursoDto
{
    public int Id { get; init; }
    public string Nome { get; init; } = string.Empty;
    public TipoRecurso Tipo { get; init; }
    public double CapacidadeHorasDia { get; init; }
    public bool Ativo { get; init; }

    public static RecursoDto De(Recurso recurso) => new()
    {
        Id = recurso.Id,
        Nome = recurso.Nome,
        Tipo = recurso.Tipo,
        CapacidadeHorasDia = recurso.CapacidadeHorasDia,
        Ativo = recurso.Ativo
    };
}
