# SUNSET - Pasta 5: aplicativo Flutter e fechamento

Quinto e último incremento do PIM IV. Inclui tudo das Pastas 1 a 4 e adiciona o aplicativo Flutter em `sunset_app/`.

## O que foi entregue

- `sunset_app/`: aplicativo Flutter que **consome a Web API** (login por e-mail, quadras e horários, reserva, Pix simulado e **Meus agendamentos** com cancelamento, que é a funcionalidade complementar). Detalhes no `sunset_app/README.md`.
- CORS liberado na Api (`Sunset.Api/Program.cs`) para o app rodando no navegador chamar a Api.
- Documentação final e roteiros de apresentação (ficam na pasta `docs/` do projeto, fora das pastas de código).

## Como rodar

1. Banco e Api como nas pastas anteriores (`dotnet run --project Sunset.Api`, porta 5218).
2. `cd sunset_app`, `flutter pub get` e `flutter run -d chrome`.
3. Entre com `bruno.os@outlook.com` / `Senha1234`, escolha uma quadra, reserve um horário, pague o Pix simulado e depois cancele em Meus agendamentos.
4. Teste rápido do app sem Api: `flutter analyze` e `flutter test`.

## Arquivos principais para explicar

- `sunset_app/lib/servico_api.dart`: chamadas HTTP para a Api.
- `sunset_app/lib/modelos.dart`: JSON para classes.
- `sunset_app/lib/telas/*.dart`: telas.
- `Sunset.Api/Controllers/*`: endpoints que o app usa.

## Publicação

- Ordem: 5 de 5.
- Commit sugerido: `feat: aplicativo flutter, cors na api e documentacao final`

## Limitações desta pasta

- O app foi testado **no navegador (Chrome)**, como combinado, e com uma Api de mentira que devolve o mesmo JSON da Api real, porque o ambiente de teste não tem SQL Server. Teste o app contra a Api real e o banco antes da apresentação.
- Não foi testado em emulador ou celular Android (o computador do grupo não tem o Android SDK). O `AndroidManifest.xml` já tem a permissão de internet e o acesso HTTP liberado para desenvolvimento.
- O app só tem o perfil de cliente: não cadastra usuário nem tem painel de gestor/Admin (isso fica no site).
- Sem token: o app guarda o id do usuário depois do login (simplificação do PIM).
- O QR Code do app é um ícone; o do site é uma imagem de exemplo. Os dois são simulação.
