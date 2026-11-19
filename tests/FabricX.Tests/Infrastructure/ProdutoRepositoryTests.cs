using FabricX.Domain.Entities;
using FabricX.Domain.Enums;
using FabricX.Infrastructure.Repositories;
using FabricX.Tests.Fakes;
using Microsoft.EntityFrameworkCore;

namespace FabricX.Tests.Infrastructure;

public class ProdutoRepositoryTests : IDisposable
{
    private static readonly CancellationToken Ct = CancellationToken.None;

    private readonly BancoSqliteFixture _banco = new();

    public void Dispose() => _banco.Dispose();

    private static Produto NovoProduto(string codigo = "P001", string nome = "Parafuso", TipoProduto tipo = TipoProduto.MateriaPrima) =>
        new(codigo, nome, tipo, "UN", estoqueAtual: 10, estoqueMinimo: 5, custoUnitario: 1.5, descricao: "Aço");

    [Fact]
    public async Task Adicionar_GeraIdEPersisteNoEsquemaOficial()
    {
        await using var contexto = _banco.CriarContexto();
        var repositorio = new ProdutoRepository(contexto);
        var produto = NovoProduto();

        await repositorio.AdicionarAsync(produto, Ct);

        Assert.True(produto.Id > 0);
    }

    [Theory]
    [InlineData(TipoProduto.MateriaPrima, "MATERIA_PRIMA")]
    [InlineData(TipoProduto.EmProcesso, "EM_PROCESSO")]
    [InlineData(TipoProduto.Acabado, "ACABADO")]
    public async Task Adicionar_GravaTipoComoTextoDoCheck(TipoProduto tipo, string esperado)
    {
        await using var contexto = _banco.CriarContexto();
        var repositorio = new ProdutoRepository(contexto);
        var produto = NovoProduto(tipo: tipo);
        await repositorio.AdicionarAsync(produto, Ct);

        var gravado = await contexto.Database
            .SqlQuery<string>($"SELECT tipo AS Value FROM produto WHERE id = {produto.Id}")
            .SingleAsync(Ct);

        Assert.Equal(esperado, gravado);
    }

    [Fact]
    public async Task ObterPorId_RecuperaTodosOsCampos()
    {
        int id;
        await using (var escrita = _banco.CriarContexto())
        {
            var produto = NovoProduto();
            await new ProdutoRepository(escrita).AdicionarAsync(produto, Ct);
            id = produto.Id;
        }

        await using var leitura = _banco.CriarContexto();
        var recuperado = await new ProdutoRepository(leitura).ObterPorIdAsync(id, Ct);

        Assert.NotNull(recuperado);
        Assert.Equal("P001", recuperado.Codigo);
        Assert.Equal(TipoProduto.MateriaPrima, recuperado.Tipo);
        Assert.Equal("Aço", recuperado.Descricao);
        Assert.Equal(10, recuperado.EstoqueAtual);
        Assert.Equal(5, recuperado.EstoqueMinimo);
        Assert.Equal(1.5, recuperado.CustoUnitario);
        Assert.True(recuperado.Ativo);
    }

    [Fact]
    public async Task ObterPorId_Inexistente_RetornaNulo()
    {
        await using var contexto = _banco.CriarContexto();

        Assert.Null(await new ProdutoRepository(contexto).ObterPorIdAsync(99, Ct));
    }

    [Fact]
    public async Task ObterPorCodigo_Existente_Retorna()
    {
        await using var contexto = _banco.CriarContexto();
        var repositorio = new ProdutoRepository(contexto);
        await repositorio.AdicionarAsync(NovoProduto("ABC"), Ct);

        var encontrado = await repositorio.ObterPorCodigoAsync("ABC", Ct);

        Assert.NotNull(encontrado);
    }

    [Fact]
    public async Task Listar_RetornaOrdenadoPorNome()
    {
        await using var contexto = _banco.CriarContexto();
        var repositorio = new ProdutoRepository(contexto);
        await repositorio.AdicionarAsync(NovoProduto("P2", "Zinco"), Ct);
        await repositorio.AdicionarAsync(NovoProduto("P1", "Alumínio"), Ct);

        var lista = await repositorio.ListarAsync(null, Ct);

        Assert.Equal(["Alumínio", "Zinco"], lista.Select(p => p.Nome));
    }

    [Fact]
    public async Task Atualizar_PersisteMudancasDaEntidade()
    {
        int id;
        await using (var contexto = _banco.CriarContexto())
        {
            var repositorio = new ProdutoRepository(contexto);
            var produto = NovoProduto();
            await repositorio.AdicionarAsync(produto, Ct);
            id = produto.Id;

            produto.RegistrarSaida(4);
            produto.Inativar();
            await repositorio.AtualizarAsync(produto, Ct);
        }

        await using var leitura = _banco.CriarContexto();
        var recuperado = await new ProdutoRepository(leitura).ObterPorIdAsync(id, Ct);

        Assert.Equal(6, recuperado!.EstoqueAtual);
        Assert.False(recuperado.Ativo);
    }

    [Fact]
    public async Task Adicionar_CodigoDuplicado_ViolaUnicidadeDoBanco()
    {
        await using var contexto = _banco.CriarContexto();
        var repositorio = new ProdutoRepository(contexto);
        await repositorio.AdicionarAsync(NovoProduto("DUP"), Ct);

        await using var outro = _banco.CriarContexto();
        await Assert.ThrowsAsync<DbUpdateException>(() => new ProdutoRepository(outro).AdicionarAsync(NovoProduto("DUP"), Ct));
    }

    [Fact]
    public async Task Listar_FiltraPorTipo()
    {
        await using var contexto = _banco.CriarContexto();
        var repositorio = new ProdutoRepository(contexto);
        await repositorio.AdicionarAsync(NovoProduto("P1", "Alumínio", TipoProduto.MateriaPrima), Ct);
        await repositorio.AdicionarAsync(NovoProduto("P2", "Garrafa", TipoProduto.Acabado), Ct);

        var lista = await repositorio.ListarAsync(TipoProduto.Acabado, Ct);

        Assert.Equal(["Garrafa"], lista.Select(p => p.Nome));
    }
}
