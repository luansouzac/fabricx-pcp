using FabricX.Infrastructure.Seguranca;

namespace FabricX.Tests.Infrastructure;

public class SenhaHasherTests
{
    private readonly SenhaHasherPbkdf2 _hasher = new();

    [Fact]
    public void Gerar_NaoGuardaASenhaEmTextoPuro()
    {
        var hash = _hasher.Gerar("Senha@123");

        Assert.DoesNotContain("Senha@123", hash);
        Assert.StartsWith("PBKDF2-SHA256$", hash);
    }

    [Fact]
    public void Gerar_MesmaSenhaDuasVezes_ProduzHashesDiferentesPorCausaDoSalt()
    {
        Assert.NotEqual(_hasher.Gerar("Senha@123"), _hasher.Gerar("Senha@123"));
    }

    [Fact]
    public void Verificar_SenhaCorreta_Aceita()
    {
        Assert.True(_hasher.Verificar("Senha@123", _hasher.Gerar("Senha@123")));
    }

    [Fact]
    public void Verificar_SenhaErrada_Recusa()
    {
        Assert.False(_hasher.Verificar("outra", _hasher.Gerar("Senha@123")));
    }

    [Theory]
    [InlineData("")]
    [InlineData("texto-puro")]
    [InlineData("PBKDF2-SHA256$abc$AAAA$AAAA")]
    [InlineData("PBKDF2-SHA256$1000$%%%$%%%")]
    [InlineData("OUTRO$1000$AAAA$AAAA")]
    public void Verificar_HashMalFormado_RecusaSemLancar(string hash)
    {
        Assert.False(_hasher.Verificar("Senha@123", hash));
    }
}
