using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

namespace FabricX.Api.Common;

public static class Limitador
{
    public const string Login = "login";

    public static void Configurar(RateLimiterOptions opcoes, int tentativasPorMinuto)
    {
        opcoes.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
        opcoes.OnRejected = (contexto, cancellationToken) =>
        {
            var resposta = WebResponse<object>.Failure("Muitas tentativas. Aguarde um minuto e tente de novo.", StatusCodes.Status429TooManyRequests);
            return new ValueTask(contexto.HttpContext.Response.WriteAsJsonAsync(resposta, cancellationToken));
        };
        opcoes.AddPolicy(Login, http => RateLimitPartition.GetFixedWindowLimiter(
            http.Connection.RemoteIpAddress?.ToString() ?? "desconhecido",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = tentativasPorMinuto,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0
            }));
    }
}
