using Microsoft.EntityFrameworkCore;

namespace FabricX.Infrastructure.Persistence;

public static class BancoInicializador
{
    public static void AplicarScriptSeVazio(AppDbContext contexto, string caminhoScript, string? caminhoSeed = null)
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

            if (caminhoSeed is not null && File.Exists(caminhoSeed))
            {
                using var carga = conexao.CreateCommand();
                carga.CommandText = File.ReadAllText(caminhoSeed);
                carga.ExecuteNonQuery();
            }
        }
        finally
        {
            conexao.Close();
        }
    }
}
