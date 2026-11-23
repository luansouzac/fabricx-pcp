using FabricX.Domain.Exceptions;

namespace FabricX.Domain.Entities;

public class RoteiroEtapa
{
    public int Id { get; private set; }
    public int ProdutoId { get; private set; }
    public int RecursoId { get; private set; }
    public int Sequencia { get; private set; }
    public string Nome { get; private set; }
    public double TempoCicloMin { get; private set; }
    public int UnidadesPorCiclo { get; private set; }

    private RoteiroEtapa()
    {
        Nome = null!;
    }

    public RoteiroEtapa(int produtoId, int recursoId, int sequencia, string nome, double tempoCicloMin, int unidadesPorCiclo = 1)
    {
        if (produtoId <= 0)
            throw new DomainException("O produto da etapa é obrigatório.");

        ProdutoId = produtoId;
        Atualizar(recursoId, sequencia, nome, tempoCicloMin, unidadesPorCiclo);
    }

    public void Atualizar(int recursoId, int sequencia, string nome, double tempoCicloMin, int unidadesPorCiclo)
    {
        if (recursoId <= 0)
            throw new DomainException("O recurso da etapa é obrigatório.");

        if (sequencia <= 0)
            throw new DomainException("A sequência da etapa deve ser maior que zero.");

        if (string.IsNullOrWhiteSpace(nome))
            throw new DomainException("O nome da etapa é obrigatório.");

        if (!(tempoCicloMin > 0))
            throw new DomainException("O tempo de ciclo deve ser maior que zero.");

        if (unidadesPorCiclo < 1)
            throw new DomainException("As unidades por ciclo devem ser pelo menos 1.");

        RecursoId = recursoId;
        Sequencia = sequencia;
        Nome = nome.Trim();
        TempoCicloMin = tempoCicloMin;
        UnidadesPorCiclo = unidadesPorCiclo;
    }

    public double CalcularDuracaoMinutos(double quantidade)
    {
        if (!(quantidade > 0))
            throw new DomainException("A quantidade deve ser maior que zero.");

        return Math.Ceiling(quantidade / UnidadesPorCiclo) * TempoCicloMin;
    }
}
