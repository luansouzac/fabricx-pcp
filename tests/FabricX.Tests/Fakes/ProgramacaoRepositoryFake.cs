using FabricX.Application.Interfaces;
using FabricX.Domain.Entities;

namespace FabricX.Tests.Fakes;

public class ProgramacaoRepositoryFake : IProgramacaoRepository
{
    private readonly List<Programacao> _programacoes = [];
    private int _proximoId = 1;

    public IReadOnlyList<Programacao> Programacoes => _programacoes;

    public Task<Programacao?> ObterPorIdAsync(int id, CancellationToken cancellationToken) =>
        Task.FromResult(_programacoes.FirstOrDefault(p => p.Id == id));

    public Task<IReadOnlyList<Programacao>> ListarPorOrdemAsync(int ordemId, CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<Programacao>>(_programacoes.Where(p => p.OrdemId == ordemId).OrderBy(p => p.InicioPrevisto).ToList());

    public Task<IReadOnlyList<Programacao>> ListarPorRecursoAsync(int recursoId, CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<Programacao>>(_programacoes.Where(p => p.RecursoId == recursoId).OrderBy(p => p.InicioPrevisto).ToList());

    public Task<IReadOnlyList<Programacao>> ListarAsync(int? recursoId, DateTime? de, DateTime? ate, CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<Programacao>>(_programacoes
            .Where(p => recursoId is null || p.RecursoId == recursoId)
            .Where(p => de is null || p.FimPrevisto > de)
            .Where(p => ate is null || p.InicioPrevisto < ate)
            .OrderBy(p => p.InicioPrevisto)
            .ToList());

    public Task AdicionarAsync(Programacao programacao, CancellationToken cancellationToken)
    {
        typeof(Programacao).GetProperty(nameof(Programacao.Id))!.SetValue(programacao, _proximoId++);
        _programacoes.Add(programacao);
        return Task.CompletedTask;
    }

    public Task AtualizarAsync(Programacao programacao, CancellationToken cancellationToken) => Task.CompletedTask;

    public Task RemoverAsync(Programacao programacao, CancellationToken cancellationToken)
    {
        _programacoes.Remove(programacao);
        return Task.CompletedTask;
    }
}
