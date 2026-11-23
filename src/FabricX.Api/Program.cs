using System.Text.Json.Serialization;
using FabricX.Api.Common;
using FabricX.Api.Middleware;
using FabricX.Application.Interfaces;
using FabricX.Application.Services;
using FabricX.Infrastructure;
using FabricX.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddControllers()
    .AddJsonOptions(opcoes => opcoes.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()))
    .ConfigureApiBehaviorOptions(opcoes =>
    {
        opcoes.InvalidModelStateResponseFactory = contexto =>
        {
            var mensagem = string.Join(" ", contexto.ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => string.IsNullOrWhiteSpace(e.ErrorMessage) ? "Requisição inválida." : e.ErrorMessage));

            var resposta = WebResponse<object>.Failure(mensagem, StatusCodes.Status400BadRequest);
            return new BadRequestObjectResult(resposta);
        };
    });

builder.Services.AddOpenApi();

builder.Services.AddCors(opcoes => opcoes.AddPolicy("Desenvolvimento", politica =>
    politica.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

builder.Services.AddInfrastructure(builder.Configuration.GetConnectionString("Default")
    ?? throw new InvalidOperationException("ConnectionStrings:Default não configurada."));
builder.Services.AddScoped<IProdutoService, ProdutoService>();
builder.Services.AddScoped<IItemEstruturaService, ItemEstruturaService>();

var app = builder.Build();

using (var escopo = app.Services.CreateScope())
{
    var contexto = escopo.ServiceProvider.GetRequiredService<AppDbContext>();
    BancoInicializador.AplicarScriptSeVazio(contexto, Path.Combine(AppContext.BaseDirectory, "database", "script.sql"),
        app.Environment.IsDevelopment() ? Path.Combine(AppContext.BaseDirectory, "database", "seed.sql") : null);
}

app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(opcoes => opcoes.SwaggerEndpoint("/openapi/v1.json", "FabricX API v1"));
    app.UseCors("Desenvolvimento");
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();

public partial class Program;
