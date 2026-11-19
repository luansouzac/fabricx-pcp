---
tema: SOLID, acoplamento e coesão
fontes:
  - Unidade_II___SOLID (1).pdf
  - Unidade_II___Acoplamento_e_Coeso (1).pdf
  - Unidade_II___Exemplo___Baixo_Acoplamento_e_Alta_coeso_1 (2).pdf
  - Arquitetura_Limpa.pdf (princípios de design: SoC, DRY, KISS)
  - Exercicios_Engenharia_de_Software (2).pdf (questões 2 a 5)
aplicado-em: todo o código
relacionado: [injecao-de-dependencia.md, repositorio-dao-service.md]
---

# SOLID, acoplamento e coesão

## Por que existem

Sintomas de design degradado: rigidez, fragilidade, imobilidade, viscosidade e complexidade desnecessária. O ciclo típico: pequena alteração → classe acoplada modificada → componentes distantes afetados → testes quebram → manutenção cara.

## Acoplamento

Grau de dependência entre componentes. Dependência é inevitável. O objetivo é controlar a **direção** e evitar acoplar a detalhes voláteis.

Sinais de acoplamento rígido (checklist do professor):

- `new` de dependência dentro da classe.
- Parâmetros declarados como classe concreta em vez de interface.
- Alterar um campo privado de uma classe faz dezenas de classes superiores não compilarem.

Tipos, do melhor ao pior: por **dados** (ideal), por **controle** (flag que diz o que executar), por **estrutura** (acessa internos, viola Lei de Demeter), **global** (estado estático, singletons).

Acoplamento não é sempre ruim: acoplar a `DateTime` ou `System.Collections.Generic` é seguro porque são estáveis. O perigo é acoplar a componentes **voláteis**: serviço web de terceiros, driver, banco. Acoplamento zero não é objetivo real, pois inviabilizaria a comunicação.

## Coesão

Olha para **dentro**: quão relacionadas são as responsabilidades de uma classe. Classe coesa tem propósito único e poucos métodos públicos ligados ao seu nome. Baixa coesão é a "Classe Deus" (`UserManager` com criar usuário, autenticar, enviar e-mail, gerar PDF, salvar arquivo e calcular imposto).

Meta de design: **baixo acoplamento, alta coesão**.

## SOLID

| Princípio | Regra | Pergunta de revisão |
|---|---|---|
| SRP | Uma classe tem um único motivo para mudar. Responsabilidade = um ator que demanda mudança | Quantos motivos essa classe tem para mudar? |
| OCP | Aberto para extensão, fechado para modificação, via interface e polimorfismo | Preciso editar código existente a cada novo tipo? |
| LSP | Subtipo substitui o tipo base sem quebrar o programa. Pré-condição não se fortalece, pós-condição não se enfraquece | O subtipo cumpre o contrato da base? |
| ISP | Cliente não depende de interface que não usa. Várias interfaces pequenas | Há métodos vazios ou `NotImplementedException`? |
| DIP | Alto nível e baixo nível dependem de abstrações. Detalhes dependem de abstrações | Dependo de classe de infraestrutura concreta? |

Violações clássicas usadas nas aulas:

- SRP: `Invoice` com `CalculateTotal`, `SaveToDatabase`, `SendEmail`, `GeneratePdf` e `Print`.
- OCP: `if (type == "Pix") ... else if ("CreditCard")` num `PaymentProcessor`. Correção: `IPaymentMethod` com uma classe por forma de pagamento.
- LSP: `Square : Rectangle` e `Penguin : Bird` que lança `NotSupportedException` em `Fly`.
- ISP: `IWorker` com `Eat`, `Sleep`, `Drive` implementado por `Robot` com `NotImplementedException`.
- DIP: `OrderService` com `new MySqlOrderRepository()`.

### As correções mostradas, uma a uma

**SRP.** `Invoice` fica só com a regra (`CalculateTotal`). Persistência, e-mail e PDF viram `InvoiceRepository`, `InvoiceEmailService` e `InvoicePdfGenerator`. Os cinco motivos para mudar (regra de valor, banco, SMTP, layout do PDF, impressora) passam a ficar em cinco classes. "Responsabilidade" é definida por **ator**: o grupo que demanda a mudança.

**OCP.** O `if` por tipo some e entra o contrato:

```csharp
public interface IPaymentMethod { void Process(decimal amount); }

public class PixPayment : IPaymentMethod { public void Process(decimal amount) { } }

public class PaymentProcessor
{
    public void Process(IPaymentMethod paymentMethod, decimal amount) => paymentMethod.Process(amount);
}
```

Novo meio de pagamento = nova classe, sem tocar no `PaymentProcessor`. Mesmo desenho do exercício de relatório (`IGeradorRelatorio`).

**LSP.** O teste que revela a violação: `TestArea(Rectangle r)` define largura 5 e altura 10 e espera área 50. Com `Square`, o setter altera os dois lados e a área vira 100. Pré-condição não pode ser fortalecida e pós-condição não pode ser enfraquecida. Para `Bird` e `Penguin`, a saída citada na aula de Arquitetura é criar uma abstração mais genérica (`MoveBehavior`) em vez de herdar um `Fly` que não se cumpre. Sinal prático: `NotImplementedException` ou `NotSupportedException` numa subclasse.

**ISP.** `IWorker` (Work, Eat, Sleep, Program, Drive) vira `IWorkable`, `IEatable`, `ISleepable`, `IProgrammable`. `RobotWorker` implementa só `IWorkable`. Outra aplicação citada: em API REST, endpoints específicos em vez de um genérico com várias responsabilidades.

**DIP.** A seta da classe concreta passa a apontar **para cima**, para a interface. `IOrderRepository` pertence ao alto nível (quem usa), `MySqlOrderRepository` é o detalhe.

### Refatoração progressiva do `OrderManager`

Antes: uma classe com criar pedido, calcular total, salvar no SQL, enviar e-mail, gerar relatório, conectar ao banco e processar pagamento com `if (method == "Pix")`. Violações apontadas: SRP, OCP e DIP juntos.

Depois: `OrderService` recebendo `IOrderRepository`, `IPaymentProcessor` e `INotificationService` por construtor. No slide, a regra (pedido sem itens é inválido) fica no serviço e o resto é delegado. No FabricX essa regra iria para a entidade, conforme o `CLAUDE.md`.

| Aspecto | Antes | Depois |
|---|---|---|
| Acoplamento | Alto, preso ao MySQL | Baixo, preso à interface |
| Coesão | Baixa, mais de 5 funções distintas | Alta, uma responsabilidade por classe |
| Testabilidade | Praticamente impossível sem banco | Fácil, com fakes em memória |
| Extensibilidade | Altera classes existentes | Cria nova implementação |

### O efeito cascata do acoplamento alto

Mudança técnica no driver de PDF → muda a assinatura de `PdfGenerator.Generate()` → `OrderService` é forçado a se adaptar → testes unitários dele quebram → novo build e mais risco de regressão.

### Respostas dos exercícios que o professor dá

| Trecho | Diagnóstico |
|---|---|
| `NotificationService` com `new MySqlConnector()` | Alto acoplamento técnico: notificação presa à persistência SQL |
| Classe de pagamento que lança `NotImplementedException` | LSP |
| `ProductService` com SQL embutido | SRP e DIP |
| `CloudPrinter` com `Scan` que lança exceção | LSP e ISP: separar em interfaces de impressão e escaneamento |

**DIP não é o mesmo que DI.** DIP é o princípio (o que queremos: dependência para abstração). Injeção de Dependência é a técnica (como entregar a dependência). Ver [injecao-de-dependencia.md](injecao-de-dependencia.md).

Sinergia: SRP gera classes focadas, que facilitam o OCP. OCP exige polimorfismo correto (LSP). ISP separa os clientes. DIP amarra tudo com baixo acoplamento.

## Como desacoplar, passo a passo (exemplo Todo do professor)

1. Programa acessa `database.Todos` direto, conhece a classe concreta e a propriedade.
2. Pensar primeiro em **o que o código precisa fazer** e criar o contrato: `ITodoListDatabase { List<Todo> GetTodos(); }`.
3. A classe concreta implementa a interface e esconde a estrutura interna.
4. O consumidor passa a usar só a interface.
5. Ainda sobra o `new` no consumidor. Retirar: quem usa a dependência **recebe** a dependência (`Run(ITodoListDatabase database)`).
6. Agora trocar `MockTodoListDatabase` por `SqlTodoListDatabase` não toca no consumidor.

Regra para guardar: não pense primeiro na classe concreta. Pense "o que o código precisa fazer?" e crie a abstração para essa necessidade.

## Testabilidade

Com DIP, o teste injeta um fake em memória e verifica o comportamento sem banco:

```csharp
public class FakeOrderRepository : IOrderRepository
{
    public bool SaveCalled { get; private set; }
    public void Save(Order order) => SaveCalled = true;
}
```

Sem a interface, testar a regra de cálculo exigiria um MySQL configurado.

## Quando NÃO aplicar

Abstração em excesso é overengineering. YAGNI pesa junto com SOLID.

| Princípio | Custo | Evitar quando |
|---|---|---|
| SRP | Muitas classes pequenas | Lógica trivial de CRUD |
| OCP | Muitas interfaces | Comportamento fixo que não vai mudar |
| LSP | Hierarquias complexas | Composição é mais simples |
| ISP | Muitas interfaces pequenas | A interface já é coesa |
| DIP | Complexidade no container de DI | Script rápido, protótipo descartável |

Não crie interface para classe que nunca terá outra implementação nem exige teste automatizado complexo.

## Outros princípios de design

- **SoC** (Separation of Concerns): camadas com responsabilidades distintas (UI, negócio, acesso a dados, infraestrutura).
- **DRY**: cada conhecimento tem uma representação. Equilibrar com KISS, pois abstração prematura complica.
- **KISS**: a solução mais simples que atende o requisito. A simplicidade não pode comprometer robustez.

## Padrões que aplicam SOLID

Strategy (OCP e DIP), Factory (SRP), Adapter (troca de implementação sem quebrar LSP).

## Exercícios que a disciplina cobra

Cada questão pede justificativa técnica e proposta de classes:

- Encapsulamento: `Conta` com `Saldo { get; set; }` permite `Saldo = -5000`. Correção: `private set`, `Sacar` valida saldo suficiente.
- Coesão e acoplamento: `Pedido` mistura cálculo, banco, e-mail, PDF e impressão. Dividir em classes por responsabilidade.
- SRP: `FuncionarioService.Cadastrar` valida, salva, envia e-mail, loga e gera relatório. Separar em `FuncionarioRepository`, `FuncionarioValidator`, `EmailService`, `LogService`.
- OCP: `Relatorio.Gerar(string tipo)` com `if` por formato. Correção: `IGeradorRelatorio` com uma implementação por formato.
- Arquitetura de vendas: propor camadas de apresentação/API, regra de negócio, acesso a dados e serviços externos, e explicar o impacto mínimo de trocar SQL Server por PostgreSQL.
