using FabricX.Application.Interfaces;
using FabricX.Domain.Entities;

namespace FabricX.Tests.Fakes;

public class RecursoRepositoryFake : IRecursoRepository
{
    private readonly List<Recurso> _recursos = [];
    private int _proximoId = 1;

    public Task<Recurso?> ObterPorIdAsync(int id, CancellationToken cancellationToken) =>
        Task.FromResult(_recursos.FirstOrDefault(r => r.Id == id));

    public Task<Recurso?> ObterPorNomeAsync(string nome, CancellationToken cancellationToken) =>
        Task.FromResult(_recursos.FirstOrDefault(r => r.Nome == nome));

    public Task<IReadOnlyList<Recurso>> ListarAsync(CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<Recurso>>(_recursos.ToList());

    public Task AdicionarAsync(Recurso recurso, CancellationToken cancellationToken)
    {
        typeof(Recurso).GetProperty(nameof(Recurso.Id))!.SetValue(recurso, _proximoId++);
        _recursos.Add(recurso);
        return Task.CompletedTask;
    }

    public Task AtualizarAsync(Recurso recurso, CancellationToken cancellationToken) => Task.CompletedTask;
}
