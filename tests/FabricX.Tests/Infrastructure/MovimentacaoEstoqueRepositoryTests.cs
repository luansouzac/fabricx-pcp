using FabricX.Domain.Entities;
using FabricX.Domain.Enums;
using FabricX.Infrastructure.Persistence;
using FabricX.Infrastructure.Repositories;
using FabricX.Tests.Fakes;
using Microsoft.EntityFrameworkCore;

namespace FabricX.Tests.Infrastructure;

public class MovimentacaoEstoqueRepositoryTests : IDisposable
{
    private static readonly CancellationToken Ct = CancellationToken.None;

    private readonly BancoSqliteFixture _banco = new();

    public void Dispose() => _banco.Dispose();

    private static async Task<Produto> CriarProdutoAsync(AppDbContext contexto, double estoque = 10)
    {
        var produto = new Produto("P001", "Parafuso", TipoProduto.MateriaPrima, "UN", estoqueAtual: estoque);
        await new ProdutoRepository(contexto).AdicionarAsync(produto, Ct);
        return produto;
    }

    [Theory]
    [InlineData(TipoMovimentacao.Entrada, "ENTRADA")]
    [InlineData(TipoMovimentacao.Saida, "SAIDA")]
    public async Task Adicionar_GravaTipoComoTextoDoCheck(TipoMovimentacao tipo, string esperado)
    {
        await using var contexto = _banco.CriarContexto();
        var produto = await CriarProdutoAsync(contexto);

        await new MovimentacaoEstoqueRepository(contexto).AdicionarAsync(new MovimentacaoEstoque(produto.Id, tipo, 1, "teste"), Ct);

        var texto = await contexto.Database.SqlQuery<string>($"SELECT tipo AS Value FROM movimentacao_estoque").SingleAsync(Ct);
        Assert.Equal(esperado, texto);
    }

    [Fact]
    public async Task Adicionar_GravaMovimentacaoESaldoNaMesmaOperacao()
    {
        await using var contexto = _banco.CriarContexto();
        var produto = await CriarProdutoAsync(contexto, estoque: 10);

        produto.Movimentar(TipoMovimentacao.Saida, 4);
        await new MovimentacaoEstoqueRepository(contexto).AdicionarAsync(new MovimentacaoEstoque(produto.Id, TipoMovimentacao.Saida, 4), Ct);

        await using var outro = _banco.CriarContexto();
        Assert.Equal(6, (await outro.Produtos.SingleAsync(Ct)).EstoqueAtual);
        Assert.Equal(1, await outro.MovimentacoesEstoque.CountAsync(Ct));
    }

    [Fact]
    public async Task ListarPorProduto_OrdemDecrescenteDeDataESoDoProduto()
    {
        await using var contexto = _banco.CriarContexto();
        var produto = await CriarProdutoAsync(contexto);
        var repositorio = new MovimentacaoEstoqueRepository(contexto);
        await repositorio.AdicionarAsync(new MovimentacaoEstoque(produto.Id, TipoMovimentacao.Entrada, 1, "primeira"), Ct);
        await repositorio.AdicionarAsync(new MovimentacaoEstoque(produto.Id, TipoMovimentacao.Saida, 1, "segunda"), Ct);

        var lista = await repositorio.ListarPorProdutoAsync(produto.Id, Ct);

        Assert.Equal(["segunda", "primeira"], lista.Select(m => m.Motivo));
        Assert.Empty(await repositorio.ListarPorProdutoAsync(999, Ct));
    }
}
