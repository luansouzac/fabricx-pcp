using FabricX.Api.Common;
using FabricX.Application.Exceptions;
using FabricX.Domain.Exceptions;

namespace FabricX.Api.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (DomainException ex)
        {
            await Write(context, WebResponse<object>.Failure(ex.Message, StatusCodes.Status400BadRequest));
        }
        catch (NotFoundException ex)
        {
            await Write(context, WebResponse<object>.Failure(ex.Message, StatusCodes.Status404NotFound));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro não tratado");
            await Write(context, WebResponse<object>.Failure("Erro interno do servidor.", StatusCodes.Status500InternalServerError));
        }
    }

    private static Task Write(HttpContext context, WebResponse<object> response)
    {
        context.Response.StatusCode = response.StatusCode;
        return context.Response.WriteAsJsonAsync(response);
    }
}
