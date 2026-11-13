using FabricX.Application.DTOs.Produtos;
using FabricX.Application.Exceptions;
using FabricX.Application.Services;
using FabricX.Domain.Enums;
using FabricX.Domain.Exceptions;
using FabricX.Tests.Fakes;

namespace FabricX.Tests.Application;

public class ProdutoServiceTests
{
    private static readonly CancellationToken Ct = CancellationToken.None;

    private readonly ProdutoRepositoryFake _repositorio = new();
    private readonly ProdutoService _service;

    public ProdutoServiceTests()
    {
        _service = new ProdutoService(_repositorio);
    }

    private static CriarProdutoDto NovoDto(string codigo = "P001", double estoque = 10, double minimo = 5) => new()
    {
        Codigo = codigo,
        Nome = "Parafuso",
        Tipo = TipoProduto.MateriaPrima,
        UnidadeMedida = "UN",
        EstoqueAtual = estoque,
        EstoqueMinimo = minimo
    };

    [Fact]
    public async Task Criar_DadosValidos_RetornaDtoComId()
    {
        var resultado = await _service.CriarAsync(NovoDto(), Ct);

        Assert.Equal(1, resultado.Id);
        Assert.Equal("P001", resultado.Codigo);
        Assert.True(resultado.Ativo);
    }

    [Fact]
    public async Task Criar_CodigoDuplicado_LancaDomainException()
    {
        await _service.CriarAsync(NovoDto(), Ct);

        await Assert.ThrowsAsync<DomainException>(() => _service.CriarAsync(NovoDto(), Ct));
    }

    [Fact]
    public async Task Criar_RegraDeDominioViolada_PropagaDomainException()
    {
        var dto = NovoDto();
        dto.Nome = " ";

        await Assert.ThrowsAsync<DomainException>(() => _service.CriarAsync(dto, Ct));
    }

    [Fact]
    public async Task ObterPorId_Inexistente_LancaNotFoundException()
    {
        await Assert.ThrowsAsync<NotFoundException>(() => _service.ObterPorIdAsync(99, Ct));
    }

    [Fact]
    public async Task ObterPorId_Existente_RetornaDto()
    {
        var criado = await _service.CriarAsync(NovoDto(), Ct);

        var resultado = await _service.ObterPorIdAsync(criado.Id, Ct);

        Assert.Equal(criado.Id, resultado.Id);
    }

    [Fact]
    public async Task Listar_RetornaTodosOsProdutos()
    {
        await _service.CriarAsync(NovoDto("P001"), Ct);
        await _service.CriarAsync(NovoDto("P002"), Ct);

        var resultado = await _service.ListarAsync(Ct);

        Assert.Equal(2, resultado.Count);
    }

    [Fact]
    public async Task ListarComEstoqueBaixo_RetornaSoOsAbaixoDoMinimoEAtivos()
    {
        await _service.CriarAsync(NovoDto("OK", estoque: 10, minimo: 5), Ct);
        await _service.CriarAsync(NovoDto("BAIXO", estoque: 1, minimo: 5), Ct);
        var inativo = await _service.CriarAsync(NovoDto("INATIVO", estoque: 1, minimo: 5), Ct);
        await _service.InativarAsync(inativo.Id, Ct);

        var resultado = await _service.ListarComEstoqueBaixoAsync(Ct);

        var item = Assert.Single(resultado);
        Assert.Equal("BAIXO", item.Codigo);
    }

    [Fact]
    public async Task Atualizar_Existente_AlteraEPersiste()
    {
        var criado = await _service.CriarAsync(NovoDto(), Ct);
        var dto = new AtualizarProdutoDto { Nome = "Parafuso M8", UnidadeMedida = "CX", EstoqueMinimo = 20, CustoUnitario = 2 };

        var resultado = await _service.AtualizarAsync(criado.Id, dto, Ct);

        Assert.Equal("Parafuso M8", resultado.Nome);
        Assert.Equal(20, resultado.EstoqueMinimo);
        Assert.Equal(1, _repositorio.TotalAtualizacoes);
    }

    [Fact]
    public async Task Atualizar_Inexistente_LancaNotFoundException()
    {
        var dto = new AtualizarProdutoDto { Nome = "X", UnidadeMedida = "UN" };

        await Assert.ThrowsAsync<NotFoundException>(() => _service.AtualizarAsync(99, dto, Ct));
    }

    [Fact]
    public async Task MovimentarEstoque_Entrada_AumentaSaldo()
    {
        var criado = await _service.CriarAsync(NovoDto(estoque: 10), Ct);

        var resultado = await _service.MovimentarEstoqueAsync(criado.Id,
            new MovimentarEstoqueDto { Tipo = TipoMovimentacao.Entrada, Quantidade = 5 }, Ct);

        Assert.Equal(15, resultado.EstoqueAtual);
    }

    [Fact]
    public async Task MovimentarEstoque_SaidaMaiorQueSaldo_LancaDomainExceptionSemPersistir()
    {
        var criado = await _service.CriarAsync(NovoDto(estoque: 10), Ct);

        await Assert.ThrowsAsync<DomainException>(() => _service.MovimentarEstoqueAsync(criado.Id,
            new MovimentarEstoqueDto { Tipo = TipoMovimentacao.Saida, Quantidade = 11 }, Ct));

        Assert.Equal(0, _repositorio.TotalAtualizacoes);
    }

    [Fact]
    public async Task MovimentarEstoque_SaidaCruzandoMinimo_IndicaAlerta()
    {
        var criado = await _service.CriarAsync(NovoDto(estoque: 10, minimo: 5), Ct);

        var resultado = await _service.MovimentarEstoqueAsync(criado.Id,
            new MovimentarEstoqueDto { Tipo = TipoMovimentacao.Saida, Quantidade = 6 }, Ct);

        Assert.True(resultado.EstoqueAbaixoDoMinimo);
    }

    [Fact]
    public async Task MovimentarEstoque_Inexistente_LancaNotFoundException()
    {
        await Assert.ThrowsAsync<NotFoundException>(() => _service.MovimentarEstoqueAsync(99,
            new MovimentarEstoqueDto { Tipo = TipoMovimentacao.Entrada, Quantidade = 1 }, Ct));
    }

    [Fact]
    public async Task Inativar_Existente_MarcaComoInativo()
    {
        var criado = await _service.CriarAsync(NovoDto(), Ct);

        var resultado = await _service.InativarAsync(criado.Id, Ct);

        Assert.False(resultado.Ativo);
        Assert.Equal(1, _repositorio.TotalAtualizacoes);
    }

    [Fact]
    public async Task Inativar_Inexistente_LancaNotFoundException()
    {
        await Assert.ThrowsAsync<NotFoundException>(() => _service.InativarAsync(99, Ct));
    }
}
