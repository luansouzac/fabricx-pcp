namespace FabricX.Application.Interfaces;

public interface ISenhaHasher
{
    string Gerar(string senha);
    bool Verificar(string senha, string hash);
}
