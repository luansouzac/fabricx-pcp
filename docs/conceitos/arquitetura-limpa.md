---
tema: arquitetura limpa em detalhe
fontes:
  - Arquitetura_Limpa.pdf (seções 2 a 9)
  - Apostila_Notas_de_Aula_-_Engenharia_de_Software (1).pdf (1.1.7)
aplicado-em: Domain, Application, Infrastructure, Api
relacionado: [arquiteturas.md, solid-acoplamento-coesao.md, repositorio-dao-service.md]
---

# Arquitetura Limpa: o que cada camada contém

Autor da teoria: Robert C. Martin. Material do professor Paulo Dutra, 23/09/2025.

## Princípios

1. Independência de frameworks.
2. Testabilidade: regra de negócio testável sem banco, UI ou serviço externo.
3. Separação de preocupações.
4. Inversão de dependência (DIP).
5. Desacoplamento.
6. Independência de UI.
7. Independência de banco de dados.
8. Independência de agentes externos: APIs e bibliotecas são detalhe de implementação.

Vantagens citadas: testabilidade do núcleo, substituibilidade (SQL Server → MongoDB) e evolução contínua sem comprometer o que existe.

## Domain (Model)

Núcleo estável. Sem referência a banco, framework web ou tecnologia.

- **Entidade**: identidade única, comportamento que encapsula regra, integridade garantida por validação. Estado com `private set`, construtor protege as invariantes e lança `DomainException`.
- **Value Object**: imutável, comparado por valor, autovalidado.
- **Domain Service**: regra que não pertence a uma entidade só, coordena várias entidades, sem estado.

```csharp
public class Produto
{
    public int Id { get; private set; }
    public string Nome { get; private set; }
    public decimal Preco { get; private set; }
    public int Estoque { get; private set; }

    public Produto(string nome, decimal preco)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new DomainException("Nome do produto é obrigatório");
        if (preco <= 0)
            throw new DomainException("Preço deve ser positivo");

        Nome = nome;
        Preco = preco;
        Estoque = 0;
    }

    public void AtualizarPreco(decimal novoPreco)
    {
        if (novoPreco <= Preco * 0.5m)
            throw new DomainException("Redução de preço excede limite permitido");
        Preco = novoPreco;
    }

    public void ReporEstoque(int quantidade)
    {
        if (quantidade <= 0)
            throw new DomainException("Quantidade deve ser positiva");
        Estoque += quantidade;
    }
}
```

Boas práticas do professor:

- Entidades ricas em comportamento, não só propriedades.
- Value Objects imutáveis.
- Domain Services sem estado.
- Nunca referenciar infraestrutura.
- Toda regra testável por teste unitário.

### Entidade ou Value Object?

| | Entidade | Value Object |
|---|---|---|
| Identidade | Sim, persiste no ciclo de vida | Não |
| Igualdade | Pelo identificador | Por todos os atributos |
| Mutabilidade | Muda de estado por métodos | Imutável |
| Exemplo do professor | `Produto`, `Cliente`, `Pedido`, `ContaBancaria` | `Endereco`, `Saldo` |

Value Object se autovalida no construtor:

```csharp
public class Endereco
{
    public string Rua { get; }
    public string Cidade { get; }
    public string CEP { get; }
    public string Estado { get; }

    public Endereco(string rua, string cidade, string cep, string estado)
    {
        if (string.IsNullOrWhiteSpace(rua))
            throw new DomainException("Rua é obrigatória");
        if (!Regex.IsMatch(cep, @"^\d{5}-\d{3}$"))
            throw new DomainException("CEP inválido");

        Rua = rua;
        Cidade = cidade;
        CEP = cep;
        Estado = estado;
    }
}
```

### Domain Service

Exemplo do professor: processar um pedido envolve carrinho, cliente e desconto, e nenhuma dessas entidades é dona da regra. O serviço recebe a dependência por interface (`IDescontoService`, declarada no próprio domínio) e não guarda estado:

```csharp
public class RealizarPedidosDomainService
{
    private readonly IDescontoService _descontoService;

    public RealizarPedidosDomainService(IDescontoService descontoService)
    {
        _descontoService = descontoService;
    }

    public Pedido Processar(Carrinho carrinho, Cliente cliente)
    {
        var pedido = new Pedido(cliente.Id);

        foreach (var item in carrinho.Itens)
        {
            var desconto = _descontoService.CalcularDesconto(cliente, item.Produto);
            pedido.AdicionarItem(new PedidoItem(item.Produto.Id, item.Quantidade, item.Produto.Preco, desconto));
        }

        if (pedido.Total > 1000 && cliente.EhPremium)
            pedido.AdicionarBrinde(new Brinde("Presente Premium"));

        return pedido;
    }
}
```

Note a diferença para o Service da Application: aqui mora **regra** (brinde para premium acima de 1000). O Service da Application apenas coordena a ordem das chamadas.

## Application (Service / casos de uso)

Responde a uma intenção do usuário ou do sistema. Sabe **quais** entidades usar e **quando**, não **como** elas funcionam. Orquestra, mas não contém regra de negócio complexa.

- Um Service por caso de uso (ex.: `ProcessarPedidoService.Execute(dto)`).
- Depende só de interfaces (`IPedidoRepository`, `IEstoqueService`).
- Recebe DTO de entrada e devolve DTO de saída.
- Não conhece banco, framework web ou rede.

### DTOs

Objetos simples de fronteira, sem comportamento. Entrada recebe dados da interface, saída devolve resposta formatada.

Por que não usar a entidade direto:

- SRP: entidade carrega regra, DTO só transporta.
- Segurança: controla exatamente o que sai ou entra (senha, ids internos).
- Desacoplamento do formato técnico (JSON, HTTP).
- Evolução: o domínio muda sem quebrar o contrato público.

Validação **estrutural** no DTO é aceita (`[Required]`, `[EmailAddress]`, `[MinLength]`). Regra de negócio fica na entidade. Mapeamento DTO ↔ entidade pode ser manual ou com AutoMapper.

Cuidado: usar DTO como substituto permanente da entidade leva ao antipadrão da **entidade anêmica**, com a lógica de negócio espalhada nas camadas externas.

### Caso de uso na prática

```csharp
public class ProcessarPedidoService
{
    private readonly IPedidoRepository _repo;
    private readonly IEstoqueService _estoque;

    public ProcessarPedidoService(IPedidoRepository repo, IEstoqueService estoque)
    {
        _repo = repo;
        _estoque = estoque;
    }

    public async Task Execute(ProcessarPedidoDTO dto)
    {
        var pedido = new Pedido(dto.Itens);
        await _estoque.ReservarItens(dto.Itens);
        await _repo.Adicionar(pedido);
    }
}
```

O que o professor destaca: o caso de uso não tem regra complexa, mas **sabe em que ordem as coisas acontecem** (reservar estoque, depois persistir). Depende só de interfaces. Retorna, se preciso, um DTO de saída. Por não conhecer o formato técnico, o mesmo caso de uso atende Web, API e CLI.

## Infrastructure

Única camada que conhece EF Core, HTTP clients, e-mail e afins. Implementa as interfaces que a Application define (DIP).

Repositório traduz entidade de domínio para o modelo do banco e executa via ORM:

```csharp
public class PedidoRepository : IPedidoRepository
{
    private readonly AppDbContext _context;

    public PedidoRepository(AppDbContext context) => _context = context;

    public async Task Adicionar(Pedido pedido)
    {
        await _context.Pedidos.AddAsync(pedido);
        await _context.SaveChangesAsync();
    }
}
```

## Controller (adaptador de interface)

- Recebe o evento da View, interpreta e aciona o caso de uso.
- Mantém a View simples, sem lógica de negócio.
- Adapta dados nos dois sentidos: entrada para o formato do Service, saída para o formato da interface.
- Permite reutilizar o caso de uso em web, mobile, API e CLI.

## Response Wrapper

Problema: sem padrão, sucesso devolve o objeto, 404 devolve `{ "erro": ... }` e 400 devolve `{ "erros": [...] }`. O cliente precisa de `if/else` para cada formato.

Solução: todo retorno vem no mesmo envelope.

```csharp
public class WebResponse<T>
{
    public T? Data { get; private set; }
    public string? Message { get; private set; }
    public bool IsSuccess { get; private set; }
    public int StatusCode { get; private set; }

    private WebResponse(T? data, bool isSuccess, int statusCode, string? message = null)
    {
        Data = data;
        IsSuccess = isSuccess;
        StatusCode = statusCode;
        Message = message;
    }

    public static WebResponse<T> Success(T data, int statusCode = 200, string? message = null)
        => new(data, true, statusCode, message);

    public static WebResponse<T> Failure(string message, int statusCode)
        => new(default, false, statusCode, message);
}
```

Uso no controller: `NotFound(WebResponse<Produto>.Failure("Produto não encontrado.", 404))` e `Ok(WebResponse<Produto>.Success(produto))`. O cliente (no material, Flutter/Dart) faz `fromJson` com uma função `fromJsonT` para o `Data` genérico e reage só a `isSuccess`. Benefícios: previsibilidade, tratamento de erro centralizado, desacoplamento, melhor experiência de desenvolvimento.

## Fluxo de uma requisição, de ponta a ponta

Do cliente ao banco (Response Wrapper, seção 9, e sequência do Repository, módulo 10):

1. O cliente envia a requisição HTTP.
2. O Controller recebe o DTO e chama o Service.
3. O Service usa entidades e chama o repositório **pela interface**.
4. A Infra executa a persistência (`INSERT`) e devolve.
5. O Service devolve DTO de saída (ou a exceção sobe).
6. O Controller monta `WebResponse<T>` com o status certo.
7. O framework serializa para JSON.
8. O cliente desserializa e reage a `isSuccess`.

A dependência de código segue o sentido oposto ao do dado só no ponto da persistência: o Service conhece a **interface**, e a Infra conhece a interface e implementa.

## Violações que o material aponta

| Violação | Por que é problema |
|---|---|
| Domain referencia EF Core, HTTP ou outra tecnologia | Quebra a independência tecnológica e a testabilidade do núcleo |
| Service com `SqlConnection` ou `DbContext` | Application passa a depender da infraestrutura (erros 1 e 2 do módulo 11 de Repository) |
| Repository com `if (price <= 0) throw` | Regra de negócio fora da camada apropriada (erro 3) |
| Controller que valida, grava no banco ou monta HTML | Acúmulo de responsabilidades (exercício 7) |
| Controller com `new` de repositório e de e-mail | Acoplamento rígido, impede teste (exercício 8) |
| Entidade só com propriedades | Entidade anêmica |
| Entidade exposta direto na API | Vaza dado interno e acopla o contrato ao domínio |

Quando cada uma dessas peças se justifica: [quando-aplicar-e-overengineering.md](quando-aplicar-e-overengineering.md).

## Exemplo completo do professor (cadastro de cliente)

- Model: entidade `Cliente` com nome, e-mail, telefone e validação de formato de e-mail.
- Service: `CadastrarCliente` valida regra adicional (e-mail duplicado) e orquestra o salvamento.
- Controller: `ClienteController` extrai dados da requisição, chama o caso de uso, prepara a resposta.
- Infra: `ClienteRepository` persiste em banco relacional e implementa a interface exigida pelo Service.
