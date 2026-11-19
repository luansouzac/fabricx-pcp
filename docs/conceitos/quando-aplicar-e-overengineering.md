---
tema: quando cada camada ou padrão se justifica e quando vira overengineering
fontes:
  - Unidade_II___Arquitetura_de_Software.pdf (módulo 22, Trade-offs Arquiteturais)
  - Unidade_II___SOLID (1).pdf (módulos 1, 11, 12)
  - Unidade_II___Repository_e_DAO.pdf (módulos 17 e 18)
  - Unidade_II___Acoplamento_e_Coeso (1).pdf (módulo 18)
  - Arquitetura_Limpa.pdf (DRY, KISS, seções 3 a 5)
  - Apostila_Notas_de_Aula_-_Engenharia_de_Software (1).pdf (1.1.8, 3.3.4, 3.3.6, 3.3.8, 3.5.1)
aplicado-em: decisão de criar ou não uma classe, interface, camada ou padrão
relacionado: [arquiteturas.md, solid-acoplamento-coesao.md, repositorio-dao-service.md, mapa-conceito-codigo.md]
---

# Quando aplicar e quando é overengineering

O professor repete um aviso em quase todas as aulas: a estrutura existe para resolver um problema real (mudança, teste, troca de tecnologia). Estrutura sem esse problema é custo. Este documento reúne as citações, por camada e padrão, para justificar tanto a presença quanto a ausência de uma classe.

## A regra geral

> "Nunca aplique uma arquitetura complexa simplesmente por modismo. A arquitetura de software deve se adequar ao tamanho e escopo da equipe e do projeto."
> Arquitetura de Software, módulo 22

O mesmo módulo expõe o trade-off:

- **Mais abstração** = maior flexibilidade e testabilidade, mas "mais classes, interfaces e complexidade de injeção de dependência".
- **Menos abstração** = "rapidez inicial de desenvolvimento (CRUD rápido), porém gera alto acoplamento e impossibilidade de estender recursos sem riscos".

Ou seja, nenhum dos dois lados é grátis. A decisão é por contexto.

> "Abstrações em excesso sem necessidade de alteração ou testes geram um código difícil de rastrear e depurar." (Complexidade Acidental)
> "Não crie interfaces para classes que nunca terão outra implementação ou que não exigem testes automatizados complexos. YAGNI (You Aren't Gonna Need It) deve ser pesado juntamente com SOLID."
> SOLID, módulo 11

Overengineering aparece na própria lista de sintomas de design ruim (SOLID, módulo 1): "Complexidade Desnecessária (Overengineering): código com abstrações inúteis", ao lado de rigidez, fragilidade, imobilidade e viscosidade. O professor trata os dois extremos como defeito.

## Teste de necessidade

Antes de criar uma camada, interface ou padrão, a resposta precisa ser "sim" para pelo menos uma destas perguntas (derivadas dos módulos citados):

1. Existe ou é plausível uma **segunda implementação**? (outro banco, outro provedor, um fake)
2. A abstração é necessária para **testar** sem recurso externo?
3. A dependência é **volátil** (banco, serviço de terceiro, driver) e não estável como `DateTime`?
4. Há **regra de negócio** que seria duplicada ou espalhada sem essa estrutura?
5. A estrutura é **exigida pelo requisito** (ex.: futura API e app mobile, troca de banco)?

Se todas forem "não", o professor recomenda não criar. Para o que vale decidir cedo, a apostila (3.3.8) manda escolher os padrões no início, avaliar alternativas, prototipar, discutir em equipe e **documentar a decisão com justificativa**.

## Por camada

### Domain: entidades com regra

**Justifica quando** a regra de negócio precisa ser protegida e testada sem infraestrutura: "Todas as regras de negócio devem ser testáveis unitariamente" (Arquitetura Limpa, seção 3.1). Entidade com identidade, invariante no construtor e comportamento.

**Sinal de excesso ou de falha:**
- Entidade só com propriedades e regra no Service é o antipadrão **entidade anêmica**: "a lógica de negócio desaparece do domínio e se dispersa pelas camadas externas" (Arquitetura Limpa, 4.1.1; Apostila 3.3.6).
- Getters e setters para tudo "violam o princípio do mínimo privilégio" e geram "código redundante" (Apostila 3.3.6).

### Domain: Value Objects e Domain Services

**Justifica quando** o conceito é definido por valor, imutável e se autovalida (Endereço, CEP, Saldo), ou quando a regra "não pertence naturalmente a uma única entidade", "coordena múltiplas entidades" ou "implementa algoritmos complexos do domínio" (Arquitetura Limpa, 3.1).

**Sinal de excesso:** criar Value Object para tipo primitivo sem regra, ou Domain Service para o que cabe num método da entidade. Condições do professor para Domain Service: sem estado, só quando a regra transita entre entidades.

### Domain: agregados, fábricas, eventos (DDD)

> "Para sistemas simples ou pequenas aplicações, usar DDD pode ser um exagero, mas para grandes sistemas corporativos, DDD pode ajudar."
> Apostila 1.1.8

**Justifica quando** o domínio é sofisticado, com muitos detalhes. **Sinal de excesso:** introduzir fábrica, evento de domínio ou contexto delimitado sem a complexidade que os pede. Adotar o vocabulário de DDD (entidade, VO, raiz de agregado) é barato. Adotar toda a cerimônia só se paga em domínio complexo.

### Application: Service (caso de uso)

**Justifica quando** há um fluxo que coordena mais de uma peça: entidades, repositórios, serviço externo. Ele "sabe em que ordem as coisas devem acontecer" (Arquitetura Limpa, 4.1.2). Também quando a mesma regra precisa atender vários clientes (web, mobile, API), pois o caso de uso "pode ser reutilizado por diferentes interfaces".

**Sinal de excesso:** Service que só repassa chamada ao repositório sem coordenar nada. O próprio slide do professor admite: "Aqui poderia haver lógica adicional, validações, etc." (Apostila 3.5, `ClienteService`). Contrário também vale: Service com regra de negócio complexa invade o Domain, pois a camada "nunca contém regras de negócio complexas" (Arquitetura Limpa, seção 4).

### Application: DTO

**Justifica quando** é preciso controlar o que entra e sai, evitar vazar dado interno e manter o domínio evoluindo sem quebrar o contrato público (quatro razões do professor, Arquitetura Limpa 4.1.1). Serve como "contrato explícito entre camadas" e documenta a API.

**Sinal de excesso:** DTO espelhando a entidade campo a campo e mapeado com ferramenta para cada tela. O professor reconhece que DTO "parece similar" à entidade e ainda assim o defende na fronteira da API. Fora dela (entre Service e Domain) não há motivo para DTO. Também alerta contra usar DTO como substituto permanente da entidade.

### Application: interface de repositório

Ver a seção de Repository abaixo.

### Infrastructure

**Justifica quando** há detalhe tecnológico a isolar: banco, HTTP externo, e-mail. É "a única camada que conhece e depende diretamente de frameworks e bibliotecas externas" (Arquitetura Limpa, seção 5).

**Sinal de excesso:** criar projeto ou pasta de Infra para adaptar algo que não é detalhe externo.

### Api / Controller

**Justifica quando** é preciso traduzir protocolo (HTTP, JSON) para o caso de uso. Deve ser fino: "manter a View o mais simples possível" e não conter regra.

**Sinal de excesso ou falha:** Controller que valida regra, grava no banco ou monta HTML (questão 7 dos exercícios).

## Por padrão

### Repository

> "Evite criar a estrutura de Repository se a aplicação for meramente de leitura e escrita direta (CRUD simples) e o framework utilizado já oferecer uma abstração adequada de repositório."
> "Abstrações artificiais sem regras de domínio adicionam arquivos desnecessários e aumentam a carga cognitiva da equipe sem agregar valor real de testabilidade ou flexibilidade."
> Repository e DAO, módulo 17

**Justifica quando** há regra de domínio para proteger, necessidade de trocar a persistência ou de testar o Service sem banco. O estudo de caso do professor mostra três implementações do mesmo contrato (ADO.NET, EF Core, memória) como prova do valor.

**Sinais de excesso:**
- Repository que só repete CRUD do ORM. O próprio texto: "Repository não significa simplesmente 'uma classe que faz CRUD'".
- Interface que simplesmente reproduz "todas as operações disponíveis no banco de dados ou no ORM" (Implementação do Repository, módulo 10).
- **Generic Repository**: "pode expor operações que não fazem sentido de negócio para certas entidades e enfraquece o conceito de agregação clara" (módulo 18).
- Repositório por tabela em vez de por **raiz de agregado** (módulo 16).

Nota para o FabricX: o `EF Core` já é um repositório/unidade de trabalho. A justificativa do Repository aqui é a troca de banco e o teste de Service com fake, ambos pedidos no plano do projeto. Sem esses dois motivos, o texto do professor desaconselha.

### DAO

Só quando o acesso é fortemente físico e orientado a tabela, sem noção de domínio. No FabricX não há motivo para DAO: o Repository cobre o caso.

### Service Locator

Listado como padrão com desvantagens: acoplamento implícito, complexidade adicional, teste e depuração difíceis (Apostila 3.5.1). Preferir DI por construtor. É um exemplo de padrão que **não** se justifica no projeto.

### Interface por classe (DIP)

Tabela do professor (SOLID, módulo 12), custo e quando evitar:

| Princípio | Custo | Evitar quando |
|---|---|---|
| SRP | Muitas classes pequenas | Lógicas triviais de CRUD |
| OCP | Uso excessivo de interfaces | Comportamentos fixos que não mudarão |
| LSP | Hierarquias complexas | Composição é muito mais simples |
| ISP | Muitas interfaces pequenas | A interface já é altamente coesa |
| DIP | Complexidade no container de DI | Scripts rápidos e protótipos descartáveis |

Regra prática: interface vale para **dependência volátil** (repositório, e-mail, ERP). Para estável (`DateTime`, coleções) é acoplamento aceitável: "Acoplar seu código à classe System.DateTime [...] é seguro" (Acoplamento e Coesão, módulo 18). "Acoplamento zero" nem é objetivo real, pois "inviabilizaria a comunicação entre objetos".

### DRY e KISS

- DRY: "É importante equilibrar o DRY com a simplicidade. Abstrações excessivas ou prematuras podem levar a código complexo e difícil de entender, violando o princípio KISS." Duas duplicações pequenas custam menos que uma abstração errada.
- KISS: "Abstrações complexas podem ser necessárias em projetos grandes, mas devem ser justificadas pelos requisitos."

### Normalização do banco

"Um excesso de normalização pode complicar as consultas e operações" (Apostila 3.3.4). Normalizar até onde a integridade exige.

## O que o professor NÃO diz

Para não superestimar o material:

- Ele não dá um limiar numérico (nº de entidades, nº de telas) para decidir. O critério é qualitativo: requisito, tamanho da equipe, necessidade de teste e de troca.
- Ele ensina a estrutura completa porque a disciplina avalia a **aplicação do conceito**; o aviso de overengineering aparece como contrapeso, em slides próprios. As questões de prova pedem a separação correta, com justificativa técnica.

## Aplicação ao FabricX

Leitura minha do material cruzada com o `docs/02-plano.md`, não citação do professor.

| Estrutura | Justificativa presente no projeto | Estado hoje |
|---|---|---|
| Domain com entidades ricas | Regras de PCP (quantidade > 0, prazo futuro, capacidade) | Em uso (`Produto`, `OrdemProducao`, `Recurso`) |
| Interface de repositório | Troca de banco declarada nas decisões fechadas e teste de Service com fake | Em uso |
| Service por agregado | Fluxos que coordenam entidade e repositório, futura API e mobile | Em uso (`ProdutoService`) |
| DTO | Fronteira da API, não vazar entidade | Em uso |
| `WebResponse<T>` | Contrato único para o cliente (Vue no futuro) | Em uso |
| Value Objects | Sem conceito de valor com regra própria ainda | Pasta vazia, e está certo |
| Domain Services | Sem regra que atravesse entidades ainda | Pasta vazia, e está certo |
| Generic Repository | Desaconselhado pelo professor | Não usar |
| Eventos de domínio, fábricas, contextos delimitados | Domínio ainda não pede | Não criar até haver necessidade |
| Integração ERP/SGQ | Requisito desejável no enunciado | Interface na Application e fake na Infra, só quando entrar |

Gatilho para introduzir cada item ausente: quando surgir uma regra que se encaixe na condição do professor (valor imutável com regra própria para VO, regra que atravessa entidades para Domain Service).
