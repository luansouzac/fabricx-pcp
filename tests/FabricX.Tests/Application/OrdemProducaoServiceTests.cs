using FabricX.Application.DTOs.Ordens;
using FabricX.Application.Exceptions;
using FabricX.Application.Services;
using FabricX.Domain.Entities;
using FabricX.Domain.Enums;
using FabricX.Domain.Exceptions;
using FabricX.Tests.Fakes;

namespace FabricX.Tests.Application;

public class OrdemProducaoServiceTests
{
    private static readonly CancellationToken Ct = CancellationToken.None;
    private static DateOnly Hoje => DateOnly.FromDateTime(DateTime.Today);

    private readonly OrdemProducaoRepositoryFake _ordens = new();
    private readonly ProdutoRepositoryFake _produtos = new();
    private readonly ItemEstruturaRepositoryFake _estruturas = new();
    private readonly MovimentacaoEstoqueRepositoryFake _movimentacoes = new();
    private readonly OrdemProducaoService _service;

    private Produto _tampinha = null!;
    private Produto _garrafa = null!;
    private Produto _garrafaDagua = null!;

    public OrdemProducaoServiceTests()
    {
        _service = new OrdemProducaoService(_ordens, _produtos, _estruturas, _movimentacoes);
    }

    private async Task<Produto> CriarProdutoAsync(string codigo, TipoProduto tipo, double estoque)
    {
        var produto = new Produto(codigo, codigo, tipo, "UN", estoque);
        await _produtos.AdicionarAsync(produto, Ct);
        return produto;
    }

    private async Task PrepararGarrafaDaguaAsync(double estoqueTampinha = 100, double estoqueGarrafa = 100)
    {
        _tampinha = await CriarProdutoAsync("TP", TipoProduto.MateriaPrima, estoqueTampinha);
        _garrafa = await CriarProdutoAsync("GF", TipoProduto.MateriaPrima, estoqueGarrafa);
        _garrafaDagua = await CriarProdutoAsync("GA", TipoProduto.Acabado, 0);
        await _estruturas.AdicionarAsync(new ItemEstrutura(_garrafaDagua.Id, _tampinha.Id, 1), Ct);
        await _estruturas.AdicionarAsync(new ItemEstrutura(_garrafaDagua.Id, _garrafa.Id, 1), Ct);
    }

    private static CriarOrdemProducaoDto Dto(Produto produto, double quantidade = 60) =>
        new() { ProdutoId = produto.Id, Quantidade = quantidade, PrazoEntrega = Hoje.AddDays(7) };

    [Fact]
    public async Task Criar_DadosValidos_GeraNumeroSequencialDoAno()
    {
        await PrepararGarrafaDaguaAsync();

        var primeira = await _service.CriarAsync(Dto(_garrafaDagua, 10), Ct);
        var segunda = await _service.CriarAsync(Dto(_garrafaDagua, 10), Ct);

        Assert.Equal($"OP-{DateTime.Now.Year}-0001", primeira.Numero);
        Assert.Equal($"OP-{DateTime.Now.Year}-0002", segunda.Numero);
        Assert.Equal(StatusOrdem.Planejada, primeira.Status);
        Assert.Equal("GA", primeira.ProdutoCodigo);
    }

    [Fact]
    public async Task Criar_ProdutoInexistente_LancaNotFoundException()
    {
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _service.CriarAsync(new CriarOrdemProducaoDto { ProdutoId = 99, Quantidade = 1, PrazoEntrega = Hoje }, Ct));
    }

    [Fact]
    public async Task Criar_ProdutoInativo_LancaDomainException()
    {
        var produto = await CriarProdutoAsync("GA", TipoProduto.Acabado, 0);
        produto.Inativar();

        await Assert.ThrowsAsync<DomainException>(() => _service.CriarAsync(Dto(produto), Ct));
    }

    [Fact]
    public async Task Criar_PrazoNoPassado_LancaDomainException()
    {
        var produto = await CriarProdutoAsync("GA", TipoProduto.Acabado, 0);
        var dto = Dto(produto);
        dto.PrazoEntrega = Hoje.AddDays(-1);

        await Assert.ThrowsAsync<DomainException>(() => _service.CriarAsync(dto, Ct));
    }

    [Fact]
    public async Task Criar_ProdutoSemEstrutura_NaoChecaEstoque()
    {
        var produto = await CriarProdutoAsync("GA", TipoProduto.Acabado, 0);

        var ordem = await _service.CriarAsync(Dto(produto, 1000), Ct);

        Assert.Equal(StatusOrdem.Planejada, ordem.Status);
    }

    [Fact]
    public async Task Criar_SaldoInsuficienteDeComponente_RecusaSemGravarAOrdem()
    {
        await PrepararGarrafaDaguaAsync(estoqueTampinha: 50);

        var erro = await Assert.ThrowsAsync<DomainException>(() => _service.CriarAsync(Dto(_garrafaDagua, 60), Ct));

        Assert.Equal("Estoque insuficiente de TP: disponível 50, necessário 60.", erro.Message);
        Assert.Empty(_ordens.Ordens);
    }

    [Fact]
    public async Task Alterar_QuantidadeAcimaDoSaldo_Recusa()
    {
        await PrepararGarrafaDaguaAsync(estoqueTampinha: 50);
        var criada = await _service.CriarAsync(Dto(_garrafaDagua, 10), Ct);

        await Assert.ThrowsAsync<DomainException>(() => _service.AlterarAsync(criada.Id,
            new AlterarOrdemProducaoDto { Quantidade = 60, PrazoEntrega = Hoje.AddDays(3), Prioridade = Prioridade.Alta }, Ct));
    }

    [Fact]
    public async Task Alterar_OrdemPlanejada_AtualizaCampos()
    {
        await PrepararGarrafaDaguaAsync();
        var criada = await _service.CriarAsync(Dto(_garrafaDagua, 10), Ct);

        var alterada = await _service.AlterarAsync(criada.Id,
            new AlterarOrdemProducaoDto { Quantidade = 20, PrazoEntrega = Hoje.AddDays(3), Prioridade = Prioridade.Urgente }, Ct);

        Assert.Equal(20, alterada.Quantidade);
        Assert.Equal(Prioridade.Urgente, alterada.Prioridade);
    }

    [Fact]
    public async Task Avancar_PlanejadaParaProgramada_NaoMexeNoEstoque()
    {
        await PrepararGarrafaDaguaAsync();
        var criada = await _service.CriarAsync(Dto(_garrafaDagua), Ct);

        var avancada = await _service.AvancarAsync(criada.Id, Ct);

        Assert.Equal(StatusOrdem.Programada, avancada.Status);
        Assert.Equal(100, _tampinha.EstoqueAtual);
        Assert.Empty(_movimentacoes.Registradas);
    }

    [Fact]
    public async Task Avancar_ParaEmProducao_BaixaComponentesELigaMovimentacoesAOrdem()
    {
        await PrepararGarrafaDaguaAsync();
        var criada = await _service.CriarAsync(Dto(_garrafaDagua), Ct);
        await _service.AvancarAsync(criada.Id, Ct);

        var iniciada = await _service.AvancarAsync(criada.Id, Ct);

        Assert.Equal(StatusOrdem.EmProducao, iniciada.Status);
        Assert.Equal(40, _tampinha.EstoqueAtual);
        Assert.Equal(40, _garrafa.EstoqueAtual);
        Assert.Equal(2, _movimentacoes.Registradas.Count);
        Assert.All(_movimentacoes.Registradas, m =>
        {
            Assert.Equal(TipoMovimentacao.Saida, m.Tipo);
            Assert.Equal(60, m.Quantidade);
            Assert.Equal(criada.Id, m.OrdemId);
        });
    }

    [Fact]
    public async Task Avancar_ParaEmProducaoSemSaldoDeUmComponente_RecusaESemAlterarNenhumSaldo()
    {
        await PrepararGarrafaDaguaAsync();
        var criada = await _service.CriarAsync(Dto(_garrafaDagua), Ct);
        await _service.AvancarAsync(criada.Id, Ct);
        await _produtos.ObterPorIdAsync(_garrafa.Id, Ct);
        _garrafa.RegistrarSaida(70);

        await Assert.ThrowsAsync<DomainException>(() => _service.AvancarAsync(criada.Id, Ct));

        Assert.Equal(100, _tampinha.EstoqueAtual);
        Assert.Equal(30, _garrafa.EstoqueAtual);
        Assert.Empty(_movimentacoes.Registradas);
        Assert.Equal(StatusOrdem.Programada, _ordens.Ordens.Single().Status);
    }

    [Fact]
    public async Task Avancar_ParaConcluida_SoEntraOAcabadoEPreencheConclusao()
    {
        await PrepararGarrafaDaguaAsync();
        var criada = await _service.CriarAsync(Dto(_garrafaDagua), Ct);
        await _service.AvancarAsync(criada.Id, Ct);
        await _service.AvancarAsync(criada.Id, Ct);

        var concluida = await _service.AvancarAsync(criada.Id, Ct);

        Assert.Equal(StatusOrdem.Concluida, concluida.Status);
        Assert.NotNull(concluida.ConcluidaEm);
        Assert.Equal(60, _garrafaDagua.EstoqueAtual);
        Assert.Equal(40, _tampinha.EstoqueAtual);
        Assert.Equal(40, _garrafa.EstoqueAtual);
        var entrada = _movimentacoes.Registradas.Last();
        Assert.Equal(TipoMovimentacao.Entrada, entrada.Tipo);
        Assert.Equal(criada.Id, entrada.OrdemId);
    }

    [Fact]
    public async Task Avancar_OrdemConcluida_LancaDomainException()
    {
        await PrepararGarrafaDaguaAsync();
        var criada = await _service.CriarAsync(Dto(_garrafaDagua), Ct);
        for (var i = 0; i < 3; i++)
            await _service.AvancarAsync(criada.Id, Ct);

        await Assert.ThrowsAsync<DomainException>(() => _service.AvancarAsync(criada.Id, Ct));
    }

    [Fact]
    public async Task Cancelar_OrdemPlanejada_Cancela()
    {
        await PrepararGarrafaDaguaAsync();
        var criada = await _service.CriarAsync(Dto(_garrafaDagua), Ct);

        var cancelada = await _service.CancelarAsync(criada.Id, Ct);

        Assert.Equal(StatusOrdem.Cancelada, cancelada.Status);
    }

    [Fact]
    public async Task Cancelar_OrdemEmProducao_LancaDomainException()
    {
        await PrepararGarrafaDaguaAsync();
        var criada = await _service.CriarAsync(Dto(_garrafaDagua), Ct);
        await _service.AvancarAsync(criada.Id, Ct);
        await _service.AvancarAsync(criada.Id, Ct);

        await Assert.ThrowsAsync<DomainException>(() => _service.CancelarAsync(criada.Id, Ct));
    }

    [Fact]
    public async Task ObterPorId_Inexistente_LancaNotFoundException()
    {
        await Assert.ThrowsAsync<NotFoundException>(() => _service.ObterPorIdAsync(99, Ct));
    }

    [Fact]
    public async Task Listar_FiltraPorStatus()
    {
        await PrepararGarrafaDaguaAsync();
        var primeira = await _service.CriarAsync(Dto(_garrafaDagua, 10), Ct);
        await _service.CriarAsync(Dto(_garrafaDagua, 10), Ct);
        await _service.AvancarAsync(primeira.Id, Ct);

        var programadas = await _service.ListarAsync(StatusOrdem.Programada, Ct);

        Assert.Equal([primeira.Id], programadas.Select(o => o.Id));
        Assert.Equal(2, (await _service.ListarAsync(null, Ct)).Count);
    }
}
