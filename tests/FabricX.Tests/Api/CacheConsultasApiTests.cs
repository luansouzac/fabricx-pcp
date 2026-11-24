using System.Net.Http.Json;
using System.Text.Json;
using FabricX.Tests.Fakes;

namespace FabricX.Tests.Api;

public class CacheConsultasApiTests : IClassFixture<ApiFactory>
{
    private readonly HttpClient _cliente;

    public CacheConsultasApiTests(ApiFactory fabrica)
    {
        _cliente = fabrica.CriarClienteAutenticado();
    }

    private async Task<JsonElement> ResumoAsync() =>
        (await (await _cliente.GetAsync("/api/indicadores/resumo")).Content.ReadFromJsonAsync<JsonElement>()).GetProperty("data");

    [Fact]
    public async Task Resumo_EscritaBemSucedidaInvalidaOCacheEOResumoRefleteOProdutoNovo()
    {
        var antes = (await ResumoAsync()).GetProperty("produtosComEstoqueBaixo").GetArrayLength();
        await ResumoAsync();

        var criado = await _cliente.PostAsJsonAsync("/api/produtos", new
        {
            codigo = $"CACHE-{Guid.NewGuid():N}"[..12],
            nome = "Item em falta",
            tipo = "MateriaPrima",
            unidadeMedida = "UN",
            estoqueAtual = 1,
            estoqueMinimo = 10,
            custoUnitario = 1
        });

        Assert.True(criado.IsSuccessStatusCode);
        var depois = (await ResumoAsync()).GetProperty("produtosComEstoqueBaixo").GetArrayLength();
        Assert.Equal(antes + 1, depois);
    }
}
