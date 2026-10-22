# SUNSET - iniciar o sistema inteiro localmente

Esta pasta serve só para **subir tudo de uma vez** no seu computador, sem precisar seguir o README de cada pasta da `entrega/`. Ela usa a versão final (`entrega/05-flutter-final`), que já tem tudo das Pastas 1 a 4 mais o app Flutter. Não tem código do sistema aqui, só os scripts.

## O que precisa estar instalado

- .NET SDK (o projeto é net9 e roda também com o SDK 10 que já está no computador).
- SQL Server Express com a instância `SQLEXPRESS` (o `instalar_dependencias_2.bat` da pasta principal instala).
- Flutter, só se for abrir o app. Sem ele, o site e a Api sobem normalmente.
- `sqlcmd` (opcional): vem com o SQL Server. Sem ele, os dados de exemplo são pulados.

## Como usar

1. Dê duplo clique em `iniciar_tudo.bat` (ou `iniciar_tudo.bat sem-app` para não abrir o Flutter).
2. O script faz, nesta ordem:
   - confere o .NET e o SQL Server (tenta iniciar o serviço se estiver parado);
   - cria o banco com `dotnet ef database update` (se o banco já existe, não muda nada);
   - insere os dados de exemplo (`banco/dados_exemplo.sql`) só se ainda não existirem;
   - abre três janelas: a Api, o site e o app Flutter no Chrome.
3. Acesse:
   - Site: http://localhost:5157
   - Api (documentação Scalar): http://localhost:5218/scalar
   - App Flutter: abre sozinho no Chrome.
4. Para encerrar, rode `parar_tudo.bat` ou feche as janelas.

## Logins de teste

- Cliente: `bruno.os@outlook.com` / `Senha1234`
- Admin: `admin@sunset.com` / `Admin@1234`

## Se o seu SQL Server tiver outro nome

Abra o `iniciar_tudo.bat` e mude a linha `set SERVIDOR=localhost\SQLEXPRESS` (por exemplo, `set SERVIDOR=localhost` para a instância padrão). O script passa esse valor para a Api, o site e o `dotnet ef` por variável de ambiente, então não precisa mexer nos `appsettings.json`.

## Problemas comuns

- **"dotnet não encontrado"**: feche e abra o terminal depois de instalar, ou rode `instalar_dependencias.bat`.
- **Erro ao criar o banco**: confira se o serviço `SQL Server (SQLEXPRESS)` está rodando e o nome do servidor no script.
- **Porta em uso (5218 ou 5157)**: rode `parar_tudo.bat` e tente de novo.
- **App Flutter não abre**: confira `flutter doctor`. O site e a Api funcionam sem ele.

## Limitações

- Os scripts foram escritos sem poder rodar no Windows do grupo, então o primeiro teste é de vocês. Se algum passo falhar, a mensagem do erro aparece na janela.
- Só o app Flutter no Chrome é aberto; emulador Android não é iniciado (o computador não tem o Android SDK).
- Não é uma das 5 pastas de publicação. Pode ficar fora do GitHub ou ser subida à parte.
