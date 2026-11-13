# FabricX: instruções do projeto

FabricX é uma API de PCP (Planejamento e Controle da Produção) em C# / .NET 10 com Arquitetura Limpa (Domain, Application, Infrastructure, Api). Banco SQLite. Toda resposta HTTP usa `WebResponse<T>`. Documentação em `docs/`.

## Comentários

- Não escreva comentários por padrão. Nomes claros de classes, métodos e variáveis substituem comentário.
- Nunca comente o que o código já diz (`// incrementa o contador`, `// retorna o produto`).
- Proibido: banners, faixas decorativas (`=====`, `-----`, `#####`), cabeçalhos de seção e comentários de separação.
- Proibido: `/// <summary>` em todo membro. Use só em API pública quando o nome não bastar.
- Comente apenas o porquê não óbvio: uma regra de negócio, uma restrição externa ou uma decisão que surpreenderia o leitor. Uma linha curta.
- Em SQL, no máximo um `--` de uma linha acima de uma constraint não óbvia. Sem cabeçalho de arquivo.
- Ao editar código existente, remova comentários redundantes que encontrar no trecho.

## Arquitetura

- Dependências de fora para dentro: Api → Application → Domain; Infrastructure → Application.
- Domain não referencia nada. Regras de negócio ficam nas entidades, que lançam `DomainException`.
- Application define DTOs, interfaces de repositório e Services (casos de uso). Não conhece HTTP. Nunca expõe entidade na Api.
- Services devolvem DTO ou lançam `DomainException` / `NotFoundException`. Nunca devolvem `WebResponse<T>`.
- Só a Infrastructure conhece EF Core e o provider do banco.
- `WebResponse<T>` fica na Api. Os controllers recebem DTO, chamam o Service e montam o `WebResponse<T>`.
- O `ExceptionHandlingMiddleware` converte exceções em `WebResponse<T>.Failure` (`DomainException` 400, `NotFoundException` 404, demais 500).

## Git

- Nunca adicione `Co-Authored-By: Claude` nem "Generated with Claude Code" em commits, PRs ou issues.
- Mensagens de commit em português, curtas, no imperativo ou descrevendo a mudança.
