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
        _cliente = fabrica.CriarClienteAutenticado();
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
    public async Task Post_VariosCamposInvalidos_ListaUmErroPorProblemaEmErrors()
    {
        var resposta = await _cliente.PostAsJsonAsync("/api/recursos", new { tipo = "Maquina", capacidadeHorasDia = 0 });

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        var corpo = await LerAsync(resposta);
        Assert.Equal(2, corpo.GetProperty("errors").GetArrayLength());
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

    private async Task<(int OrdemId, int Montagem, int Embalagem, int Montadora, int Linha)> PrepararKitAsync(double quantidade = 60)
    {
        var kit = await CriarProdutoAsync();
        var montadora = await CriarAsync(NomeUnico());
        var linha = await CriarAsync(NomeUnico());
        var montagem = (await LerAsync(await AdicionarEtapaAsync(kit, montadora, 1, 2, 10))).GetProperty("data").GetProperty("id").GetInt32();
        var embalagem = (await LerAsync(await _cliente.PostAsJsonAsync($"/api/produtos/{kit}/roteiro",
            new { recursoId = linha, sequencia = 2, nome = "Embalagem", tempoCicloMin = 2, unidadesPorCiclo = 1 }))).GetProperty("data").GetProperty("id").GetInt32();
        var ordem = await _cliente.PostAsJsonAsync("/api/ordens-producao", new
        {
            produtoId = kit,
            quantidade,
            prazoEntrega = DateOnly.FromDateTime(DateTime.Today.AddDays(40)).ToString("yyyy-MM-dd")
        });
        var ordemId = (await LerAsync(ordem)).GetProperty("data").GetProperty("id").GetInt32();
        return (ordemId, montagem, embalagem, montadora, linha);
    }

    private Task<HttpResponseMessage> AgendarAsync(int ordemId, int etapaId, DateTime inicio, int? recursoId = null) =>
        _cliente.PostAsJsonAsync($"/api/ordens-producao/{ordemId}/programacao",
            new { etapaId, inicio = inicio.ToString("yyyy-MM-ddTHH:mm:ss"), recursoId });

    private static DateTime Dia() => DateTime.Today.AddDays(15);

    [Fact]
    public async Task Programacao_FimCalculadoEOrdemFicaProgramadaAoAgendarTodasAsEtapas()
    {
        var (ordem, montagem, embalagem, _, _) = await PrepararKitAsync(60);

        var primeira = await AgendarAsync(ordem, montagem, Dia().AddHours(8));
        Assert.Equal(HttpStatusCode.Created, primeira.StatusCode);
        var dados = (await LerAsync(primeira)).GetProperty("data");
        Assert.Equal(Dia().AddHours(8).AddMinutes(12), dados.GetProperty("fimPrevisto").GetDateTime());
        Assert.Equal("Agendada", dados.GetProperty("status").GetString());

        var statusAntes = (await LerAsync(await _cliente.GetAsync($"/api/ordens-producao/{ordem}"))).GetProperty("data").GetProperty("status").GetString();
        Assert.Equal("Planejada", statusAntes);

        var segunda = await AgendarAsync(ordem, embalagem, Dia().AddHours(9));
        Assert.Equal(HttpStatusCode.Created, segunda.StatusCode);
        Assert.Equal(Dia().AddHours(9).AddMinutes(120), (await LerAsync(segunda)).GetProperty("data").GetProperty("fimPrevisto").GetDateTime());

        var statusDepois = (await LerAsync(await _cliente.GetAsync($"/api/ordens-producao/{ordem}"))).GetProperty("data").GetProperty("status").GetString();
        Assert.Equal("Programada", statusDepois);
        Assert.Equal(2, (await LerAsync(await _cliente.GetAsync($"/api/ordens-producao/{ordem}/programacao"))).GetProperty("data").GetArrayLength());
    }

    [Fact]
    public async Task Programacao_ConflitoDeRecurso_Retorna400()
    {
        var (ordem1, montagem1, _, montadora, _) = await PrepararKitAsync(60);
        var (ordem2, montagem2, _, _, _) = await PrepararKitAsync(60);
        await AgendarAsync(ordem1, montagem1, Dia().AddHours(8));

        var resposta = await AgendarAsync(ordem2, montagem2, Dia().AddHours(8).AddMinutes(5), montadora);

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Contains("já está ocupado", (await LerAsync(resposta)).GetProperty("message").GetString());
    }

    [Fact]
    public async Task Programacao_SegundaEtapaAntesDaPrimeira_Retorna400()
    {
        var (ordem, _, embalagem, _, _) = await PrepararKitAsync();

        var resposta = await AgendarAsync(ordem, embalagem, Dia().AddHours(8));

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
    }

    [Fact]
    public async Task Programacao_OrdemOuEtapaInexistente_Retorna404()
    {
        var (ordem, _, _, _, _) = await PrepararKitAsync();

        Assert.Equal(HttpStatusCode.NotFound, (await AgendarAsync(999999, 1, Dia())).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await AgendarAsync(ordem, 999999, Dia())).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await _cliente.GetAsync("/api/ordens-producao/999999/programacao")).StatusCode);
    }

    [Fact]
    public async Task Programacao_CronogramaPorRecursoEPeriodo_OrdenadoPorInicio()
    {
        var (ordem, montagem, embalagem, montadora, linha) = await PrepararKitAsync(10);
        await AgendarAsync(ordem, montagem, Dia().AddHours(8));
        await AgendarAsync(ordem, embalagem, Dia().AddHours(10));
        var dia = Dia().ToString("yyyy-MM-dd");

        var doRecurso = (await LerAsync(await _cliente.GetAsync($"/api/programacao?recursoId={linha}&de={dia}&ate={dia}"))).GetProperty("data").EnumerateArray().ToList();
        var outroDia = (await LerAsync(await _cliente.GetAsync($"/api/programacao?recursoId={montadora}&de={Dia().AddDays(2):yyyy-MM-dd}"))).GetProperty("data");

        Assert.Single(doRecurso);
        Assert.Equal("Embalagem", doRecurso[0].GetProperty("etapaNome").GetString());
        Assert.Equal(0, outroDia.GetArrayLength());
    }

    [Fact]
    public async Task Programacao_ReprogramaERemove()
    {
        var (ordem, montagem, _, _, _) = await PrepararKitAsync();
        var id = (await LerAsync(await AgendarAsync(ordem, montagem, Dia().AddHours(8)))).GetProperty("data").GetProperty("id").GetInt32();

        var nova = await _cliente.PutAsJsonAsync($"/api/programacao/{id}", new { inicio = Dia().AddHours(10).ToString("yyyy-MM-ddTHH:mm:ss") });
        Assert.Equal(HttpStatusCode.OK, nova.StatusCode);
        Assert.Equal(Dia().AddHours(10), (await LerAsync(nova)).GetProperty("data").GetProperty("inicioPrevisto").GetDateTime());

        Assert.Equal(HttpStatusCode.OK, (await _cliente.DeleteAsync($"/api/programacao/{id}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await _cliente.DeleteAsync($"/api/programacao/{id}")).StatusCode);
    }

    private async Task<int> AgendarMontagemAsync(double quantidade)
    {
        var (ordem, montagem, _, _, _) = await PrepararKitAsync(quantidade);
        var resposta = await AgendarAsync(ordem, montagem, Dia().AddHours(8));
        return (await LerAsync(resposta)).GetProperty("data").GetProperty("id").GetInt32();
    }

    private Task<HttpResponseMessage> ApontarAsync(int programacaoId, double produzida, double defeituosa = 0, double tempo = 10) =>
        _cliente.PostAsJsonAsync($"/api/programacao/{programacaoId}/apontamentos",
            new { quantidadeProduzida = produzida, quantidadeDefeituosa = defeituosa, tempoGastoMin = tempo });

    [Fact]
    public async Task Apontamento_PrimeiroColocaEmAndamentoESomaAtingidaConclui()
    {
        var programacao = await AgendarMontagemAsync(60);

        var primeiro = await ApontarAsync(programacao, 40, 2, 8);
        Assert.Equal(HttpStatusCode.Created, primeiro.StatusCode);
        Assert.Equal("EmAndamento", (await LerAsync(primeiro)).GetProperty("data").GetProperty("statusProgramacao").GetString());

        var ultimo = await ApontarAsync(programacao, 20, 0, 4);
        Assert.Equal("Concluida", (await LerAsync(ultimo)).GetProperty("data").GetProperty("statusProgramacao").GetString());

        Assert.Equal(HttpStatusCode.BadRequest, (await ApontarAsync(programacao, 1)).StatusCode);
        Assert.Equal(2, (await LerAsync(await _cliente.GetAsync($"/api/programacao/{programacao}/apontamentos"))).GetProperty("data").GetArrayLength());
    }

    [Fact]
    public async Task Apontamento_ValoresInvalidosOuProgramacaoInexistente_Retornam400E404()
    {
        var programacao = await AgendarMontagemAsync(60);

        Assert.Equal(HttpStatusCode.BadRequest, (await ApontarAsync(programacao, 0, 0)).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await ApontarAsync(programacao, 5, 0, 0)).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await ApontarAsync(programacao, -1, 3)).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await ApontarAsync(999999, 5)).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await _cliente.GetAsync("/api/programacao/999999/apontamentos")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await _cliente.GetAsync("/api/ordens-producao/999999/apontamentos")).StatusCode);
    }

    [Fact]
    public async Task Indicadores_CalculamComOsApontamentosDoProduto()
    {
        var (ordem, montagem, _, _, _) = await PrepararKitAsync(100);
        var produtoId = (await LerAsync(await _cliente.GetAsync($"/api/ordens-producao/{ordem}"))).GetProperty("data").GetProperty("produtoId").GetInt32();
        var programacao = (await LerAsync(await AgendarAsync(ordem, montagem, Dia().AddHours(8)))).GetProperty("data").GetProperty("id").GetInt32();
        await ApontarAsync(programacao, 18, 2, 5);
        await ApontarAsync(programacao, 25, 0, 8);

        var dados = (await LerAsync(await _cliente.GetAsync($"/api/indicadores?produtoId={produtoId}"))).GetProperty("data");

        Assert.Equal(2.0 / 45, dados.GetProperty("qualidade").GetDouble(), 6);
        Assert.Equal(10.0 / 13, dados.GetProperty("eficiencia").GetDouble(), 6);
        Assert.Equal(43 / (13.0 / 60), dados.GetProperty("produtividadePorHora").GetDouble(), 4);
        Assert.Equal(JsonValueKind.Null, dados.GetProperty("cumprimentoDePrazo").ValueKind);
    }

    [Fact]
    public async Task Indicadores_SemDados_VoltamNulosEResumoResponde()
    {
        var vazio = (await LerAsync(await _cliente.GetAsync("/api/indicadores?produtoId=999999"))).GetProperty("data");
        Assert.Equal(JsonValueKind.Null, vazio.GetProperty("eficiencia").ValueKind);

        var resumo = await _cliente.GetAsync("/api/indicadores/resumo");

        Assert.Equal(HttpStatusCode.OK, resumo.StatusCode);
        var dados = (await LerAsync(resumo)).GetProperty("data");
        Assert.Equal(5, dados.GetProperty("ordensPorStatus").GetArrayLength());
        Assert.Equal(JsonValueKind.Array, dados.GetProperty("produtosComEstoqueBaixo").ValueKind);
    }

    private Task<HttpResponseMessage> CriarDemandaAsync(int produtoId, string periodo, double quantidade = 600) =>
        _cliente.PostAsJsonAsync("/api/demanda-prevista", new { produtoId, periodo, quantidade });

    [Fact]
    public async Task Demanda_CriaListaAlteraERemove_EValidaDuplicidadeEPeriodo()
    {
        var produto = await CriarProdutoAsync();
        var periodo = "2031-05";

        var criada = await CriarDemandaAsync(produto, periodo);
        Assert.Equal(HttpStatusCode.Created, criada.StatusCode);
        var id = (await LerAsync(criada)).GetProperty("data").GetProperty("id").GetInt32();

        Assert.Equal(HttpStatusCode.BadRequest, (await CriarDemandaAsync(produto, periodo)).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await CriarDemandaAsync(produto, "2031-13")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await CriarDemandaAsync(999999, periodo)).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await _cliente.GetAsync("/api/demanda-prevista?periodo=maio")).StatusCode);

        var lista = (await LerAsync(await _cliente.GetAsync($"/api/demanda-prevista?periodo={periodo}"))).GetProperty("data").EnumerateArray().ToList();
        Assert.Contains(lista, d => d.GetProperty("id").GetInt32() == id);

        var alterada = await _cliente.PutAsJsonAsync($"/api/demanda-prevista/{id}", new { periodo, quantidade = 900 });
        Assert.Equal(900, (await LerAsync(alterada)).GetProperty("data").GetProperty("quantidade").GetDouble());

        Assert.Equal(HttpStatusCode.OK, (await _cliente.DeleteAsync($"/api/demanda-prevista/{id}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await _cliente.DeleteAsync($"/api/demanda-prevista/{id}")).StatusCode);
    }

    [Fact]
    public async Task Capacidade_MostraDemandaProgramacaoEClassificacaoPorRecurso()
    {
        var kit = await CriarProdutoAsync();
        var linha = await CriarAsync(NomeUnico(), 8);
        await _cliente.PostAsJsonAsync($"/api/produtos/{kit}/roteiro",
            new { recursoId = linha, sequencia = 1, nome = "Embalagem", tempoCicloMin = 2, unidadesPorCiclo = 1 });
        await CriarDemandaAsync(kit, "2032-01", 8000);

        var resposta = await _cliente.GetAsync("/api/capacidade?periodo=2032-01");

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        var dados = (await LerAsync(resposta)).GetProperty("data");
        Assert.Equal(31, dados.GetProperty("diasNoPeriodo").GetInt32());
        var item = dados.GetProperty("recursos").EnumerateArray().Single(r => r.GetProperty("recursoId").GetInt32() == linha);
        Assert.Equal(248, item.GetProperty("capacidadeHorasPeriodo").GetDouble());
        Assert.Equal(16000.0 / 60, item.GetProperty("horasNecessariasDemanda").GetDouble(), 6);
        Assert.Equal("Gargalo", item.GetProperty("classificacao").GetString());
        Assert.Equal(HttpStatusCode.BadRequest, (await _cliente.GetAsync("/api/capacidade?periodo=2032-99")).StatusCode);
    }
}
