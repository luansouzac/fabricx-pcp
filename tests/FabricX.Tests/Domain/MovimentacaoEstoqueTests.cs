using FabricX.Domain.Entities;
using FabricX.Domain.Enums;
using FabricX.Domain.Exceptions;

namespace FabricX.Tests.Domain;

public class MovimentacaoEstoqueTests
{
    [Fact]
    public void Construtor_DadosValidos_PreencheCampos()
    {
        var movimentacao = new MovimentacaoEstoque(3, TipoMovimentacao.Saida, 2.5, "  Consumo  ", ordemId: 7);

        Assert.Equal(3, movimentacao.ProdutoId);
        Assert.Equal(7, movimentacao.OrdemId);
        Assert.Equal(TipoMovimentacao.Saida, movimentacao.Tipo);
        Assert.Equal(2.5, movimentacao.Quantidade);
        Assert.Equal("Consumo", movimentacao.Motivo);
        Assert.True(movimentacao.DataHora > DateTime.UtcNow.AddMinutes(-1));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("   ")]
    public void Construtor_MotivoVazio_GuardaNulo(string? motivo)
    {
        var movimentacao = new MovimentacaoEstoque(1, TipoMovimentacao.Entrada, 1, motivo);

        Assert.Null(movimentacao.Motivo);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(double.NaN)]
    public void Construtor_QuantidadeInvalida_LancaDomainException(double quantidade)
    {
        Assert.Throws<DomainException>(() => new MovimentacaoEstoque(1, TipoMovimentacao.Entrada, quantidade));
    }

    [Fact]
    public void Construtor_TipoInvalido_LancaDomainException()
    {
        Assert.Throws<DomainException>(() => new MovimentacaoEstoque(1, (TipoMovimentacao)99, 1));
    }
}
