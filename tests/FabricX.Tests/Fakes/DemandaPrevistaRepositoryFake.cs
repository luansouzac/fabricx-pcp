using FabricX.Application.Interfaces;
using FabricX.Domain.Entities;

namespace FabricX.Tests.Fakes;

public class DemandaPrevistaRepositoryFake : IDemandaPrevistaRepository
{
    private readonly List<DemandaPrevista> _demandas = [];
    private int _proximoId = 1;

    public IReadOnlyList<DemandaPrevista> Demandas => _demandas;

    public Task<DemandaPrevista?> ObterPorIdAsync(int id, CancellationToken cancellationToken) =>
        Task.FromResult(_demandas.FirstOrDefault(d => d.Id == id));

    public Task<DemandaPrevista?> ObterPorProdutoEPeriodoAsync(int produtoId, string periodo, CancellationToken cancellationToken) =>
        Task.FromResult(_demandas.FirstOrDefault(d => d.ProdutoId == produtoId && d.Periodo == periodo));

    public Task<IReadOnlyList<DemandaPrevista>> ListarAsync(string? periodo, CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<DemandaPrevista>>(_demandas.Where(d => periodo is null || d.Periodo == periodo).ToList());

    public Task AdicionarAsync(DemandaPrevista demanda, CancellationToken cancellationToken)
    {
        typeof(DemandaPrevista).GetProperty(nameof(DemandaPrevista.Id))!.SetValue(demanda, _proximoId++);
        _demandas.Add(demanda);
        return Task.CompletedTask;
    }

    public Task AtualizarAsync(DemandaPrevista demanda, CancellationToken cancellationToken) => Task.CompletedTask;

    public Task RemoverAsync(DemandaPrevista demanda, CancellationToken cancellationToken)
    {
        _demandas.Remove(demanda);
        return Task.CompletedTask;
    }
}
