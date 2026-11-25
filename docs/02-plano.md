# FabricX — Planejamento e Controle da Produção (PCP)

O nome vem de *fábrica* + *X*.

Trabalho de Engenharia de Software (IF Sudeste MG – Prof. Paulo Dutra), em C# / .NET 10 com Arquitetura Limpa, autenticação JWT, cache e o padrão `WebResponse<T>` para respostas. O cliente é o front-end Vue em `web/`.

## 1. Estrutura da solução

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

## 3. Andamento

Todas as etapas abaixo estão concluídas.

1. **Modelagem**: DER em `docs/der` (`.mmd`, `.png` e `.pdf`) e DDL para SQLite em `database/script.sql`.
2. **Domain**: entidades com regras e enums de status e prioridade.
3. **Application**: DTOs, interfaces de repositório e Services.
4. **Infrastructure**: `AppDbContext` (EF Core), repositórios e injeção de dependência.
5. **Api**: controllers por módulo e middleware global de exceções.
6. **Testes** unitários e de integração (xUnit).
7. **Documentação** do Processo Unificado em `docs/processo-unificado`.
8. **Autenticação JWT**, perfis, limite de tentativas de login e **cache** de consultas, com log de MISS, HIT e invalidação.
9. **Front-end** em Vue 3 e Vuetify (`web/`), com login, token, tratamento do `WebResponse<T>` e tema claro e escuro.
10. **Docker**: `docker compose up --build` sobe API e front.

## 4. Entregáveis do trabalho

A entrega é o repositório Git (enunciado ASP.NET Core, seção 10): https://github.com/luansouzac/fabricx-pcp

- [x] Código-fonte completo da API (`src/`)
- [x] Código atualizado da aplicação cliente (`web/`)
- [x] `README.md` com os itens da seção 9 do enunciado
- [x] Arquivos de configuração e execução (`Dockerfile`, `docker-compose.yml`, `appsettings`)
- [x] Estrutura em camadas
- [x] Implementação do `WebResponse<T>` (ResultWrapper)
- [x] Implementação da autenticação JWT
- [x] Implementação do cache
- [x] Link do repositório no GitHub

Extras mantidos no repositório: script DDL e seed em `database/`, DER em `docs/der/` e documentação do Processo Unificado.

## 5. Decisões fechadas

- **Banco: SQLite**, mas trocável a qualquer momento. Para isso:
  - Os Services dependem só de interfaces (`IOrdemProducaoRepository`, ...) definidas na Application.
  - Só a `Infrastructure` conhece o provider (EF Core). Trocar para MariaDB/MySQL/SQL Server = trocar o pacote do provider e a linha de registro no `Program.cs`, sem tocar em Domain, Application ou Controllers.
  - A string de conexão e o provider ficam no `appsettings.json`.
- **Escopo**: API ASP.NET Core Web API mais o front-end Vue em `web/`, que consome a API com token JWT e trata o `WebResponse<T>`. O Swagger UI continua disponível em Development para testar a API sem o front.
- **Respostas**: toda resposta HTTP usa `WebResponse<T>`, montado na Api (controllers e middleware).
- **Autenticação**: JWT com senhas em PBKDF2 e dois perfis (Administrador e Operador). A chave de assinatura vem de configuração, nunca do código.
- **Cache**: `IMemoryCache` por 1 minuto nas consultas de indicadores e capacidade, invalidado por qualquer escrita bem-sucedida.
