using FabricX.Application.Interfaces;

namespace FabricX.Tests.Fakes;

public class SemCacheFake : ICacheDeConsultas
{
    public Task<T> ObterOuBuscarAsync<T>(string chave, Func<Task<T>> buscar) => buscar();

    public void InvalidarTudo()
    {
    }
}
