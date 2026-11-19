using FabricX.Application.DTOs.Produtos;
using FabricX.Domain.Enums;

namespace FabricX.Application.Interfaces;

public interface IProdutoService
{
    Task<IReadOnlyList<ProdutoDto>> ListarAsync(TipoProduto? tipo, CancellationToken cancellationToken);
    Task<IReadOnlyList<ProdutoDto>> ListarComEstoqueBaixoAsync(CancellationToken cancellationToken);
    Task<ProdutoDto> ObterPorIdAsync(int id, CancellationToken cancellationToken);
    Task<ProdutoDto> CriarAsync(CriarProdutoDto dto, CancellationToken cancellationToken);
    Task<ProdutoDto> AtualizarAsync(int id, AtualizarProdutoDto dto, CancellationToken cancellationToken);
    Task<ProdutoDto> MovimentarEstoqueAsync(int id, MovimentarEstoqueDto dto, CancellationToken cancellationToken);
    Task<IReadOnlyList<MovimentacaoEstoqueDto>> ListarMovimentacoesAsync(int id, CancellationToken cancellationToken);
    Task<ProdutoDto> InativarAsync(int id, CancellationToken cancellationToken);
}
