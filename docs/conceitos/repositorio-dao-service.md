---
tema: Repository, DAO, Service e Service Locator
fontes:
  - Unidade_II___Repository_e_DAO.pdf
  - Unidade_II___Implementao_do_Repository_Pattern_em_CSharp_.pdf
  - Apostila_Notas_de_Aula_-_Engenharia_de_Software (1).pdf (3.4, 3.5, 3.5.1)
aplicado-em: Application (interfaces e Services), Infrastructure (repositórios)
relacionado: [arquiteturas.md, injecao-de-dependencia.md, aspnet-core-ef.md]
---

# Repository, DAO, Service e Service Locator

## O problema

Regra de negócio misturada com SQL e conexão (`ProductService` com `SqlConnection` e `Console.WriteLine`): alto acoplamento ao banco, viola SRP e é impossível testar sem banco real.

Active Record (o modelo salva e carrega a si mesmo, ex.: `User.save()` com SQLite) também gera acoplamento: mistura negócio e persistência, qualquer mudança de schema altera a classe, teste unitário vira teste de integração.

## DAO

Encapsula acesso a dados e operações de persistência. Perto da estrutura física, normalmente uma tabela. **Sem regra de negócio.** Vocabulário: `Insert`, `Update`, `Delete`, `FindById`, `FindAll`.

DAO não deve: verificar se cliente pode comprar, calcular desconto, decidir se pedido cancela, aplicar aprovação. A divisão do professor: **DAO → acesso aos dados; Serviço/Domínio → regras de negócio.**

```csharp
public interface IProductDAO
{
    Product? FindById(int id);
    IEnumerable<Product> FindAll();
    void Insert(Product product);
    void Update(Product product);
    void Delete(int id);
}
```

Cada método da implementação corresponde a um comando SQL (`SELECT ... WHERE Id = @id`, `INSERT`, `UPDATE`, `DELETE`). A camada de aplicação não executa SQL diretamente.

## Repository

> "Faz a mediação entre o domínio e as camadas de mapeamento de dados, agindo como uma coleção em memória de objetos de domínio." (Fowler)

Vocabulário de coleção: `GetById`, `GetAll`, `Add`, `Remove`, mais consultas orientadas ao domínio.

```csharp
public interface IProductRepository
{
    Product? GetById(int id);
    IEnumerable<Product> GetAll();
    void Add(Product product);
    void Remove(Product product);
}
```

| | DAO | Repository |
|---|---|---|
| Pergunta | Como acessar os dados? | Como recuperar e persistir objetos? |
| Nível | Infraestrutura / data access | Domínio / abstração |
| Vocabulário | SQL, Insert, Update, Delete | Add, Remove, Get |
| Relação com o domínio | Fraca | Forte |
| Conexão com o banco | Direta | Indireta |

A diferença é o **nível de abstração**. Atenção do professor: Repository não é "uma classe que faz CRUD". Expor `Update` e `Insert` pode quebrar a semântica de coleção (questão de exercício). Nas aulas de implementação, porém, o contrato usa `Add`, `Update` e `Delete`. Ver divergências em [mapa-conceito-codigo.md](mapa-conceito-codigo.md).

### Onde fica

- A **interface** pertence a Domain ou Application (quiz do professor).
- A **implementação** fica na Infrastructure.
- Infra depende da camada interna, nunca o contrário.

### Regras

- Interface reflete a necessidade do domínio, não replica todas as operações do banco ou do ORM.
- Pode ir além de CRUD: `FindByPriceRange(min, max)`, `FindAvailableProducts()`.
- Em DDD, **um repositório por raiz de agregado** (acessa `ItemPedido` só pelo `Pedido`).
- Generic Repository (`IRepository<T>`): menos código repetido, mas expõe operações sem sentido para certas entidades e enfraquece a ideia de agregado.
- Não precisa de Repository em CRUD simples quando o framework já oferece abstração adequada. Abstração sem regra de domínio só adiciona arquivos.

### Três erros comuns

1. SQL dentro do Service (`SqlConnection` no `ProductService`).
2. Service dependente de `DbContext` (acoplado ao EF Core).
3. Repository contendo regra de negócio (`if (price <= 0) throw ...` dentro do `Add`).

### Repository e SOLID (módulo 15)

- **S**: o Service valida regras, o Repository cuida da persistência.
- **O**: mudar para NoSQL é uma nova implementação da mesma interface, sem modificar o Service.
- **D**: o fluxo depende de abstrações estáveis, não de bibliotecas ou drivers instáveis.

### Agregado: o estudo de caso de vendas

Cadeia mostrada na aula de SOLID (módulo 22): `Customer` (entidade) → `Order` (raiz do agregado) → `IOrderRepository` (no Domain) → `OrderRepository` (na Infra). `OrderItem` só é alcançado **através do `Order`**, por isso não tem repositório próprio. O exercício final do material pede exatamente isso: projetar as interfaces e classes para o agregado `Order` com seus `OrderItem`.

### ADO.NET x EF Core

| Aspecto | ADO.NET | EF Core |
|---|---|---|
| SQL manual | Sim | Não necessariamente |
| Mapeamento | Manual, coluna a coluna | Automatizado |
| Abstração | Menor | Maior |
| Controle sobre o SQL | Alto | Menor |
| Produtividade | Menor | Maior |
| Complexidade do código | Maior | Menor |

Com ADO.NET cada operação abre `SqlConnection`, monta `SqlCommand` com **parâmetros** (nunca concatenação), executa e lê com `SqlDataReader`. Com EF Core o mesmo `GetById` vira `_context.Products.FirstOrDefault(p => p.Id == id)`, e `Add`, `Update` e `Delete` terminam com `SaveChanges()`.

### Atenção: ausência silenciosa

Nas implementações do material, `Delete` (EF) retorna sem erro quando o produto não existe, e `Update` em memória também ignora o caso. O material não discute isso. No FabricX, "não encontrado" é um resultado relevante e vira `NotFoundException` no Service, então o contrato precisa deixar claro quem trata a ausência.

### Um contrato, várias implementações

Mesma interface `IProductRepository` implementada por:

| Implementação | Tecnologia | Uso |
|---|---|---|
| `SqlProductRepository` | ADO.NET (`SqlConnection`, `SqlCommand`, `SqlDataReader`, parâmetros, mapeamento manual) | Controle total do SQL |
| `EfProductRepository` | EF Core (`DbContext`, `DbSet`, LINQ, `SaveChanges`) | Produtividade |
| `InMemoryProductRepository` | `List<Product>` | Testes |

O `ProductService` não muda. A troca é só no registro de DI:

```csharp
builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connectionString));
builder.Services.AddScoped<IProductRepository, EfProductRepository>();
```

Para ADO.NET: `builder.Services.AddScoped<IProductRepository>(sp => new SqlProductRepository(connectionString));`.

Teste sem banco:

```csharp
var repository = new InMemoryProductRepository();
var service = new ProductService(repository);
service.RegisterProduct(new Product { Id = 1, Name = "Notebook", Price = 3500m });
Assert.Equal("Notebook", service.GetProduct(1)!.Name);
```

Quadro do professor: Database (infra, armazenamento), ORM (infra, tradução tabela-objeto), DAO (data access), Repository (abstração/domínio), Service (application, orquestração de casos de uso).

## Service

Encapsula lógica de negócio complexa e operações que envolvem várias entidades, coordena Repository e entidades. Separa negócio de acesso a dados e de apresentação.

Vantagens: separação de preocupações, reuso, manutenção em ponto único, extensibilidade (OCP), testabilidade, clareza.

Estrutura do estudo de caso: `PedidoService`, `PagamentoService`, `FaturamentoService` falam com `ClienteRepositorio`, `ProdutoRepositorio`, `PedidoRepositorio`, que falam com a fonte de dados (MySQL ou memória).

Fluxo de uma operação (sequência dos slides): Usuário → HTTP POST → Controller `Create(product)` → Service → Repository `Add(product)` → `INSERT` no banco → retorno.

Quem valida o quê: o Service do slide valida `price <= 0` antes de `Add`. No FabricX, a regra vive na **entidade** (que lança `DomainException`) e o Service orquestra.

## Service Locator

Catálogo central onde os serviços são registrados e a classe pede o que precisa.

Vantagens: desacoplamento, centralização, flexibilidade em tempo de execução, testabilidade.

Desvantagens: **acoplamento implícito** (a dependência não aparece no construtor), complexidade, teste e depuração difíceis, risco de erro de configuração central.

Recomendação para o projeto: usar **injeção de dependência por construtor**, não Service Locator. Ver [injecao-de-dependencia.md](injecao-de-dependencia.md).

## Outros padrões citados (apostila 3.5)

Observer (notificar status de fatura), Strategy (cálculo de preço ou desconto), Factory (criar pedido, fatura, pagamento), State (estados da fatura: pendente, pago, atrasado), Decorator (log em serviços e repositórios), Chain of Responsibility (fluxo de aprovação).
