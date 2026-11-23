using FabricX.Domain.Entities;
using FabricX.Domain.Enums;
using FabricX.Infrastructure.Persistence;
using FabricX.Infrastructure.Repositories;
using FabricX.Tests.Fakes;
using Microsoft.EntityFrameworkCore;

namespace FabricX.Tests.Infrastructure;

public class ProgramacaoRepositoryTests : IDisposable
{
    private static readonly CancellationToken Ct = CancellationToken.None;
    private static readonly DateTime Dia = new(2026, 12, 1);

    private readonly BancoSqliteFixture _banco = new();

    public void Dispose() => _banco.Dispose();

    private static async Task<(OrdemProducao Ordem, RoteiroEtapa Etapa, Recurso Recurso)> PrepararAsync(AppDbContext contexto)
    {
        var produto = new Produto("PA-003", "Kit", TipoProduto.Acabado);
        var recurso = new Recurso("Montadora", TipoRecurso.Maquina, 8);
        await new ProdutoRepository(contexto).AdicionarAsync(produto, Ct);
        await new RecursoRepository(contexto).AdicionarAsync(recurso, Ct);
        var etapa = new RoteiroEtapa(produto.Id, recurso.Id, 1, "Montagem", 2, 10);
        await new RoteiroEtapaRepository(contexto).AdicionarAsync(etapa, Ct);
        var ordem = new OrdemProducao("OP-2026-0001", produto.Id, 60, DateOnly.FromDateTime(DateTime.Today.AddDays(30)));
        await new OrdemProducaoRepository(contexto).AdicionarAsync(ordem, Ct);
        return (ordem, etapa, recurso);
    }

    [Fact]
    public async Task Adicionar_PersisteNoEsquemaOficialComStatusEmTexto()
    {
        await using var contexto = _banco.CriarContexto();
        var (ordem, etapa, recurso) = await PrepararAsync(contexto);
        var programacao = new Programacao(ordem.Id, etapa.Id, recurso.Id, Dia.AddHours(8), Dia.AddHours(8).AddMinutes(12));

        await new ProgramacaoRepository(contexto).AdicionarAsync(programacao, Ct);

        Assert.True(programacao.Id > 0);
        var status = await contexto.Database.SqlQuery<string>($"SELECT status AS Value FROM programacao").SingleAsync(Ct);
        Assert.Equal("AGENDADA", status);
        await using var outro = _banco.CriarContexto();
        Assert.Equal(Dia.AddHours(8).AddMinutes(12), (await outro.Programacoes.SingleAsync(Ct)).FimPrevisto);
    }

    [Fact]
    public async Task Adicionar_MesmaEtapaNaMesmaOrdem_ViolaUnique()
    {
        await using var contexto = _banco.CriarContexto();
        var (ordem, etapa, recurso) = await PrepararAsync(contexto);
        var repositorio = new ProgramacaoRepository(contexto);
        await repositorio.AdicionarAsync(new Programacao(ordem.Id, etapa.Id, recurso.Id, Dia.AddHours(8), Dia.AddHours(9)), Ct);

        await Assert.ThrowsAsync<DbUpdateException>(() =>
            repositorio.AdicionarAsync(new Programacao(ordem.Id, etapa.Id, recurso.Id, Dia.AddHours(10), Dia.AddHours(11)), Ct));
    }

    [Fact]
    public async Task Listar_FiltraPorRecursoEPeriodoOrdenadoPorInicio()
    {
        await using var contexto = _banco.CriarContexto();
        var (ordem, etapa, recurso) = await PrepararAsync(contexto);
        var repositorio = new ProgramacaoRepository(contexto);
        await repositorio.AdicionarAsync(new Programacao(ordem.Id, etapa.Id, recurso.Id, Dia.AddHours(8), Dia.AddHours(9)), Ct);

        Assert.Single(await repositorio.ListarAsync(recurso.Id, null, null, Ct));
        Assert.Empty(await repositorio.ListarAsync(recurso.Id + 1, null, null, Ct));
        Assert.Single(await repositorio.ListarAsync(null, Dia, Dia.AddDays(1), Ct));
        Assert.Empty(await repositorio.ListarAsync(null, Dia.AddDays(1), null, Ct));
        Assert.Single(await repositorio.ListarPorOrdemAsync(ordem.Id, Ct));
        Assert.Single(await repositorio.ListarPorRecursoAsync(recurso.Id, Ct));
    }

    [Fact]
    public async Task AtualizarERemover_PersistemAMudanca()
    {
        await using var contexto = _banco.CriarContexto();
        var (ordem, etapa, recurso) = await PrepararAsync(contexto);
        var repositorio = new ProgramacaoRepository(contexto);
        var programacao = new Programacao(ordem.Id, etapa.Id, recurso.Id, Dia.AddHours(8), Dia.AddHours(9));
        await repositorio.AdicionarAsync(programacao, Ct);

        programacao.Iniciar();
        await repositorio.AtualizarAsync(programacao, Ct);
        await using (var outro = _banco.CriarContexto())
            Assert.Equal(StatusProgramacao.EmAndamento, (await outro.Programacoes.SingleAsync(Ct)).Status);

        await repositorio.RemoverAsync(programacao, Ct);
        await using (var outro = _banco.CriarContexto())
            Assert.Empty(await outro.Programacoes.ToListAsync(Ct));
    }
}
