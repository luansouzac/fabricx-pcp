---
tema: ASP.NET Core, EF Core e estrutura de solução
fontes:
  - AspNET_Core.pdf (capítulos 1 a 3)
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

## Namespaces

Agrupam tipos e evitam colisão de nomes. Podem ser hierárquicas. Usar `using` ou o nome completo para desambiguar.

## O que o tutorial ensina que o projeto NÃO segue

- Tutorial referencia `Data` direto da `Api`. No FabricX a Api só referencia a Infrastructure para registrar DI.
- Tutorial usa `DbContext` direto no endpoint (`app.MapGet("/alunos", async (AppDbContext db) => ...)`). No FabricX o acesso passa por Service e repositório.
- Entidades do tutorial usam `{ get; set; }` públicos. No FabricX o estado é `private set` (ver [arquitetura-limpa.md](arquitetura-limpa.md)).
