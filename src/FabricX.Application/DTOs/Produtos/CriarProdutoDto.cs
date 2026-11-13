using System.ComponentModel.DataAnnotations;
using FabricX.Domain.Enums;

namespace FabricX.Application.DTOs.Produtos;

public class CriarProdutoDto
{
    [Required, StringLength(50)]
    public string Codigo { get; set; } = string.Empty;

    [Required, StringLength(150)]
    public string Nome { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Descricao { get; set; }

    [Required, EnumDataType(typeof(TipoProduto))]
    public TipoProduto Tipo { get; set; }

    [Required, StringLength(10)]
    public string UnidadeMedida { get; set; } = "UN";

    [Range(0, double.MaxValue)]
    public double EstoqueAtual { get; set; }

    [Range(0, double.MaxValue)]
    public double EstoqueMinimo { get; set; }

    [Range(0, double.MaxValue)]
    public double CustoUnitario { get; set; }
}
