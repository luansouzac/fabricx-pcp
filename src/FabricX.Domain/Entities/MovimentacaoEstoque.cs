using FabricX.Domain.Enums;
using FabricX.Domain.Exceptions;

namespace FabricX.Domain.Entities;

public class MovimentacaoEstoque
{
    public int Id { get; private set; }
    public int ProdutoId { get; private set; }
    public int? OrdemId { get; private set; }
    public TipoMovimentacao Tipo { get; private set; }
    public double Quantidade { get; private set; }
    public string? Motivo { get; private set; }
    public DateTime DataHora { get; private set; }

    private MovimentacaoEstoque()
    {
    }

    public MovimentacaoEstoque(int produtoId, TipoMovimentacao tipo, double quantidade, string? motivo = null, int? ordemId = null)
    {
        if (!Enum.IsDefined(tipo))
            throw new DomainException("Tipo de movimentação inválido.");

        if (!(quantidade > 0))
            throw new DomainException("A quantidade deve ser maior que zero.");

        ProdutoId = produtoId;
        OrdemId = ordemId;
        Tipo = tipo;
        Quantidade = quantidade;
        Motivo = string.IsNullOrWhiteSpace(motivo) ? null : motivo.Trim();
        DataHora = DateTime.UtcNow;
    }
}
