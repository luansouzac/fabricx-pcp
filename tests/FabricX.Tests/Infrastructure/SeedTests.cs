using FabricX.Tests.Fakes;
using Microsoft.EntityFrameworkCore;

namespace FabricX.Tests.Infrastructure;

public class SeedTests : IDisposable
{
    private static readonly CancellationToken Ct = CancellationToken.None;

    private readonly BancoSqliteFixture _banco = new();

    public void Dispose() => _banco.Dispose();

    private static string LerSeed()
    {
        var diretorio = new DirectoryInfo(AppContext.BaseDirectory);

        while (diretorio is not null)
        {
            var caminho = Path.Combine(diretorio.FullName, "database", "seed.sql");
            if (File.Exists(caminho))
                return File.ReadAllText(caminho);

            diretorio = diretorio.Parent;
        }

        throw new FileNotFoundException("database/seed.sql não encontrado.");
    }

    [Fact]
    public async Task Seed_CarregaNoEsquemaOficialComOExemploDaGarrafa()
    {
        await using var contexto = _banco.CriarContexto();

        await contexto.Database.ExecuteSqlRawAsync(LerSeed(), Ct);

        Assert.Equal(11, await contexto.Produtos.CountAsync(Ct));
        Assert.Equal(6, await contexto.Recursos.CountAsync(Ct));
        Assert.Equal(2, await contexto.Database.SqlQuery<int>($"SELECT COUNT(*) AS Value FROM roteiro_etapa").SingleAsync(Ct));
    }

    [Fact]
    public async Task Seed_Ordem60GarrafasDaguaTemSaldoEOrdem10KitsNao()
    {
        await using var contexto = _banco.CriarContexto();
        await contexto.Database.ExecuteSqlRawAsync(LerSeed(), Ct);

        var saldoComponentes = await contexto.Database.SqlQuery<double>(
            $"SELECT p.estoque_atual AS Value FROM item_estrutura i JOIN produto p ON p.id = i.componente_id WHERE i.produto_pai_id = (SELECT id FROM produto WHERE codigo = 'EP-003')").ToListAsync(Ct);
        var saldoGarrafaDagua = await contexto.Database.SqlQuery<double>(
            $"SELECT estoque_atual AS Value FROM produto WHERE codigo = 'EP-003'").SingleAsync(Ct);

        Assert.All(saldoComponentes, saldo => Assert.True(saldo >= 60));
        Assert.True(saldoGarrafaDagua < 60);
    }
}
