using FabricX.Domain.Enums;
using FabricX.Domain.Exceptions;

namespace FabricX.Domain.Entities;

public class Recurso
{
    public int Id { get; private set; }
    public string Nome { get; private set; }
    public TipoRecurso Tipo { get; private set; }
    public double CapacidadeHorasDia { get; private set; }
    public bool Ativo { get; private set; }

    private Recurso()
    {
        Nome = null!;
    }

    public Recurso(string nome, TipoRecurso tipo, double capacidadeHorasDia)
    {
        Nome = ValidarNome(nome);
        Tipo = ValidarTipo(tipo);
        CapacidadeHorasDia = ValidarCapacidade(capacidadeHorasDia);
        Ativo = true;
    }

    public void Atualizar(string nome, TipoRecurso tipo, double capacidadeHorasDia)
    {
        Nome = ValidarNome(nome);
        Tipo = ValidarTipo(tipo);
        CapacidadeHorasDia = ValidarCapacidade(capacidadeHorasDia);
    }

    public void Ativar() => Ativo = true;

    public void Inativar() => Ativo = false;

    private static string ValidarNome(string? nome)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new DomainException("O nome do recurso é obrigatório.");

        return nome.Trim();
    }

    private static TipoRecurso ValidarTipo(TipoRecurso tipo)
    {
        if (!Enum.IsDefined(tipo))
            throw new DomainException("Tipo de recurso inválido.");

        return tipo;
    }

    private static double ValidarCapacidade(double capacidadeHorasDia)
    {
        if (!(capacidadeHorasDia > 0))
            throw new DomainException("A capacidade em horas por dia deve ser maior que zero.");

        return capacidadeHorasDia;
    }
}
