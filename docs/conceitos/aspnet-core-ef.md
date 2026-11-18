---
tema: ASP.NET Core, EF Core e estrutura de solução
fontes:
  - AspNET_Core.pdf (capítulos 1 a 3)
  - https://dutrapaulovm.github.io/gymnasium-code-42/dotnet/tutorial-basics/introducao/
  - https://learn.microsoft.com/pt-br/ef/core/ e https://learn.microsoft.com/pt-br/ef/
  - https://learn.microsoft.com/pt-br/ef/core/dbcontext-configuration/
  - https://learn.microsoft.com/pt-br/ef/efcore-and-ef6/
  - https://entityframework.net/ (terceiro, comercial)
aplicado-em: Infrastructure (EF Core, migrations), Api (Program.cs), estrutura da solução
relacionado: [injecao-de-dependencia.md, arquiteturas.md]
---

# ASP.NET Core e EF Core

## ORM e EF Core

ORM faz a ponte entre objetos (classes, herança) e o modelo relacional (tabelas, chaves). Em vez de `INSERT/SELECT` manual, manipula-se objetos e o ORM gera o SQL.

Vantagens: produtividade, foco na regra de negócio, manutenção simples, abstração do SGBD (trocar de banco com pouca ou nenhuma alteração), segurança contra SQL injection por consulta parametrizada.

ORMs do .NET citados: **EF Core** (oficial, migrations, LINQ, tracking), **Dapper** (micro-ORM, você escreve o SQL), **NHibernate**.

EF Core permite Code First: criar tabelas a partir de classes. Por convenção, propriedade `Id` ou `<Classe>Id` vira chave primária.

## Pacotes

| Pacote | Função |
|---|---|
| `Microsoft.EntityFrameworkCore` | Núcleo, obrigatório |
| `Microsoft.EntityFrameworkCore.Sqlite` | Provider SQLite (local, testes, pequeno porte) |
| `Microsoft.EntityFrameworkCore.Design` | Suporte em tempo de design, necessário para migrations |
| `Microsoft.EntityFrameworkCore.Tools` | Comandos de CLI |
| `Pomelo.EntityFrameworkCore.MySql` | Provider MySQL/MariaDB da comunidade, maior compatibilidade e performance |
| `MySql.EntityFrameworkCore` | Provider oficial da Oracle, pode ter limitações |

O próprio material é inconsistente sobre MySQL: a introdução diz que o conector oficial da Oracle é usado "sem dependência de bibliotecas alternativas como Pomelo", enquanto os tutoriais instalam o Pomelo. Para o FabricX isso não importa, pois o banco é SQLite.

Na solução em camadas, **todos os pacotes do EF Core ficam no projeto de dados** (Data/Infrastructure). O projeto Web, que só consome a API, não precisa deles.

## CLI de migrations

```
dotnet tool install --global dotnet-ef
dotnet ef migrations add NomeDaMigration
dotnet ef database update
dotnet ef migrations list
dotnet ef migrations remove
dotnet ef dbcontext info
```

Migration é controle de versão do esquema do banco. Fluxo: alterar modelo → `migrations add` → `database update` → `migrations list`.

No FabricX o esquema oficial é o `database/script.sql` (ver skill `fabricx-arquitetura`), então migrations não são o caminho principal.

## DbContext

Sessão com o banco. Em aplicação real recebe `DbContextOptions<AppDbContext>` no construtor e é registrado por `AddDbContext`, não configurado em `OnConfiguring` com string fixa:

```csharp
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Aluno> Alunos { get; set; }
}
```

Troca de banco com uma linha: `UseSqlite("Data Source=x.db")` ou `UseMySql(connectionString, ServerVersion.AutoDetect(connectionString))`. A versão do servidor precisa estar correta (MySQL 8.0.33, MariaDB 10.11.6 etc.).

## Estrutura de uma aplicação ASP.NET Core

- `dotnet new web` cria Minimal API: `.csproj`, `Program.cs`, `launchSettings.json`.
- `Program.cs` é o ponto de entrada: `WebApplication.CreateBuilder(args)` configura serviços e DI, `builder.Build()` cria o app, `app.MapGet(...)` define rotas, `app.Run()` sobe o Kestrel.
- `appsettings.json`: configuração central (connection string, log, chaves externas), com variação por ambiente.
- `bin/` (binários) e `obj/` (intermediários, `project.assets.json`) são gerados e não se editam.

## Solução (.sln) e projetos

A solução é um **contêiner** (não contém código, não é um artefato). Agrupa projetos, gerencia dependências entre eles e a ordem de build.

Analogia do professor: solução = planta e terreno; `Core` = fundação; `Infrastructure` = instalações; `Api` = acabamento visível; `Tests` = inspeção de qualidade.

```
dotnet new sln -n Nome
dotnet new webapi -n Nome.Api
dotnet new classlib -n Nome.Data
dotnet sln add Nome.Api/Nome.Api.csproj
dotnet add Nome.Api/Nome.Api.csproj reference Nome.Data/Nome.Data.csproj
```

Pastas do exemplo em camadas: `Models` (classes mapeadas) e `Data` (`AppDbContext`). No MVC, `Controllers` e `Views` (com `_Layout.cshtml`, `_ViewStart.cshtml`, `_ViewImports.cshtml`). O FabricX não usa Views.

## Fluxo do tutorial de console (passo a passo)

O capítulo 2 monta uma aplicação de console com EF Core em pastas inspiradas no MVC:

1. `dotnet new sln` e `dotnet new console`, adicionando o projeto à solução.
2. Pastas `Models`, `Data`, `Controllers`, `Views`. `Data` é "o único lugar que comunica diretamente com o banco".
3. Instalar os pacotes do EF Core e do provider.
4. Criar a entidade `Aluno` e o `AppDbContext` com `DbSet<Aluno>`.
5. `dotnet ef migrations add CriacaoInicial` e `dotnet ef database update`.
6. `AlunoController` usa o contexto para adicionar e listar; o `Program.cs` simula a interação.

O tutorial tem duas ressalvas internas: o controller faz `new AppDbContext()` (marcado como má prática, ver [injecao-de-dependencia.md](injecao-de-dependencia.md)) e a configuração do banco está em `OnConfiguring` com string fixa. O tutorial da solução (capítulo 3) corrige isso com `AddDbContext` no `Program.cs`.

## Fluxo do tutorial de solução (capítulo 3)

Solução `ControleAluno` com três projetos: `Web` (interface), `Api` (endpoints REST) e `Data` (EF Core). Referências: Api → Data e Web → Data. Pacotes do EF Core no `Data` e na `Api`. Depois: modelo `Aluno`, `AppDbContext` com construtor de opções, `AddDbContext` com `UseSqlite("Data Source=controlealunos.db")`, migration `CriarTabelaAlunos`, endpoint `app.MapGet("/alunos", ...)`.

O material **não** inclui uma camada de Domain ou Application nesse tutorial. Ela vem das aulas de Arquitetura Limpa. O FabricX combina as duas fontes.

## Fontes online adicionais

### Site do professor (Gymnasium Code 42)

https://dutrapaulovm.github.io/gymnasium-code-42/dotnet/tutorial-basics/introducao/ é a versão web do mesmo tutorial do PDF: projeto `dotnet new web` do zero, pastas `Models` e `Data`, pacotes do EF Core, `AppDbContext`, SQLite com `"Data Source=meubanco.db"`, migration `CriarTabelaProdutos` e `app.MapGet("/produtos", async (AppDbContext db) => await db.Produtos.ToListAsync())`. Pontos novos em relação ao PDF:

- pede .NET SDK "8 ou superior" e VS Code com C# Dev Kit;
- SQLite indicado para "aplicações locais ou de pequeno porte";
- migrations aplicadas de forma controlada, "sem perder dados";
- a seção de Views só explica estrutura (`Views/Shared/_Layout.cshtml` etc.) e anuncia que as Views virão depois. Não há controller nem view completos na página;
- tem inconsistências internas, como o PDF: cita `dotnet ef dbcontext list` num ponto e `info` noutro, e a tabela Pomelo x Oracle não traz fonte. O exemplo de MySQL tem senha em texto no código, o que o FabricX não deve imitar (connection string vem de configuração).

Links irmãos listados na barra lateral: `dotnet/intro` e `tutorials`. Não lidos.

### Documentação oficial do EF Core

Fonte: https://learn.microsoft.com/pt-br/ef/core/ (página de visão geral, tradução automática, atualizada em 2026-03-27).

- EF Core é a versão leve, extensível, de código aberto e multiplataforma do Entity Framework, um O/RM.
- Modelo = classes de entidade + `DbContext` (sessão com o banco). Três caminhos: gerar a partir de banco existente, codificar à mão, ou criar o banco pelo modelo com Migrations.
- A página tem uma seção de considerações gerais (testes de integração, desempenho e carga, logs, revisão de segurança, planejar migrações em produção) e um aviso: em SQL Server, `nvarchar(max)` e `decimal(18, 2)` são os padrões do EF e "raramente são os melhores tipos" para produção. No FabricX o esquema vem do `database/script.sql`, então os tipos são decididos lá, não pelo EF.
- O exemplo da página usa `OnConfiguring` com string fixa, o mesmo que o professor marca como pouco adequado para aplicação real.
- Seções que existem e não foram lidas: modelagem, provedores, consultas, salvar dados, migrations, desempenho.

### Tempo de vida do DbContext

Fonte: https://learn.microsoft.com/pt-br/ef/core/dbcontext-configuration/ (lida por completo, tradução automática).

- O `DbContext` "foi projetado para ser usado em uma *única* unidade de trabalho": criar, rastrear entidades, alterar, `SaveChanges`, descartar. Em aplicação web, "cada solicitação HTTP corresponde a uma única unidade de trabalho". Por isso o tempo de vida é curto.
- `AddDbContext` registra o contexto com escopo. Exige construtor público que recebe `DbContextOptions<T>` e repassa à base. É o que `DependencyInjection.cs` do FabricX faz.
- "DbContext não é thread-safe. Não compartilhe contextos entre threads." Não há suporte a várias operações simultâneas na mesma instância, nem consultas assíncronas em paralelo: dar `await` antes de continuar. Se o EF detectar, lança `InvalidOperationException` ("Uma segunda operação foi iniciada nesse contexto antes de uma operação anterior ser concluída"). Se não detectar, a página fala em "comportamento indefinido, falhas de aplicativo e corrupção de dados". Compartilhar o contexto por requisição é seguro no ASP.NET Core porque cada requisição tem escopo próprio.
- Uma `InvalidOperationException` do EF "pode colocar o contexto em um estado irrecuperável".
- `OnConfiguring` "é sempre chamado", mesmo com `AddDbContext`, e pode complementá-lo. Cada contexto usa um único provedor, e configurar outro provedor não remove o anterior.
- `AddDbContextFactory` serve para casos sem escopo adequado (Blazor) ou várias unidades de trabalho na mesma requisição. Quem cria o contexto pela fábrica precisa descartá-lo.
- `EnableSensitiveDataLogging` põe dados da aplicação em logs e exceções. O provedor em memória "não foi projetado para ser usado em produção".
- Não cobre: `AddDbContextPool` (o termo não aparece), tempo de vida do serviço de fábrica, criação em tempo de design.

### EF Core x EF6

Fonte: https://learn.microsoft.com/pt-br/ef/efcore-and-ef6/ (lida por completo, tradução automática).

- Recomendação oficial: "Use o EF Core no .NET Core para todos os novos aplicativos, a menos que o aplicativo precise de algo que só tenha suporte no .NET Framework."
- EF6 é "um produto estável e compatível, mas não está mais sendo desenvolvido ativamente". Continuar nele só se o código de acesso a dados estiver estável e sem previsão de novos recursos.
- Só no EF Core: filtros de consulta global, pool de DbContext, conversões de valor, tipos sem chave, provedor Cosmos DB. Só no EF6: designer e EDMX, assistente "Criar modelo a partir do banco de dados" do Visual Studio, Entity SQL.
- "O EF Core não é uma substituição direta para o EF6." Migrar exige mudanças. Ganho de desempenho não é garantido: "nem todos os cenários são mais rápidos".
- Não cobre passos de migração (remete a `efcore-and-ef6/porting/`, não lida) nem a versão atual do EF Core. O FabricX usa EF Core, sem nenhuma decisão a tomar aqui.

Hub https://learn.microsoft.com/pt-br/ef/: a página **não** compara EF Core com EF6, apenas aponta para a página acima. Lista SQLite e o provedor em memória (para testes) entre os suportados. Provedores de PostgreSQL, MySQL/MariaDB, MongoDB e Firebird apontam para sites de terceiros.

### entityframework.net

Não é da Microsoft. O rodapé diz "Documentation made by ZZZ Projects & .NET Community" e o site promove produtos da ZZZ Projects (Entity Framework Extensions com "Try for free now", Dapper Plus, entre outros), então tem componente comercial. A página inicial só traz títulos e links (modelagem, `/include`, `/as-no-tracking`, `/async`, `/fastest-way-to-insert`, migrations). A única frase conceitual é "Entity Framework is an ORM made by Microsoft". O título diz EF 6 e o texto fala de EF Core, sem esclarecer a versão. Uso: no máximo como índice de artigos. Não adotar como fonte de regra nem de pacote.

## Namespaces

Agrupam tipos e evitam colisão de nomes. Podem ser hierárquicas. Usar `using` ou o nome completo para desambiguar.

## O que o tutorial ensina que o projeto NÃO segue

- Tutorial referencia `Data` direto da `Api`. No FabricX a Api só referencia a Infrastructure para registrar DI.
- Tutorial usa `DbContext` direto no endpoint (`app.MapGet("/alunos", async (AppDbContext db) => ...)`). No FabricX o acesso passa por Service e repositório.
- Entidades do tutorial usam `{ get; set; }` públicos. No FabricX o estado é `private set` (ver [arquitetura-limpa.md](arquitetura-limpa.md)).
