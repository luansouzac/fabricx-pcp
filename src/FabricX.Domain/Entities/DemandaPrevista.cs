using FabricX.Domain.Exceptions;
using FabricX.Domain.ValueObjects;

namespace FabricX.Domain.Entities;

public class DemandaPrevista
{
    public int Id { get; private set; }
    public int ProdutoId { get; private set; }
    public string Periodo { get; private set; }
    public double Quantidade { get; private set; }

    private DemandaPrevista()
    {
        Periodo = null!;
    }

    public DemandaPrevista(int produtoId, string periodo, double quantidade)
    {
        if (produtoId <= 0)
            throw new DomainException("O produto da demanda é obrigatório.");

        ProdutoId = produtoId;
        Periodo = string.Empty;
        Atualizar(periodo, quantidade);
    }

    public void Atualizar(string periodo, double quantidade)
    {
        var validado = PeriodoMensal.Parse(periodo);

        if (!(quantidade > 0))
            throw new DomainException("A quantidade prevista deve ser maior que zero.");

        Periodo = validado.ToString();
        Quantidade = quantidade;
    }
}
