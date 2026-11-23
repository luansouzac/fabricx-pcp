using FabricX.Domain.Entities;
using FabricX.Domain.Enums;
using FabricX.Infrastructure.Persistence;
using FabricX.Infrastructure.Repositories;
using FabricX.Tests.Fakes;
using Microsoft.EntityFrameworkCore;

namespace FabricX.Tests.Infrastructure;

public class ApontamentoRepositoryTests : IDisposable
{
    private static readonly CancellationToken Ct = CancellationToken.None;

    private readonly BancoSqliteFixture _banco = new();

    public void Dispose() => _banco.Dispose();

    private static async Task<(OrdemProducao Ordem, Programacao Programacao)> PrepararAsync(AppDbContext contexto, string numero = "OP-2026-0001")
    {
        var produto = new Produto($"P-{numero}", "Kit", TipoProduto.Acabado);
        await new ProdutoRepository(contexto).AdicionarAsync(produto, Ct);
        var recurso = new Recurso($"R-{numero}", TipoRecurso.Maquina, 8);
        await new RecursoRepository(contexto).AdicionarAsync(recurso, Ct);
        var etapa = new RoteiroEtapa(produto.Id, recurso.Id, 1, "Montagem", 2, 10);
        await new RoteiroEtapaRepository(contexto).AdicionarAsync(etapa, Ct);
        var ordem = new OrdemProducao(numero, produto.Id, 60, DateOnly.FromDateTime(DateTime.Today.AddDays(30)));
        await new OrdemProducaoRepository(contexto).AdicionarAsync(ordem, Ct);
        var inicio = DateTime.Today.AddDays(2).AddHours(8);
        var programacao = new Programacao(ordem.Id, etapa.Id, recurso.Id, inicio, inicio.AddHours(1));
        await new ProgramacaoRepository(contexto).AdicionarAsync(programacao, Ct);
        return (ordem, programacao);
    }

    [Fact]
    public async Task Adicionar_PersisteNoEsquemaOficial()
    {
        await using var contexto = _banco.CriarContexto();
        var (_, programacao) = await PrepararAsync(contexto);
        var apontamento = new Apontamento(programacao.Id, 18, 2, 5, "turno A");

        await new ApontamentoRepository(contexto).AdicionarAsync(apontamento, Ct);

        Assert.True(apontamento.Id > 0);
        await using var outro = _banco.CriarContexto();
        var lido = await outro.Apontamentos.SingleAsync(Ct);
        Assert.Equal(18, lido.QtdProduzida);
        Assert.Equal(2, lido.QtdDefeituosa);
        Assert.Equal("turno A", lido.Observacao);
    }

    [Fact]
    public async Task Adicionar_GravaApontamentoEStatusDaProgramacaoNaMesmaOperacao()
    {
        await using var contexto = _banco.CriarContexto();
        var (_, programacao) = await PrepararAsync(contexto);

        programacao.Iniciar();
        await new ApontamentoRepository(contexto).AdicionarAsync(new Apontamento(programacao.Id, 5, 0, 5), Ct);

        await using var outro = _banco.CriarContexto();
        Assert.Equal(StatusProgramacao.EmAndamento, (await outro.Programacoes.SingleAsync(Ct)).Status);
        Assert.Equal(1, await outro.Apontamentos.CountAsync(Ct));
    }

    [Fact]
    public async Task Listar_PorProgramacaoPorOrdemEPorPeriodo()
    {
        await using var contexto = _banco.CriarContexto();
        var (ordem1, prog1) = await PrepararAsync(contexto, "OP-2026-0001");
        var (_, prog2) = await PrepararAsync(contexto, "OP-2026-0002");
        var repositorio = new ApontamentoRepository(contexto);
        await repositorio.AdicionarAsync(new Apontamento(prog1.Id, 5, 0, 5), Ct);
        await repositorio.AdicionarAsync(new Apontamento(prog2.Id, 7, 0, 5), Ct);

        Assert.Single(await repositorio.ListarPorProgramacaoAsync(prog1.Id, Ct));
        Assert.Single(await repositorio.ListarPorOrdemAsync(ordem1.Id, Ct));
        Assert.Equal(2, (await repositorio.ListarAsync(null, null, Ct)).Count);
        Assert.Equal(2, (await repositorio.ListarAsync(DateTime.Today, DateTime.Today.AddDays(1), Ct)).Count);
        Assert.Empty(await repositorio.ListarAsync(DateTime.Today.AddDays(1), null, Ct));
    }
}
