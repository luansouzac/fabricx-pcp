using FabricX.Application.Interfaces;
using FabricX.Domain.Entities;
using FabricX.Domain.Enums;

namespace FabricX.Tests.Fakes;

public class ProdutoRepositoryFake : IProdutoRepository
{
    private readonly List<Produto> _produtos = [];
    private int _proximoId = 1;

    public int TotalAtualizacoes { get; private set; }

    public Task<Produto?> ObterPorIdAsync(int id, CancellationToken cancellationToken) =>
        Task.FromResult(_produtos.FirstOrDefault(p => p.Id == id));

    public Task<Produto?> ObterPorCodigoAsync(string codigo, CancellationToken cancellationToken) =>
        Task.FromResult(_produtos.FirstOrDefault(p => p.Codigo == codigo));

    public Task<IReadOnlyList<Produto>> ListarAsync(TipoProduto? tipo, CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<Produto>>(_produtos.Where(p => tipo is null || p.Tipo == tipo).ToList());

    public Task AdicionarAsync(Produto produto, CancellationToken cancellationToken)
    {
        typeof(Produto).GetProperty(nameof(Produto.Id))!.SetValue(produto, _proximoId++);
        _produtos.Add(produto);
        return Task.CompletedTask;
    }

    public Task AtualizarAsync(Produto produto, CancellationToken cancellationToken)
    {
        TotalAtualizacoes++;
        return Task.CompletedTask;
    }
}
