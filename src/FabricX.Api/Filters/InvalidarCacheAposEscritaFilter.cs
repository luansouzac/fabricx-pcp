using FabricX.Api.Controllers;
using FabricX.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace FabricX.Api.Filters;

public class InvalidarCacheAposEscritaFilter : IAsyncActionFilter
{
    private readonly ICacheDeConsultas _cache;

    public InvalidarCacheAposEscritaFilter(ICacheDeConsultas cache)
    {
        _cache = cache;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var executada = await next();

        var ehEscrita = !HttpMethods.IsGet(context.HttpContext.Request.Method)
            && !HttpMethods.IsHead(context.HttpContext.Request.Method)
            && !HttpMethods.IsOptions(context.HttpContext.Request.Method);
        var afetaConsultas = context.Controller is not (AuthController or UsuariosController);
        var sucesso = executada.Exception is null
            && executada.Result is ObjectResult { StatusCode: null or < 400 };

        if (ehEscrita && afetaConsultas && sucesso)
            _cache.InvalidarTudo();
    }
}
