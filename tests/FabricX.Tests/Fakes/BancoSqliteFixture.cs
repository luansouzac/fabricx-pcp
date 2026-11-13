using FabricX.Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace FabricX.Tests.Fakes;

public sealed class BancoSqliteFixture : IDisposable
{
    private readonly SqliteConnection _conexao;

    public BancoSqliteFixture()
    {
        _conexao = new SqliteConnection("Data Source=:memory:");
        _conexao.Open();

        using var comando = _conexao.CreateCommand();
        comando.CommandText = File.ReadAllText(LocalizarScript());
        comando.ExecuteNonQuery();
    }

    public AppDbContext CriarContexto()
    {
        var opcoes = new DbContextOptionsBuilder<AppDbContext>().UseSqlite(_conexao).Options;
        return new AppDbContext(opcoes);
    }

    public void Dispose() => _conexao.Dispose();

    private static string LocalizarScript()
    {
        var diretorio = new DirectoryInfo(AppContext.BaseDirectory);

        while (diretorio is not null)
        {
            var caminho = Path.Combine(diretorio.FullName, "database", "script.sql");
            if (File.Exists(caminho))
                return caminho;

            diretorio = diretorio.Parent;
        }

        throw new FileNotFoundException("database/script.sql não encontrado.");
    }
}
