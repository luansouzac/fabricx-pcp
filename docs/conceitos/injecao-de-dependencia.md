---
tema: injeção de dependência no ASP.NET Core
fontes:
  - Injeo_de_Dependencia.pdf (slides 2 a 15)
  - Unidade_II___SOLID (1).pdf (módulo 8)
  - Unidade_II___Implementao_do_Repository_Pattern_em_CSharp_.pdf (módulo 6)
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

Nos exemplos do professor o repositório é registrado com `AddScoped`. O material não discute qual tempo de vida escolher para cada caso, apenas define os três. Observação fora do material (comportamento do .NET): `AddDbContext` registra o contexto como Scoped por padrão, então o repositório que o recebe também deve ser Scoped.

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
