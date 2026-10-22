# SUNSET - Pasta 1: estrutura em camadas e banco

Primeiro incremento do PIM IV. Já é uma solution completa, que compila e roda sozinha.

## O que foi entregue

- Solution `Sunset.sln` com os 5 projetos do manual (Modelos, Repositorio, Negocio, Web e Api), no sentido de referência certo:
  `Web/Api -> Negocio -> Repositorio -> Modelos`.
- Banco SQL Server criado por migration do Entity Framework Core: 6 tabelas (Usuario, Enderecos, Quadras, Pagamentos, Agendamentos, LogExclusaoQuadras), 3 triggers e a procedure `CadastrarAgendamento`.
- A procedure é chamada pelo `AgendamentoRepositorio` com Dapper (`CommandType.StoredProcedure`).
- `HasTrigger` mapeado no `AppDbContext` para as tabelas Quadras e Usuario.
- Cadastro de quadras (com endereço): telas MVC em `/Quadras` e endpoints `GET /api/quadras`.
- Endpoint `POST /api/agendamentos` que usa a procedure (conflito de horário calculado no banco).
- Injeção de dependência no `Program.cs` da Web e da Api. Connection string só no `appsettings.json`.
- VLibras e visual roxo na Web.

## Como rodar

1. Confira o `Server=` da connection string nos dois `appsettings.json` (Web e Api). Vem como `localhost\SQLEXPRESS`. Se o seu SQL Server for a instância padrão, use `Server=localhost`.
2. Crie o banco (na pasta do `.sln`):
   ```
   dotnet ef database update --project Sunset.Repositorio --startup-project Sunset.Api
   ```
   Se faltar a ferramenta: `dotnet tool install --global dotnet-ef`.
3. (Opcional) No SSMS, rode `banco/dados_exemplo.sql` e depois `banco/testes.sql`.
4. Api: `dotnet run --project Sunset.Api` e abra `http://localhost:5218/scalar`.
5. Web: `dotnet run --project Sunset.Web` e abra `http://localhost:5157`.

Administrador criado pela migration: `admin@sunset.com` / `Admin@1234` (a tela de login chega na Pasta 2).

## Arquivos principais para explicar

- `Sunset.Repositorio/Contexto/AppDbContext.cs`: mapeamento das tabelas, checks e `HasTrigger`.
- `Sunset.Repositorio/Migrations/*_Inicial.cs`: triggers e procedure escritas em SQL dentro da migration.
- `Sunset.Repositorio/AgendamentoRepositorio.cs`: chamada da procedure com Dapper.
- `Sunset.Negocio/QuadraNegocio.cs`: regras das quadras.
- `Sunset.Web/Controllers/QuadrasController.cs` e `Sunset.Api/Controllers/QuadrasController.cs`.

## O que vem depois

Pasta 2: cadastro de usuário, login por e-mail, perfis Admin, Gestor e Cliente.

## Publicação

- Ordem: 1 de 5.
- Commit sugerido: `feat: estrutura em camadas e banco SQL Server`

## Limitações desta pasta

- O código compila e a Web/Api sobem, mas o banco (migration, triggers, procedure) **não foi executado num SQL Server real** por quem montou esta pasta. Rode `banco/testes.sql` e confira o resultado esperado de cada teste.
- O `INJECAO-DE-DEPENDENCIA.md` é o guia do professor e ainda usa `Cliente` nos exemplos.

## Responsividade no celular

Nas telas `Minhas quadras`, a tabela vira uma lista de cartões quando a tela tem menos de 768px (antes ela rolava para o lado e escondia colunas e botões). A regra está no final de `Sunset.Web/wwwroot/css/site.css` (`.tabela-celular`) e cada `<td>` das views tem um `data-label` com o nome da coluna. Em tela larga nada mudou.
