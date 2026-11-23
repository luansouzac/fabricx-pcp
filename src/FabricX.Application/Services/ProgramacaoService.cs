using FabricX.Application.DTOs.Programacao;
using FabricX.Application.Exceptions;
using FabricX.Application.Interfaces;
using FabricX.Domain.Entities;
using FabricX.Domain.Enums;
using FabricX.Domain.Exceptions;
using FabricX.Domain.Services;

namespace FabricX.Application.Services;

public class ProgramacaoService : IProgramacaoService
{
    private readonly IProgramacaoRepository _programacoes;
    private readonly IOrdemProducaoRepository _ordens;
    private readonly IProdutoRepository _produtos;
    private readonly IRoteiroEtapaRepository _etapas;
    private readonly IRecursoRepository _recursos;

    public ProgramacaoService(
        IProgramacaoRepository programacoes,
        IOrdemProducaoRepository ordens,
        IProdutoRepository produtos,
        IRoteiroEtapaRepository etapas,
        IRecursoRepository recursos)
    {
        _programacoes = programacoes;
        _ordens = ordens;
        _produtos = produtos;
        _etapas = etapas;
        _recursos = recursos;
    }

    public async Task<ProgramacaoDto> AgendarAsync(int ordemId, AgendarEtapaDto dto, CancellationToken cancellationToken)
    {
        var ordem = await ObterOrdemOuFalharAsync(ordemId, cancellationToken);
        GarantirOrdemProgramavel(ordem);

        var roteiro = await ObterRoteiroOuFalharAsync(ordem, cancellationToken);
        var etapa = roteiro.FirstOrDefault(e => e.Id == dto.EtapaId)
            ?? throw new NotFoundException($"Etapa {dto.EtapaId} não encontrada no roteiro do produto da ordem {ordem.Numero}.");

        var daOrdem = await _programacoes.ListarPorOrdemAsync(ordem.Id, cancellationToken);
        if (daOrdem.Any(p => p.EtapaId == etapa.Id))
            throw new DomainException($"A etapa {etapa.Nome} já está programada na ordem {ordem.Numero}.");

        var recurso = await ObterRecursoAtivoOuFalharAsync(dto.RecursoId ?? etapa.RecursoId, cancellationToken);
        var inicio = dto.Inicio!.Value;
        var fim = inicio.AddMinutes(etapa.CalcularDuracaoMinutos(ordem.Quantidade));

        await ConferirRegrasAsync(etapa, recurso, inicio, fim, roteiro, daOrdem, null, cancellationToken);

        var programacao = new Programacao(ordem.Id, etapa.Id, recurso.Id, inicio, fim);

        if (ordem.Status == StatusOrdem.Planejada && roteiro.All(e => e.Id == etapa.Id || daOrdem.Any(p => p.EtapaId == e.Id)))
            ordem.Avancar();

        await _programacoes.AdicionarAsync(programacao, cancellationToken);
        return ProgramacaoDto.De(programacao, ordem, etapa, recurso);
    }

    public async Task<IReadOnlyList<ProgramacaoDto>> ListarPorOrdemAsync(int ordemId, CancellationToken cancellationToken)
    {
        var ordem = await ObterOrdemOuFalharAsync(ordemId, cancellationToken);
        var programacoes = await _programacoes.ListarPorOrdemAsync(ordem.Id, cancellationToken);
        return await MontarDtosAsync(programacoes, cancellationToken);
    }

    public async Task<IReadOnlyList<ProgramacaoDto>> ListarCronogramaAsync(int? recursoId, DateOnly? de, DateOnly? ate, CancellationToken cancellationToken)
    {
        var inicio = de?.ToDateTime(TimeOnly.MinValue);
        var fim = ate?.AddDays(1).ToDateTime(TimeOnly.MinValue);

        var programacoes = await _programacoes.ListarAsync(recursoId, inicio, fim, cancellationToken);
        return await MontarDtosAsync(programacoes, cancellationToken);
    }

    public async Task<ProgramacaoDto> ReprogramarAsync(int id, ReprogramarEtapaDto dto, CancellationToken cancellationToken)
    {
        var programacao = await ObterProgramacaoOuFalharAsync(id, cancellationToken);
        programacao.GarantirPodeSerReprogramada();

        var ordem = await ObterOrdemOuFalharAsync(programacao.OrdemId, cancellationToken);
        var roteiro = await ObterRoteiroOuFalharAsync(ordem, cancellationToken);
        var etapa = roteiro.Single(e => e.Id == programacao.EtapaId);
        var recurso = await ObterRecursoAtivoOuFalharAsync(dto.RecursoId ?? programacao.RecursoId, cancellationToken);

        var inicio = dto.Inicio!.Value;
        var fim = inicio.AddMinutes(etapa.CalcularDuracaoMinutos(ordem.Quantidade));
        var demaisDaOrdem = (await _programacoes.ListarPorOrdemAsync(ordem.Id, cancellationToken)).Where(p => p.Id != id).ToList();

        await ConferirRegrasAsync(etapa, recurso, inicio, fim, roteiro, demaisDaOrdem, id, cancellationToken);

        programacao.Reprogramar(recurso.Id, inicio, fim);

        await _programacoes.AtualizarAsync(programacao, cancellationToken);
        return ProgramacaoDto.De(programacao, ordem, etapa, recurso);
    }

    public async Task<ProgramacaoDto> RemoverAsync(int id, CancellationToken cancellationToken)
    {
        var programacao = await ObterProgramacaoOuFalharAsync(id, cancellationToken);
        programacao.GarantirPodeSerRemovida();

        var dto = (await MontarDtosAsync([programacao], cancellationToken)).Single();

        await _programacoes.RemoverAsync(programacao, cancellationToken);
        return dto;
    }

    private async Task ConferirRegrasAsync(
        RoteiroEtapa etapa,
        Recurso recurso,
        DateTime inicio,
        DateTime fim,
        IReadOnlyList<RoteiroEtapa> roteiro,
        IReadOnlyList<Programacao> demaisDaOrdem,
        int? idAtual,
        CancellationToken cancellationToken)
    {
        RegrasDeProgramacao.ConferirSequencia(etapa, inicio, fim, roteiro, demaisDaOrdem);

        var demaisDoRecurso = (await _programacoes.ListarPorRecursoAsync(recurso.Id, cancellationToken))
            .Where(p => p.Id != idAtual)
            .ToList();

        RegrasDeProgramacao.ConferirSemSobreposicao(recurso, inicio, fim, demaisDoRecurso);
        RegrasDeProgramacao.ConferirCapacidade(recurso, inicio, fim, demaisDoRecurso);
    }

    private async Task<IReadOnlyList<ProgramacaoDto>> MontarDtosAsync(IReadOnlyList<Programacao> programacoes, CancellationToken cancellationToken)
    {
        var dtos = new List<ProgramacaoDto>();
        var ordens = new Dictionary<int, OrdemProducao>();
        var etapas = new Dictionary<int, RoteiroEtapa>();
        var recursos = (await _recursos.ListarAsync(cancellationToken)).ToDictionary(r => r.Id);

        foreach (var programacao in programacoes)
        {
            if (!ordens.TryGetValue(programacao.OrdemId, out var ordem))
            {
                ordem = await ObterOrdemOuFalharAsync(programacao.OrdemId, cancellationToken);
                ordens[ordem.Id] = ordem;
                foreach (var e in await _etapas.ListarPorProdutoAsync(ordem.ProdutoId, cancellationToken))
                    etapas[e.Id] = e;
            }

            dtos.Add(ProgramacaoDto.De(programacao, ordem, etapas[programacao.EtapaId], recursos[programacao.RecursoId]));
        }

        return dtos;
    }

    private static void GarantirOrdemProgramavel(OrdemProducao ordem)
    {
        if (ordem.Status is not (StatusOrdem.Planejada or StatusOrdem.Programada))
            throw new DomainException($"A ordem {ordem.Numero} está {ordem.Status} e não pode ser programada.");
    }

    private async Task<IReadOnlyList<RoteiroEtapa>> ObterRoteiroOuFalharAsync(OrdemProducao ordem, CancellationToken cancellationToken)
    {
        var roteiro = await _etapas.ListarPorProdutoAsync(ordem.ProdutoId, cancellationToken);
        if (roteiro.Count == 0)
        {
            var produto = await _produtos.ObterPorIdAsync(ordem.ProdutoId, cancellationToken);
            throw new DomainException($"O produto {produto?.Codigo} não tem roteiro cadastrado.");
        }

        return roteiro;
    }

    private async Task<OrdemProducao> ObterOrdemOuFalharAsync(int id, CancellationToken cancellationToken) =>
        await _ordens.ObterPorIdAsync(id, cancellationToken)
            ?? throw new NotFoundException($"Ordem de produção {id} não encontrada.");

    private async Task<Programacao> ObterProgramacaoOuFalharAsync(int id, CancellationToken cancellationToken) =>
        await _programacoes.ObterPorIdAsync(id, cancellationToken)
            ?? throw new NotFoundException($"Programação {id} não encontrada.");

    private async Task<Recurso> ObterRecursoAtivoOuFalharAsync(int id, CancellationToken cancellationToken)
    {
        var recurso = await _recursos.ObterPorIdAsync(id, cancellationToken)
            ?? throw new NotFoundException($"Recurso {id} não encontrado.");

        if (!recurso.Ativo)
            throw new DomainException($"O recurso {recurso.Nome} está inativo.");

        return recurso;
    }
}
