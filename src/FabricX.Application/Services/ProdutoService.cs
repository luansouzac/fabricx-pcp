using FabricX.Application.DTOs.Produtos;
using FabricX.Application.Exceptions;
using FabricX.Application.Interfaces;
using FabricX.Domain.Entities;
using FabricX.Domain.Exceptions;

namespace FabricX.Application.Services;

public class ProdutoService : IProdutoService
{
    private readonly IProdutoRepository _repositorio;

    public ProdutoService(IProdutoRepository repositorio)
    {
        _repositorio = repositorio;
    }

    public async Task<IReadOnlyList<ProdutoDto>> ListarAsync(CancellationToken cancellationToken)
    {
        var produtos = await _repositorio.ListarAsync(cancellationToken);
        return produtos.Select(ProdutoDto.De).ToList();
    }

    public async Task<IReadOnlyList<ProdutoDto>> ListarComEstoqueBaixoAsync(CancellationToken cancellationToken)
    {
        var produtos = await _repositorio.ListarAsync(cancellationToken);
        return produtos
            .Where(p => p.Ativo && p.EstoqueAbaixoDoMinimo)
            .Select(ProdutoDto.De)
            .ToList();
    }

    public async Task<ProdutoDto> ObterPorIdAsync(int id, CancellationToken cancellationToken)
    {
        var produto = await ObterOuFalharAsync(id, cancellationToken);
        return ProdutoDto.De(produto);
    }

    public async Task<ProdutoDto> CriarAsync(CriarProdutoDto dto, CancellationToken cancellationToken)
    {
        var codigo = dto.Codigo?.Trim() ?? string.Empty;

        if (await _repositorio.ObterPorCodigoAsync(codigo, cancellationToken) is not null)
            throw new DomainException($"Já existe um produto com o código {codigo}.");

        var produto = new Produto(
            dto.Codigo!,
            dto.Nome,
            dto.Tipo,
            dto.UnidadeMedida,
            dto.EstoqueAtual,
            dto.EstoqueMinimo,
            dto.CustoUnitario,
            dto.Descricao);

        await _repositorio.AdicionarAsync(produto, cancellationToken);
        return ProdutoDto.De(produto);
    }

    public async Task<ProdutoDto> AtualizarAsync(int id, AtualizarProdutoDto dto, CancellationToken cancellationToken)
    {
        var produto = await ObterOuFalharAsync(id, cancellationToken);

        produto.Atualizar(dto.Nome, dto.Descricao, dto.UnidadeMedida, dto.EstoqueMinimo, dto.CustoUnitario);

        await _repositorio.AtualizarAsync(produto, cancellationToken);
        return ProdutoDto.De(produto);
    }

    public async Task<ProdutoDto> MovimentarEstoqueAsync(int id, MovimentarEstoqueDto dto, CancellationToken cancellationToken)
    {
        var produto = await ObterOuFalharAsync(id, cancellationToken);

        produto.Movimentar(dto.Tipo, dto.Quantidade);

        await _repositorio.AtualizarAsync(produto, cancellationToken);
        return ProdutoDto.De(produto);
    }

    public async Task<ProdutoDto> InativarAsync(int id, CancellationToken cancellationToken)
    {
        var produto = await ObterOuFalharAsync(id, cancellationToken);

        produto.Inativar();

        await _repositorio.AtualizarAsync(produto, cancellationToken);
        return ProdutoDto.De(produto);
    }

    private async Task<Produto> ObterOuFalharAsync(int id, CancellationToken cancellationToken)
    {
        return await _repositorio.ObterPorIdAsync(id, cancellationToken)
            ?? throw new NotFoundException($"Produto {id} não encontrado.");
    }
}
