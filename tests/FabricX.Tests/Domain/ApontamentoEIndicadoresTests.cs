using FabricX.Domain.Entities;
using FabricX.Domain.Exceptions;
using FabricX.Domain.Services;

namespace FabricX.Tests.Domain;

public class ApontamentoTests
{
    [Fact]
    public void Construtor_DadosValidos_PreencheCampos()
    {
        var apontamento = new Apontamento(1, 18, 2, 5, "  turno A ");

        Assert.Equal(20, apontamento.QtdProcessada);
        Assert.Equal("turno A", apontamento.Observacao);
        Assert.True(apontamento.DataHora > DateTime.Now.AddMinutes(-1));
    }

    [Fact]
    public void Construtor_SoDefeituosa_Permite()
    {
        Assert.Equal(3, new Apontamento(1, 0, 3, 5).QtdProcessada);
    }

    [Theory]
    [InlineData(-1, 1, 5)]
    [InlineData(1, -1, 5)]
    [InlineData(0, 0, 5)]
    [InlineData(1, 0, 0)]
    [InlineData(1, 0, -2)]
    [InlineData(double.NaN, 0, 5)]
    public void Construtor_ValoresInvalidos_LancaDomainException(double produzida, double defeituosa, double tempo)
    {
        Assert.Throws<DomainException>(() => new Apontamento(1, produzida, defeituosa, tempo));
    }

    [Fact]
    public void Construtor_ProgramacaoInvalida_LancaDomainException()
    {
        Assert.Throws<DomainException>(() => new Apontamento(0, 1, 0, 5));
    }
}

public class CalculadoraDeIndicadoresTests
{
    private static readonly RoteiroEtapa EtapaParalela = new(1, 1, 1, "Montagem", 2, 10);
    private static readonly RoteiroEtapa EtapaSimples = new(1, 1, 1, "Embalagem", 2, 1);

    private static ApontamentoComEtapa Ap(RoteiroEtapa etapa, double produzida, double defeituosa, double tempo) =>
        new(new Apontamento(1, produzida, defeituosa, tempo), etapa);

    [Fact]
    public void CasoCalculadoAMao_ComUnidadesPorCicloMaiorQueUm()
    {
        var apontamentos = new[]
        {
            Ap(EtapaParalela, 18, 2, 5),
            Ap(EtapaParalela, 25, 0, 8)
        };

        Assert.Equal(2.0 / 45, CalculadoraDeIndicadores.Qualidade(apontamentos)!.Value, 10);
        Assert.Equal(10.0 / 13, CalculadoraDeIndicadores.Eficiencia(apontamentos)!.Value, 10);
        Assert.Equal(43 / (13.0 / 60), CalculadoraDeIndicadores.Produtividade(apontamentos)!.Value, 10);
    }

    [Fact]
    public void Eficiencia_UsaTempoPadraoDeCadaEtapa()
    {
        var apontamentos = new[]
        {
            Ap(EtapaParalela, 20, 0, 4),
            Ap(EtapaSimples, 3, 0, 12)
        };

        Assert.Equal((4.0 + 6) / (4 + 12), CalculadoraDeIndicadores.Eficiencia(apontamentos)!.Value, 10);
    }

    [Fact]
    public void SemApontamentos_IndicadoresSaoNulosSemExcecao()
    {
        Assert.Null(CalculadoraDeIndicadores.Qualidade([]));
        Assert.Null(CalculadoraDeIndicadores.Eficiencia([]));
        Assert.Null(CalculadoraDeIndicadores.Produtividade([]));
        Assert.Null(CalculadoraDeIndicadores.CumprimentoDePrazo([]));
    }

    [Fact]
    public void Qualidade_SoDefeituosas_EhUm()
    {
        Assert.Equal(1, CalculadoraDeIndicadores.Qualidade([Ap(EtapaSimples, 0, 4, 10)]));
    }

    [Fact]
    public void CumprimentoDePrazo_ConsideraConclusaoNoDiaDoPrazoComoNoPrazo()
    {
        OrdemProducao Concluida(int diasDePrazo, int diasAteConcluir)
        {
            var ordem = new OrdemProducao("OP", 1, 1, DateOnly.FromDateTime(DateTime.Today.AddDays(diasDePrazo)));
            ordem.Avancar();
            ordem.Avancar();
            ordem.Avancar();
            typeof(OrdemProducao).GetProperty(nameof(OrdemProducao.ConcluidaEm))!.SetValue(ordem, DateTime.Today.AddDays(diasAteConcluir).AddHours(18));
            return ordem;
        }

        var ordens = new[] { Concluida(5, 5), Concluida(5, 2), Concluida(5, 6), Concluida(5, 8) };

        Assert.Equal(0.5, CalculadoraDeIndicadores.CumprimentoDePrazo(ordens));
    }
}
