using FabricX.Domain.Entities;
using FabricX.Domain.Enums;
using FabricX.Infrastructure.Persistence;
using FabricX.Infrastructure.Repositories;
using FabricX.Tests.Fakes;
using Microsoft.EntityFrameworkCore;

namespace FabricX.Tests.Infrastructure;

public class ItemEstruturaRepositoryTests : IDisposable
{
    private static readonly CancellationToken Ct = CancellationToken.None;

    private readonly BancoSqliteFixture _banco = new();

    public void Dispose() => _banco.Dispose();

    private static async Task<(Produto Pai, Produto Componente)> CriarParAsync(AppDbContext contexto)
    {
        var produtos = new ProdutoRepository(contexto);
        var pai = new Produto("GA", "Garrafa d'água", TipoProduto.Acabado);
        var componente = new Produto("TP", "Tampinha", TipoProduto.MateriaPrima);
        await produtos.AdicionarAsync(pai, Ct);
        await produtos.AdicionarAsync(componente, Ct);
        return (pai, componente);
    }

    [Fact]
    public async Task Adicionar_GeraIdEPersisteNoEsquemaOficial()
    {
        await using var contexto = _banco.CriarContexto();
        var (pai, componente) = await CriarParAsync(contexto);
        var item = new ItemEstrutura(pai.Id, componente.Id, 2);

        await new ItemEstruturaRepository(contexto).AdicionarAsync(item, Ct);

        Assert.True(item.Id > 0);
    }

    [Fact]
    public async Task Adicionar_ParRepetido_ViolaUnique()
    {
        await using var contexto = _banco.CriarContexto();
        var (pai, componente) = await CriarParAsync(contexto);
        var repositorio = new ItemEstruturaRepository(contexto);
        await repositorio.AdicionarAsync(new ItemEstrutura(pai.Id, componente.Id, 1), Ct);

        await Assert.ThrowsAsync<DbUpdateException>(() =>
            repositorio.AdicionarAsync(new ItemEstrutura(pai.Id, componente.Id, 3), Ct));
    }

    [Fact]
    public async Task ObterEListar_FiltramPorPai()
    {
        await using var contexto = _banco.CriarContexto();
        var (pai, componente) = await CriarParAsync(contexto);
        var repositorio = new ItemEstruturaRepository(contexto);
        await repositorio.AdicionarAsync(new ItemEstrutura(pai.Id, componente.Id, 1), Ct);

        Assert.NotNull(await repositorio.ObterAsync(pai.Id, componente.Id, Ct));
        Assert.Null(await repositorio.ObterAsync(componente.Id, pai.Id, Ct));
        Assert.Single(await repositorio.ListarPorPaiAsync(pai.Id, Ct));
        Assert.Empty(await repositorio.ListarPorPaiAsync(componente.Id, Ct));
        Assert.Single(await repositorio.ListarTodosAsync(Ct));
    }

    [Fact]
    public async Task AtualizarERemover_PersistemAMudanca()
    {
        await using var contexto = _banco.CriarContexto();
        var (pai, componente) = await CriarParAsync(contexto);
        var repositorio = new ItemEstruturaRepository(contexto);
        var item = new ItemEstrutura(pai.Id, componente.Id, 1);
        await repositorio.AdicionarAsync(item, Ct);

        item.AlterarQuantidade(5);
        await repositorio.AtualizarAsync(item, Ct);
        await using (var outro = _banco.CriarContexto())
            Assert.Equal(5, (await outro.ItensEstrutura.SingleAsync(Ct)).Quantidade);

        await repositorio.RemoverAsync(item, Ct);
        await using (var outro = _banco.CriarContexto())
            Assert.Empty(await outro.ItensEstrutura.ToListAsync(Ct));
    }
}
