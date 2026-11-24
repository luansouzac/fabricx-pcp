using FabricX.Domain.Entities;

namespace FabricX.Application.Interfaces;

public record TokenGerado(string Token, DateTime ExpiraEmUtc);

public interface IGeradorDeToken
{
    TokenGerado Gerar(Usuario usuario);
}
