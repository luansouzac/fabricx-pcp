namespace FabricX.Application.Interfaces;

public interface ICacheDeConsultas
{
    Task<T> ObterOuBuscarAsync<T>(string chave, Func<Task<T>> buscar);
    void InvalidarTudo();
}
