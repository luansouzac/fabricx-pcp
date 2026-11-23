using FabricX.Domain.Enums;
using FabricX.Domain.Exceptions;

namespace FabricX.Domain.Entities;

public class OrdemProducao
{
    public int Id { get; private set; }
    public string Numero { get; private set; }
    public int ProdutoId { get; private set; }
    public double Quantidade { get; private set; }
    public DateOnly PrazoEntrega { get; private set; }
    public Prioridade Prioridade { get; private set; }
    public StatusOrdem Status { get; private set; }
    public DateTime CriadaEm { get; private set; }
    public DateTime? ConcluidaEm { get; private set; }

    private OrdemProducao()
    {
        Numero = null!;
    }

    public OrdemProducao(string numero, int produtoId, double quantidade, DateOnly prazoEntrega, Prioridade prioridade = Prioridade.Media)
    {
        if (string.IsNullOrWhiteSpace(numero))
            throw new DomainException("O número da ordem é obrigatório.");

        if (produtoId <= 0)
            throw new DomainException("O produto da ordem é obrigatório.");

        Numero = numero.Trim();
        ProdutoId = produtoId;
        Quantidade = ValidarQuantidade(quantidade);
        PrazoEntrega = ValidarPrazo(prazoEntrega);
        Prioridade = ValidarPrioridade(prioridade);
        Status = StatusOrdem.Planejada;
        CriadaEm = DateTime.Now;
    }

    public void AlterarPlanejamento(double quantidade, DateOnly prazoEntrega, Prioridade prioridade)
    {
        if (Status != StatusOrdem.Planejada)
            throw new DomainException("Só é possível alterar uma ordem que ainda está planejada.");

        Quantidade = ValidarQuantidade(quantidade);
        PrazoEntrega = ValidarPrazo(prazoEntrega);
        Prioridade = ValidarPrioridade(prioridade);
    }

    public void Avancar()
    {
        switch (Status)
        {
            case StatusOrdem.Planejada:
                Status = StatusOrdem.Programada;
                break;
            case StatusOrdem.Programada:
                Status = StatusOrdem.EmProducao;
                break;
            case StatusOrdem.EmProducao:
                Status = StatusOrdem.Concluida;
                ConcluidaEm = DateTime.Now;
                break;
            case StatusOrdem.Concluida:
                throw new DomainException("A ordem já foi concluída.");
            case StatusOrdem.Cancelada:
                throw new DomainException("Uma ordem cancelada não pode avançar.");
        }
    }

    public void Cancelar()
    {
        if (Status == StatusOrdem.Concluida)
            throw new DomainException("Uma ordem concluída não pode ser cancelada.");

        if (Status == StatusOrdem.EmProducao)
            throw new DomainException("Uma ordem em produção não pode ser cancelada: o material já foi consumido.");

        if (Status == StatusOrdem.Cancelada)
            throw new DomainException("A ordem já está cancelada.");

        Status = StatusOrdem.Cancelada;
    }

    private static double ValidarQuantidade(double quantidade)
    {
        if (!(quantidade > 0))
            throw new DomainException("A quantidade deve ser maior que zero.");

        return quantidade;
    }

    private static DateOnly ValidarPrazo(DateOnly prazoEntrega)
    {
        if (prazoEntrega < DateOnly.FromDateTime(DateTime.Today))
            throw new DomainException("O prazo de entrega não pode estar no passado.");

        return prazoEntrega;
    }

    private static Prioridade ValidarPrioridade(Prioridade prioridade)
    {
        if (!Enum.IsDefined(prioridade))
            throw new DomainException("Prioridade inválida.");

        return prioridade;
    }
}
