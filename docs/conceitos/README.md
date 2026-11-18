# Conceitos da disciplina (Engenharia de Software, Prof. Paulo Dutra)

Destilado dos PDFs e do diagrama da disciplina. Cada arquivo traz a fonte no cabeçalho. Os PDFs originais ficam fora do repositório, em `~/Downloads/_Organizado/Faculdade/eng2`.

| Arquivo | Leia quando | Fontes |
|---|---|---|
| [arquiteturas.md](arquiteturas.md) | Decidir estrutura, ler o diagrama de fluxo, comparar estilos | Arquitetura de Software, Arquitetura Limpa, DIAGRAMA_ARQUITETURA.png, Apostila, exercícios |
| [arquitetura-limpa.md](arquitetura-limpa.md) | Escrever entidade, DTO, Service, repositório, `WebResponse<T>` | Arquitetura Limpa, Apostila 1.1.7 |
| [quando-aplicar-e-overengineering.md](quando-aplicar-e-overengineering.md) | Decidir se uma camada, interface ou padrão se justifica, ou se é excesso | Módulos de trade-offs, YAGNI, Repository módulo 17, DDD, KISS e DRY |
| [solid-acoplamento-coesao.md](solid-acoplamento-coesao.md) | Revisar design, refatorar, justificar uma decisão | SOLID, Acoplamento e Coesão, Exemplo Baixo Acoplamento, exercícios |
| [repositorio-dao-service.md](repositorio-dao-service.md) | Criar contrato e implementação de persistência, decidir o que vai no Service | Repository e DAO, Implementação do Repository, Apostila 3.4 e 3.5 |
| [injecao-de-dependencia.md](injecao-de-dependencia.md) | Registrar serviços, escolher tempo de vida | Injeção de Dependência, SOLID |
| [aspnet-core-ef.md](aspnet-core-ef.md) | Configurar EF Core, solução, projetos, migrations | ASP.NET Core |
| [engenharia-de-software.md](engenharia-de-software.md) | Documentar o projeto, escrever requisitos e regras de negócio | Apostila, exercícios |
| [mapa-conceito-codigo.md](mapa-conceito-codigo.md) | Saber onde cada conceito vive no FabricX e onde o projeto diverge do material | todos |

Ordem sugerida para quem chega agora: `arquiteturas` → `arquitetura-limpa` → `mapa-conceito-codigo`. O resto é consulta.

Convenção: as regras de código do projeto estão no `CLAUDE.md` e na skill `fabricx-arquitetura`. Estes documentos explicam o **porquê** a partir do material do professor e, em caso de conflito, o `CLAUDE.md` vence.
