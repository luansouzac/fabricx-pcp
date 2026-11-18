---
tema: Web API no ASP.NET Core (controllers, [ApiController], erros, OpenAPI)
fontes:
  - https://learn.microsoft.com/en-us/aspnet/core/web-api/?view=aspnetcore-9.0
  - https://learn.microsoft.com/en-us/aspnet/core/tutorials/web-api-help-pages-using-swagger?view=aspnetcore-8.0
  - https://learn.microsoft.com/pt-br/aspnet/core/fundamentals/error-handling-api?view=aspnetcore-9.0
  - https://learn.microsoft.com/pt-br/aspnet/core/?view=aspnetcore-9.0 (hub de tópicos)
aplicado-em: FabricX.Api (controllers, Program.cs, middleware)
relacionado: [arquitetura-limpa.md, aspnet-core-ef.md, mapa-conceito-codigo.md]
---

# Web API no ASP.NET Core

Documentação oficial da Microsoft, não é material do professor. A página de Web API é multi-versão (9.0 e 10.0 quase idênticas). Os trechos entre aspas são da fonte.

## Controllers

- Derivar de `ControllerBase`, não de `Controller`, porque este "adiciona suporte para views". Exceção: controller que serve views e API.
- Controllers são ativados e descartados por requisição.
- `CreatedAtAction(nameof(GetById), new { id }, objeto)` devolve 201.
- `[ProducesResponseType]` documenta os status que a action pode devolver.
- `[Consumes]` com content type diferente do enviado devolve 415.

## [ApiController]

Liga quatro comportamentos, além de exigir roteamento por atributo:

1. **400 automático** para erro de validação de modelo. Torna desnecessário `if (!ModelState.IsValid) return BadRequest(ModelState)`.
2. **Inferência de binding source**: `[FromBody]` para tipo complexo, `[FromRoute]` quando o nome bate com o template, `[FromQuery]` para o resto. `[FromBody]` não é inferido para `string` ou `int`. Mais de um parâmetro de corpo gera exceção.
3. **Inferência de multipart/form-data** para `IFormFile`.
4. **ProblemDetails** (RFC 7807) para respostas de erro: "MVC transforms an error result (a result with status code 400 or higher) to a result with ProblemDetails".

Cada comportamento tem opt-out em `ApiBehaviorOptions` (`SuppressModelStateInvalidFilter`, `SuppressInferBindingSourcesForParameters`, `SuppressMapClientErrors`). Para controlar o corpo do 400 automático, a fonte cita `InvalidModelStateResponseFactory`.

## Tratamento de erro

Fonte: https://learn.microsoft.com/pt-br/aspnet/core/fundamentals/error-handling-api?view=aspnetcore-9.0 (lida por completo, tradução automática). A página avisa que .NET 7 a 9 não têm mais suporte.

Opções que a página apresenta:

| Opção | Uso segundo a página |
|---|---|
| Página de exceção do desenvolvedor | Ativa por padrão em `Development` com `WebApplication.CreateBuilder`. "Não habilite [...] a menos que o aplicativo esteja em execução no `Development`": pode expor stack trace, query string, cookies e headers |
| `UseExceptionHandler` | Para ambientes que não são de desenvolvimento. Sem handler próprio, gera resposta `ProblemDetails` |
| `AddProblemDetails` | Registra o serviço padrão de `ProblemDetails` (RFC 7807). O middleware só gera o Problem quando a resposta **não tem corpo** |
| `UseStatusCodePages` | Preenche respostas 400-499 e 500-599 sem corpo |
| `IExceptionHandler` | Citado só de passagem (ASP.NET Core 8+, para `BadHttpRequestException`). A página não mostra implementação nem registro |
| `HttpResponseException` + filtro de ação | Exemplo para controllers: exceção com status e valor, capturada por filtro com `Order = int.MaxValue - 10`, que define `context.Result` e `ExceptionHandled = true` |

Pontos a registrar:

- Personalização do `ProblemDetails`: `CustomizeProblemDetails`, `ProblemDetailsFactory` (controllers), `ClientErrorMapping[...].Link`.
- Em APIs mínimas, `ThrowOnBadRequest` vale `true` em Development e `false` em produção: falha de binding vira exceção em um e 400 direto no outro. Handlers próprios devem preservar `BadHttpRequestException.StatusCode`, senão erro de cliente vira 500 (nas versões 10 e anteriores o padrão é 500).
- Se o handler de erro for um método de controller: não usar atributo de verbo HTTP nele, e `[ApiExplorerSettings(IgnoreApi = true)]` para tirá-lo do OpenAPI.
- A página se contradiz na ordem: diz que a página do desenvolvedor roda "no início do pipeline", mas os exemplos de controllers a registram depois de `UseExceptionHandler`. Não explica a diferença nem dá regra de ordem entre `UseExceptionHandler` e `UseStatusCodePages`.

**Não cobre:** como implementar `IExceptionHandler`, middleware próprio de tratamento de erro, logging, tabela exceção para status code.

Consequência para o FabricX: ele usa `app.UseMiddleware<ExceptionHandlingMiddleware>()` (antes do bloco de Development, em `Program.cs`) em vez de `UseExceptionHandler`. A página não recomenda nem desaconselha middleware próprio, então a escolha não é contrariada. A troca tem preço claro: o middleware próprio dá o envelope `WebResponse<T>` em todo erro de domínio, mas fica fora do mecanismo `ProblemDetails` padrão. Inferência minha, não da página: como a página do desenvolvedor fica no início do pipeline, o middleware próprio, mais interno, captura a exceção primeiro e devolve o envelope também em Development.

## Wrapper próprio e ProblemDetails

A fonte não trata de wrapper de resposta. Pontos de atenção que se aplicam ao `WebResponse<T>` do FabricX (leitura do código, não afirmação da fonte):

- Erros automáticos do framework (400 de validação, 404 sem rota, 415) saem como `ProblemDetails`, não como `WebResponse<T>`, a menos que se customize. O FabricX já cobre o 400 de validação: `Program.cs` define `InvalidModelStateResponseFactory` e devolve `WebResponse<object>.Failure(..., 400)`, e os DTOs usam Data Annotations (`[Required]`, `[StringLength]`, `[Range]`). Lacuna conhecida: 404 de rota inexistente e 415 continuam em `ProblemDetails`. Só tratar se virar requisito (`SuppressMapClientErrors`).
- `[ProducesResponseType(typeof(WebResponse<T>), ...)]`, como em `ProdutosController`, é o jeito de documentar o envelope no OpenAPI.

## OpenAPI

Do tutorial de Swagger (que cobre até .NET 8):

- OpenAPI é a especificação. Swagger é o conjunto de ferramentas (SwaggerUI etc.).
- Implementações citadas: **Swashbuckle.AspNetCore** e **NSwag**.
- Configuração do tutorial: `AddEndpointsApiExplorer()` + `AddSwaggerGen()`, e `UseSwagger()` + `UseSwaggerUI()` dentro de `if (app.Environment.IsDevelopment())`.
- A página diz que, a partir do .NET 9, o suporte OpenAPI **embutido** "replaces Swashbuckle as the default" e que o Swashbuckle "is no longer included in project templates", seguindo disponível como pacote da comunidade.
- Para proteger os endpoints de documentação: `app.MapSwagger().RequireAuthorization()`.
- A página não explica comentários XML nem `ProducesResponseType`.

No FabricX: `AddOpenApi()` e `MapOpenApi()` em `Program.cs`, que é o caminho embutido recomendado para .NET 9+. `MapOpenApi()` está dentro de `if (app.Environment.IsDevelopment())`, igual à restrição que o tutorial usa para o Swagger UI. Não adicionar Swashbuckle.

## O que o hub de documentação lista (para consulta futura)

Tópicos existentes na documentação oficial, sem conteúdo lido: APIs mínimas contra controllers (`fundamentals/apis`), middleware (`fundamentals/middleware/`), logging (`fundamentals/logging/`), testes de integração (`test/integration-tests`), segurança e identidade (`security/`), boas práticas de performance (`performance/performance-best-practices`), hospedagem e Docker (`host-and-deploy/`). Só ler quando o projeto precisar.
