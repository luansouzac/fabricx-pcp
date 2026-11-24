using FabricX.Application.DTOs.Demanda;
using FabricX.Application.Exceptions;
using FabricX.Application.Services;
using FabricX.Domain.Entities;
using FabricX.Domain.Enums;
using FabricX.Domain.Exceptions;
using FabricX.Tests.Fakes;

namespace FabricX.Tests.Application;

public class DemandaECapacidadeServiceTests
{
    private static readonly CancellationToken Ct = CancellationToken.None;

    private readonly DemandaPrevistaRepositoryFake _demandas = new();
    private readonly ProdutoRepositoryFake _produtos = new();
    private readonly RecursoRepositoryFake _recursos = new();
    private readonly ProgramacaoRepositoryFake _programacoes = new();
    private readonly RoteiroEtapaRepositoryFake _etapas = new();
    private readonly DemandaPrevistaService _service;
    private readonly CapacidadeService _capacidade;

    private Produto _garrafa = null!;
    private Produto _kit = null!;

    public DemandaECapacidadeServiceTests()
    {
        _service = new DemandaPrevistaService(_demandas, _produtos);
        _capacidade = new CapacidadeService(_recursos, _programacoes, _demandas, _etapas, new SemCacheFake());
    }

    private async Task PrepararAsync()
    {
        _garrafa = new Produto("EP-003", "Garrafa d'água", TipoProduto.EmProcesso);
        _kit = new Produto("PA-003", "Kit", TipoProduto.Acabado);
        await _produtos.AdicionarAsync(_garrafa, Ct);
        await _produtos.AdicionarAsync(_kit, Ct);
    }

    private static CriarDemandaPrevistaDto Dto(Produto produto, string periodo = "2026-12", double quantidade = 600) =>
        new() { ProdutoId = produto.Id, Periodo = periodo, Quantidade = quantidade };

    [Fact]
    public async Task Criar_DadosValidos_DevolveDemandaComProduto()
    {
        await PrepararAsync();

        var demanda = await _service.CriarAsync(Dto(_garrafa), Ct);

        Assert.Equal("EP-003", demanda.ProdutoCodigo);
        Assert.Equal("2026-12", demanda.Periodo);
    }

    [Fact]
    public async Task Criar_ProdutoERepetidoNoPeriodo_LancaDomainException()
    {
        await PrepararAsync();
        await _service.CriarAsync(Dto(_garrafa), Ct);

        await Assert.ThrowsAsync<DomainException>(() => _service.CriarAsync(Dto(_garrafa), Ct));
        await _service.CriarAsync(Dto(_garrafa, "2027-01"), Ct);
    }

    [Fact]
    public async Task Criar_PeriodoInvalido_LancaDomainException()
    {
        await PrepararAsync();

        await Assert.ThrowsAsync<DomainException>(() => _service.CriarAsync(Dto(_garrafa, "12/2026"), Ct));
    }

    [Fact]
    public async Task Criar_ProdutoInexistenteOuInativo_LancaNotFoundOuDomainException()
    {
        await PrepararAsync();
        _kit.Inativar();

        await Assert.ThrowsAsync<NotFoundException>(() => _service.CriarAsync(new CriarDemandaPrevistaDto { ProdutoId = 99, Periodo = "2026-12", Quantidade = 1 }, Ct));
        await Assert.ThrowsAsync<DomainException>(() => _service.CriarAsync(Dto(_kit), Ct));
    }

    [Fact]
    public async Task Atualizar_AlteraPeriodoEQuantidadeEProibeColidirComOutra()
    {
        await PrepararAsync();
        var primeira = await _service.CriarAsync(Dto(_garrafa, "2026-12"), Ct);
        await _service.CriarAsync(Dto(_garrafa, "2027-01"), Ct);

        var atualizada = await _service.AtualizarAsync(primeira.Id, new AtualizarDemandaPrevistaDto { Periodo = "2026-12", Quantidade = 900 }, Ct);

        Assert.Equal(900, atualizada.Quantidade);
        await Assert.ThrowsAsync<DomainException>(() => _service.AtualizarAsync(primeira.Id, new AtualizarDemandaPrevistaDto { Periodo = "2027-01", Quantidade = 1 }, Ct));
        await Assert.ThrowsAsync<NotFoundException>(() => _service.AtualizarAsync(99, new AtualizarDemandaPrevistaDto { Periodo = "2026-12", Quantidade = 1 }, Ct));
    }

    [Fact]
    public async Task Listar_FiltraPorPeriodoEValidaFormato()
    {
        await PrepararAsync();
        await _service.CriarAsync(Dto(_garrafa, "2026-12"), Ct);
        await _service.CriarAsync(Dto(_garrafa, "2027-01"), Ct);

        Assert.Single(await _service.ListarAsync("2026-12", Ct));
        Assert.Equal(2, (await _service.ListarAsync(null, Ct)).Count);
        await Assert.ThrowsAsync<DomainException>(() => _service.ListarAsync("xx", Ct));
    }

    [Fact]
    public async Task Remover_ExcluiEInexistenteLanca404()
    {
        await PrepararAsync();
        var criada = await _service.CriarAsync(Dto(_garrafa), Ct);

        await _service.RemoverAsync(criada.Id, Ct);

        Assert.Empty(_demandas.Demandas);
        await Assert.ThrowsAsync<NotFoundException>(() => _service.RemoverAsync(criada.Id, Ct));
    }

    [Fact]
    public async Task Capacidade_ComOsNumerosDoSeed_ClassificaOciosoEGargalo()
    {
        await PrepararAsync();
        var montadora = new Recurso("Montadora", TipoRecurso.Maquina, 8);
        var linha = new Recurso("Linha de embalagem", TipoRecurso.Linha, 8);
        await _recursos.AdicionarAsync(montadora, Ct);
        await _recursos.AdicionarAsync(linha, Ct);
        await _etapas.AdicionarAsync(new RoteiroEtapa(_garrafa.Id, montadora.Id, 1, "Montagem", 2, 10), Ct);
        await _etapas.AdicionarAsync(new RoteiroEtapa(_kit.Id, linha.Id, 1, "Embalagem", 2, 1), Ct);
        await _service.CriarAsync(Dto(_garrafa, "2026-12", 600), Ct);
        await _service.CriarAsync(Dto(_kit, "2026-12", 8000), Ct);

        var resultado = await _capacidade.CalcularAsync("2026-12", Ct);

        Assert.Equal(31, resultado.DiasNoPeriodo);
        var m = resultado.Recursos.Single(r => r.RecursoNome == "Montadora");
        Assert.Equal(248, m.CapacidadeHorasPeriodo);
        Assert.Equal(2, m.HorasNecessariasDemanda, 10);
        Assert.Equal(ClassificacaoCarga.Ocioso, m.Classificacao);
        var l = resultado.Recursos.Single(r => r.RecursoNome == "Linha de embalagem");
        Assert.Equal(16000.0 / 60, l.HorasNecessariasDemanda, 10);
        Assert.Equal(ClassificacaoCarga.Gargalo, l.Classificacao);
    }

    [Fact]
    public async Task Capacidade_IgnoraRecursoInativoEPeriodoInvalidoLanca400()
    {
        var inativo = new Recurso("Parada", TipoRecurso.Maquina, 8);
        inativo.Inativar();
        await _recursos.AdicionarAsync(inativo, Ct);

        Assert.Empty((await _capacidade.CalcularAsync("2026-12", Ct)).Recursos);
        await Assert.ThrowsAsync<DomainException>(() => _capacidade.CalcularAsync("2026-99", Ct));
    }

    [Fact]
    public async Task Capacidade_SemPeriodo_UsaOMesAtual()
    {
        var resultado = await _capacidade.CalcularAsync(null, Ct);

        Assert.Equal(DateTime.Today.ToString("yyyy-MM"), resultado.Periodo);
    }
}
