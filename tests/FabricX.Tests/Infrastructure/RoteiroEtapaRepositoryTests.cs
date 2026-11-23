using FabricX.Domain.Entities;
using FabricX.Domain.Enums;
using FabricX.Infrastructure.Persistence;
using FabricX.Infrastructure.Repositories;
using FabricX.Tests.Fakes;
using Microsoft.EntityFrameworkCore;

namespace FabricX.Tests.Infrastructure;

public class RoteiroEtapaRepositoryTests : IDisposable
{
    private static readonly CancellationToken Ct = CancellationToken.None;

    private readonly BancoSqliteFixture _banco = new();

    public void Dispose() => _banco.Dispose();

    private static async Task<(Produto Produto, Recurso Recurso)> PrepararAsync(AppDbContext contexto)
    {
        var produto = new Produto("EP-003", "Garrafa d'água", TipoProduto.EmProcesso);
        var recurso = new Recurso("Montadora", TipoRecurso.Maquina, 8);
        await new ProdutoRepository(contexto).AdicionarAsync(produto, Ct);
        await new RecursoRepository(contexto).AdicionarAsync(recurso, Ct);
        return (produto, recurso);
    }

    [Fact]
    public async Task Adicionar_PersisteCicloEUnidadesPorCicloNoEsquemaOficial()
    {
        await using var contexto = _banco.CriarContexto();
        var (produto, recurso) = await PrepararAsync(contexto);
        var etapa = new RoteiroEtapa(produto.Id, recurso.Id, 1, "Montagem", 2, 10);

        await new RoteiroEtapaRepository(contexto).AdicionarAsync(etapa, Ct);

        Assert.True(etapa.Id > 0);
        await using var outro = _banco.CriarContexto();
        var lida = await outro.EtapasRoteiro.SingleAsync(Ct);
        Assert.Equal(2, lida.TempoCicloMin);
        Assert.Equal(10, lida.UnidadesPorCiclo);
    }

    [Fact]
    public async Task Adicionar_SequenciaRepetidaNoMesmoProduto_ViolaUnique()
    {
        await using var contexto = _banco.CriarContexto();
        var (produto, recurso) = await PrepararAsync(contexto);
        var repositorio = new RoteiroEtapaRepository(contexto);
        await repositorio.AdicionarAsync(new RoteiroEtapa(produto.Id, recurso.Id, 1, "A", 2), Ct);

        await Assert.ThrowsAsync<DbUpdateException>(() =>
            repositorio.AdicionarAsync(new RoteiroEtapa(produto.Id, recurso.Id, 1, "B", 2), Ct));
    }

    [Fact]
    public async Task ListarPorProduto_OrdenaPorSequenciaEFiltraOProduto()
    {
        await using var contexto = _banco.CriarContexto();
        var (produto, recurso) = await PrepararAsync(contexto);
        var repositorio = new RoteiroEtapaRepository(contexto);
        await repositorio.AdicionarAsync(new RoteiroEtapa(produto.Id, recurso.Id, 2, "Segunda", 2), Ct);
        await repositorio.AdicionarAsync(new RoteiroEtapa(produto.Id, recurso.Id, 1, "Primeira", 2), Ct);

        Assert.Equal(["Primeira", "Segunda"], (await repositorio.ListarPorProdutoAsync(produto.Id, Ct)).Select(e => e.Nome));
        Assert.Empty(await repositorio.ListarPorProdutoAsync(999, Ct));
    }

    [Fact]
    public async Task AtualizarERemover_PersistemAMudanca()
    {
        await using var contexto = _banco.CriarContexto();
        var (produto, recurso) = await PrepararAsync(contexto);
        var repositorio = new RoteiroEtapaRepository(contexto);
        var etapa = new RoteiroEtapa(produto.Id, recurso.Id, 1, "A", 2);
        await repositorio.AdicionarAsync(etapa, Ct);

        etapa.Atualizar(recurso.Id, 1, "A", 5, 3);
        await repositorio.AtualizarAsync(etapa, Ct);
        await using (var outro = _banco.CriarContexto())
            Assert.Equal(3, (await outro.EtapasRoteiro.SingleAsync(Ct)).UnidadesPorCiclo);

        await repositorio.RemoverAsync(etapa, Ct);
        await using (var outro = _banco.CriarContexto())
            Assert.Empty(await outro.EtapasRoteiro.ToListAsync(Ct));
    }
}
