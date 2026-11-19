---
tema: arquiteturas indicadas pelo professor
fontes:
  - Unidade_II___Arquitetura_de_Software.pdf (módulos 5, 7, 11, 14, 15, 17, 22)
  - Arquitetura_Limpa.pdf (seções 1 a 9)
  - Apostila_Notas_de_Aula_-_Engenharia_de_Software (1).pdf (1.1.7, 1.1.8, 3.3.8)
  - Unidade_II___Repository_e_DAO.pdf (módulos 10 a 12)
  - Exercicios_Engenharia_de_Software (2).pdf (questões 6 a 8)
  - DIAGRAMA_ARQUITETURA.png
aplicado-em: estrutura da solução, regra de dependência, fluxo de requisição
relacionado: [arquitetura-limpa.md, repositorio-dao-service.md, mapa-conceito-codigo.md]
---

# Arquiteturas indicadas pelo professor

O material apresenta cinco estilos e um diagrama de fluxo. O estilo a seguir no FabricX é a **Arquitetura Limpa** (seção 3). Os outros servem de contexto e de vocabulário para as questões de prova.

| Estilo | Ideia central | Papel no FabricX |
|---|---|---|
| N camadas | Presentation → Negócio → Dados | Ponto de partida a evitar na forma tradicional |
| MVC | Model, View, Controller | Vocabulário dos controllers da Api |
| Clean Architecture | Círculos concêntricos, dependência para dentro | **Estilo adotado** |
| Hexagonal | Core + Ports + Adapters | Mesma ideia da Clean com outro vocabulário |
| DDD | Modelar o núcleo a partir do domínio | Guia a modelagem do Domain |

## 0. Por que arquitetura

Definição usada pelo professor (Bass, Clements e Kazman): a estrutura do sistema, que engloba os elementos de software, suas propriedades externamente visíveis e as relações entre eles. Trata de **decisões estruturais difíceis de alterar depois** e dos limites (físicos e lógicos) que dizem onde cada lógica vive.

Três níveis de abstração (módulo 3):

| Nível | Trata de |
|---|---|
| Arquitetura | Componentes principais, camadas, fluxo, regras de dependência |
| Design | Relação entre classes, padrões de projeto (Strategy, Factory) |
| Código | Sintaxe limpa, tratamento de exceção, coleções |

O contraexemplo do módulo 2 é o "monólito desorganizado": um `OrderService.CreateOrder` que valida, abre `SqlConnection`, faz `INSERT` e envia e-mail por `SmtpClient` no mesmo método. Consequências citadas: não dá para rodar teste sem servidor real, o acoplamento é destrutivo e não se estende comportamento sem quebrar a regra de negócio.

## 1. N camadas

```
Presentation (Controllers, JSON, APIs)
Business Logic Layer (regras de negócio)
Data Access Layer (SQL, repositórios, EF Core)
```

Problema apontado pelo professor: na direção tradicional a regra de negócio **depende** da camada de dados. Se a tecnologia de persistência muda, o núcleo do negócio muda junto.

Versão corrigida, usada nos slides de Repository (módulo 11): quatro camadas, com a Infraestrutura **dependendo do Domain** (dependência invertida).

```
Presentation (Controllers, Views, APIs)
Application  (orquestração de casos de uso)
Domain       (entidades, regras puras, interfaces de repositório)
Infrastructure (SQL, banco, implementações físicas)
```

## 2. MVC

- **Model**: estado, regras de negócio e acesso a dados.
- **View**: apresentação e renderização.
- **Controller**: recebe a entrada (HTTP), interage com o Model e escolhe a View.

Setas do diagrama do professor: o Controller **atualiza** o Model e **seleciona** a View; o Model **notifica/alimenta** a View. O Controller é o intermediário de entrada.

Note que no MVC clássico o Model reúne regra **e** acesso a dados. A Clean Architecture separa os dois (Model puro, persistência na Infra). Essa é a diferença prática entre os dois estilos para o código do projeto.

Erros que a lista de exercícios (questões 7 e 8) quer que o aluno reconheça num controller:

- validar regra de negócio;
- gravar direto no banco;
- montar HTML da resposta;
- fazer `new` de repositório e de serviço de e-mail (acoplamento rígido).

Correção esperada: validação e regra no Model, apresentação na View, o Controller só traduz entrada em chamada e coordena. Dependências por interface (`IProdutoRepository`, `IEmailService`).

No ASP.NET Core MVC as pastas são `Models`, `Views`, `Controllers` e `Data`. Na Web API do FabricX não há View: o JSON no formato `WebResponse<T>` faz esse papel.

## 3. Clean Architecture (versão da disciplina)

Camadas, de fora para dentro:

| Camada | Responsabilidade | Conhece |
|---|---|---|
| View | Interagir com o usuário | Controller |
| Controller | Adaptador: traduz evento em chamada de caso de uso e formata a saída | Service, DTOs |
| Service | Casos de uso: orquestra entidades, repositórios (por interface) e serviços externos | Model, interfaces |
| Model | Entidades e regras de negócio, classes puras | nada |
| Infra | Implementa as interfaces que o Service declara (banco, HTTP, e-mail) | Service (interfaces), frameworks |

Fluxo oficial da apostila:

```
View → Controller → Service → Model
                       ↑ (via interface)
                      Infra
```

**Regra da Dependência**: código-fonte só depende de coisas mais internas. O domínio não conhece infraestrutura. Frameworks, banco e UI são detalhes. Detalhes completos em [arquitetura-limpa.md](arquitetura-limpa.md).

## 4. Hexagonal (Ports and Adapters)

```
REST Controller → Port de Entrada → Core/Domínio → Port de Saída → SQL Adapter
```

Comparativo do professor:

| Característica | Clean | Hexagonal |
|---|---|---|
| Divisão central | Círculos concêntricos | Hexágono (core) |
| Terminologia | Use Cases, Entities | Ports, Adapters |
| Foco | Camadas internas | Interior versus ambiente externo |
| Independência de framework | Alta | Alta |

O material trata a Hexagonal de forma breve (um diagrama no módulo 15 e a tabela no 17). Não há código nem aprofundamento além do que está acima. O quiz do módulo 14 liga o conceito ao Repository: o contrato fica no Domínio ou na Application, a implementação na Infraestrutura.

Equivalência no FabricX: port de saída = `IOrdemProducaoRepository` na Application; adapter de saída = repositório EF na Infrastructure; adapter de entrada = Controller.

## 5. DDD

Recomendado para domínio complexo. Para sistema simples é exagero. Conceitos que o professor lista:

- **Linguagem ubíqua**: o mesmo vocabulário no código e na conversa com o cliente.
- **Contexto delimitado**: fronteira onde um modelo vale. Comunicação entre contextos por interface.
- **Entidade**: tem identidade, muda ao longo do tempo.
- **Objeto de valor**: sem identidade, imutável, igual por atributos.
- **Agregado e raiz**: grupo tratado como unidade, só a raiz é acessível de fora. Repositório só para raiz de agregado.
- **Serviço de domínio**: regra que não cabe numa entidade só ou envolve várias.
- **Fábrica**: criação consistente de objetos complexos.
- **Evento de domínio**: algo relevante que aconteceu (ex.: `PedidoConcluido`).

## 6. Diagrama de fluxo (DIAGRAMA_ARQUITETURA.png)

O diagrama tem duas variantes. Em ambas, clientes de várias tecnologias (C#, Java, PHP, Python, C++, C, web, mobile) falam JSON via API Gateway com POST, GET, PUT e DELETE, e recebem 200, 201, 400 ou 500.

**Variante A** (dois níveis):

```
Server → Controller ──DTO──► DomainService → banco
         ◄── ApplicationResult (200, 201, 400, 500)
                              ◄── ApplicationResult do DomainService
```

**Variante B** (três níveis):

```
Server → Controller → ApplicationService ──DTO──► DomainService → Repositório → banco
   ApplicationResult      200/201/400/500            DomainResult:
                                                     1001 Validação incorreta
                                                     1002 Dados não encontrados
                                                     1003 Operação concluída
```

Leitura: cada camada devolve um **objeto de resultado** próprio. O domínio responde com código de negócio (`DomainResult`), a aplicação traduz para um resultado com status (`ApplicationResult`) e o Controller transforma isso em HTTP. O DTO é o veículo que cruza a fronteira.

**Diferença para o FabricX**: o diagrama usa objetos de resultado entre camadas. O FabricX usa exceções (`DomainException`, `NotFoundException`) e o `ExceptionHandlingMiddleware` faz a tradução para `WebResponse<T>`. Equivalência prática:

| Diagrama | FabricX |
|---|---|
| 1001 Validação incorreta | `DomainException` → 400 |
| 1002 Dados não encontrados | `NotFoundException` → 404 |
| 1003 Operação concluída | retorno de DTO → 200 ou 201 |
| 500 | exceção não tratada → 500 |

O diagrama lista só 200, 201, 400 e 500. Decisão: usar 404 para "dados não encontrados", por seguir a semântica HTTP e o exemplo `GetById` do próprio professor (`Failure("Produto não encontrado.", 404)`).

## 7. O que a questão de arquitetura da prova exige

A questão 6 da lista (sistema de vendas com cadastros, pedidos, desconto, banco, e-mail, relatórios e futura API REST e mobile) pede, no mínimo:

- componente de **apresentação/API**;
- componente de **regras de negócio**;
- componente de **acesso a dados**;
- componentes de **serviços externos** (e-mail, por exemplo);
- ao menos cinco classes ou interfaces, cada uma com sua responsabilidade;
- justificativa de como a proposta aumenta coesão, reduz acoplamento, aplica dois princípios SOLID e facilita mudança;
- como trocar SQL Server por PostgreSQL com o menor impacto nas regras: interface de repositório declarada no núcleo, implementação e provider só na infraestrutura.

O "monólito" a criticar é `SistemaVendas` com todos os métodos numa classe só (cadastrar, calcular desconto, salvar, enviar e-mail, gerar relatório, autenticar).

## 8. Como escolher

- Mais abstração dá flexibilidade e testabilidade, mas custa mais classes, interfaces e configuração de DI. Menos abstração dá rapidez inicial e alto acoplamento.
- Regra do professor: **nunca aplicar arquitetura complexa por modismo**. Ela deve caber no tamanho da equipe e do projeto.
- Decidir os padrões no início, envolver a equipe e **documentar a decisão** com justificativa e diagrama (apostila 3.3.8).
- O padrão escolhido na apostila para o estudo de caso foi **Repository + Service**.
- Citações, sinais de excesso e gatilhos por camada e padrão: [quando-aplicar-e-overengineering.md](quando-aplicar-e-overengineering.md).
