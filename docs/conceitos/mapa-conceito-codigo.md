---
tema: onde cada conceito do professor aparece no FabricX
fontes: todos os documentos de docs/conceitos
aplicado-em: revisão de código, decisão de onde colocar uma classe
relacionado: [arquiteturas.md, ../02-plano.md, ../../.claude/skills/fabricx-arquitetura/SKILL.md]
---

# Mapa conceito → código

## Camadas

| Camada do professor | Projeto FabricX | Conteúdo |
|---|---|---|
| Model | `FabricX.Domain` | Entidades, value objects, enums, domain services, `DomainException` |
| Service | `FabricX.Application` | DTOs, interfaces de repositório, Services, `NotFoundException` |
| Infra | `FabricX.Infrastructure` | `AppDbContext`, repositórios EF, DI |
| Controller | `FabricX.Api` | Controllers, `WebResponse<T>`, `ExceptionHandlingMiddleware`, `Program.cs` |
| View | não existe | O JSON em `WebResponse<T>` faz esse papel. Front-end Vue é futuro |

## Conceito → decisão no projeto

| Conceito | Onde aparece |
|---|---|
| Regra da Dependência | Api → Application → Domain; Infrastructure → Application |
| DIP | `I...Repository` declarado na Application, implementado na Infrastructure |
| DI por construtor | Services e repositórios recebem interfaces; registro no `Program.cs` |
| Entidade rica | Estado `private set`, regra no método, `DomainException` |
| DTO de fronteira | Entrada e saída dos Services; entidade nunca sai pela Api |
| Response Wrapper | `WebResponse<T>` na Api, montado por controller e middleware |
| Repository por raiz de agregado | Um repositório por agregado (ex.: `OrdemProducao`), não por tabela |
| Troca de banco | Só Infrastructure e `Program.cs` (SQLite hoje) |
| Teste sem banco | Fake em memória do repositório nos testes de Service |
| YAGNI, KISS | Sem interface para o que nunca terá segunda implementação ou teste |

## Divergências entre o material e o projeto

O professor ensina mais de um jeito. O projeto escolheu um. Registrar a escolha evita reabrir a discussão.

| Tema | Material | Escolha do FabricX |
|---|---|---|
| Resultado entre camadas | Diagrama: `DomainResult` (1001, 1002, 1003) e `ApplicationResult` (200, 201, 400, 500) | Exceções (`DomainException`, `NotFoundException`) e middleware gera `WebResponse<T>.Failure` |
| 404 | Diagrama não lista 404, mas a teoria de REST/HTTP o prevê e o exemplo `GetById` do professor usa `Failure("Produto não encontrado.", 404)` | `NotFoundException` → 404. **Decisão fechada** (preferência pela teoria) |
| Camada de aplicação | Diagrama variante A: Controller chama `DomainService` direto. Variante B: Controller → `ApplicationService` → `DomainService` | Variante B: Controller → Service (Application) → entidades e domain services |
| Operações do Repository | Teoria: `Add`/`Remove`/`Get`, `Update` quebra a semântica. Aula prática: `Add`/`Update`/`Delete` | Preferência pela teoria. Hoje `IProdutoRepository` tem `AtualizarAsync`, que diverge. **Pendente**: decidir se remove (exige definir onde ocorre o commit) ou mantém com justificativa |
| Onde validar | Slide: Service valida `price <= 0` | Regra de negócio na entidade; validação estrutural no DTO |
| Interface do repositório | Domain ou Application | Application |
| Provider MySQL | Pomelo ou Oracle | SQLite |
| Acesso a dados na Api | Tutorial: `DbContext` no endpoint | Sempre via Service e repositório |
| Comentários em código | Slides comentam bastante | `CLAUDE.md` proíbe comentário redundante. Ao copiar exemplo, remova |

## Checklist de revisão (perguntas do professor)

1. SRP: quantos motivos essa classe tem para mudar?
2. OCP: preciso editar código existente a cada novo tipo ou comportamento?
3. LSP: o subtipo cumpre o contrato da base? Lança exceção inesperada?
4. ISP: há método vazio ou `NotImplementedException`?
5. DIP: a classe depende de infraestrutura concreta ou usa `new` para dependência volátil?
6. Domínio referencia EF Core, HTTP ou algo externo?
7. Service devolve ou recebe entidade? Conhece `StatusCode` ou `WebResponse<T>`?
8. Repository contém regra de negócio? Service contém SQL ou `DbContext`?
9. A entidade virou anêmica (só propriedades, regra no Service)?
10. A abstração se justifica por teste ou segunda implementação, ou é modismo?
