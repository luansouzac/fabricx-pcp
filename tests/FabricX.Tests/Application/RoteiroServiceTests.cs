using FabricX.Application.DTOs.Roteiro;
using FabricX.Application.Exceptions;
using FabricX.Application.Services;
using FabricX.Domain.Entities;
using FabricX.Domain.Enums;
using FabricX.Domain.Exceptions;
using FabricX.Tests.Fakes;

namespace FabricX.Tests.Application;

public class RoteiroServiceTests
{
    private static readonly CancellationToken Ct = CancellationToken.None;

    private readonly ProdutoRepositoryFake _produtos = new();
    private readonly RecursoRepositoryFake _recursos = new();
    private readonly RoteiroEtapaRepositoryFake _etapas = new();
    private readonly RoteiroService _service;

    private Produto _produto = null!;
    private Recurso _recurso = null!;

    public RoteiroServiceTests()
    {
        _service = new RoteiroService(_etapas, _produtos, _recursos);
    }

    private async Task PrepararAsync()
    {
        _produto = new Produto("EP-003", "Garrafa d'água", TipoProduto.EmProcesso);
        await _produtos.AdicionarAsync(_produto, Ct);
        _recurso = new Recurso("Montadora", TipoRecurso.Maquina, 8);
        await _recursos.AdicionarAsync(_recurso, Ct);
    }

    private CriarEtapaDto Dto(int sequencia = 1, double ciclo = 2, int unidades = 10) =>
        new() { RecursoId = _recurso.Id, Sequencia = sequencia, Nome = "Montagem", TempoCicloMin = ciclo, UnidadesPorCiclo = unidades };

    [Fact]
    public async Task Adicionar_DadosValidos_DevolveEtapaComNomeDoRecurso()
    {
        await PrepararAsync();

        var etapa = await _service.AdicionarAsync(_produto.Id, Dto(), Ct);

        Assert.Equal("Montadora", etapa.RecursoNome);
        Assert.Equal(10, etapa.UnidadesPorCiclo);
        Assert.Single(_etapas.Etapas);
    }

    [Fact]
    public async Task Adicionar_ProdutoInexistente_LancaNotFoundException()
    {
        await PrepararAsync();

        await Assert.ThrowsAsync<NotFoundException>(() => _service.AdicionarAsync(99, Dto(), Ct));
    }

    [Fact]
    public async Task Adicionar_RecursoInexistente_LancaNotFoundException()
    {
        await PrepararAsync();
        var dto = Dto();
        dto.RecursoId = 99;

        await Assert.ThrowsAsync<NotFoundException>(() => _service.AdicionarAsync(_produto.Id, dto, Ct));
    }

    [Fact]
    public async Task Adicionar_RecursoInativo_LancaDomainException()
    {
        await PrepararAsync();
        _recurso.Inativar();

        await Assert.ThrowsAsync<DomainException>(() => _service.AdicionarAsync(_produto.Id, Dto(), Ct));
    }

    [Fact]
    public async Task Adicionar_SequenciaDuplicada_LancaDomainException()
    {
        await PrepararAsync();
        await _service.AdicionarAsync(_produto.Id, Dto(1), Ct);

        await Assert.ThrowsAsync<DomainException>(() => _service.AdicionarAsync(_produto.Id, Dto(1), Ct));
    }

    [Fact]
    public async Task Adicionar_CicloOuUnidadesInvalidos_LancaDomainException()
    {
        await PrepararAsync();

        await Assert.ThrowsAsync<DomainException>(() => _service.AdicionarAsync(_produto.Id, Dto(ciclo: 0), Ct));
        await Assert.ThrowsAsync<DomainException>(() => _service.AdicionarAsync(_produto.Id, Dto(unidades: 0), Ct));
    }

    [Fact]
    public async Task Listar_OrdenaPorSequencia()
    {
        await PrepararAsync();
        await _service.AdicionarAsync(_produto.Id, Dto(2), Ct);
        await _service.AdicionarAsync(_produto.Id, Dto(1), Ct);

        var etapas = await _service.ListarAsync(_produto.Id, Ct);

        Assert.Equal([1, 2], etapas.Select(e => e.Sequencia));
    }

    [Fact]
    public async Task Atualizar_MantendoASequenciaDaPropriaEtapa_Permite()
    {
        await PrepararAsync();
        var criada = await _service.AdicionarAsync(_produto.Id, Dto(1), Ct);

        var atualizada = await _service.AtualizarAsync(_produto.Id, criada.Id,
            new AtualizarEtapaDto { RecursoId = _recurso.Id, Sequencia = 1, Nome = "Nova", TempoCicloMin = 4, UnidadesPorCiclo = 5 }, Ct);

        Assert.Equal("Nova", atualizada.Nome);
        Assert.Equal(4, atualizada.TempoCicloMin);
    }

    [Fact]
    public async Task Atualizar_SequenciaDeOutraEtapa_LancaDomainException()
    {
        await PrepararAsync();
        await _service.AdicionarAsync(_produto.Id, Dto(1), Ct);
        var segunda = await _service.AdicionarAsync(_produto.Id, Dto(2), Ct);

        await Assert.ThrowsAsync<DomainException>(() => _service.AtualizarAsync(_produto.Id, segunda.Id,
            new AtualizarEtapaDto { RecursoId = _recurso.Id, Sequencia = 1, Nome = "X", TempoCicloMin = 1, UnidadesPorCiclo = 1 }, Ct));
    }

    [Fact]
    public async Task Atualizar_EtapaDeOutroProduto_LancaNotFoundException()
    {
        await PrepararAsync();
        var outro = new Produto("PA-003", "Kit", TipoProduto.Acabado);
        await _produtos.AdicionarAsync(outro, Ct);
        var etapa = await _service.AdicionarAsync(_produto.Id, Dto(), Ct);

        await Assert.ThrowsAsync<NotFoundException>(() => _service.AtualizarAsync(outro.Id, etapa.Id,
            new AtualizarEtapaDto { RecursoId = _recurso.Id, Sequencia = 1, Nome = "X", TempoCicloMin = 1, UnidadesPorCiclo = 1 }, Ct));
    }

    [Fact]
    public async Task Remover_ExcluiAEtapa()
    {
        await PrepararAsync();
        var criada = await _service.AdicionarAsync(_produto.Id, Dto(), Ct);

        await _service.RemoverAsync(_produto.Id, criada.Id, Ct);

        Assert.Empty(_etapas.Etapas);
    }

    [Fact]
    public async Task Remover_Inexistente_LancaNotFoundException()
    {
        await PrepararAsync();

        await Assert.ThrowsAsync<NotFoundException>(() => _service.RemoverAsync(_produto.Id, 99, Ct));
    }
}
