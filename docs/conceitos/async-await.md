---
tema: programação assíncrona com async e await no ASP.NET Core
fontes:
  - https://code-maze.com/asynchronous-programming-with-async-and-await-in-asp-net-core/ (Marinko Spasojević, atualizado em 13/05/2024)
aplicado-em: Services, repositórios e controllers (todos os métodos `*Async`)
relacionado: [aspnet-core-ef.md, repositorio-dao-service.md]
---

# async e await no ASP.NET Core

Fonte externa (blog Code Maze), não é material do professor. Serve de apoio técnico. Os trechos entre aspas são do artigo.

## Ideia central

Async dá **escalabilidade, não velocidade**. Uma consulta de 3 segundos continua levando 3 segundos, mas a thread é devolvida ao pool durante a espera de I/O e atende outras requisições. O artigo descreve o caso extremo como "thread pool starvation".

## Como funciona

- `async` sozinho não torna nada assíncrono: o método roda de forma síncrona até o primeiro `await`.
- `await` verifica se a operação já terminou. Se sim, segue síncrono. Se não, pausa o método e devolve uma `Task` incompleta. O resto do método é a continuação.
- Retornos válidos: `Task<T>`, `Task` e `void`.
- No ASP.NET Core não existe `SynchronizationContext`.

## Regras do artigo

| Regra | Motivo dado |
|---|---|
| Usar `async` e `await` juntos | `async` sem `await` não ganha nada |
| Método sem valor retorna `Task`, não `void` | `void` é difícil de testar, de tratar erro e de informar status |
| `async void` só em event handler | idem |
| Obter resultado com `await`, nunca `.Result` ou `.Wait()` | "These can cause a deadlock in our app" |
| Propagar async por toda a cadeia (controller → service → repositório) | ficar síncrono no meio anula o ganho |
| Não remover o `await` | sem ele a `Task` "swallows the exception": o `catch` não roda e o erro aparece depois, em outro ponto |

## Exemplo do artigo (mesmo desenho do FabricX)

Interface do repositório com `Task`, implementação com `ToListAsync`, `SingleOrDefaultAsync` e `SaveChangesAsync`, controller `async Task<IActionResult>` que dá `await` no repositório.

## O que o artigo NÃO cobre

`ConfigureAwait`, `CancellationToken`, I/O-bound contra CPU-bound, `Task.Run`, sufixo `Async` como regra, concorrência e transações no EF Core, testes de método async. Qualquer afirmação sobre esses pontos neste projeto vem do código, não desta fonte.

## No FabricX

- Todo o fluxo é async ponta a ponta: `ProdutosController` → `ProdutoService` → `IProdutoRepository` (`ObterPorIdAsync`, `ListarAsync`, `AdicionarAsync`, `AtualizarAsync`).
- Todos os métodos recebem `CancellationToken`, mesmo o artigo não falando dele. É convenção do projeto, não do material.
- Nenhum `.Result` ou `.Wait()` no código (verificado por busca em `src/`). Manter assim.
- Limite que o artigo não cobre e a documentação do EF Core cobre (ver [aspnet-core-ef.md](aspnet-core-ef.md)): o `DbContext` não é thread-safe e não aceita duas operações simultâneas. Dentro de um Service, não disparar várias consultas do mesmo repositório com `Task.WhenAll`. Sequência de `await` é o padrão.
- Service e repositório não devem ficar síncronos "só porque é SQLite local": a regra de propagar vale para a cadeia inteira.
