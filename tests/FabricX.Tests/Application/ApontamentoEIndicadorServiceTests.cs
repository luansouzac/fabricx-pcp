using FabricX.Application.DTOs.Apontamentos;
using FabricX.Application.Exceptions;
using FabricX.Application.Services;
using FabricX.Domain.Entities;
using FabricX.Domain.Enums;
using FabricX.Domain.Exceptions;
using FabricX.Tests.Fakes;

namespace FabricX.Tests.Application;

public class ApontamentoEIndicadorServiceTests
{
    private static readonly CancellationToken Ct = CancellationToken.None;
    private static readonly DateTime Dia = DateTime.Today.AddDays(3);

    private readonly ProgramacaoRepositoryFake _programacoes = new();
    private readonly OrdemProducaoRepositoryFake _ordens = new();
    private readonly ProdutoRepositoryFake _produtos = new();
    private readonly RoteiroEtapaRepositoryFake _etapas = new();
    private readonly ApontamentoRepositoryFake _apontamentos;
    private readonly ApontamentoService _service;
    private readonly IndicadorService _indicadores;

    private Produto _produto = null!;
    private OrdemProducao _ordem = null!;
    private Programacao _programacao = null!;

    public ApontamentoEIndicadorServiceTests()
    {
        _apontamentos = new ApontamentoRepositoryFake(_programacoes);
        _service = new ApontamentoService(_apontamentos, _programacoes, _ordens);
        _indicadores = new IndicadorService(_apontamentos, _programacoes, _ordens, _etapas, _produtos, new SemCacheFake());
    }

    private async Task PrepararAsync(double quantidade = 60)
    {
        _produto = new Produto("EP-003", "Garrafa d'água", TipoProduto.EmProcesso);
        await _produtos.AdicionarAsync(_produto, Ct);
        var etapa = new RoteiroEtapa(_produto.Id, 1, 1, "Montagem", 2, 10);
        await _etapas.AdicionarAsync(etapa, Ct);
        _ordem = new OrdemProducao("OP-1", _produto.Id, quantidade, DateOnly.FromDateTime(Dia.AddDays(5)));
        await _ordens.AdicionarAsync(_ordem, Ct);
        _programacao = new Programacao(_ordem.Id, etapa.Id, 1, Dia.AddHours(8), Dia.AddHours(9));
        await _programacoes.AdicionarAsync(_programacao, Ct);
    }

    private static RegistrarApontamentoDto Dto(double produzida, double defeituosa = 0, double tempo = 10) =>
        new() { QuantidadeProduzida = produzida, QuantidadeDefeituosa = defeituosa, TempoGastoMin = tempo };

    [Fact]
    public async Task Registrar_PrimeiroApontamento_ColocaAProgramacaoEmAndamento()
    {
        await PrepararAsync();

        var apontamento = await _service.RegistrarAsync(_programacao.Id, Dto(20), Ct);

        Assert.Equal(StatusProgramacao.EmAndamento, apontamento.StatusProgramacao);
        Assert.Equal(_ordem.Id, apontamento.OrdemId);
    }

    [Fact]
    public async Task Registrar_SomaDaProduzidaAtingeAQuantidadeDaOrdem_ConcluiAProgramacao()
    {
        await PrepararAsync(60);
        await _service.RegistrarAsync(_programacao.Id, Dto(40), Ct);

        var ultimo = await _service.RegistrarAsync(_programacao.Id, Dto(20, defeituosa: 5), Ct);

        Assert.Equal(StatusProgramacao.Concluida, ultimo.StatusProgramacao);
    }

    [Fact]
    public async Task Registrar_DefeituosasNaoContamParaConcluir()
    {
        await PrepararAsync(60);

        var apontamento = await _service.RegistrarAsync(_programacao.Id, Dto(50, defeituosa: 20), Ct);

        Assert.Equal(StatusProgramacao.EmAndamento, apontamento.StatusProgramacao);
    }

    [Fact]
    public async Task Registrar_ProgramacaoConcluida_LancaDomainException()
    {
        await PrepararAsync(10);
        await _service.RegistrarAsync(_programacao.Id, Dto(10), Ct);

        await Assert.ThrowsAsync<DomainException>(() => _service.RegistrarAsync(_programacao.Id, Dto(1), Ct));
    }

    [Fact]
    public async Task Registrar_ValoresInvalidos_LancaDomainExceptionSemMudarStatus()
    {
        await PrepararAsync();

        await Assert.ThrowsAsync<DomainException>(() => _service.RegistrarAsync(_programacao.Id, Dto(0, 0), Ct));
        await Assert.ThrowsAsync<DomainException>(() => _service.RegistrarAsync(_programacao.Id, Dto(5, 0, 0), Ct));

        Assert.Equal(StatusProgramacao.Agendada, _programacao.Status);
        Assert.Empty(_apontamentos.Apontamentos);
    }

    [Fact]
    public async Task Registrar_ProgramacaoInexistente_LancaNotFoundException()
    {
        await Assert.ThrowsAsync<NotFoundException>(() => _service.RegistrarAsync(99, Dto(1), Ct));
    }

    [Fact]
    public async Task Listar_PorProgramacaoEPorOrdem_FuncionamEValidam404()
    {
        await PrepararAsync();
        await _service.RegistrarAsync(_programacao.Id, Dto(5), Ct);
        await _service.RegistrarAsync(_programacao.Id, Dto(6), Ct);

        Assert.Equal(2, (await _service.ListarPorProgramacaoAsync(_programacao.Id, Ct)).Count);
        Assert.Equal(2, (await _service.ListarPorOrdemAsync(_ordem.Id, Ct)).Count);
        await Assert.ThrowsAsync<NotFoundException>(() => _service.ListarPorProgramacaoAsync(99, Ct));
        await Assert.ThrowsAsync<NotFoundException>(() => _service.ListarPorOrdemAsync(99, Ct));
    }

    [Fact]
    public async Task Indicadores_BatemComOCasoCalculadoAMaoEFiltramPorProduto()
    {
        await PrepararAsync(100);
        await _service.RegistrarAsync(_programacao.Id, Dto(18, 2, 5), Ct);
        await _service.RegistrarAsync(_programacao.Id, Dto(25, 0, 8), Ct);

        var geral = await _indicadores.CalcularAsync(null, null, null, null, Ct);
        var doProduto = await _indicadores.CalcularAsync(null, null, _produto.Id, null, Ct);
        var outroProduto = await _indicadores.CalcularAsync(null, null, 999, null, Ct);

        Assert.Equal(2.0 / 45, geral.Qualidade!.Value, 10);
        Assert.Equal(10.0 / 13, geral.Eficiencia!.Value, 10);
        Assert.Equal(43 / (13.0 / 60), geral.ProdutividadePorHora!.Value, 10);
        Assert.Equal(43, geral.TotalProduzido);
        Assert.Equal(13, geral.TempoApontadoMin);
        Assert.Equal(geral.Eficiencia, doProduto.Eficiencia);
        Assert.Null(outroProduto.Eficiencia);
    }

    [Fact]
    public async Task Indicadores_SemDadosNoPeriodo_VoltamNulos()
    {
        await PrepararAsync();
        await _service.RegistrarAsync(_programacao.Id, Dto(5), Ct);
        var ontem = DateOnly.FromDateTime(DateTime.Today.AddDays(-5));

        var resultado = await _indicadores.CalcularAsync(ontem, ontem, null, null, Ct);

        Assert.Null(resultado.Qualidade);
        Assert.Null(resultado.Eficiencia);
        Assert.Null(resultado.ProdutividadePorHora);
        Assert.Null(resultado.CumprimentoDePrazo);
    }

    [Fact]
    public async Task Resumo_TrazOrdensPorStatusEEstoqueBaixo()
    {
        await PrepararAsync();
        var baixo = new Produto("MP-1", "Baixo", TipoProduto.MateriaPrima, "UN", 1, 10);
        await _produtos.AdicionarAsync(baixo, Ct);

        var resumo = await _indicadores.ObterResumoAsync(Ct);

        Assert.Equal(1, resumo.OrdensPorStatus.Single(o => o.Status == StatusOrdem.Planejada).Quantidade);
        Assert.Equal(0, resumo.OrdensPorStatus.Single(o => o.Status == StatusOrdem.Concluida).Quantidade);
        Assert.Equal(["MP-1"], resumo.ProdutosComEstoqueBaixo.Select(p => p.Codigo));
    }
}
