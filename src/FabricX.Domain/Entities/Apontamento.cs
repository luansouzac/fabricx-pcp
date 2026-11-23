using FabricX.Domain.Exceptions;

namespace FabricX.Domain.Entities;

public class Apontamento
{
    public int Id { get; private set; }
    public int ProgramacaoId { get; private set; }
    public DateTime DataHora { get; private set; }
    public double QtdProduzida { get; private set; }
    public double QtdDefeituosa { get; private set; }
    public double TempoGastoMin { get; private set; }
    public string? Observacao { get; private set; }

    public double QtdProcessada => QtdProduzida + QtdDefeituosa;

    private Apontamento()
    {
    }

    public Apontamento(int programacaoId, double qtdProduzida, double qtdDefeituosa, double tempoGastoMin, string? observacao = null)
    {
        if (programacaoId <= 0)
            throw new DomainException("A programação do apontamento é obrigatória.");

        if (!(qtdProduzida >= 0) || !(qtdDefeituosa >= 0))
            throw new DomainException("As quantidades produzida e defeituosa não podem ser negativas.");

        if (qtdProduzida + qtdDefeituosa <= 0)
            throw new DomainException("Informe ao menos uma quantidade produzida ou defeituosa maior que zero.");

        if (!(tempoGastoMin > 0))
            throw new DomainException("O tempo gasto deve ser maior que zero.");

        ProgramacaoId = programacaoId;
        QtdProduzida = qtdProduzida;
        QtdDefeituosa = qtdDefeituosa;
        TempoGastoMin = tempoGastoMin;
        Observacao = string.IsNullOrWhiteSpace(observacao) ? null : observacao.Trim();
        DataHora = DateTime.Now;
    }
}
