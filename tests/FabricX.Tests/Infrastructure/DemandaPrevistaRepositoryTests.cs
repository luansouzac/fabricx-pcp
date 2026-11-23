using FabricX.Domain.Entities;
using FabricX.Domain.Enums;
using FabricX.Infrastructure.Repositories;
using FabricX.Tests.Fakes;
using Microsoft.EntityFrameworkCore;

namespace FabricX.Tests.Infrastructure;

public class DemandaPrevistaRepositoryTests : IDisposable
{
    private static readonly CancellationToken Ct = CancellationToken.None;

    private readonly BancoSqliteFixture _banco = new();

    public void Dispose() => _banco.Dispose();

    private static async Task<Produto> CriarProdutoAsync(FabricX.Infrastructure.Persistence.AppDbContext contexto)
    {
        var produto = new Produto("PA-003", "Kit", TipoProduto.Acabado);
        await new ProdutoRepository(contexto).AdicionarAsync(produto, Ct);
        return produto;
    }

    [Fact]
    public async Task Adicionar_PersisteNoEsquemaOficialComOCheckDoPeriodo()
    {
        await using var contexto = _banco.CriarContexto();
        var produto = await CriarProdutoAsync(contexto);
        var demanda = new DemandaPrevista(produto.Id, "2026-12", 100);

        await new DemandaPrevistaRepository(contexto).AdicionarAsync(demanda, Ct);

        Assert.True(demanda.Id > 0);
        await using var outro = _banco.CriarContexto();
        Assert.Equal("2026-12", (await outro.DemandasPrevistas.SingleAsync(Ct)).Periodo);
    }

    [Fact]
    public async Task Adicionar_ProdutoEPeriodoRepetidos_ViolaUnique()
    {
        await using var contexto = _banco.CriarContexto();
        var produto = await CriarProdutoAsync(contexto);
        var repositorio = new DemandaPrevistaRepository(contexto);
        await repositorio.AdicionarAsync(new DemandaPrevista(produto.Id, "2026-12", 100), Ct);

        await Assert.ThrowsAsync<DbUpdateException>(() =>
            repositorio.AdicionarAsync(new DemandaPrevista(produto.Id, "2026-12", 5), Ct));
    }

    [Fact]
    public async Task ListarObterAtualizarERemover()
    {
        await using var contexto = _banco.CriarContexto();
        var produto = await CriarProdutoAsync(contexto);
        var repositorio = new DemandaPrevistaRepository(contexto);
        var demanda = new DemandaPrevista(produto.Id, "2026-12", 100);
        await repositorio.AdicionarAsync(demanda, Ct);
        await repositorio.AdicionarAsync(new DemandaPrevista(produto.Id, "2027-01", 50), Ct);

        Assert.Single(await repositorio.ListarAsync("2026-12", Ct));
        Assert.Equal(2, (await repositorio.ListarAsync(null, Ct)).Count);
        Assert.NotNull(await repositorio.ObterPorProdutoEPeriodoAsync(produto.Id, "2027-01", Ct));
        Assert.Null(await repositorio.ObterPorProdutoEPeriodoAsync(produto.Id, "2030-01", Ct));

        demanda.Atualizar("2026-12", 999);
        await repositorio.AtualizarAsync(demanda, Ct);
        await using (var outro = _banco.CriarContexto())
            Assert.Equal(999, (await outro.DemandasPrevistas.FirstAsync(d => d.Id == demanda.Id, Ct)).Quantidade);

        await repositorio.RemoverAsync(demanda, Ct);
        Assert.Single(await repositorio.ListarAsync(null, Ct));
    }
}
