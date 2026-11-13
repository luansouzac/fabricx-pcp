using FabricX.Domain.Enums;
using FabricX.Domain.Exceptions;

namespace FabricX.Domain.Entities;

public class Produto
{
    public int Id { get; private set; }
    public string Codigo { get; private set; }
    public string Nome { get; private set; }
    public string? Descricao { get; private set; }
    public TipoProduto Tipo { get; private set; }
    public string UnidadeMedida { get; private set; }
    public double EstoqueAtual { get; private set; }
    public double EstoqueMinimo { get; private set; }
    public double CustoUnitario { get; private set; }
    public bool Ativo { get; private set; }

    public bool EstoqueAbaixoDoMinimo => EstoqueAtual < EstoqueMinimo;

    private Produto()
    {
        Codigo = null!;
        Nome = null!;
        UnidadeMedida = null!;
    }

    public Produto(
        string codigo,
        string nome,
        TipoProduto tipo,
        string unidadeMedida = "UN",
        double estoqueAtual = 0,
        double estoqueMinimo = 0,
        double custoUnitario = 0,
        string? descricao = null)
    {
        Codigo = ValidarTextoObrigatorio(codigo, "O código do produto é obrigatório.");
        Nome = ValidarTextoObrigatorio(nome, "O nome do produto é obrigatório.");
        UnidadeMedida = ValidarTextoObrigatorio(unidadeMedida, "A unidade de medida é obrigatória.");
        Tipo = ValidarTipo(tipo);
        EstoqueAtual = ValidarNaoNegativo(estoqueAtual, "O estoque atual não pode ser negativo.");
        EstoqueMinimo = ValidarNaoNegativo(estoqueMinimo, "O estoque mínimo não pode ser negativo.");
        CustoUnitario = ValidarNaoNegativo(custoUnitario, "O custo unitário não pode ser negativo.");
        Descricao = descricao;
        Ativo = true;
    }

    public void Atualizar(string nome, string? descricao, string unidadeMedida, double estoqueMinimo, double custoUnitario)
    {
        Nome = ValidarTextoObrigatorio(nome, "O nome do produto é obrigatório.");
        UnidadeMedida = ValidarTextoObrigatorio(unidadeMedida, "A unidade de medida é obrigatória.");
        EstoqueMinimo = ValidarNaoNegativo(estoqueMinimo, "O estoque mínimo não pode ser negativo.");
        CustoUnitario = ValidarNaoNegativo(custoUnitario, "O custo unitário não pode ser negativo.");
        Descricao = descricao;
    }

    public void Movimentar(TipoMovimentacao tipo, double quantidade)
    {
        switch (tipo)
        {
            case TipoMovimentacao.Entrada:
                RegistrarEntrada(quantidade);
                break;
            case TipoMovimentacao.Saida:
                RegistrarSaida(quantidade);
                break;
            default:
                throw new DomainException("Tipo de movimentação inválido.");
        }
    }

    public void RegistrarEntrada(double quantidade)
    {
        ValidarQuantidadePositiva(quantidade);
        EstoqueAtual += quantidade;
    }

    public void RegistrarSaida(double quantidade)
    {
        ValidarQuantidadePositiva(quantidade);

        if (quantidade > EstoqueAtual)
            throw new DomainException($"Estoque insuficiente de {Codigo}: disponível {EstoqueAtual}, solicitado {quantidade}.");

        EstoqueAtual -= quantidade;
    }

    public void Ativar() => Ativo = true;

    public void Inativar() => Ativo = false;

    private static void ValidarQuantidadePositiva(double quantidade)
    {
        if (!(quantidade > 0))
            throw new DomainException("A quantidade deve ser maior que zero.");
    }

    private static double ValidarNaoNegativo(double valor, string mensagem)
    {
        if (!(valor >= 0))
            throw new DomainException(mensagem);

        return valor;
    }

    private static string ValidarTextoObrigatorio(string? valor, string mensagem)
    {
        if (string.IsNullOrWhiteSpace(valor))
            throw new DomainException(mensagem);

        return valor.Trim();
    }

    private static TipoProduto ValidarTipo(TipoProduto tipo)
    {
        if (!Enum.IsDefined(tipo))
            throw new DomainException("Tipo de produto inválido.");

        return tipo;
    }
}
