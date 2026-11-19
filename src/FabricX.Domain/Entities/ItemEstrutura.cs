using FabricX.Domain.Exceptions;

namespace FabricX.Domain.Entities;

public class ItemEstrutura
{
    public int Id { get; private set; }
    public int ProdutoPaiId { get; private set; }
    public int ComponenteId { get; private set; }
    public double Quantidade { get; private set; }

    private ItemEstrutura()
    {
    }

    public ItemEstrutura(int produtoPaiId, int componenteId, double quantidade)
    {
        if (produtoPaiId == componenteId)
            throw new DomainException("Um produto não pode ser componente dele mesmo.");

        ProdutoPaiId = produtoPaiId;
        ComponenteId = componenteId;
        AlterarQuantidade(quantidade);
    }

    public void AlterarQuantidade(double quantidade)
    {
        if (!(quantidade > 0))
            throw new DomainException("A quantidade do componente deve ser maior que zero.");

        Quantidade = quantidade;
    }
}
