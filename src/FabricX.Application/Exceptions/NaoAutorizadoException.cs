namespace FabricX.Application.Exceptions;

public class NaoAutorizadoException : Exception
{
    public NaoAutorizadoException(string message) : base(message)
    {
    }
}
