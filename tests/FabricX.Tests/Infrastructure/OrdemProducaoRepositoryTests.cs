using FabricX.Domain.Entities;
using FabricX.Domain.Enums;
using FabricX.Infrastructure.Persistence;
using FabricX.Infrastructure.Repositories;
using FabricX.Tests.Fakes;
using Microsoft.EntityFrameworkCore;

namespace FabricX.Tests.Infrastructure;

public class OrdemProducaoRepositoryTests : IDisposable
{
    private static readonly CancellationToken Ct = CancellationToken.None;
    private static DateOnly Hoje => DateOnly.FromDateTime(DateTime.Today);

    private readonly BancoSqliteFixture _banco = new();

    public void Dispose() => _banco.Dispose();

    private static async Task<Produto> CriarProdutoAsync(AppDbContext contexto)
    {
        var produto = new Produto("GA", "Garrafa d'água", TipoProduto.Acabado);
        await new ProdutoRepository(contexto).AdicionarAsync(produto, Ct);
        return produto;
    }

    [Fact]
    public async Task Adicionar_PersisteNoEsquemaOficialComTextosDosChecks()
    {
        await using var contexto = _banco.CriarContexto();
        var produto = await CriarProdutoAsync(contexto);
        var ordem = new OrdemProducao("OP-2026-0001", produto.Id, 10, Hoje.AddDays(5), Prioridade.Alta);
        ordem.Avancar();
        ordem.Avancar();

        await new OrdemProducaoRepository(contexto).AdicionarAsync(ordem, Ct);

        Assert.True(ordem.Id > 0);
        var status = await contexto.Database.SqlQuery<string>($"SELECT status AS Value FROM ordem_producao").SingleAsync(Ct);
        var prioridade = await contexto.Database.SqlQuery<string>($"SELECT prioridade AS Value FROM ordem_producao").SingleAsync(Ct);
        Assert.Equal("EM_PRODUCAO", status);
        Assert.Equal("ALTA", prioridade);
    }

    [Fact]
    public async Task Adicionar_NumeroRepetido_ViolaUnique()
    {
        await using var contexto = _banco.CriarContexto();
        var produto = await CriarProdutoAsync(contexto);
        var repositorio = new OrdemProducaoRepository(contexto);
        await repositorio.AdicionarAsync(new OrdemProducao("OP-2026-0001", produto.Id, 1, Hoje), Ct);

        await Assert.ThrowsAsync<DbUpdateException>(() =>
            repositorio.AdicionarAsync(new OrdemProducao("OP-2026-0001", produto.Id, 1, Hoje), Ct));
    }

    [Fact]
    public async Task ObterMaiorSequencialDoAno_ConsideraSoOAnoPedido()
    {
        await using var contexto = _banco.CriarContexto();
        var produto = await CriarProdutoAsync(contexto);
        var repositorio = new OrdemProducaoRepository(contexto);
        await repositorio.AdicionarAsync(new OrdemProducao("OP-2026-0003", produto.Id, 1, Hoje), Ct);
        await repositorio.AdicionarAsync(new OrdemProducao("OP-2026-0012", produto.Id, 1, Hoje), Ct);
        await repositorio.AdicionarAsync(new OrdemProducao("OP-2025-0099", produto.Id, 1, Hoje), Ct);

        Assert.Equal(12, await repositorio.ObterMaiorSequencialDoAnoAsync(2026, Ct));
        Assert.Equal(0, await repositorio.ObterMaiorSequencialDoAnoAsync(2030, Ct));
    }

    [Fact]
    public async Task Listar_FiltraPorStatusEOrdenaPorPrazo()
    {
        await using var contexto = _banco.CriarContexto();
        var produto = await CriarProdutoAsync(contexto);
        var repositorio = new OrdemProducaoRepository(contexto);
        await repositorio.AdicionarAsync(new OrdemProducao("OP-2026-0001", produto.Id, 1, Hoje.AddDays(9)), Ct);
        await repositorio.AdicionarAsync(new OrdemProducao("OP-2026-0002", produto.Id, 1, Hoje.AddDays(2)), Ct);
        var cancelada = new OrdemProducao("OP-2026-0003", produto.Id, 1, Hoje.AddDays(1));
        cancelada.Cancelar();
        await repositorio.AdicionarAsync(cancelada, Ct);

        var planejadas = await repositorio.ListarAsync(StatusOrdem.Planejada, Ct);

        Assert.Equal(["OP-2026-0002", "OP-2026-0001"], planejadas.Select(o => o.Numero));
        Assert.Equal(3, (await repositorio.ListarAsync(null, Ct)).Count);
    }
}
