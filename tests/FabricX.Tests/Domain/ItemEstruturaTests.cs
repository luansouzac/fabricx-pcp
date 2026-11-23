using FabricX.Domain.Entities;
using FabricX.Domain.Exceptions;
using FabricX.Domain.Services;

namespace FabricX.Tests.Domain;

public class ItemEstruturaTests
{
    [Fact]
    public void Construtor_DadosValidos_PreencheCampos()
    {
        var item = new ItemEstrutura(1, 2, 0.5);

        Assert.Equal(1, item.ProdutoPaiId);
        Assert.Equal(2, item.ComponenteId);
        Assert.Equal(0.5, item.Quantidade);
    }

    [Fact]
    public void Construtor_PaiIgualAoComponente_LancaDomainException()
    {
        Assert.Throws<DomainException>(() => new ItemEstrutura(1, 1, 1));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-2)]
    [InlineData(double.NaN)]
    public void Construtor_QuantidadeInvalida_LancaDomainException(double quantidade)
    {
        Assert.Throws<DomainException>(() => new ItemEstrutura(1, 2, quantidade));
    }

    [Fact]
    public void AlterarQuantidade_Invalida_LancaDomainExceptionEMantemValor()
    {
        var item = new ItemEstrutura(1, 2, 3);

        Assert.Throws<DomainException>(() => item.AlterarQuantidade(0));
        Assert.Equal(3, item.Quantidade);
    }
}

public class DetectorCicloEstruturaTests
{
    private static ItemEstrutura Liga(int pai, int componente) => new(pai, componente, 1);

    [Fact]
    public void SemRelacaoEntreOsProdutos_NaoHaCiclo()
    {
        var existentes = new[] { Liga(1, 2), Liga(3, 4) };

        Assert.Null(DetectorCicloEstrutura.CaminhoDoCiclo(2, 3, existentes));
    }

    [Fact]
    public void LigacaoDiretaInversa_FormaCiclo()
    {
        var existentes = new[] { Liga(1, 2) };

        var caminho = DetectorCicloEstrutura.CaminhoDoCiclo(2, 1, existentes);

        Assert.Equal([2, 1, 2], caminho);
    }

    [Fact]
    public void CicloEmTresNiveis_DevolveOCaminhoCompleto()
    {
        const int tampinha = 1, garrafa = 2, kit = 3, garrafaDagua = 4;
        var existentes = new[] { Liga(garrafaDagua, tampinha), Liga(garrafaDagua, garrafa), Liga(kit, garrafaDagua) };

        var caminho = DetectorCicloEstrutura.CaminhoDoCiclo(tampinha, kit, existentes);

        Assert.Equal([tampinha, kit, garrafaDagua, tampinha], caminho);
    }

    [Fact]
    public void ComponenteCompartilhado_EmLosango_NaoEhCiclo()
    {
        var existentes = new[] { Liga(1, 2), Liga(1, 3), Liga(2, 4), Liga(3, 4) };

        Assert.Null(DetectorCicloEstrutura.CaminhoDoCiclo(1, 4, existentes));
    }
}
