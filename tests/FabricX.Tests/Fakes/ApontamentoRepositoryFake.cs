using FabricX.Application.Interfaces;
using FabricX.Domain.Entities;

namespace FabricX.Tests.Fakes;

public class ApontamentoRepositoryFake : IApontamentoRepository
{
    private readonly List<Apontamento> _apontamentos = [];
    private readonly ProgramacaoRepositoryFake _programacoes;
    private int _proximoId = 1;

    public ApontamentoRepositoryFake(ProgramacaoRepositoryFake programacoes)
    {
        _programacoes = programacoes;
    }

    public IReadOnlyList<Apontamento> Apontamentos => _apontamentos;

    public Task<IReadOnlyList<Apontamento>> ListarPorProgramacaoAsync(int programacaoId, CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<Apontamento>>(_apontamentos.Where(a => a.ProgramacaoId == programacaoId).ToList());

    public Task<IReadOnlyList<Apontamento>> ListarPorOrdemAsync(int ordemId, CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<Apontamento>>(_apontamentos
            .Where(a => _programacoes.Programacoes.Any(p => p.Id == a.ProgramacaoId && p.OrdemId == ordemId))
            .ToList());

    public Task<IReadOnlyList<Apontamento>> ListarAsync(DateTime? de, DateTime? ate, CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<Apontamento>>(_apontamentos
            .Where(a => de is null || a.DataHora >= de)
            .Where(a => ate is null || a.DataHora < ate)
            .ToList());

    public Task AdicionarAsync(Apontamento apontamento, CancellationToken cancellationToken)
    {
        typeof(Apontamento).GetProperty(nameof(Apontamento.Id))!.SetValue(apontamento, _proximoId++);
        _apontamentos.Add(apontamento);
        return Task.CompletedTask;
    }
}
