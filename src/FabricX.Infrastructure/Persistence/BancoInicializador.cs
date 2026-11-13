using Microsoft.EntityFrameworkCore;

namespace FabricX.Infrastructure.Persistence;

public static class BancoInicializador
{
    public static void AplicarScriptSeVazio(AppDbContext contexto, string caminhoScript)
    {
        var conexao = contexto.Database.GetDbConnection();
        conexao.Open();

        try
        {
            using var consulta = conexao.CreateCommand();
            consulta.CommandText = "SELECT COUNT(*) FROM sqlite_master WHERE type = 'table' AND name = 'produto'";

            if (Convert.ToInt32(consulta.ExecuteScalar()) > 0)
                return;

            using var criacao = conexao.CreateCommand();
            criacao.CommandText = File.ReadAllText(caminhoScript);
            criacao.ExecuteNonQuery();
        }
        finally
        {
            conexao.Close();
        }
    }
}
