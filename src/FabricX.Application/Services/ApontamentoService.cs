using FabricX.Application.DTOs.Apontamentos;
using FabricX.Application.Exceptions;
using FabricX.Application.Interfaces;
using FabricX.Domain.Entities;
using FabricX.Domain.Enums;

namespace FabricX.Application.Services;

public class ApontamentoService : IApontamentoService
{
    private readonly IApontamentoRepository _apontamentos;
    private readonly IProgramacaoRepository _programacoes;
    private readonly IOrdemProducaoRepository _ordens;

    public ApontamentoService(IApontamentoRepository apontamentos, IProgramacaoRepository programacoes, IOrdemProducaoRepository ordens)
    {
        _apontamentos = apontamentos;
        _programacoes = programacoes;
        _ordens = ordens;
    }

    public async Task<ApontamentoDto> RegistrarAsync(int programacaoId, RegistrarApontamentoDto dto, CancellationToken cancellationToken)
    {
        var programacao = await ObterProgramacaoOuFalharAsync(programacaoId, cancellationToken);
        var ordem = await _ordens.ObterPorIdAsync(programacao.OrdemId, cancellationToken)
            ?? throw new NotFoundException($"Ordem de produção {programacao.OrdemId} não encontrada.");

        var apontamento = new Apontamento(programacao.Id, dto.QuantidadeProduzida, dto.QuantidadeDefeituosa, dto.TempoGastoMin, dto.Observacao);

        if (programacao.Status == StatusProgramacao.Agendada)
            programacao.Iniciar();
        else if (programacao.Status == StatusProgramacao.Concluida)
            programacao.GarantirPodeReceberApontamento();

        var jaProduzido = (await _apontamentos.ListarPorProgramacaoAsync(programacao.Id, cancellationToken)).Sum(a => a.QtdProduzida);
        if (jaProduzido + apontamento.QtdProduzida >= ordem.Quantidade)
            programacao.Concluir();

        await _apontamentos.AdicionarAsync(apontamento, cancellationToken);
        return ApontamentoDto.De(apontamento, programacao);
    }

    public async Task<IReadOnlyList<ApontamentoDto>> ListarPorProgramacaoAsync(int programacaoId, CancellationToken cancellationToken)
    {
        var programacao = await ObterProgramacaoOuFalharAsync(programacaoId, cancellationToken);
        var apontamentos = await _apontamentos.ListarPorProgramacaoAsync(programacao.Id, cancellationToken);
        return apontamentos.Select(a => ApontamentoDto.De(a, programacao)).ToList();
    }

    public async Task<IReadOnlyList<ApontamentoDto>> ListarPorOrdemAsync(int ordemId, CancellationToken cancellationToken)
    {
        _ = await _ordens.ObterPorIdAsync(ordemId, cancellationToken)
            ?? throw new NotFoundException($"Ordem de produção {ordemId} não encontrada.");

        var programacoes = (await _programacoes.ListarPorOrdemAsync(ordemId, cancellationToken)).ToDictionary(p => p.Id);
        var apontamentos = await _apontamentos.ListarPorOrdemAsync(ordemId, cancellationToken);

        return apontamentos.Select(a => ApontamentoDto.De(a, programacoes[a.ProgramacaoId])).ToList();
    }

    private async Task<Programacao> ObterProgramacaoOuFalharAsync(int id, CancellationToken cancellationToken) =>
        await _programacoes.ObterPorIdAsync(id, cancellationToken)
            ?? throw new NotFoundException($"Programação {id} não encontrada.");
}
