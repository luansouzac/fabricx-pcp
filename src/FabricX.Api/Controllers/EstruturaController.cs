using FabricX.Api.Common;
using FabricX.Application.DTOs.Estrutura;
using FabricX.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FabricX.Api.Controllers;

[ApiController]
[Route("api/produtos/{produtoId:int}/estrutura")]
public class EstruturaController : ControllerBase
{
    private readonly IItemEstruturaService _service;

    public EstruturaController(IItemEstruturaService service)
    {
        _service = service;
    }

    [HttpGet]
    [ProducesResponseType(typeof(WebResponse<IReadOnlyList<ItemEstruturaDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(WebResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Listar(int produtoId, CancellationToken cancellationToken)
    {
        var itens = await _service.ListarAsync(produtoId, cancellationToken);
        return Ok(WebResponse<IReadOnlyList<ItemEstruturaDto>>.Success(itens));
    }

    [HttpPost]
    [ProducesResponseType(typeof(WebResponse<ItemEstruturaDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(WebResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(WebResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Adicionar(int produtoId, [FromBody] AdicionarItemEstruturaDto dto, CancellationToken cancellationToken)
    {
        var item = await _service.AdicionarAsync(produtoId, dto, cancellationToken);
        var resposta = WebResponse<ItemEstruturaDto>.Success(item, StatusCodes.Status201Created);
        return Created($"/api/produtos/{produtoId}/estrutura", resposta);
    }

    [HttpPut("{componenteId:int}")]
    [ProducesResponseType(typeof(WebResponse<ItemEstruturaDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(WebResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(WebResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AlterarQuantidade(int produtoId, int componenteId, [FromBody] AlterarQuantidadeItemEstruturaDto dto, CancellationToken cancellationToken)
    {
        var item = await _service.AlterarQuantidadeAsync(produtoId, componenteId, dto, cancellationToken);
        return Ok(WebResponse<ItemEstruturaDto>.Success(item));
    }

    [HttpDelete("{componenteId:int}")]
    [ProducesResponseType(typeof(WebResponse<ItemEstruturaDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(WebResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Remover(int produtoId, int componenteId, CancellationToken cancellationToken)
    {
        var item = await _service.RemoverAsync(produtoId, componenteId, cancellationToken);
        return Ok(WebResponse<ItemEstruturaDto>.Success(item));
    }
}
