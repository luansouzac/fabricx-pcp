using FabricX.Application.DTOs.Recursos;
using FabricX.Application.Exceptions;
using FabricX.Application.Services;
using FabricX.Domain.Enums;
using FabricX.Domain.Exceptions;
using FabricX.Tests.Fakes;

namespace FabricX.Tests.Application;

public class RecursoServiceTests
{
    private static readonly CancellationToken Ct = CancellationToken.None;

    private readonly RecursoService _service = new(new RecursoRepositoryFake());

    private static CriarRecursoDto NovoDto(string nome = "Montadora", double capacidade = 8) =>
        new() { Nome = nome, Tipo = TipoRecurso.Maquina, CapacidadeHorasDia = capacidade };

    [Fact]
    public async Task Criar_DadosValidos_RetornaDtoAtivoComId()
    {
        var recurso = await _service.CriarAsync(NovoDto(), Ct);

        Assert.Equal(1, recurso.Id);
        Assert.Equal("Montadora", recurso.Nome);
        Assert.True(recurso.Ativo);
    }

    [Fact]
    public async Task Criar_NomeDuplicado_LancaDomainException()
    {
        await _service.CriarAsync(NovoDto(), Ct);

        await Assert.ThrowsAsync<DomainException>(() => _service.CriarAsync(NovoDto(), Ct));
    }

    [Fact]
    public async Task Criar_CapacidadeZero_LancaDomainException()
    {
        await Assert.ThrowsAsync<DomainException>(() => _service.CriarAsync(NovoDto(capacidade: 0), Ct));
    }

    [Fact]
    public async Task Atualizar_MantendoOProprioNome_Permite()
    {
        var criado = await _service.CriarAsync(NovoDto(), Ct);

        var atualizado = await _service.AtualizarAsync(criado.Id,
            new AtualizarRecursoDto { Nome = "Montadora", Tipo = TipoRecurso.Linha, CapacidadeHorasDia = 6 }, Ct);

        Assert.Equal(TipoRecurso.Linha, atualizado.Tipo);
        Assert.Equal(6, atualizado.CapacidadeHorasDia);
    }

    [Fact]
    public async Task Atualizar_NomeDeOutroRecurso_LancaDomainException()
    {
        await _service.CriarAsync(NovoDto("Montadora"), Ct);
        var outro = await _service.CriarAsync(NovoDto("Embaladora"), Ct);

        await Assert.ThrowsAsync<DomainException>(() => _service.AtualizarAsync(outro.Id,
            new AtualizarRecursoDto { Nome = "Montadora", Tipo = TipoRecurso.Maquina, CapacidadeHorasDia = 8 }, Ct));
    }

    [Fact]
    public async Task Atualizar_Inexistente_LancaNotFoundException()
    {
        await Assert.ThrowsAsync<NotFoundException>(() => _service.AtualizarAsync(99,
            new AtualizarRecursoDto { Nome = "X", Tipo = TipoRecurso.Maquina, CapacidadeHorasDia = 1 }, Ct));
    }

    [Fact]
    public async Task Inativar_MarcaInativoSemApagar()
    {
        var criado = await _service.CriarAsync(NovoDto(), Ct);

        var inativo = await _service.InativarAsync(criado.Id, Ct);

        Assert.False(inativo.Ativo);
        Assert.Single(await _service.ListarAsync(Ct));
    }

    [Fact]
    public async Task ObterPorId_Inexistente_LancaNotFoundException()
    {
        await Assert.ThrowsAsync<NotFoundException>(() => _service.ObterPorIdAsync(99, Ct));
    }
}
