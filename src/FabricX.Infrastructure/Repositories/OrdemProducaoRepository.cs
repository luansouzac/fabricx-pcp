using FabricX.Application.Interfaces;
using FabricX.Domain.Entities;
using FabricX.Domain.Enums;
using FabricX.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FabricX.Infrastructure.Repositories;

public class OrdemProducaoRepository : IOrdemProducaoRepository
{
    private readonly AppDbContext _contexto;

    public OrdemProducaoRepository(AppDbContext contexto)
    {
        _contexto = contexto;
    }

    public Task<OrdemProducao?> ObterPorIdAsync(int id, CancellationToken cancellationToken) =>
        _contexto.OrdensProducao.FirstOrDefaultAsync(o => o.Id == id, cancellationToken);

    public async Task<IReadOnlyList<OrdemProducao>> ListarAsync(StatusOrdem? status, CancellationToken cancellationToken) =>
        await _contexto.OrdensProducao.AsNoTracking()
            .Where(o => status == null || o.Status == status)
            .OrderBy(o => o.PrazoEntrega)
            .ThenBy(o => o.Id)
            .ToListAsync(cancellationToken);

    public async Task<int> ObterMaiorSequencialDoAnoAsync(int ano, CancellationToken cancellationToken)
    {
        var prefixo = $"OP-{ano}-";
        var numeros = await _contexto.OrdensProducao.AsNoTracking()
            .Where(o => o.Numero.StartsWith(prefixo))
            .Select(o => o.Numero)
            .ToListAsync(cancellationToken);

        return numeros
            .Select(n => int.TryParse(n[prefixo.Length..], out var sequencial) ? sequencial : 0)
            .DefaultIfEmpty(0)
            .Max();
    }

    public async Task AdicionarAsync(OrdemProducao ordem, CancellationToken cancellationToken)
    {
        _contexto.OrdensProducao.Add(ordem);
        await _contexto.SaveChangesAsync(cancellationToken);
    }

    public Task AtualizarAsync(OrdemProducao ordem, CancellationToken cancellationToken) =>
        _contexto.SaveChangesAsync(cancellationToken);
}
