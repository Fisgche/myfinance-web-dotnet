# CLAUDE.md

Este arquivo orienta o Claude Code (claude.ai/code) ao trabalhar neste repositório.

## Visão geral

**myfinance-web-dotnet** é um sistema financeiro web simples em ASP.NET Core MVC (.NET 9) com Entity Framework Core e SQL Server. Permite cadastrar, editar, listar e excluir:

- **Plano de Contas** (`PlanoConta`): categorias com `Descricao` e `Tipo` (`D` = Despesa, `R` = Receita, `I` = Investimento).
- **Transações** (`Transacao`): `Historico`, `Date`, `Valor` e um `PlanoConta` associado.
- **Transações Recorrentes** (`TransacaoRecorrente`): `Valor`, `DataInicio`, `DataFim` e um `PlanoConta` associado.

Código, nomes de classes/métodos, mensagens de commit e UI estão em **português** (sem acentos nos identificadores). Mantenha esse padrão.

## Estrutura da solução

A solução (`myfinance-web-dotnet/myfinance-web-dotnet.sln`) tem 4 projetos, todos `net9.0` com `Nullable` e `ImplicitUsings` habilitados:

| Projeto | Namespace raiz | Papel | Depende de |
|---|---|---|---|
| `myfinance-web-dotnet-domain` | `myfinance_web_dotnet_domain` | Entidades (`Entities/`), base `EntityBase` com `int? Id` (`[Key]`) | — |
| `myfinance-web-dotnet-infra` | `myfinance_web_dotnet_infra` | `MyFinanceDbContext`, repositório genérico `Repository<T>`, repositórios concretos e interfaces | domain, EF Core 9.0.1 (SqlServer) |
| `myfinance-web-dotnet-service` | `myfinance_web_dotnet_service` | Serviços de aplicação e suas interfaces | domain, infra |
| `myfinance-web-dotnet` | `myfinance_web_dotnet` | App web MVC: `Program.cs`, Controllers, Models (view models), Views (Razor), `wwwroot` | service, domain, infra |

Observe que o arquivo `.sln` fica **dentro** da pasta do projeto web, não na raiz.

## Arquitetura e fluxo

```
Controller (MVC) → I*Service → I*Repository → Repository<T> → MyFinanceDbContext → SQL Server
```

- **Domain**: entidades herdam de `EntityBase`. `Transacao` e `TransacaoRecorrente` têm FK `PlanoContaId` + navegação `PlanoConta`.
- **Infra**:
  - `Repository<T>` (abstrato, `where T : EntityBase, new()`) implementa `IRepository<T>` com `Cadastrar`, `Excluir`, `ListarRegistros` (virtual) e `RetornarRegistro`.
  - `Cadastrar` faz *upsert*: `Id == null` → `Add`; caso contrário → `Attach` + `EntityState.Modified`. Sempre chama `SaveChanges()`.
  - `Excluir` cria um stub `new T { Id = id }`, faz `Attach` + `Remove`.
  - `RetornarRegistro` usa `.First()` (lança exceção se não existir).
  - Repositórios concretos sobrescrevem `ListarRegistros` quando precisam de `Include(x => x.PlanoConta)` (ex.: `TransacaoRecorrenteRepository`).
  - `MyFinanceDbContext` recebe `IConfiguration` e configura o provider em `OnConfiguring` usando a connection string `"Database"`.
- **Service**:
  - `IPlanoContaService` e `ITransacaoRecorrenteService` **herdam** das interfaces de repositório correspondentes; os serviços apenas delegam ao repositório.
  - `TransacaoService` é a exceção (legado): acessa `MyFinanceDbContext` diretamente, sem repositório, e `ITransacaoService` declara os métodos explicitamente. Ao refatorar, siga o padrão dos outros (criar `ITransacaoRepository`/`TransacaoRepository` com `Include` em `ListarRegistros`).
- **Web**:
  - Controllers usam **attribute routing** (`[Route("[controller]")]` + `[Route("Index")]`, `[Route("Cadastrar")]`, `[Route("Cadastrar/{Id}")]`, `[Route("Excluir/{Id}")]`).
  - `Cadastrar` GET serve tanto para criação quanto para edição (com `Id`); `Cadastrar` POST faz upsert e redireciona para `Index`. `Excluir` é um **GET**.
  - Controllers mapeiam manualmente entidade ↔ view model (`Models/*Model.cs`); não há AutoMapper.
  - `Index` passa a lista via `ViewBag.Lista*`; as views fazem cast para `List<*Model>`.
  - Formulários de transação usam `SelectList(_planoContaService.ListarRegistros(), "Id", "Descricao")` em `ListPlanoConta` para o dropdown.
  - Botões nas views `Index` navegam via JavaScript inline (`window.location.href = "../<Controller>/Cadastrar/" + id`).
  - Layout em `Views/Shared/_Layout.cshtml` (Bootstrap + jQuery em `wwwroot/lib`, versionados no repo).

### Injeção de dependência (`myfinance-web-dotnet/Program.cs`)

`AddDbContext<MyFinanceDbContext>()` + registros `AddScoped` para cada serviço e repositório. **Ao criar uma nova entidade, registre o serviço e o repositório aqui.**

## Como adicionar uma nova entidade (padrão do projeto)

1. `myfinance-web-dotnet-domain/Entities/<Entidade>.cs` herdando de `EntityBase`.
2. `DbSet<Entidade>` em `MyFinanceDbContext`.
3. `myfinance-web-dotnet-infra/Interfaces/I<Entidade>Repository.cs` (`: IRepository<Entidade>`) e `Repositories/<Entidade>Repository.cs` (`: Repository<Entidade>, I<Entidade>Repository`; sobrescreva `ListarRegistros` se houver navegações).
4. `myfinance-web-dotnet-service/Interfaces/I<Entidade>Service.cs` (`: I<Entidade>Repository`) e `<Entidade>Service.cs` delegando ao repositório.
5. Registrar ambos em `Program.cs`.
6. `Models/<Entidade>Model.cs`, `Controllers/<Entidade>Controller.cs` e views `Views/<Entidade>/Index.cshtml` e `Cadastrar.cshtml`.
7. Adicionar link no menu em `_Layout.cshtml`.
8. Criar a tabela no banco (ver abaixo).

## Banco de dados

- SQL Server. Connection string `ConnectionStrings:Database` em `appsettings.Development.json` (aponta para uma instância SQLEXPRESS local com Trusted_Connection). `appsettings.json` e `appsettings.Production.json` **não** têm connection string — em produção ela deve vir da configuração do ambiente (Azure).
- **Não há migrations do EF Core** no repositório; as tabelas (`PlanoConta`, `Transacao`, `TransacaoRecorrente`, nomes iguais aos `DbSet`) são criadas manualmente no banco. Se alterar entidades, avise que o schema precisa ser atualizado.

## Comandos

Executar a partir da raiz do repositório:

```bash
dotnet build myfinance-web-dotnet/myfinance-web-dotnet.sln     # compilar tudo
dotnet run --project myfinance-web-dotnet/myfinance-web-dotnet.csproj   # rodar (http://localhost:5085, https://localhost:7216)
dotnet watch run --project myfinance-web-dotnet/myfinance-web-dotnet.csproj  # hot reload
dotnet publish myfinance-web-dotnet/myfinance-web-dotnet.csproj
```

Também existem tarefas equivalentes em `.vscode/tasks.json` (`build`, `publish`, `watch`) e configuração de debug em `.vscode/launch.json`.

- **Não há projeto de testes** nem lint configurados.
- Rodar a aplicação exige um SQL Server acessível com o banco `myfinance` já criado.

## Deploy

Perfil de publicação Azure (Zip Deploy) em `myfinance-web-dotnet/Properties/ServiceDependencies/myfinanceweb - Zip Deploy/`.

## Pontos de atenção conhecidos

- Não há validação de modelo (`ModelState`) nem tratamento de erro nos controllers; `RetornarRegistro` com id inexistente lança exceção.
- `Excluir` via GET e sem confirmação.
- `TransacaoRecorrenteController` usa `ILogger<PlanoContaController>` (copiado); `Error()` nos controllers retorna `View("Error!")`, que não existe.
- `TransacaoService` não segue o padrão repositório (ver acima).
- Propriedades `string` não anuláveis nas entidades/models geram avisos de nullable.
