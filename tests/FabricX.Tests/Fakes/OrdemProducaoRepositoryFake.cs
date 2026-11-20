using FabricX.Application.Interfaces;
using FabricX.Domain.Entities;
using FabricX.Domain.Enums;

namespace FabricX.Tests.Fakes;

public class OrdemProducaoRepositoryFake : IOrdemProducaoRepository
{
    private readonly List<OrdemProducao> _ordens = [];
    private int _proximoId = 1;

    public IReadOnlyList<OrdemProducao> Ordens => _ordens;

    public Task<OrdemProducao?> ObterPorIdAsync(int id, CancellationToken cancellationToken) =>
        Task.FromResult(_ordens.FirstOrDefault(o => o.Id == id));

    public Task<IReadOnlyList<OrdemProducao>> ListarAsync(StatusOrdem? status, CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<OrdemProducao>>(_ordens.Where(o => status is null || o.Status == status).ToList());

    public Task<int> ObterMaiorSequencialDoAnoAsync(int ano, CancellationToken cancellationToken)
    {
        var prefixo = $"OP-{ano}-";
        var maior = _ordens
            .Where(o => o.Numero.StartsWith(prefixo))
            .Select(o => int.Parse(o.Numero[prefixo.Length..]))
            .DefaultIfEmpty(0)
            .Max();
        return Task.FromResult(maior);
    }

    public Task AdicionarAsync(OrdemProducao ordem, CancellationToken cancellationToken)
    {
        typeof(OrdemProducao).GetProperty(nameof(OrdemProducao.Id))!.SetValue(ordem, _proximoId++);
        _ordens.Add(ordem);
        return Task.CompletedTask;
    }

    public Task AtualizarAsync(OrdemProducao ordem, CancellationToken cancellationToken) => Task.CompletedTask;
}
