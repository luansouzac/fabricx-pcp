using FabricX.Application.Interfaces;
using FabricX.Domain.Entities;

namespace FabricX.Tests.Fakes;

public class RoteiroEtapaRepositoryFake : IRoteiroEtapaRepository
{
    private readonly List<RoteiroEtapa> _etapas = [];
    private int _proximoId = 1;

    public IReadOnlyList<RoteiroEtapa> Etapas => _etapas;

    public Task<RoteiroEtapa?> ObterPorIdAsync(int id, CancellationToken cancellationToken) =>
        Task.FromResult(_etapas.FirstOrDefault(e => e.Id == id));

    public Task<IReadOnlyList<RoteiroEtapa>> ListarPorProdutoAsync(int produtoId, CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<RoteiroEtapa>>(_etapas.Where(e => e.ProdutoId == produtoId).OrderBy(e => e.Sequencia).ToList());

    public Task AdicionarAsync(RoteiroEtapa etapa, CancellationToken cancellationToken)
    {
        typeof(RoteiroEtapa).GetProperty(nameof(RoteiroEtapa.Id))!.SetValue(etapa, _proximoId++);
        _etapas.Add(etapa);
        return Task.CompletedTask;
    }

    public Task AtualizarAsync(RoteiroEtapa etapa, CancellationToken cancellationToken) => Task.CompletedTask;

    public Task RemoverAsync(RoteiroEtapa etapa, CancellationToken cancellationToken)
    {
        _etapas.Remove(etapa);
        return Task.CompletedTask;
    }
}
