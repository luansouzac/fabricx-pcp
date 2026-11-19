---
tema: análise e projeto orientados a objetos, requisitos e Processo Unificado
fontes:
  - Apostila_Notas_de_Aula_-_Engenharia_de_Software (1).pdf (capítulos 1 a 3)
  - Exercicios_Engenharia_de_Software (2).pdf
aplicado-em: documentação do projeto (modelo da disciplina), modelagem do Domain
relacionado: [arquiteturas.md, solid-acoplamento-coesao.md]
---

# Engenharia de software: processo, requisitos e modelagem

## AOO e POO

- **Análise Orientada a Objetos**: entender os requisitos pela ótica dos objetos. Identifica entidades, relacionamentos e colaborações. Diagramas de classes, casos de uso e sequência. Independente de tecnologia.
- **Projeto Orientado a Objetos**: transforma a análise em design concreto (classes, interfaces, métodos). Diferença: nível de detalhe.

## Conceitos de projeto

- **Abstração**: modelo simplificado, esconde detalhes de implementação.
- **Arquitetura**: estrutura global, componentes, interações e decisões que orientam o desenvolvimento.
- **Padrões de projeto**: criação (Singleton, Factory, Abstract Factory, Builder, Prototype), estruturais (Adapter, Bridge, Composite, Decorator, Facade, Flyweight, Proxy), comportamentais (Observer, Strategy, Command, State, Template Method, Chain of Responsibility, Interpreter, Visitor, Mediator).
- **Modularidade**: módulos pequenos, independentes, com interface bem definida. Permite reuso, teste preciso e trabalho em paralelo.
- **Separação por interesse**: agrupar o que tem responsabilidade semelhante. Evita a "classe de Deus".
- **Separação por afinidade**: agrupar o que colabora ou se relaciona semanticamente (persistência junto, UI junto).
- **Refatoração**: melhorar a estrutura interna **sem mudar o comportamento externo**, em passos pequenos, com testes rodando a cada passo. Reduz dívida técnica.

## Domain Model

Representação das entidades, atributos, relacionamentos e regras de negócio do domínio. Não é estático: evolui com o entendimento do problema. Benefícios: compreensibilidade, abstração, manutenibilidade, reuso, testabilidade e alinhamento com o negócio.

DDD (conceitos) está resumido em [arquiteturas.md](arquiteturas.md).

### Refatoração: os quatro conceitos

1. **Preservação do comportamento**: o resultado externo não muda.
2. **Melhoria contínua**: acompanha o ciclo de vida, não é um evento.
3. **Incrementos**: passos pequenos, com os testes rodando a cada passo.
4. **Uso de testes**: sem testes automatizados não há garantia de que nada quebrou.

Dívida técnica é a metáfora para decisões rápidas ou paliativas tomadas sob pressão de prazo, que acumulam complexidade. Refatorar reduz essa dívida.

## Concepção e planejamento

Sete atividades da **concepção**: levantamento de requisitos, definição de escopo, análise de viabilidade, definição da arquitetura de alto nível, prototipagem, planejamento do projeto, avaliação e validação contínuas.

Sete atividades do **planejamento do desenvolvimento**: escopo, estimativa de tempo e recursos, dependências entre tarefas, cronograma, alocação de tarefas, monitoramento e controle, gestão de riscos.

## Processo Unificado (PU)

Iterativo e incremental, com ênfase em requisitos, modelagem, implementação e testes contínuos.

| Fase | Foco |
|---|---|
| Iniciação | Escopo, viabilidade, requisitos, casos de uso principais, riscos, plano inicial |
| Elaboração | Requisitos detalhados, arquitetura (classes e componentes), protótipos, plano de iterações |
| Construção | Implementar casos de uso, testes unitários e de integração, refatoração, documentação |
| Transição | Teste de aceitação, treinamento, implantação, suporte |

O trabalho exige documentar essas quatro fases no modelo da disciplina, com repositório Git e branches por iteração (ver [../02-plano.md](../02-plano.md)).

## Requisitos

Etapas: identificar → levantar → validar → classificar → priorizar → documentar → revisar → publicar. Também definir **escopo** antes (o que faz, que problemas resolve, quem usa) e **stakeholders** (usuários finais, administradores, TI, gestores).

- Técnicas de levantamento: entrevistas, workshops, questionários, observação, análise de documentos, prototipação.
- Classificação: **funcionais** (o que faz) e **não funcionais** (qualidade e restrições).
- Priorização **MoSCoW**: Must have, Should have, Could have, Won't have. Critérios: impacto no negócio, complexidade, dependências, necessidade do usuário.
- Documentação: ERS (Especificação de Requisitos de Software), casos de uso e UML, critérios de aceitação.
- Concepção inclui também viabilidade, definição de arquitetura de alto nível, prototipagem e planejamento (escopo, estimativa, dependências, cronograma, alocação, monitoramento, riscos).

## Como iniciar o projeto (ordem sugerida)

1. Identificar entidades e atributos.
2. Modelar o banco (DER) e normalizar.
3. Definir as classes a partir do modelo (tabela → classe, FK → associação).
4. Implementar a lógica de negócio nas classes.

Normalização reduz redundância e garante integridade, mas excesso complica consultas.

## Regras de negócio

Definição: diretrizes e restrições que definem como a organização opera. Tipos: validações, processos e fluxos, cálculos, restrições de acesso, políticas, qualidade, precificação.

Como modelar: identificar e documentar, definir processos, validações, cálculos, fluxos, **tratamento de exceções** (produto não encontrado, pagamento maior que a fatura), integrações, validar com os interessados.

Exemplo do estudo de caso (Contas a Receber, "E-Shop"), tabela de regras numeradas:

| Código | Regra |
|---|---|
| RNG-01 | Cliente informa e-mail válido ao se registrar |
| RNG-02 | Valor total do pedido é a soma dos totais dos itens |
| RNG-03 | Status de pagamento da fatura: P (Pago) ou N (Não Pago) |
| RNG-04 | Produto em estoque tem quantidade disponível maior que zero |
| RNG-05 | Pedido só é faturado se todos os itens estiverem em estoque |
| RNG-06 | Valor pago não excede o valor da fatura |
| RNG-07 | Data de emissão da fatura posterior à data do pedido |
| RNG-08 | Cliente só tem um pedido em andamento por vez |
| RNG-09 | Preço de produto não é negativo |

Boa prática para o FabricX: numerar as regras de negócio do PCP no mesmo formato (RNG-xx) e referenciar o código nos testes.

Entidades do estudo de caso: Cliente, Produto, Pedido, ItemPedido (associativa), ContasReceber (fatura), Pagamento. O status da fatura (pendente, pago, atrasado) é candidato a padrão State.

## Modelo anêmico

O professor explica que as classes do exemplo não têm getters e setters porque o uso excessivo deles:

- enfraquece o encapsulamento (expõe todo o interno);
- aumenta complexidade e código redundante;
- compromete a flexibilidade;
- viola o princípio do mínimo privilégio.

Em vez disso, expor uma interface pública enxuta e, quando um atributo exige comportamento, criar um método específico. Complementa o alerta de "entidades anêmicas" em [arquitetura-limpa.md](arquitetura-limpa.md).

## Estilo das questões de prova

A lista de exercícios avalia **justificativa técnica** e capacidade de aplicar o conceito. Nas questões de arquitetura a solução precisa demonstrar separação adequada de responsabilidades. Não exige implementar tudo, salvo quando pedido explicitamente. Temas: C# básico, encapsulamento, coesão e acoplamento, SRP, OCP, proposta de arquitetura (apresentação, negócio, dados, serviços externos), MVC e diagrama View → Controller → Model/Serviços.
