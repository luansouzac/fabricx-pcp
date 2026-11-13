using System.ComponentModel.DataAnnotations;
using FabricX.Application.DTOs.Produtos;
using FabricX.Domain.Entities;
using FabricX.Domain.Enums;

namespace FabricX.Tests.Application;

public class ProdutoDtoTests
{
    private static List<ValidationResult> Validar(object dto)
    {
        var resultados = new List<ValidationResult>();
        Validator.TryValidateObject(dto, new ValidationContext(dto), resultados, validateAllProperties: true);
        return resultados;
    }

    private static CriarProdutoDto CriarValido() => new()
    {
        Codigo = "P001",
        Nome = "Parafuso",
        Tipo = TipoProduto.MateriaPrima,
        UnidadeMedida = "UN"
    };

    [Fact]
    public void CriarProdutoDto_DadosValidos_PassaNaValidacao()
    {
        Assert.Empty(Validar(CriarValido()));
    }

    [Fact]
    public void CriarProdutoDto_CodigoVazio_Falha()
    {
        var dto = CriarValido();
        dto.Codigo = "";

        Assert.NotEmpty(Validar(dto));
    }

    [Fact]
    public void CriarProdutoDto_EstoqueNegativo_Falha()
    {
        var dto = CriarValido();
        dto.EstoqueAtual = -1;

        Assert.NotEmpty(Validar(dto));
    }

    [Fact]
    public void CriarProdutoDto_TipoInexistente_Falha()
    {
        var dto = CriarValido();
        dto.Tipo = (TipoProduto)99;

        Assert.NotEmpty(Validar(dto));
    }

    [Fact]
    public void AtualizarProdutoDto_NomeVazio_Falha()
    {
        var dto = new AtualizarProdutoDto { Nome = "", UnidadeMedida = "UN" };

        Assert.NotEmpty(Validar(dto));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-2)]
    public void MovimentarEstoqueDto_QuantidadeNaoPositiva_Falha(double quantidade)
    {
        var dto = new MovimentarEstoqueDto { Tipo = TipoMovimentacao.Entrada, Quantidade = quantidade };

        Assert.NotEmpty(Validar(dto));
    }

    [Fact]
    public void MovimentarEstoqueDto_DadosValidos_PassaNaValidacao()
    {
        var dto = new MovimentarEstoqueDto { Tipo = TipoMovimentacao.Saida, Quantidade = 3 };

        Assert.Empty(Validar(dto));
    }

    [Fact]
    public void ProdutoDto_De_CopiaCamposEIndicaAlerta()
    {
        var produto = new Produto("P001", "Parafuso", TipoProduto.MateriaPrima, "UN", estoqueAtual: 2, estoqueMinimo: 5);

        var dto = ProdutoDto.De(produto);

        Assert.Equal("P001", dto.Codigo);
        Assert.Equal(TipoProduto.MateriaPrima, dto.Tipo);
        Assert.Equal(2, dto.EstoqueAtual);
        Assert.True(dto.EstoqueAbaixoDoMinimo);
    }
}
