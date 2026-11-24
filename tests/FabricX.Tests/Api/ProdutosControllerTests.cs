using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FabricX.Tests.Fakes;

namespace FabricX.Tests.Api;

public class ProdutosControllerTests : IClassFixture<ApiFactory>
{
    private readonly HttpClient _cliente;

    public ProdutosControllerTests(ApiFactory fabrica)
    {
        _cliente = fabrica.CriarClienteAutenticado();
    }

    private static object NovoProduto(string codigo, double estoque = 10, double minimo = 5) => new
    {
        codigo,
        nome = "Parafuso",
        tipo = "MateriaPrima",
        unidadeMedida = "UN",
        estoqueAtual = estoque,
        estoqueMinimo = minimo
    };

    private static string CodigoUnico() => $"P{Guid.NewGuid():N}"[..12];

    private static async Task<JsonElement> LerAsync(HttpResponseMessage resposta) =>
        await resposta.Content.ReadFromJsonAsync<JsonElement>();

    private async Task<int> CriarAsync(string codigo, double estoque = 10, double minimo = 5)
    {
        var resposta = await _cliente.PostAsJsonAsync("/api/produtos", NovoProduto(codigo, estoque, minimo));
        return (await LerAsync(resposta)).GetProperty("data").GetProperty("id").GetInt32();
    }

    [Fact]
    public async Task Post_DadosValidos_Retorna201ComWebResponse()
    {
        var resposta = await _cliente.PostAsJsonAsync("/api/produtos", NovoProduto(CodigoUnico()));

        Assert.Equal(HttpStatusCode.Created, resposta.StatusCode);
        Assert.NotNull(resposta.Headers.Location);
        var corpo = await LerAsync(resposta);
        Assert.True(corpo.GetProperty("isSuccess").GetBoolean());
        Assert.Equal(201, corpo.GetProperty("statusCode").GetInt32());
        Assert.Equal("MateriaPrima", corpo.GetProperty("data").GetProperty("tipo").GetString());
    }

    [Fact]
    public async Task Post_CodigoDuplicado_Retorna400()
    {
        var codigo = CodigoUnico();
        await CriarAsync(codigo);

        var resposta = await _cliente.PostAsJsonAsync("/api/produtos", NovoProduto(codigo));

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.False((await LerAsync(resposta)).GetProperty("isSuccess").GetBoolean());
    }

    [Fact]
    public async Task Post_EstoqueNegativo_Retorna400NoFormatoWebResponse()
    {
        var resposta = await _cliente.PostAsJsonAsync("/api/produtos", NovoProduto(CodigoUnico(), estoque: -1));

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        var corpo = await LerAsync(resposta);
        Assert.False(corpo.GetProperty("isSuccess").GetBoolean());
        Assert.Equal(400, corpo.GetProperty("statusCode").GetInt32());
        Assert.False(string.IsNullOrWhiteSpace(corpo.GetProperty("message").GetString()));
    }

    [Fact]
    public async Task Get_PorId_Existente_Retorna200()
    {
        var id = await CriarAsync(CodigoUnico());

        var resposta = await _cliente.GetAsync($"/api/produtos/{id}");

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        Assert.Equal(id, (await LerAsync(resposta)).GetProperty("data").GetProperty("id").GetInt32());
    }

    [Fact]
    public async Task Get_PorId_Inexistente_Retorna404NoFormatoWebResponse()
    {
        var resposta = await _cliente.GetAsync("/api/produtos/999999");

        Assert.Equal(HttpStatusCode.NotFound, resposta.StatusCode);
        var corpo = await LerAsync(resposta);
        Assert.False(corpo.GetProperty("isSuccess").GetBoolean());
        Assert.Equal(404, corpo.GetProperty("statusCode").GetInt32());
    }

    [Fact]
    public async Task Get_Lista_Retorna200ComColecao()
    {
        await CriarAsync(CodigoUnico());

        var resposta = await _cliente.GetAsync("/api/produtos");

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        Assert.True((await LerAsync(resposta)).GetProperty("data").GetArrayLength() >= 1);
    }

    [Fact]
    public async Task Put_DadosValidos_AtualizaProduto()
    {
        var id = await CriarAsync(CodigoUnico());

        var resposta = await _cliente.PutAsJsonAsync($"/api/produtos/{id}",
            new { nome = "Parafuso M8", unidadeMedida = "CX", estoqueMinimo = 20, custoUnitario = 2 });

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        Assert.Equal("Parafuso M8", (await LerAsync(resposta)).GetProperty("data").GetProperty("nome").GetString());
    }

    [Fact]
    public async Task Put_Inexistente_Retorna404()
    {
        var resposta = await _cliente.PutAsJsonAsync("/api/produtos/999999",
            new { nome = "X", unidadeMedida = "UN", estoqueMinimo = 0, custoUnitario = 0 });

        Assert.Equal(HttpStatusCode.NotFound, resposta.StatusCode);
    }

    [Fact]
    public async Task Movimentacao_Saida_AtualizaSaldoEAlerta()
    {
        var id = await CriarAsync(CodigoUnico(), estoque: 10, minimo: 5);

        var resposta = await _cliente.PostAsJsonAsync($"/api/produtos/{id}/movimentacoes",
            new { tipo = "Saida", quantidade = 6 });

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        var dados = (await LerAsync(resposta)).GetProperty("data");
        Assert.Equal(4, dados.GetProperty("estoqueAtual").GetDouble());
        Assert.True(dados.GetProperty("estoqueAbaixoDoMinimo").GetBoolean());
    }

    [Fact]
    public async Task Movimentacao_SaidaMaiorQueEstoque_Retorna400EMantemSaldo()
    {
        var id = await CriarAsync(CodigoUnico(), estoque: 10);

        var resposta = await _cliente.PostAsJsonAsync($"/api/produtos/{id}/movimentacoes",
            new { tipo = "Saida", quantidade = 11 });

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        var atual = await LerAsync(await _cliente.GetAsync($"/api/produtos/{id}"));
        Assert.Equal(10, atual.GetProperty("data").GetProperty("estoqueAtual").GetDouble());
    }

    [Fact]
    public async Task Movimentacao_QuantidadeZero_Retorna400()
    {
        var id = await CriarAsync(CodigoUnico());

        var resposta = await _cliente.PostAsJsonAsync($"/api/produtos/{id}/movimentacoes",
            new { tipo = "Entrada", quantidade = 0 });

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
    }

    [Fact]
    public async Task EstoqueBaixo_ListaSoOsProdutosAbaixoDoMinimo()
    {
        var codigoBaixo = CodigoUnico();
        await CriarAsync(codigoBaixo, estoque: 1, minimo: 5);
        await CriarAsync(CodigoUnico(), estoque: 50, minimo: 5);

        var resposta = await _cliente.GetAsync("/api/produtos/estoque-baixo");

        var itens = (await LerAsync(resposta)).GetProperty("data").EnumerateArray().ToList();
        Assert.Contains(itens, i => i.GetProperty("codigo").GetString() == codigoBaixo);
        Assert.All(itens, i => Assert.True(i.GetProperty("estoqueAbaixoDoMinimo").GetBoolean()));
    }

    [Fact]
    public async Task Delete_InativaProduto()
    {
        var id = await CriarAsync(CodigoUnico());

        var resposta = await _cliente.DeleteAsync($"/api/produtos/{id}");

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        Assert.False((await LerAsync(resposta)).GetProperty("data").GetProperty("ativo").GetBoolean());
    }

    [Fact]
    public async Task Movimentacao_RegistraNoHistoricoEmOrdemDecrescente()
    {
        var id = await CriarAsync(CodigoUnico(), estoque: 10);
        await _cliente.PostAsJsonAsync($"/api/produtos/{id}/movimentacoes", new { tipo = "Entrada", quantidade = 2, motivo = "Compra" });
        await _cliente.PostAsJsonAsync($"/api/produtos/{id}/movimentacoes", new { tipo = "Saida", quantidade = 3 });

        var resposta = await _cliente.GetAsync($"/api/produtos/{id}/movimentacoes");

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        var itens = (await LerAsync(resposta)).GetProperty("data").EnumerateArray().ToList();
        Assert.Equal(2, itens.Count);
        Assert.Equal("Saida", itens[0].GetProperty("tipo").GetString());
        Assert.Equal(3, itens[0].GetProperty("quantidade").GetDouble());
        Assert.Equal("Entrada", itens[1].GetProperty("tipo").GetString());
        Assert.Equal("Compra", itens[1].GetProperty("motivo").GetString());
    }

    [Fact]
    public async Task Movimentacao_SaidaRecusada_NaoGravaHistorico()
    {
        var id = await CriarAsync(CodigoUnico(), estoque: 10);
        await _cliente.PostAsJsonAsync($"/api/produtos/{id}/movimentacoes", new { tipo = "Saida", quantidade = 11 });

        var historico = await LerAsync(await _cliente.GetAsync($"/api/produtos/{id}/movimentacoes"));

        Assert.Empty(historico.GetProperty("data").EnumerateArray());
    }

    [Fact]
    public async Task Historico_ProdutoInexistente_Retorna404()
    {
        var resposta = await _cliente.GetAsync("/api/produtos/999999/movimentacoes");

        Assert.Equal(HttpStatusCode.NotFound, resposta.StatusCode);
    }

    [Fact]
    public async Task Listar_FiltraPorTipo()
    {
        var codigo = CodigoUnico();
        await _cliente.PostAsJsonAsync("/api/produtos", new { codigo, nome = "Garrafa", tipo = "Acabado", unidadeMedida = "UN" });

        var corpo = await LerAsync(await _cliente.GetAsync("/api/produtos?tipo=Acabado"));

        var itens = corpo.GetProperty("data").EnumerateArray().ToList();
        Assert.Contains(itens, p => p.GetProperty("codigo").GetString() == codigo);
        Assert.All(itens, p => Assert.Equal("Acabado", p.GetProperty("tipo").GetString()));
    }

    [Fact]
    public async Task Listar_TipoInvalido_Retorna400()
    {
        var resposta = await _cliente.GetAsync("/api/produtos?tipo=Banana");

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
    }

    private async Task<int> CriarComTipoAsync(string codigo, string nome, string tipo)
    {
        var resposta = await _cliente.PostAsJsonAsync("/api/produtos", new { codigo, nome, tipo, unidadeMedida = "UN" });
        return (await LerAsync(resposta)).GetProperty("data").GetProperty("id").GetInt32();
    }

    private Task<HttpResponseMessage> AdicionarComponenteAsync(int paiId, int componenteId, double quantidade = 1) =>
        _cliente.PostAsJsonAsync($"/api/produtos/{paiId}/estrutura", new { componenteId, quantidade });

    [Fact]
    public async Task Estrutura_AdicionaEListaComCodigoENomeDoComponente()
    {
        var pai = await CriarComTipoAsync(CodigoUnico(), "Garrafa d'água", "Acabado");
        var codigo = CodigoUnico();
        var componente = await CriarComTipoAsync(codigo, "Tampinha", "MateriaPrima");

        var criado = await AdicionarComponenteAsync(pai, componente, 2);

        Assert.Equal(HttpStatusCode.Created, criado.StatusCode);
        var lista = (await LerAsync(await _cliente.GetAsync($"/api/produtos/{pai}/estrutura"))).GetProperty("data").EnumerateArray().ToList();
        var item = Assert.Single(lista);
        Assert.Equal(codigo, item.GetProperty("componenteCodigo").GetString());
        Assert.Equal("Tampinha", item.GetProperty("componenteNome").GetString());
        Assert.Equal(2, item.GetProperty("quantidade").GetDouble());
    }

    [Fact]
    public async Task Estrutura_ParRepetido_Retorna400()
    {
        var pai = await CriarComTipoAsync(CodigoUnico(), "Garrafa d'água", "Acabado");
        var componente = await CriarComTipoAsync(CodigoUnico(), "Tampinha", "MateriaPrima");
        await AdicionarComponenteAsync(pai, componente);

        var resposta = await AdicionarComponenteAsync(pai, componente);

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
    }

    [Fact]
    public async Task Estrutura_ComponenteInexistente_Retorna404()
    {
        var pai = await CriarComTipoAsync(CodigoUnico(), "Garrafa d'água", "Acabado");

        var resposta = await AdicionarComponenteAsync(pai, 999999);

        Assert.Equal(HttpStatusCode.NotFound, resposta.StatusCode);
    }

    [Fact]
    public async Task Estrutura_QuantidadeZero_Retorna400()
    {
        var pai = await CriarComTipoAsync(CodigoUnico(), "Garrafa d'água", "Acabado");
        var componente = await CriarComTipoAsync(CodigoUnico(), "Tampinha", "MateriaPrima");

        var resposta = await AdicionarComponenteAsync(pai, componente, 0);

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
    }

    [Fact]
    public async Task Estrutura_Ciclo_Retorna400ComMensagemClara()
    {
        var tampinha = await CriarComTipoAsync(CodigoUnico(), "Tampinha", "MateriaPrima");
        var garrafaDagua = await CriarComTipoAsync(CodigoUnico(), "Garrafa d'água", "Acabado");
        var kit = await CriarComTipoAsync(CodigoUnico(), "Kit", "Acabado");
        await AdicionarComponenteAsync(garrafaDagua, tampinha);
        await AdicionarComponenteAsync(kit, garrafaDagua, 6);

        var resposta = await AdicionarComponenteAsync(tampinha, kit);

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        var mensagem = (await LerAsync(resposta)).GetProperty("message").GetString();
        Assert.Equal("A estrutura formaria um ciclo: Tampinha → Kit → Garrafa d'água → Tampinha", mensagem);
    }

    [Fact]
    public async Task Estrutura_AlteraQuantidadeERemove()
    {
        var pai = await CriarComTipoAsync(CodigoUnico(), "Garrafa d'água", "Acabado");
        var componente = await CriarComTipoAsync(CodigoUnico(), "Tampinha", "MateriaPrima");
        await AdicionarComponenteAsync(pai, componente);

        var alterada = await _cliente.PutAsJsonAsync($"/api/produtos/{pai}/estrutura/{componente}", new { quantidade = 5 });
        Assert.Equal(HttpStatusCode.OK, alterada.StatusCode);
        Assert.Equal(5, (await LerAsync(alterada)).GetProperty("data").GetProperty("quantidade").GetDouble());

        var removida = await _cliente.DeleteAsync($"/api/produtos/{pai}/estrutura/{componente}");
        Assert.Equal(HttpStatusCode.OK, removida.StatusCode);
        var lista = await LerAsync(await _cliente.GetAsync($"/api/produtos/{pai}/estrutura"));
        Assert.Empty(lista.GetProperty("data").EnumerateArray());
    }

    [Fact]
    public async Task Estrutura_RemoverItemInexistente_Retorna404()
    {
        var pai = await CriarComTipoAsync(CodigoUnico(), "Garrafa d'água", "Acabado");

        var resposta = await _cliente.DeleteAsync($"/api/produtos/{pai}/estrutura/999999");

        Assert.Equal(HttpStatusCode.NotFound, resposta.StatusCode);
    }

    private async Task<(int Tampinha, int Garrafa, int GarrafaDagua)> PrepararGarrafaDaguaAsync(double estoqueTampinha = 100)
    {
        var tampinha = await CriarComTipoAsync(CodigoUnico(), "Tampinha", "MateriaPrima");
        var garrafa = await CriarComTipoAsync(CodigoUnico(), "Garrafa", "MateriaPrima");
        var garrafaDagua = await CriarComTipoAsync(CodigoUnico(), "Garrafa d'água", "Acabado");
        await _cliente.PostAsJsonAsync($"/api/produtos/{tampinha}/movimentacoes", new { tipo = "Entrada", quantidade = estoqueTampinha });
        await _cliente.PostAsJsonAsync($"/api/produtos/{garrafa}/movimentacoes", new { tipo = "Entrada", quantidade = 100 });
        await AdicionarComponenteAsync(garrafaDagua, tampinha);
        await AdicionarComponenteAsync(garrafaDagua, garrafa);
        return (tampinha, garrafa, garrafaDagua);
    }

    private Task<HttpResponseMessage> CriarOrdemAsync(int produtoId, double quantidade) =>
        _cliente.PostAsJsonAsync("/api/ordens-producao", new
        {
            produtoId,
            quantidade,
            prazoEntrega = DateOnly.FromDateTime(DateTime.Today.AddDays(10)).ToString("yyyy-MM-dd")
        });

    private async Task<double> SaldoAsync(int produtoId) =>
        (await LerAsync(await _cliente.GetAsync($"/api/produtos/{produtoId}"))).GetProperty("data").GetProperty("estoqueAtual").GetDouble();

    [Fact]
    public async Task Ordem_FluxoCompletoDe60GarrafasDagua_AjustaOsSaldos()
    {
        var (tampinha, garrafa, garrafaDagua) = await PrepararGarrafaDaguaAsync();

        var criada = await CriarOrdemAsync(garrafaDagua, 60);
        Assert.Equal(HttpStatusCode.Created, criada.StatusCode);
        var dados = (await LerAsync(criada)).GetProperty("data");
        var id = dados.GetProperty("id").GetInt32();
        Assert.StartsWith($"OP-{DateTime.Now.Year}-", dados.GetProperty("numero").GetString());
        Assert.Equal("Planejada", dados.GetProperty("status").GetString());

        foreach (var esperado in new[] { "Programada", "EmProducao", "Concluida" })
        {
            var avancada = await _cliente.PostAsync($"/api/ordens-producao/{id}/avancar", null);
            Assert.Equal(HttpStatusCode.OK, avancada.StatusCode);
            Assert.Equal(esperado, (await LerAsync(avancada)).GetProperty("data").GetProperty("status").GetString());
        }

        Assert.Equal(40, await SaldoAsync(tampinha));
        Assert.Equal(40, await SaldoAsync(garrafa));
        Assert.Equal(60, await SaldoAsync(garrafaDagua));
        var historico = (await LerAsync(await _cliente.GetAsync($"/api/produtos/{garrafaDagua}/movimentacoes"))).GetProperty("data").EnumerateArray().ToList();
        Assert.Equal(id, historico.Single().GetProperty("ordemId").GetInt32());
    }

    [Fact]
    public async Task Ordem_SaldoInsuficienteNaCriacao_Retorna400ESemGravar()
    {
        var (_, _, garrafaDagua) = await PrepararGarrafaDaguaAsync(estoqueTampinha: 50);

        var resposta = await CriarOrdemAsync(garrafaDagua, 60);

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Contains("Estoque insuficiente de", (await LerAsync(resposta)).GetProperty("message").GetString());
    }

    [Fact]
    public async Task Ordem_IniciarSemSaldo_Retorna400ENaoAlteraSaldos()
    {
        var (tampinha, garrafa, garrafaDagua) = await PrepararGarrafaDaguaAsync();
        var id = (await LerAsync(await CriarOrdemAsync(garrafaDagua, 60))).GetProperty("data").GetProperty("id").GetInt32();
        await _cliente.PostAsync($"/api/ordens-producao/{id}/avancar", null);
        await _cliente.PostAsJsonAsync($"/api/produtos/{garrafa}/movimentacoes", new { tipo = "Saida", quantidade = 70 });

        var resposta = await _cliente.PostAsync($"/api/ordens-producao/{id}/avancar", null);

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Equal(100, await SaldoAsync(tampinha));
        Assert.Equal(30, await SaldoAsync(garrafa));
    }

    [Fact]
    public async Task Ordem_CancelarEmProducao_Retorna400()
    {
        var (_, _, garrafaDagua) = await PrepararGarrafaDaguaAsync();
        var id = (await LerAsync(await CriarOrdemAsync(garrafaDagua, 10))).GetProperty("data").GetProperty("id").GetInt32();
        await _cliente.PostAsync($"/api/ordens-producao/{id}/avancar", null);
        await _cliente.PostAsync($"/api/ordens-producao/{id}/avancar", null);

        var resposta = await _cliente.PostAsync($"/api/ordens-producao/{id}/cancelar", null);

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
    }

    [Fact]
    public async Task Ordem_CancelarPlanejada_Retorna200()
    {
        var (_, _, garrafaDagua) = await PrepararGarrafaDaguaAsync();
        var id = (await LerAsync(await CriarOrdemAsync(garrafaDagua, 10))).GetProperty("data").GetProperty("id").GetInt32();

        var resposta = await _cliente.PostAsync($"/api/ordens-producao/{id}/cancelar", null);

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        Assert.Equal("Cancelada", (await LerAsync(resposta)).GetProperty("data").GetProperty("status").GetString());
    }

    [Fact]
    public async Task Ordem_Inexistente_Retorna404()
    {
        Assert.Equal(HttpStatusCode.NotFound, (await _cliente.GetAsync("/api/ordens-producao/999999")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await _cliente.PostAsync("/api/ordens-producao/999999/avancar", null)).StatusCode);
    }

    [Fact]
    public async Task Ordem_PrazoNoPassadoOuQuantidadeZero_Retorna400()
    {
        var (_, _, garrafaDagua) = await PrepararGarrafaDaguaAsync();

        var passado = await _cliente.PostAsJsonAsync("/api/ordens-producao", new { produtoId = garrafaDagua, quantidade = 1, prazoEntrega = "2020-01-01" });
        var zero = await CriarOrdemAsync(garrafaDagua, 0);

        Assert.Equal(HttpStatusCode.BadRequest, passado.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, zero.StatusCode);
    }

    [Fact]
    public async Task Ordem_ListaComFiltroDeStatus()
    {
        var (_, _, garrafaDagua) = await PrepararGarrafaDaguaAsync();
        var id = (await LerAsync(await CriarOrdemAsync(garrafaDagua, 5))).GetProperty("data").GetProperty("id").GetInt32();
        await _cliente.PostAsync($"/api/ordens-producao/{id}/cancelar", null);

        var canceladas = (await LerAsync(await _cliente.GetAsync("/api/ordens-producao?status=Cancelada"))).GetProperty("data").EnumerateArray().ToList();

        Assert.Contains(canceladas, o => o.GetProperty("id").GetInt32() == id);
        Assert.All(canceladas, o => Assert.Equal("Cancelada", o.GetProperty("status").GetString()));
        Assert.Equal(HttpStatusCode.BadRequest, (await _cliente.GetAsync("/api/ordens-producao?status=Banana")).StatusCode);
    }

    [Fact]
    public async Task Integracao_ErpListaPedidosSimulados()
    {
        var resposta = await _cliente.GetAsync("/api/integracoes/erp/pedidos");

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        Assert.Equal(2, (await LerAsync(resposta)).GetProperty("data").GetArrayLength());
    }

    [Fact]
    public async Task Integracao_SgqConfirmaResultado_EValidaEntrada()
    {
        var ok = await _cliente.PostAsJsonAsync("/api/integracoes/sgq/resultados",
            new { ordemNumero = "OP-2026-0001", quantidadeInspecionada = 20, quantidadeDefeituosa = 2 });
        var invalido = await _cliente.PostAsJsonAsync("/api/integracoes/sgq/resultados",
            new { ordemNumero = "OP-2026-0001", quantidadeInspecionada = 2, quantidadeDefeituosa = 3 });

        Assert.Equal(HttpStatusCode.OK, ok.StatusCode);
        Assert.Equal("SGQ-SIM-OP-2026-0001", (await LerAsync(ok)).GetProperty("data").GetProperty("protocolo").GetString());
        Assert.Equal(HttpStatusCode.BadRequest, invalido.StatusCode);
    }
}
