# FabricX — Planejamento e Controle da Produção (PCP)

O nome vem de *fábrica* + *X*.

Trabalho de Engenharia de Software I (IF Sudeste MG – Prof. Paulo Dutra), em C# / .NET 10 com Arquitetura Limpa e o padrão `WebResponse<T>` para respostas.

## 1. O que foi criado (esqueleto)

```
fabricx-pcp/
├── FabricX.slnx
├── src/
│   ├── FabricX.Domain/           Model: entidades, value objects, enums, DomainException
│   ├── FabricX.Application/      Service: DTOs, interfaces (repositórios), casos de uso, exceções de aplicação
│   ├── FabricX.Infrastructure/   Infra: acesso a dados (EF Core), implementação dos repositórios
│   └── FabricX.Api/              Controller: ASP.NET Core Web API (controllers, WebResponse<T>, middleware de erros)
├── tests/FabricX.Tests/          xUnit
├── database/                 scripts DDL (.sql)
└── docs/                     explicações, plano, entidades e DER
```

Já existem e compilam: `DomainException` e `WebResponse<T>` (conforme o PDF). Pastas vazias estão marcadas com `.gitkeep`.

### Regra de dependência (de fora para dentro)

```
Api ──► Application ──► Domain
Infrastructure ──► Application ──► Domain
```

- `Domain` não referencia nada.
- `Application` conhece só o `Domain` e define as interfaces (`IOrdemProducaoRepository` etc.).
- `Infrastructure` implementa essas interfaces (inversão de dependência).
- `Api` só orquestra: recebe DTO, chama o Service, devolve `WebResponse<T>`. O `WebResponse<T>` fica aqui porque carrega status HTTP, que a Application não deve conhecer.
- Services devolvem DTO ou lançam `DomainException` / `NotFoundException`; o `ExceptionHandlingMiddleware` transforma em `WebResponse<T>.Failure`.

## 2. Requisitos do PCP (do enunciado) → módulos

| # | Requisito | Módulo |
|---|-----------|--------|
| 1 | Gestão de Ordens de Produção (produto, quantidade, prazo, recursos) | `OrdemProducao` |
| 2 | Programação da Produção (recursos, capacidade, prioridade, cronograma por ordem) | `Programacao`, `Recurso` |
| 3 | Estoque de matérias-primas, produtos em processo e acabados, com alerta de reposição | `Estoque`, `MovimentacaoEstoque` |
| 4 | Monitoramento de desempenho (eficiência, produtividade, qualidade) e relatórios | `Apontamento`, relatórios |
| 5 | Integração com ERP/SGQ (desejável) | Interfaces na Application, implementação fake na Infra |
| 6 | Planejamento de capacidade (demanda × capacidade, gargalos, ociosidade) | `Capacidade` |

## 3. Próximos passos (em ordem)

1. **Modelagem** (DER feito em `docs/der`; falta o DDL): DER (draw.io / MySQL Workbench) + `database/script.sql` com DDL para **SQLite**.
2. **Domain**: entidades com regras (ex.: `OrdemProducao` não aceita quantidade ≤ 0 nem prazo no passado), enums de status e prioridade.
3. **Application**: DTOs de entrada/saída, interfaces de repositório, Services (casos de uso) que devolvem DTO ou lançam exceção.
4. **Infrastructure**: `AppDbContext` (EF Core), repositórios, injeção de dependência.
5. **Api**: controllers por módulo, middleware global que converte exceções (`DomainException` → 400, não encontrado → 404, demais → 500) em `WebResponse<T>.Failure`.
6. **Testes** unitários do Domain e dos Services.
7. **Documentação** do Processo Unificado (Iniciação, Elaboração, Construção, Transição) no modelo da disciplina + repositório Git com branches por iteração.

## 4. Entregáveis do trabalho (SIGAA, zip)

- [ ] Script DDL `.sql`
- [ ] Documentação do projeto (modelo da disciplina)
- [ ] Projeto completo (somente API; Swagger como interface de uso)
- [ ] DER em imagem/PDF + arquivo do diagrama
- [ ] Link do repositório no GitHub

Não atualizar o projeto depois da data de entrega.

## 5. Decisões fechadas

- **Banco: SQLite**, mas trocável a qualquer momento. Para isso:
  - Os Services dependem só de interfaces (`IOrdemProducaoRepository`, ...) definidas na Application.
  - Só a `Infrastructure` conhece o provider (EF Core). Trocar para MariaDB/MySQL/SQL Server = trocar o pacote do provider e a linha de registro no `Program.cs`, sem tocar em Domain, Application ou Controllers.
  - A string de conexão e o provider ficam no `appsettings.json`.
- **Escopo atual: apenas API** (ASP.NET Core Web API). Testes manuais via Swagger/OpenAPI e arquivo `.http`.
- **Front-end futuro: Vue.js**, em projeto separado (`web/`), consumindo a API e o contrato `WebResponse<T>`. Fica fora do escopo por enquanto; nada dele é criado agora.
- **Respostas**: toda resposta HTTP usa `WebResponse<T>`, montado na Api (controllers e middleware).
- **Ponto de atenção**: o enunciado exige "interface gráfica". Enquanto não houver o front-end em Vue, o Swagger UI é o que mais se aproxima disso. Vale confirmar com o professor se API pura é aceita.
