using FabricX.Application.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace FabricX.Api.Common;

public static class RespostasDeAutenticacao
{
    public static JwtBearerEvents Eventos() => new()
    {
        OnTokenValidated = async contexto =>
        {
            var repositorio = contexto.HttpContext.RequestServices.GetRequiredService<IUsuarioRepository>();
            var idDoToken = contexto.Principal?.FindFirst("sub")?.Value;
            var usuario = int.TryParse(idDoToken, out var id)
                ? await repositorio.ObterPorIdAsync(id, contexto.HttpContext.RequestAborted)
                : null;

            if (usuario is not { Ativo: true })
                contexto.Fail("Usuário inexistente ou inativo.");
        },
        OnChallenge = contexto =>
        {
            contexto.HandleResponse();
            return Escrever(contexto.HttpContext, StatusCodes.Status401Unauthorized, "Autenticação necessária: envie um token JWT válido no cabeçalho Authorization.");
        },
        OnForbidden = contexto =>
            Escrever(contexto.HttpContext, StatusCodes.Status403Forbidden, "O perfil do usuário não permite esta operação.")
    };

    private static Task Escrever(HttpContext contexto, int status, string mensagem)
    {
        contexto.Response.StatusCode = status;
        return contexto.Response.WriteAsJsonAsync(WebResponse<object>.Failure(mensagem, status));
    }
}
