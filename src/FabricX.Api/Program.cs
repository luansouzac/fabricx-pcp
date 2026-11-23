using System.Text;
using System.Text.Json.Serialization;
using FabricX.Api.Common;
using FabricX.Api.Middleware;
using FabricX.Application.Interfaces;
using FabricX.Application.Services;
using FabricX.Infrastructure;
using FabricX.Infrastructure.Persistence;
using FabricX.Infrastructure.Seguranca;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

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

builder.Services.AddOptions<JwtOptions>()
    .Bind(builder.Configuration.GetSection(JwtOptions.Secao))
    .Validate(o => o.Chave.Length >= JwtOptions.TamanhoMinimoDaChave, $"Jwt:Chave deve ter ao menos {JwtOptions.TamanhoMinimoDaChave} caracteres e vir de configuração (variável Jwt__Chave).")
    .Validate(o => o.Emissor.Length > 0 && o.Audiencia.Length > 0, "Jwt:Emissor e Jwt:Audiencia são obrigatórios.")
    .Validate(o => o.ExpiraEmMinutos > 0, "Jwt:ExpiraEmMinutos deve ser maior que zero.")
    .ValidateOnStart();

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer();

builder.Services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
    .Configure<IOptions<JwtOptions>>((opcoes, jwt) =>
    {
        opcoes.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwt.Value.Emissor,
            ValidateAudience = true,
            ValidAudience = jwt.Value.Audiencia,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Value.Chave)),
            ClockSkew = TimeSpan.FromSeconds(30),
            NameClaimType = "sub",
            RoleClaimType = GeradorDeTokenJwt.ClaimPerfil
        };
        opcoes.MapInboundClaims = false;
        opcoes.Events = RespostasDeAutenticacao.Eventos();
    });

builder.Services.AddAuthorizationBuilder()
    .SetFallbackPolicy(new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build());

builder.Services.AddOpenApi(opcoes => opcoes.AddDocumentTransformer<BearerOpenApiTransformer>());

builder.Services.AddCors(opcoes => opcoes.AddPolicy("Desenvolvimento", politica =>
    politica.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

builder.Services.AddInfrastructure(builder.Configuration.GetConnectionString("Default")
    ?? throw new InvalidOperationException("ConnectionStrings:Default não configurada."));
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUsuarioService, UsuarioService>();
builder.Services.AddScoped<IProdutoService, ProdutoService>();
builder.Services.AddScoped<IItemEstruturaService, ItemEstruturaService>();
builder.Services.AddScoped<IOrdemProducaoService, OrdemProducaoService>();
builder.Services.AddScoped<IIntegracaoService, IntegracaoService>();
builder.Services.AddScoped<IRecursoService, RecursoService>();
builder.Services.AddScoped<IProgramacaoService, ProgramacaoService>();
builder.Services.AddScoped<IApontamentoService, ApontamentoService>();
builder.Services.AddScoped<IIndicadorService, IndicadorService>();
builder.Services.AddScoped<IDemandaPrevistaService, DemandaPrevistaService>();
builder.Services.AddScoped<ICapacidadeService, CapacidadeService>();
builder.Services.AddScoped<IRoteiroService, RoteiroService>();

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
    app.MapOpenApi().AllowAnonymous();
    app.UseSwaggerUI(opcoes => opcoes.SwaggerEndpoint("/openapi/v1.json", "FabricX API v1"));
    app.UseCors("Desenvolvimento");
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

public partial class Program;
