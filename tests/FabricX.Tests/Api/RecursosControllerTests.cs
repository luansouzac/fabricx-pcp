using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FabricX.Tests.Fakes;

namespace FabricX.Tests.Api;

public class RecursosControllerTests : IClassFixture<ApiFactory>
{
    private readonly HttpClient _cliente;

    public RecursosControllerTests(ApiFactory fabrica)
    {
        _cliente = fabrica.CreateClient();
    }

    private static string NomeUnico() => $"Recurso-{Guid.NewGuid():N}"[..16];

    private static async Task<JsonElement> LerAsync(HttpResponseMessage resposta) =>
        await resposta.Content.ReadFromJsonAsync<JsonElement>();

    private async Task<int> CriarAsync(string nome, double capacidade = 8)
    {
        var resposta = await _cliente.PostAsJsonAsync("/api/recursos", new { nome, tipo = "Maquina", capacidadeHorasDia = capacidade });
        return (await LerAsync(resposta)).GetProperty("data").GetProperty("id").GetInt32();
    }

    [Fact]
    public async Task Post_DadosValidos_Retorna201ComWebResponse()
    {
        var resposta = await _cliente.PostAsJsonAsync("/api/recursos", new { nome = NomeUnico(), tipo = "Linha", capacidadeHorasDia = 8 });

        Assert.Equal(HttpStatusCode.Created, resposta.StatusCode);
        var corpo = await LerAsync(resposta);
        Assert.True(corpo.GetProperty("isSuccess").GetBoolean());
        Assert.Equal("Linha", corpo.GetProperty("data").GetProperty("tipo").GetString());
        Assert.True(corpo.GetProperty("data").GetProperty("ativo").GetBoolean());
    }

    [Fact]
    public async Task Post_NomeDuplicado_Retorna400()
    {
        var nome = NomeUnico();
        await CriarAsync(nome);

        var resposta = await _cliente.PostAsJsonAsync("/api/recursos", new { nome, tipo = "Maquina", capacidadeHorasDia = 8 });

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Contains("Já existe", (await LerAsync(resposta)).GetProperty("message").GetString());
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(25)]
    public async Task Post_CapacidadeForaDoIntervalo_Retorna400(double capacidade)
    {
        var resposta = await _cliente.PostAsJsonAsync("/api/recursos", new { nome = NomeUnico(), tipo = "Maquina", capacidadeHorasDia = capacidade });

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
    }

    [Fact]
    public async Task Get_Inexistente_Retorna404()
    {
        var resposta = await _cliente.GetAsync("/api/recursos/999999");

        Assert.Equal(HttpStatusCode.NotFound, resposta.StatusCode);
        Assert.False((await LerAsync(resposta)).GetProperty("isSuccess").GetBoolean());
    }

    [Fact]
    public async Task Put_AtualizaCampos()
    {
        var id = await CriarAsync(NomeUnico());

        var resposta = await _cliente.PutAsJsonAsync($"/api/recursos/{id}", new { nome = NomeUnico(), tipo = "Operador", capacidadeHorasDia = 6 });

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        var dados = (await LerAsync(resposta)).GetProperty("data");
        Assert.Equal("Operador", dados.GetProperty("tipo").GetString());
        Assert.Equal(6, dados.GetProperty("capacidadeHorasDia").GetDouble());
    }

    [Fact]
    public async Task Delete_InativaSemApagar()
    {
        var id = await CriarAsync(NomeUnico());

        var resposta = await _cliente.DeleteAsync($"/api/recursos/{id}");

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        Assert.False((await LerAsync(resposta)).GetProperty("data").GetProperty("ativo").GetBoolean());
        Assert.Equal(HttpStatusCode.OK, (await _cliente.GetAsync($"/api/recursos/{id}")).StatusCode);
    }

    [Fact]
    public async Task Get_Lista_RetornaOsRecursosCriados()
    {
        var nome = NomeUnico();
        await CriarAsync(nome);

        var itens = (await LerAsync(await _cliente.GetAsync("/api/recursos"))).GetProperty("data").EnumerateArray().ToList();

        Assert.Contains(itens, r => r.GetProperty("nome").GetString() == nome);
    }

    private static string CodigoUnico() => $"P{Guid.NewGuid():N}"[..12];

    private async Task<int> CriarProdutoAsync()
    {
        var resposta = await _cliente.PostAsJsonAsync("/api/produtos",
            new { codigo = CodigoUnico(), nome = "Garrafa d'água", tipo = "EmProcesso", unidadeMedida = "UN" });
        return (await LerAsync(resposta)).GetProperty("data").GetProperty("id").GetInt32();
    }

    private Task<HttpResponseMessage> AdicionarEtapaAsync(int produtoId, int recursoId, int sequencia = 1, double ciclo = 2, int unidades = 10) =>
        _cliente.PostAsJsonAsync($"/api/produtos/{produtoId}/roteiro",
            new { recursoId, sequencia, nome = "Montagem", tempoCicloMin = ciclo, unidadesPorCiclo = unidades });

    [Fact]
    public async Task Roteiro_AdicionaEListaPorSequenciaComNomeDoRecurso()
    {
        var produto = await CriarProdutoAsync();
        var nome = NomeUnico();
        var recurso = await CriarAsync(nome);

        Assert.Equal(HttpStatusCode.Created, (await AdicionarEtapaAsync(produto, recurso, 2)).StatusCode);
        Assert.Equal(HttpStatusCode.Created, (await AdicionarEtapaAsync(produto, recurso, 1)).StatusCode);

        var itens = (await LerAsync(await _cliente.GetAsync($"/api/produtos/{produto}/roteiro"))).GetProperty("data").EnumerateArray().ToList();
        Assert.Equal([1, 2], itens.Select(e => e.GetProperty("sequencia").GetInt32()));
        Assert.Equal(nome, itens[0].GetProperty("recursoNome").GetString());
        Assert.Equal(10, itens[0].GetProperty("unidadesPorCiclo").GetInt32());
    }

    [Fact]
    public async Task Roteiro_SequenciaDuplicadaCicloZeroOuUnidadesZero_Retorna400()
    {
        var produto = await CriarProdutoAsync();
        var recurso = await CriarAsync(NomeUnico());
        await AdicionarEtapaAsync(produto, recurso, 1);

        Assert.Equal(HttpStatusCode.BadRequest, (await AdicionarEtapaAsync(produto, recurso, 1)).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await AdicionarEtapaAsync(produto, recurso, 2, ciclo: 0)).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await AdicionarEtapaAsync(produto, recurso, 3, unidades: 0)).StatusCode);
    }

    [Fact]
    public async Task Roteiro_RecursoInexistente_Retorna404EInativo_Retorna400()
    {
        var produto = await CriarProdutoAsync();
        var inativo = await CriarAsync(NomeUnico());
        await _cliente.DeleteAsync($"/api/recursos/{inativo}");

        Assert.Equal(HttpStatusCode.NotFound, (await AdicionarEtapaAsync(produto, 999999)).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await AdicionarEtapaAsync(produto, inativo)).StatusCode);
    }

    [Fact]
    public async Task Roteiro_AlteraERemove()
    {
        var produto = await CriarProdutoAsync();
        var recurso = await CriarAsync(NomeUnico());
        var etapa = (await LerAsync(await AdicionarEtapaAsync(produto, recurso))).GetProperty("data").GetProperty("id").GetInt32();

        var alterada = await _cliente.PutAsJsonAsync($"/api/produtos/{produto}/roteiro/{etapa}",
            new { recursoId = recurso, sequencia = 1, nome = "Nova", tempoCicloMin = 4, unidadesPorCiclo = 5 });
        Assert.Equal(HttpStatusCode.OK, alterada.StatusCode);
        Assert.Equal(5, (await LerAsync(alterada)).GetProperty("data").GetProperty("unidadesPorCiclo").GetInt32());

        Assert.Equal(HttpStatusCode.OK, (await _cliente.DeleteAsync($"/api/produtos/{produto}/roteiro/{etapa}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await _cliente.DeleteAsync($"/api/produtos/{produto}/roteiro/{etapa}")).StatusCode);
    }

    [Fact]
    public async Task Roteiro_ProdutoInexistente_Retorna404()
    {
        Assert.Equal(HttpStatusCode.NotFound, (await _cliente.GetAsync("/api/produtos/999999/roteiro")).StatusCode);
    }
}
