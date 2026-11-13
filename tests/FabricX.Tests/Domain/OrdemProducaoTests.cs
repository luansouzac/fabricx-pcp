using FabricX.Domain.Entities;
using FabricX.Domain.Enums;
using FabricX.Domain.Exceptions;

namespace FabricX.Tests.Domain;

public class OrdemProducaoTests
{
    private static DateOnly Hoje => DateOnly.FromDateTime(DateTime.Today);

    private static OrdemProducao CriarOrdem(double quantidade = 10, DateOnly? prazo = null) =>
        new("OP-001", 1, quantidade, prazo ?? Hoje.AddDays(7));

    private static OrdemProducao CriarOrdemNoStatus(StatusOrdem alvo)
    {
        var ordem = CriarOrdem();
        while (ordem.Status != alvo)
            ordem.Avancar();
        return ordem;
    }

    [Fact]
    public void Construtor_DadosValidos_IniciaPlanejadaComPrioridadeMedia()
    {
        var ordem = CriarOrdem();

        Assert.Equal(StatusOrdem.Planejada, ordem.Status);
        Assert.Equal(Prioridade.Media, ordem.Prioridade);
        Assert.Null(ordem.ConcluidaEm);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    [InlineData(double.NaN)]
    public void Construtor_QuantidadeNaoPositiva_LancaDomainException(double quantidade)
    {
        Assert.Throws<DomainException>(() => CriarOrdem(quantidade));
    }

    [Fact]
    public void Construtor_PrazoNoPassado_LancaDomainException()
    {
        Assert.Throws<DomainException>(() => CriarOrdem(prazo: Hoje.AddDays(-1)));
    }

    [Fact]
    public void Construtor_PrazoHoje_Aceita()
    {
        var ordem = CriarOrdem(prazo: Hoje);

        Assert.Equal(Hoje, ordem.PrazoEntrega);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void Construtor_NumeroVazio_LancaDomainException(string numero)
    {
        Assert.Throws<DomainException>(() => new OrdemProducao(numero, 1, 10, Hoje.AddDays(1)));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Construtor_ProdutoInvalido_LancaDomainException(int produtoId)
    {
        Assert.Throws<DomainException>(() => new OrdemProducao("OP-001", produtoId, 10, Hoje.AddDays(1)));
    }

    [Fact]
    public void Construtor_PrioridadeInvalida_LancaDomainException()
    {
        Assert.Throws<DomainException>(() => new OrdemProducao("OP-001", 1, 10, Hoje.AddDays(1), (Prioridade)99));
    }

    [Fact]
    public void Avancar_SegueOFluxoCompleto()
    {
        var ordem = CriarOrdem();

        ordem.Avancar();
        Assert.Equal(StatusOrdem.Programada, ordem.Status);

        ordem.Avancar();
        Assert.Equal(StatusOrdem.EmProducao, ordem.Status);

        ordem.Avancar();
        Assert.Equal(StatusOrdem.Concluida, ordem.Status);
    }

    [Fact]
    public void Avancar_AoConcluir_RegistraDataDeConclusao()
    {
        var ordem = CriarOrdemNoStatus(StatusOrdem.EmProducao);

        ordem.Avancar();

        Assert.NotNull(ordem.ConcluidaEm);
    }

    [Fact]
    public void Avancar_OrdemConcluida_LancaDomainException()
    {
        var ordem = CriarOrdemNoStatus(StatusOrdem.Concluida);

        Assert.Throws<DomainException>(() => ordem.Avancar());
    }

    [Fact]
    public void Avancar_OrdemCancelada_LancaDomainException()
    {
        var ordem = CriarOrdem();
        ordem.Cancelar();

        Assert.Throws<DomainException>(() => ordem.Avancar());
    }

    [Theory]
    [InlineData(StatusOrdem.Planejada)]
    [InlineData(StatusOrdem.Programada)]
    [InlineData(StatusOrdem.EmProducao)]
    public void Cancelar_AntesDeConcluir_Permitido(StatusOrdem status)
    {
        var ordem = CriarOrdemNoStatus(status);

        ordem.Cancelar();

        Assert.Equal(StatusOrdem.Cancelada, ordem.Status);
    }

    [Fact]
    public void Cancelar_OrdemConcluida_LancaDomainException()
    {
        var ordem = CriarOrdemNoStatus(StatusOrdem.Concluida);

        Assert.Throws<DomainException>(() => ordem.Cancelar());
        Assert.Equal(StatusOrdem.Concluida, ordem.Status);
    }

    [Fact]
    public void Cancelar_OrdemJaCancelada_LancaDomainException()
    {
        var ordem = CriarOrdem();
        ordem.Cancelar();

        Assert.Throws<DomainException>(() => ordem.Cancelar());
    }

    [Fact]
    public void AlterarPlanejamento_OrdemPlanejada_AlteraCampos()
    {
        var ordem = CriarOrdem();
        var novoPrazo = Hoje.AddDays(30);

        ordem.AlterarPlanejamento(25, novoPrazo, Prioridade.Urgente);

        Assert.Equal(25, ordem.Quantidade);
        Assert.Equal(novoPrazo, ordem.PrazoEntrega);
        Assert.Equal(Prioridade.Urgente, ordem.Prioridade);
    }

    [Fact]
    public void AlterarPlanejamento_OrdemJaProgramada_LancaDomainException()
    {
        var ordem = CriarOrdemNoStatus(StatusOrdem.Programada);

        Assert.Throws<DomainException>(() => ordem.AlterarPlanejamento(5, Hoje.AddDays(3), Prioridade.Alta));
    }

    [Fact]
    public void AlterarPlanejamento_QuantidadeInvalida_LancaDomainException()
    {
        var ordem = CriarOrdem();

        Assert.Throws<DomainException>(() => ordem.AlterarPlanejamento(0, Hoje.AddDays(3), Prioridade.Alta));
    }

    [Fact]
    public void AlterarPlanejamento_PrazoNoPassado_LancaDomainException()
    {
        var ordem = CriarOrdem();

        Assert.Throws<DomainException>(() => ordem.AlterarPlanejamento(5, Hoje.AddDays(-1), Prioridade.Alta));
    }
}
