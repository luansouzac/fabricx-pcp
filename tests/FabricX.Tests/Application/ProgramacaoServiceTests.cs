using FabricX.Application.DTOs.Programacao;
using FabricX.Application.Exceptions;
using FabricX.Application.Services;
using FabricX.Domain.Entities;
using FabricX.Domain.Enums;
using FabricX.Domain.Exceptions;
using FabricX.Tests.Fakes;

namespace FabricX.Tests.Application;

public class ProgramacaoServiceTests
{
    private static readonly CancellationToken Ct = CancellationToken.None;
    private static readonly DateTime Dia = DateTime.Today.AddDays(10);

    private readonly ProgramacaoRepositoryFake _programacoes = new();
    private readonly OrdemProducaoRepositoryFake _ordens = new();
    private readonly ProdutoRepositoryFake _produtos = new();
    private readonly RoteiroEtapaRepositoryFake _etapas = new();
    private readonly RecursoRepositoryFake _recursos = new();
    private readonly ProgramacaoService _service;

    private Produto _kit = null!;
    private Recurso _montadora = null!;
    private Recurso _embalagem = null!;
    private RoteiroEtapa _montagem = null!;
    private RoteiroEtapa _embalar = null!;
    private OrdemProducao _ordem = null!;

    public ProgramacaoServiceTests()
    {
        _service = new ProgramacaoService(_programacoes, _ordens, _produtos, _etapas, _recursos);
    }

    private async Task PrepararAsync(double quantidade = 60)
    {
        _kit = new Produto("PA-003", "Kit", TipoProduto.Acabado);
        await _produtos.AdicionarAsync(_kit, Ct);
        _montadora = new Recurso("Montadora", TipoRecurso.Maquina, 8);
        _embalagem = new Recurso("Linha de embalagem", TipoRecurso.Linha, 8);
        await _recursos.AdicionarAsync(_montadora, Ct);
        await _recursos.AdicionarAsync(_embalagem, Ct);
        _montagem = new RoteiroEtapa(_kit.Id, _montadora.Id, 1, "Montagem", 2, 10);
        _embalar = new RoteiroEtapa(_kit.Id, _embalagem.Id, 2, "Embalagem", 2, 1);
        await _etapas.AdicionarAsync(_montagem, Ct);
        await _etapas.AdicionarAsync(_embalar, Ct);
        _ordem = new OrdemProducao("OP-1", _kit.Id, quantidade, DateOnly.FromDateTime(Dia.AddDays(5)));
        await _ordens.AdicionarAsync(_ordem, Ct);
    }

    private AgendarEtapaDto Agendar(RoteiroEtapa etapa, DateTime inicio, int? recursoId = null) =>
        new() { EtapaId = etapa.Id, Inicio = inicio, RecursoId = recursoId };

    [Fact]
    public async Task Agendar_CalculaOFimPelaFormulaDoCiclo()
    {
        await PrepararAsync(60);

        var programacao = await _service.AgendarAsync(_ordem.Id, Agendar(_montagem, Dia.AddHours(8)), Ct);

        Assert.Equal(Dia.AddHours(8).AddMinutes(12), programacao.FimPrevisto);
        Assert.Equal("Montadora", programacao.RecursoNome);
        Assert.Equal("OP-1", programacao.OrdemNumero);
        Assert.Equal(StatusProgramacao.Agendada, programacao.Status);
    }

    [Fact]
    public async Task Agendar_RecursoInformado_SubstituiORecursoDoRoteiro()
    {
        await PrepararAsync();
        var outra = new Recurso("Montadora 2", TipoRecurso.Maquina, 8);
        await _recursos.AdicionarAsync(outra, Ct);

        var programacao = await _service.AgendarAsync(_ordem.Id, Agendar(_montagem, Dia.AddHours(8), outra.Id), Ct);

        Assert.Equal("Montadora 2", programacao.RecursoNome);
    }

    [Fact]
    public async Task Agendar_OrdemInexistente_LancaNotFoundException()
    {
        await PrepararAsync();

        await Assert.ThrowsAsync<NotFoundException>(() => _service.AgendarAsync(99, Agendar(_montagem, Dia), Ct));
    }

    [Fact]
    public async Task Agendar_EtapaInexistente_LancaNotFoundException()
    {
        await PrepararAsync();

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _service.AgendarAsync(_ordem.Id, new AgendarEtapaDto { EtapaId = 99, Inicio = Dia }, Ct));
    }

    [Fact]
    public async Task Agendar_ProdutoSemRoteiro_LancaDomainException()
    {
        await PrepararAsync();
        var semRoteiro = new Produto("MP-9", "Sem roteiro", TipoProduto.MateriaPrima);
        await _produtos.AdicionarAsync(semRoteiro, Ct);
        var ordem = new OrdemProducao("OP-2", semRoteiro.Id, 1, DateOnly.FromDateTime(Dia));
        await _ordens.AdicionarAsync(ordem, Ct);

        var erro = await Assert.ThrowsAsync<DomainException>(() => _service.AgendarAsync(ordem.Id, Agendar(_montagem, Dia), Ct));

        Assert.Contains("não tem roteiro", erro.Message);
    }

    [Theory]
    [InlineData(StatusOrdem.EmProducao)]
    [InlineData(StatusOrdem.Cancelada)]
    public async Task Agendar_OrdemForaDePlanejadaEProgramada_LancaDomainException(StatusOrdem status)
    {
        await PrepararAsync();
        if (status == StatusOrdem.Cancelada)
            _ordem.Cancelar();
        else
            while (_ordem.Status != status)
                _ordem.Avancar();

        await Assert.ThrowsAsync<DomainException>(() => _service.AgendarAsync(_ordem.Id, Agendar(_montagem, Dia), Ct));
    }

    [Fact]
    public async Task Agendar_RecursoInativo_LancaDomainException()
    {
        await PrepararAsync();
        _montadora.Inativar();

        await Assert.ThrowsAsync<DomainException>(() => _service.AgendarAsync(_ordem.Id, Agendar(_montagem, Dia), Ct));
    }

    [Fact]
    public async Task Agendar_EtapaJaProgramada_LancaDomainException()
    {
        await PrepararAsync();
        await _service.AgendarAsync(_ordem.Id, Agendar(_montagem, Dia.AddHours(8)), Ct);

        await Assert.ThrowsAsync<DomainException>(() => _service.AgendarAsync(_ordem.Id, Agendar(_montagem, Dia.AddHours(14)), Ct));
    }

    [Fact]
    public async Task Agendar_SegundaEtapaSemAPrimeira_LancaDomainException()
    {
        await PrepararAsync();

        await Assert.ThrowsAsync<DomainException>(() => _service.AgendarAsync(_ordem.Id, Agendar(_embalar, Dia.AddHours(8)), Ct));
    }

    [Fact]
    public async Task Agendar_SegundaEtapaAntesDoFimDaPrimeira_LancaDomainException()
    {
        await PrepararAsync();
        await _service.AgendarAsync(_ordem.Id, Agendar(_montagem, Dia.AddHours(8)), Ct);

        await Assert.ThrowsAsync<DomainException>(() => _service.AgendarAsync(_ordem.Id, Agendar(_embalar, Dia.AddHours(8).AddMinutes(5)), Ct));
    }

    [Fact]
    public async Task Agendar_ConflitoNoMesmoRecurso_LancaDomainExceptionComMensagemClara()
    {
        await PrepararAsync();
        var outraOrdem = new OrdemProducao("OP-3", _kit.Id, 60, DateOnly.FromDateTime(Dia.AddDays(5)));
        await _ordens.AdicionarAsync(outraOrdem, Ct);
        await _service.AgendarAsync(_ordem.Id, Agendar(_montagem, Dia.AddHours(8)), Ct);

        var erro = await Assert.ThrowsAsync<DomainException>(() => _service.AgendarAsync(outraOrdem.Id, Agendar(_montagem, Dia.AddHours(8).AddMinutes(6)), Ct));

        Assert.Contains("Montadora já está ocupado", erro.Message);
    }

    [Fact]
    public async Task Agendar_EstourandoACapacidadeDoDia_LancaDomainException()
    {
        await PrepararAsync(quantidade: 10 * 60 * 5);
        var erro = await Assert.ThrowsAsync<DomainException>(() => _service.AgendarAsync(_ordem.Id, Agendar(_montagem, Dia.AddHours(8)), Ct));

        Assert.Contains("excederia a capacidade", erro.Message);
    }

    [Fact]
    public async Task Agendar_TodasAsEtapas_MarcaAOrdemComoProgramada()
    {
        await PrepararAsync();

        await _service.AgendarAsync(_ordem.Id, Agendar(_montagem, Dia.AddHours(8)), Ct);
        Assert.Equal(StatusOrdem.Planejada, _ordem.Status);

        await _service.AgendarAsync(_ordem.Id, Agendar(_embalar, Dia.AddHours(9)), Ct);
        Assert.Equal(StatusOrdem.Programada, _ordem.Status);
    }

    [Fact]
    public async Task Agendar_OrdemJaProgramada_MantemOStatus()
    {
        await PrepararAsync();
        _ordem.Avancar();

        await _service.AgendarAsync(_ordem.Id, Agendar(_montagem, Dia.AddHours(8)), Ct);

        Assert.Equal(StatusOrdem.Programada, _ordem.Status);
    }

    [Fact]
    public async Task Reprogramar_RecalculaOFimEValidaDeNovo()
    {
        await PrepararAsync();
        var criada = await _service.AgendarAsync(_ordem.Id, Agendar(_montagem, Dia.AddHours(8)), Ct);

        var nova = await _service.ReprogramarAsync(criada.Id, new ReprogramarEtapaDto { Inicio = Dia.AddHours(10) }, Ct);

        Assert.Equal(Dia.AddHours(10), nova.InicioPrevisto);
        Assert.Equal(Dia.AddHours(10).AddMinutes(12), nova.FimPrevisto);
    }

    [Fact]
    public async Task Reprogramar_ParaHorarioOcupado_LancaDomainExceptionEMantemAAgenda()
    {
        await PrepararAsync();
        var outraOrdem = new OrdemProducao("OP-3", _kit.Id, 60, DateOnly.FromDateTime(Dia.AddDays(5)));
        await _ordens.AdicionarAsync(outraOrdem, Ct);
        await _service.AgendarAsync(_ordem.Id, Agendar(_montagem, Dia.AddHours(8)), Ct);
        var segunda = await _service.AgendarAsync(outraOrdem.Id, Agendar(_montagem, Dia.AddHours(12)), Ct);

        await Assert.ThrowsAsync<DomainException>(() => _service.ReprogramarAsync(segunda.Id, new ReprogramarEtapaDto { Inicio = Dia.AddHours(8).AddMinutes(3) }, Ct));

        Assert.Equal(Dia.AddHours(12), _programacoes.Programacoes.Single(p => p.Id == segunda.Id).InicioPrevisto);
    }

    [Fact]
    public async Task Reprogramar_NaoConfliteComASiMesma()
    {
        await PrepararAsync();
        var criada = await _service.AgendarAsync(_ordem.Id, Agendar(_montagem, Dia.AddHours(8)), Ct);

        var nova = await _service.ReprogramarAsync(criada.Id, new ReprogramarEtapaDto { Inicio = Dia.AddHours(8).AddMinutes(5) }, Ct);

        Assert.Equal(Dia.AddHours(8).AddMinutes(5), nova.InicioPrevisto);
    }

    [Fact]
    public async Task Reprogramar_EmAndamento_LancaDomainException()
    {
        await PrepararAsync();
        var criada = await _service.AgendarAsync(_ordem.Id, Agendar(_montagem, Dia.AddHours(8)), Ct);
        _programacoes.Programacoes.Single().Iniciar();

        await Assert.ThrowsAsync<DomainException>(() => _service.ReprogramarAsync(criada.Id, new ReprogramarEtapaDto { Inicio = Dia.AddHours(10) }, Ct));
    }

    [Fact]
    public async Task Reprogramar_Inexistente_LancaNotFoundException()
    {
        await Assert.ThrowsAsync<NotFoundException>(() => _service.ReprogramarAsync(99, new ReprogramarEtapaDto { Inicio = Dia }, Ct));
    }

    [Fact]
    public async Task Remover_Agendada_Exclui()
    {
        await PrepararAsync();
        var criada = await _service.AgendarAsync(_ordem.Id, Agendar(_montagem, Dia.AddHours(8)), Ct);

        var removida = await _service.RemoverAsync(criada.Id, Ct);

        Assert.Equal(criada.Id, removida.Id);
        Assert.Empty(_programacoes.Programacoes);
    }

    [Fact]
    public async Task Remover_EmAndamento_LancaDomainException()
    {
        await PrepararAsync();
        var criada = await _service.AgendarAsync(_ordem.Id, Agendar(_montagem, Dia.AddHours(8)), Ct);
        _programacoes.Programacoes.Single().Iniciar();

        await Assert.ThrowsAsync<DomainException>(() => _service.RemoverAsync(criada.Id, Ct));
    }

    [Fact]
    public async Task ListarCronograma_FiltraPorRecursoEPeriodoOrdenadoPorInicio()
    {
        await PrepararAsync();
        var outraOrdem = new OrdemProducao("OP-3", _kit.Id, 10, DateOnly.FromDateTime(Dia.AddDays(5)));
        await _ordens.AdicionarAsync(outraOrdem, Ct);
        await _service.AgendarAsync(_ordem.Id, Agendar(_montagem, Dia.AddHours(14)), Ct);
        await _service.AgendarAsync(outraOrdem.Id, Agendar(_montagem, Dia.AddHours(8)), Ct);
        await _service.AgendarAsync(_ordem.Id, Agendar(_embalar, Dia.AddHours(16)), Ct);

        var doRecurso = await _service.ListarCronogramaAsync(_montadora.Id, null, null, Ct);
        var doDia = await _service.ListarCronogramaAsync(null, DateOnly.FromDateTime(Dia), DateOnly.FromDateTime(Dia), Ct);
        var outroDia = await _service.ListarCronogramaAsync(null, DateOnly.FromDateTime(Dia.AddDays(3)), null, Ct);

        Assert.Equal(["OP-3", "OP-1"], doRecurso.Select(p => p.OrdemNumero));
        Assert.Equal(3, doDia.Count);
        Assert.Empty(outroDia);
    }

    [Fact]
    public async Task ListarPorOrdem_OrdemInexistente_LancaNotFoundException()
    {
        await Assert.ThrowsAsync<NotFoundException>(() => _service.ListarPorOrdemAsync(99, Ct));
    }
}
