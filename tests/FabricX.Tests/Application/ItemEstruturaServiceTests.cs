using FabricX.Application.DTOs.Estrutura;
using FabricX.Application.Exceptions;
using FabricX.Application.Services;
using FabricX.Domain.Entities;
using FabricX.Domain.Enums;
using FabricX.Domain.Exceptions;
using FabricX.Tests.Fakes;

namespace FabricX.Tests.Application;

public class ItemEstruturaServiceTests
{
    private static readonly CancellationToken Ct = CancellationToken.None;

    private readonly ProdutoRepositoryFake _produtos = new();
    private readonly ItemEstruturaRepositoryFake _itens = new();
    private readonly ItemEstruturaService _service;

    public ItemEstruturaServiceTests()
    {
        _service = new ItemEstruturaService(_itens, _produtos);
    }

    private async Task<Produto> CriarProdutoAsync(string codigo, string nome, TipoProduto tipo = TipoProduto.MateriaPrima)
    {
        var produto = new Produto(codigo, nome, tipo);
        await _produtos.AdicionarAsync(produto, Ct);
        return produto;
    }

    private static AdicionarItemEstruturaDto Dto(Produto componente, double quantidade = 1) =>
        new() { ComponenteId = componente.Id, Quantidade = quantidade };

    [Fact]
    public async Task Adicionar_DadosValidos_DevolveItemComCodigoENomeDoComponente()
    {
        var pai = await CriarProdutoAsync("GA", "Garrafa d'água", TipoProduto.Acabado);
        var tampinha = await CriarProdutoAsync("TP", "Tampinha");

        var item = await _service.AdicionarAsync(pai.Id, Dto(tampinha, 2), Ct);

        Assert.Equal("TP", item.ComponenteCodigo);
        Assert.Equal("Tampinha", item.ComponenteNome);
        Assert.Equal(2, item.Quantidade);
        Assert.Single(_itens.Itens);
    }

    [Fact]
    public async Task Adicionar_PaiInexistente_LancaNotFoundException()
    {
        var componente = await CriarProdutoAsync("TP", "Tampinha");

        await Assert.ThrowsAsync<NotFoundException>(() => _service.AdicionarAsync(99, Dto(componente), Ct));
    }

    [Fact]
    public async Task Adicionar_ComponenteInexistente_LancaNotFoundException()
    {
        var pai = await CriarProdutoAsync("GA", "Garrafa d'água");

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _service.AdicionarAsync(pai.Id, new AdicionarItemEstruturaDto { ComponenteId = 99, Quantidade = 1 }, Ct));
    }

    [Fact]
    public async Task Adicionar_ComponenteInativo_LancaDomainException()
    {
        var pai = await CriarProdutoAsync("GA", "Garrafa d'água");
        var componente = await CriarProdutoAsync("TP", "Tampinha");
        componente.Inativar();

        await Assert.ThrowsAsync<DomainException>(() => _service.AdicionarAsync(pai.Id, Dto(componente), Ct));
    }

    [Fact]
    public async Task Adicionar_PaiInativo_LancaDomainException()
    {
        var pai = await CriarProdutoAsync("GA", "Garrafa d'água");
        var componente = await CriarProdutoAsync("TP", "Tampinha");
        pai.Inativar();

        await Assert.ThrowsAsync<DomainException>(() => _service.AdicionarAsync(pai.Id, Dto(componente), Ct));
    }

    [Fact]
    public async Task Adicionar_ParRepetido_LancaDomainException()
    {
        var pai = await CriarProdutoAsync("GA", "Garrafa d'água");
        var componente = await CriarProdutoAsync("TP", "Tampinha");
        await _service.AdicionarAsync(pai.Id, Dto(componente), Ct);

        await Assert.ThrowsAsync<DomainException>(() => _service.AdicionarAsync(pai.Id, Dto(componente), Ct));
    }

    [Fact]
    public async Task Adicionar_ProdutoComoComponenteDeSiMesmo_LancaDomainException()
    {
        var produto = await CriarProdutoAsync("GA", "Garrafa d'água");

        await Assert.ThrowsAsync<DomainException>(() => _service.AdicionarAsync(produto.Id, Dto(produto), Ct));
    }

    [Fact]
    public async Task Adicionar_KitComoComponenteDaTampinha_RecusaComMensagemDoCiclo()
    {
        var tampinha = await CriarProdutoAsync("TP", "Tampinha");
        var garrafa = await CriarProdutoAsync("GF", "Garrafa");
        var garrafaDagua = await CriarProdutoAsync("GA", "Garrafa d'água", TipoProduto.Acabado);
        var kit = await CriarProdutoAsync("KT", "Kit", TipoProduto.Acabado);
        await _service.AdicionarAsync(garrafaDagua.Id, Dto(tampinha), Ct);
        await _service.AdicionarAsync(garrafaDagua.Id, Dto(garrafa), Ct);
        await _service.AdicionarAsync(kit.Id, Dto(garrafaDagua, 6), Ct);

        var erro = await Assert.ThrowsAsync<DomainException>(() => _service.AdicionarAsync(tampinha.Id, Dto(kit), Ct));

        Assert.Equal("A estrutura formaria um ciclo: Tampinha → Kit → Garrafa d'água → Tampinha", erro.Message);
        Assert.Equal(3, _itens.Itens.Count);
    }

    [Fact]
    public async Task AlterarQuantidade_AtualizaOItem()
    {
        var pai = await CriarProdutoAsync("GA", "Garrafa d'água");
        var componente = await CriarProdutoAsync("TP", "Tampinha");
        await _service.AdicionarAsync(pai.Id, Dto(componente, 1), Ct);

        var item = await _service.AlterarQuantidadeAsync(pai.Id, componente.Id, new AlterarQuantidadeItemEstruturaDto { Quantidade = 4 }, Ct);

        Assert.Equal(4, item.Quantidade);
    }

    [Fact]
    public async Task AlterarQuantidade_ItemInexistente_LancaNotFoundException()
    {
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _service.AlterarQuantidadeAsync(1, 2, new AlterarQuantidadeItemEstruturaDto { Quantidade = 4 }, Ct));
    }

    [Fact]
    public async Task Remover_ExcluiOItem()
    {
        var pai = await CriarProdutoAsync("GA", "Garrafa d'água");
        var componente = await CriarProdutoAsync("TP", "Tampinha");
        await _service.AdicionarAsync(pai.Id, Dto(componente), Ct);

        await _service.RemoverAsync(pai.Id, componente.Id, Ct);

        Assert.Empty(_itens.Itens);
    }

    [Fact]
    public async Task Listar_PaiInexistente_LancaNotFoundException()
    {
        await Assert.ThrowsAsync<NotFoundException>(() => _service.ListarAsync(99, Ct));
    }

    [Fact]
    public async Task Listar_DevolveSoOsComponentesDoPai()
    {
        var ga = await CriarProdutoAsync("GA", "Garrafa d'água");
        var kit = await CriarProdutoAsync("KT", "Kit");
        var tampinha = await CriarProdutoAsync("TP", "Tampinha");
        await _service.AdicionarAsync(ga.Id, Dto(tampinha), Ct);
        await _service.AdicionarAsync(kit.Id, Dto(ga, 6), Ct);

        var itens = await _service.ListarAsync(kit.Id, Ct);

        Assert.Equal(["GA"], itens.Select(i => i.ComponenteCodigo));
    }
}
