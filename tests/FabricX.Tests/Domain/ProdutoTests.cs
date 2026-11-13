using FabricX.Domain.Entities;
using FabricX.Domain.Enums;
using FabricX.Domain.Exceptions;

namespace FabricX.Tests.Domain;

public class ProdutoTests
{
    private static Produto CriarProduto(double estoqueAtual = 10, double estoqueMinimo = 5) =>
        new("P001", "Parafuso", TipoProduto.MateriaPrima, "UN", estoqueAtual, estoqueMinimo);

    [Fact]
    public void Construtor_DadosValidos_CriaProdutoAtivo()
    {
        var produto = CriarProduto();

        Assert.Equal("P001", produto.Codigo);
        Assert.Equal(10, produto.EstoqueAtual);
        Assert.True(produto.Ativo);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Construtor_CodigoVazio_LancaDomainException(string codigo)
    {
        Assert.Throws<DomainException>(() => new Produto(codigo, "Parafuso", TipoProduto.MateriaPrima));
    }

    [Fact]
    public void Construtor_NomeVazio_LancaDomainException()
    {
        Assert.Throws<DomainException>(() => new Produto("P001", " ", TipoProduto.MateriaPrima));
    }

    [Fact]
    public void Construtor_TipoInvalido_LancaDomainException()
    {
        Assert.Throws<DomainException>(() => new Produto("P001", "Parafuso", (TipoProduto)99));
    }

    [Fact]
    public void Construtor_EstoqueAtualNegativo_LancaDomainException()
    {
        Assert.Throws<DomainException>(() => CriarProduto(estoqueAtual: -1));
    }

    [Fact]
    public void Construtor_EstoqueMinimoNegativo_LancaDomainException()
    {
        Assert.Throws<DomainException>(() => CriarProduto(estoqueMinimo: -1));
    }

    [Fact]
    public void Construtor_CustoNegativo_LancaDomainException()
    {
        Assert.Throws<DomainException>(() => new Produto("P001", "Parafuso", TipoProduto.MateriaPrima, custoUnitario: -1));
    }

    [Fact]
    public void RegistrarEntrada_QuantidadeValida_SomaAoEstoque()
    {
        var produto = CriarProduto(estoqueAtual: 10);

        produto.RegistrarEntrada(5);

        Assert.Equal(15, produto.EstoqueAtual);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-3)]
    [InlineData(double.NaN)]
    public void RegistrarEntrada_QuantidadeNaoPositiva_LancaDomainException(double quantidade)
    {
        var produto = CriarProduto();

        Assert.Throws<DomainException>(() => produto.RegistrarEntrada(quantidade));
    }

    [Fact]
    public void RegistrarSaida_QuantidadeValida_SubtraiDoEstoque()
    {
        var produto = CriarProduto(estoqueAtual: 10);

        produto.RegistrarSaida(4);

        Assert.Equal(6, produto.EstoqueAtual);
    }

    [Fact]
    public void RegistrarSaida_QuantidadeIgualAoEstoque_ZeraSemFicarNegativo()
    {
        var produto = CriarProduto(estoqueAtual: 10);

        produto.RegistrarSaida(10);

        Assert.Equal(0, produto.EstoqueAtual);
    }

    [Fact]
    public void RegistrarSaida_MaiorQueEstoque_LancaDomainExceptionEMantemSaldo()
    {
        var produto = CriarProduto(estoqueAtual: 10);

        Assert.Throws<DomainException>(() => produto.RegistrarSaida(10.5));
        Assert.Equal(10, produto.EstoqueAtual);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void RegistrarSaida_QuantidadeNaoPositiva_LancaDomainException(double quantidade)
    {
        var produto = CriarProduto();

        Assert.Throws<DomainException>(() => produto.RegistrarSaida(quantidade));
    }

    [Fact]
    public void Movimentar_Entrada_AumentaEstoque()
    {
        var produto = CriarProduto(estoqueAtual: 10);

        produto.Movimentar(TipoMovimentacao.Entrada, 2);

        Assert.Equal(12, produto.EstoqueAtual);
    }

    [Fact]
    public void Movimentar_Saida_DiminuiEstoque()
    {
        var produto = CriarProduto(estoqueAtual: 10);

        produto.Movimentar(TipoMovimentacao.Saida, 2);

        Assert.Equal(8, produto.EstoqueAtual);
    }

    [Fact]
    public void Movimentar_TipoInvalido_LancaDomainException()
    {
        var produto = CriarProduto();

        Assert.Throws<DomainException>(() => produto.Movimentar((TipoMovimentacao)99, 1));
    }

    [Fact]
    public void EstoqueAbaixoDoMinimo_EstoqueMenorQueMinimo_RetornaTrue()
    {
        var produto = CriarProduto(estoqueAtual: 4, estoqueMinimo: 5);

        Assert.True(produto.EstoqueAbaixoDoMinimo);
    }

    [Fact]
    public void EstoqueAbaixoDoMinimo_EstoqueIgualAoMinimo_RetornaFalse()
    {
        var produto = CriarProduto(estoqueAtual: 5, estoqueMinimo: 5);

        Assert.False(produto.EstoqueAbaixoDoMinimo);
    }

    [Fact]
    public void RegistrarSaida_CruzandoOMinimo_AtivaAlerta()
    {
        var produto = CriarProduto(estoqueAtual: 10, estoqueMinimo: 5);
        Assert.False(produto.EstoqueAbaixoDoMinimo);

        produto.RegistrarSaida(6);

        Assert.True(produto.EstoqueAbaixoDoMinimo);
    }

    [Fact]
    public void RegistrarEntrada_RecuperandoOMinimo_DesativaAlerta()
    {
        var produto = CriarProduto(estoqueAtual: 2, estoqueMinimo: 5);

        produto.RegistrarEntrada(3);

        Assert.False(produto.EstoqueAbaixoDoMinimo);
    }

    [Fact]
    public void Atualizar_EstoqueMinimoNegativo_LancaDomainException()
    {
        var produto = CriarProduto();

        Assert.Throws<DomainException>(() => produto.Atualizar("Parafuso", null, "UN", -1, 0));
    }

    [Fact]
    public void Atualizar_DadosValidos_AlteraCampos()
    {
        var produto = CriarProduto();

        produto.Atualizar("Parafuso M8", "Aço", "CX", 20, 1.5);

        Assert.Equal("Parafuso M8", produto.Nome);
        Assert.Equal("Aço", produto.Descricao);
        Assert.Equal("CX", produto.UnidadeMedida);
        Assert.Equal(20, produto.EstoqueMinimo);
        Assert.Equal(1.5, produto.CustoUnitario);
    }

    [Fact]
    public void InativarEAtivar_AlternamOEstado()
    {
        var produto = CriarProduto();

        produto.Inativar();
        Assert.False(produto.Ativo);

        produto.Ativar();
        Assert.True(produto.Ativo);
    }
}
