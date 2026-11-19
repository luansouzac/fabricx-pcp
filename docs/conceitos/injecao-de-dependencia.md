---
tema: injeção de dependência no ASP.NET Core
fontes:
  - Injeo_de_Dependencia.pdf (slides 2 a 15)
  - Unidade_II___SOLID (1).pdf (módulo 8)
  - Unidade_II___Implementao_do_Repository_Pattern_em_CSharp_.pdf (módulo 6)
  - https://learn.microsoft.com/pt-br/aspnet/core/fundamentals/dependency-injection?view=aspnetcore-9.0 (complemento)
  - https://learn.microsoft.com/pt-br/dotnet/core/extensions/dependency-injection/service-lifetimes e service-registration (complemento)
aplicado-em: Program.cs da Api, construtores de Services e repositórios
relacionado: [solid-acoplamento-coesao.md, repositorio-dao-service.md]
---

# Injeção de dependência (DI)

Padrão que desacopla componentes. Facilita trocar dependências, testar com mocks e manter o código. A classe deixa de depender de implementação concreta e passa a depender de interface.

## Problema: forte acoplamento

`ProdutoController` que faz `new ProdutoRepositorio()` dentro dele:

- o repositório fica "fixo", não dá para trocar por mock;
- mudar a implementação do repositório obriga a mudar o controller;
- código "engessado": mudança numa dependência se espalha por várias partes;
- não dá para testar a classe isoladamente.

Mesma ideia no exemplo de SOLID: `OrderService` com `new MySqlOrderRepository()` no construtor acessa o banco real **na criação**, então a lógica de cálculo e validação não pode ser testada sem um MySQL configurado.

Implementação concreta, no vocabulário do slide: classe completa, instanciável diretamente, ao contrário de uma abstrata. Ela é o "detalhe" que o consumidor não deveria conhecer.

## Solução

1. Desacoplar com interface ou classe abstrata.
2. **Injetar pelo construtor** em vez de criar a dependência dentro da classe.

```csharp
public class ProdutoController
{
    private readonly IProdutoRepositorio _repositorio;

    public ProdutoController(IProdutoRepositorio repositorio)
    {
        _repositorio = repositorio;
    }
}
```

Tipos de injeção (aula de SOLID):

| Tipo | Quando |
|---|---|
| Por construtor | Padrão. Dependências obrigatórias |
| Por propriedade | Dependências opcionais |
| Por método | Dependência necessária a um único método |

## Container do ASP.NET Core

O container gerencia criação e tempo de vida dos objetos. Os serviços são registrados no `Program.cs`.

| Tempo de vida | Comportamento |
|---|---|
| Transient | Criado toda vez que é solicitado |
| Scoped | Criado uma vez por requisição HTTP |
| Singleton | Criado uma vez e compartilhado por toda a aplicação |

Registro típico do material:

```csharp
builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlite("Data Source=fabricx.db"));
builder.Services.AddScoped<IProductRepository, EfProductRepository>();
```

Nos exemplos do professor o repositório é registrado com `AddScoped`. O material do professor não discute qual tempo de vida escolher para cada caso, apenas define os três.

## Complemento: documentação oficial da Microsoft

Fonte: https://learn.microsoft.com/pt-br/aspnet/core/fundamentals/dependency-injection?view=aspnetcore-9.0. A leitura recebeu o texto literal só até cerca de 80% da página. Itens marcados **[resumo]** vieram de resumo, não de texto literal.

- O consumidor depende de interface e o contêiner (`IServiceProvider`) cria e descarta a instância. Registro no `Program`.
- Registrar o mesmo tipo várias vezes: vence o último ao resolver `IMyDependency`. `IEnumerable<IMyDependency>` devolve todos, na ordem de registro.
- Convenção para agrupar registros: método de extensão `Add{Grupo}`, no namespace `Microsoft.Extensions.DependencyInjection`. É o que `AddInfrastructure` faz no FabricX.
- **DbContext**: por padrão é registrado "usando o tempo de vida com escopo", e serviços de vida mais longa "não devem usar um contexto de banco de dados com um tempo de vida menor". Isso confirma a observação anterior: o repositório que recebe o `AppDbContext` deve ser Scoped, como em `DependencyInjection.cs` do FabricX.
- **Middleware**: serviço Scoped injetado no construtor de um middleware "gera uma exceção de runtime porque força o serviço com escopo a se comportar como um singleton". Alternativa: parâmetros de `InvokeAsync`. Relevante para o `ExceptionHandlingMiddleware`: se ele um dia precisar de Service, receber em `InvokeAsync`.
- **Fora de uma requisição** (ex.: inicialização): usar `CreateScope()` e `GetRequiredService<T>()`. É o que `Program.cs` faz para aplicar o script do banco.
- Recomendações oficiais: preferir dependências no construtor a `RequestServices` ou `GetService`; evitar classes e membros estáticos com estado; evitar `new` de dependência; manter serviços pequenos, pois muitas dependências injetadas "podem indicar violação do princípio de responsabilidade única" (**[resumo]** nas duas últimas).
- Descarte: o contêiner descarta o que ele criou. Instância criada fora do contêiner e passada a `AddSingleton(new X())` não é descartada pela estrutura **[resumo]**.
- Serviços com chave (`AddKeyedScoped`, `[FromKeyedServices]`) existem, mas só se justificam com várias implementações da mesma interface em uso simultâneo. Não há esse caso no FabricX.

### Tempos de vida e registro em detalhe

Fontes: `dotnet/core/extensions/dependency-injection/service-lifetimes` e `service-registration` (pt-br, lidas por completo, tradução automática).

| Tempo de vida | Segundo a documentação |
|---|---|
| Transient | "criado sempre que é solicitado". Descartado ao fim da requisição. Gera alocação a cada resolução |
| Scoped | Uma vez por requisição de cliente. Descartado ao fim da requisição. `AddDbContext` registra o `DbContext` como scoped por padrão |
| Singleton | Criado na primeira solicitação, mesma instância depois. Deve ser thread safe e "geralmente [é] usado em serviços sem estado". Memória só é liberada no desligamento |

Regras de resolução:

- Scoped precisa de escopo: implícito (requisição no ASP.NET Core) ou explícito via `IServiceScopeFactory.CreateScope()`.
- **Proibido**: resolver scoped dentro de singleton por construtor. O scoped passa a se comportar como singleton e "pode levar a um estado incorreto". Aceitável só se o singleton criar escopo explícito.
- Permitido: singleton dentro de scoped ou transient; scoped dentro de scoped ou transient.
- Em Development, resolver serviço de vida mais longa a partir de um de vida mais curta "gera uma exceção" por padrão (validação de escopo).
- Singleton próprio: "permita que o contêiner [...] gerencie a vida útil", sem implementar o padrão Singleton à mão.
- `TryAdd{Tempo}` só registra se o tipo ainda não tiver implementação. `TryAddEnumerable` só registra se não houver implementação **do mesmo tipo**, útil para bibliotecas. "O registro de serviço é independente da ordem, exceto ao registrar várias implementações do mesmo tipo."
- Descarte: o código que resolveu o serviço nunca deve descartá-lo. Instância entregue pronta (`AddSingleton(new X())`) não é descartada pelo contêiner.
- `ServiceDescriptor` com fábrica (`_ => new X(chave)`) permite construir o serviço com valor de configuração.

Aplicação ao FabricX: `ProdutoService` e `ProdutoRepository` são Scoped e dependem do `AppDbContext`, também Scoped, o que respeita as regras. Se um dia um Service virar Singleton (por exemplo, cache), ele não pode receber o repositório por construtor.

Não aparecem nas páginas: critério de escolha entre transient e scoped como decisão de design, `ValidateScopes` e `ValidateOnBuild` pelo nome, o termo "captive dependency". A seção "Validação de escopo" (`overview`) e as diretrizes de `IDisposable` não foram lidas.

## O que muda no teste

```csharp
var fakeRepo = new FakeOrderRepository();
var service = new OrderService(fakeRepo);
service.CreateOrder(new Order());
Assert.True(fakeRepo.SaveCalled);
```

O teste não abre conexão. A aula de Acoplamento e Coesão (módulo 12) afirma que, com baixo acoplamento, testar a autenticação "leva milissegundos sem mockar banco de dados inteiro". A pergunta equivalente no quiz de SOLID fica sem resposta no material.

## Benefícios

Manutenção (mudança numa dependência não afeta quem usa), testabilidade (mocks e fakes em memória), menor acoplamento (separação entre lógica e dependências).

## DI x DIP x Service Locator

- DIP: princípio (dependa de abstração).
- DI: técnica (entregue a dependência de fora).
- Service Locator: alternativa que esconde as dependências. O professor lista como desvantagem o acoplamento implícito. Preferir DI por construtor.

## Alerta do próprio material

No tutorial de console com EF, o `AlunoController` faz `new AppDbContext()`. O professor marca isso como má prática e diz que em aplicações reais o `DbContext` deve ser injetado no construtor.
