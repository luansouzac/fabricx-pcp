using FabricX.Domain.Entities;
using FabricX.Domain.Enums;
using FabricX.Domain.Exceptions;

namespace FabricX.Tests.Domain;

public class RecursoTests
{
    [Fact]
    public void Construtor_DadosValidos_CriaRecursoAtivo()
    {
        var recurso = new Recurso("Torno CNC", TipoRecurso.Maquina, 8);

        Assert.Equal("Torno CNC", recurso.Nome);
        Assert.Equal(TipoRecurso.Maquina, recurso.Tipo);
        Assert.Equal(8, recurso.CapacidadeHorasDia);
        Assert.True(recurso.Ativo);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void Construtor_NomeVazio_LancaDomainException(string nome)
    {
        Assert.Throws<DomainException>(() => new Recurso(nome, TipoRecurso.Maquina, 8));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(double.NaN)]
    public void Construtor_CapacidadeNaoPositiva_LancaDomainException(double capacidade)
    {
        Assert.Throws<DomainException>(() => new Recurso("Torno", TipoRecurso.Maquina, capacidade));
    }

    [Fact]
    public void Construtor_TipoInvalido_LancaDomainException()
    {
        Assert.Throws<DomainException>(() => new Recurso("Torno", (TipoRecurso)99, 8));
    }

    [Fact]
    public void Atualizar_CapacidadeInvalida_LancaDomainExceptionEMantemValor()
    {
        var recurso = new Recurso("Torno", TipoRecurso.Maquina, 8);

        Assert.Throws<DomainException>(() => recurso.Atualizar("Torno", TipoRecurso.Maquina, 0));
        Assert.Equal(8, recurso.CapacidadeHorasDia);
    }

    [Fact]
    public void Atualizar_DadosValidos_AlteraCampos()
    {
        var recurso = new Recurso("Torno", TipoRecurso.Maquina, 8);

        recurso.Atualizar("Linha 1", TipoRecurso.Linha, 16);

        Assert.Equal("Linha 1", recurso.Nome);
        Assert.Equal(TipoRecurso.Linha, recurso.Tipo);
        Assert.Equal(16, recurso.CapacidadeHorasDia);
    }

    [Fact]
    public void InativarEAtivar_AlternamOEstado()
    {
        var recurso = new Recurso("Torno", TipoRecurso.Maquina, 8);

        recurso.Inativar();
        Assert.False(recurso.Ativo);

        recurso.Ativar();
        Assert.True(recurso.Ativo);
    }
}
