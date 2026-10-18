# SUNSET - Pasta 2: usuários, login e perfis

Segundo incremento do PIM IV. Inclui tudo da Pasta 1 e adiciona cadastro, login por e-mail e controle de acesso por perfil.

## O que foi entregue

- Cadastro de usuário na Web (`/Conta/Cadastro`) e na Api (`POST /api/usuarios`).
- Login por **e-mail** e senha (`/Conta/Login` e `POST /api/usuarios/login`). Mensagem de erro igual para e-mail ou senha errados: "Email ou senha incorretos".
- Perfis **Admin**, **Gestor** e **Cliente**. Todo cadastro novo entra como Cliente. Quem cadastra uma quadra vira Gestor (a trigger `trg_VirarGestor` faz isso no banco). O Admin vem da migration.
- Na tela de cadastro, a opção "Quero cadastrar minha quadra (ser gestor)" leva direto para `/Quadras/Criar` depois do login.
- Senha guardada com SHA256 (`Seguranca.cs`), mínimo de 8 caracteres.
- CPF com validação dos dígitos verificadores (`Validacao.cs`) e e-mail/CPF únicos.
- Login por cookie na Web. Cadastrar quadra exige estar logado; editar e excluir exigem Gestor ou Admin.
- Menu mostra "Entrar / Cadastrar" ou "Olá, nome (perfil) / Sair".

## Como rodar

Igual à Pasta 1 (veja o passo a passo lá: connection string, `dotnet ef database update`, `dotnet run`).

- Web: `dotnet run --project Sunset.Web` -> `http://localhost:5157`
- Api: `dotnet run --project Sunset.Api` -> `http://localhost:5218/scalar`

Contas para testar (depois de rodar `banco/dados_exemplo.sql`): `amanda.r@gmail.com`, `bruno.os@outlook.com` e `mariana.costa@hotmail.com`, todas com a senha `Senha1234`. O administrador é `admin@sunset.com` / `Admin@1234`.

## Arquivos principais para explicar

- `Sunset.Negocio/UsuarioNegocio.cs`: regras do cadastro e do login.
- `Sunset.Negocio/Seguranca.cs` e `Validacao.cs`: hash SHA256 e CPF.
- `Sunset.Repositorio/UsuarioRepositorio.cs`: acesso ao banco.
- `Sunset.Web/Controllers/ContaController.cs`: cookie de login com id, nome e perfil.
- `Sunset.Api/Controllers/UsuariosController.cs`.

## O que vem depois

Pasta 3: agendamento pelo cliente, cancelamento, tela do gestor com os próximos 5 dias.

## Publicação

- Ordem: 2 de 5.
- Commit sugerido: `feat: cadastro, login por e-mail e perfis`

## Limitações desta pasta

- A regra de negócio de usuário foi testada com um teste local sem banco (cadastro, CPF, senha, login). O SQL Server real ainda precisa ser testado pelo grupo.
- A Api não usa token: o login só devolve os dados do usuário. Na Pasta 3 em diante o app envia o id do usuário, então ele pode ser forjado. É uma simplificação assumida para o PIM.
- SHA256 sem sal, como pede a documentação do semestre anterior.
- Editar e excluir quadra ainda não conferem se a quadra é do gestor logado (chega na Pasta 3).
