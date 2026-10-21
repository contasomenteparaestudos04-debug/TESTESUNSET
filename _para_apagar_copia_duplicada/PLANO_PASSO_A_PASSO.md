# Plano passo a passo – PIM IV SUNSET

Arquivo de acompanhamento para continuar em outra sessão. Criado em 16/10/2026. **Última atualização: 16/10/2026, 21:30 (pausa pedida pelo Patrick).**

## 0. LEIA PRIMEIRO: onde paramos

**Prontas e guardadas em `entrega/` (5 pastas de código):**

| Ordem | Pasta | Conteúdo | Commit sugerido |
|---|---|---|---|
| 1 | `01-base-banco-camadas` | 5 projetos, 6 entidades, migration (tabelas, 3 triggers, procedure), chamada da procedure com Dapper, telas e endpoints de quadras | `feat: estrutura em camadas e banco SQL Server` |
| 2 | `02-usuarios-login` | cadastro, login por e-mail (cookie na Web, endpoint na Api), perfis, CPF com dígitos verificadores, SHA256 | `feat: cadastro, login por e-mail e perfis` |
| 3 | `03-agendamento-telas` | reserva pela procedure, cancelamento, telas do cliente (por esporte, horários, meus agendamentos) e do gestor (agenda de 5 dias), dono da quadra | `feat: agendamento, cancelamento e telas de cliente e gestor` |
| 4 | `04-pix-admin-relatorio` | Pix simulado (QR de exemplo, 5 min conferido no servidor), painel Admin (usuários, agendamentos, exclusão com `ExecutorID`), relatório TXT | `feat: pix simulado, painel do administrador e relatorio txt` |
| 5 | `05-flutter-final` | app Flutter (`sunset_app/`), CORS na Api | `feat: aplicativo flutter, cors na api e documentacao final` |

Cada pasta tem `README.md` próprio (o que entregou, como rodar, arquivos para explicar, o que vem depois, commit e limitações). Ordem de publicação no GitHub: 1, 2, 3, 4, 5.

**Falta fazer:**
1. `DOCUMENTACAO_SUNSET.docx` (ver seção 6). **A documentação antiga (`Documentacao_PIM.docx`) ainda não foi relida nesta sessão para o texto final**: ler de novo (converter com pandoc), manter o que continua valendo e acrescentar o novo.
2. `ROTEIROS_APRESENTACAO.md` com os 6 integrantes (seção 7).
3. Colocar a documentação e os roteiros em `docs/` e citar no README da Pasta 5 (já cita a pasta `docs/`).
4. Verificação final (seção 8): compilar cada pasta de novo, requisitos do PIM, telas x documentação antiga, documento x manual, roteiros x código.
5. Rodar no PC do grupo o que a nuvem não consegue: banco real (ver seção 9).

## 1. Decisões já tomadas pelo grupo

| # | Decisão |
|---|---|
| 1 | Integrantes que apresentam (6): Nicholas Jones Teixeira, Leonardo Manini de Souza, Pedro Geordane Melão Rodrigues, Vinicius Fortunato Freitas, Pedro Henrique de Paiva, Patrick Pelegi Mariano. Marcos e Miguel saem dos roteiros e da capa. |
| 2 | "Usuário" do login é o **e-mail**. |
| 3 | O **cliente cancela** o próprio agendamento (site e app). O horário volta a ficar livre. |
| 4 | Pix **simulado**: QR de exemplo, cronômetro de 5 minutos, botão "Simular pagamento". |
| 5 | A Pasta 1 já nasce com banco e camadas. |
| 6 | Código adaptado ao template novo (`templatePIM`): solution `Sunset.*`, .NET 9, EF Core + Dapper, OpenAPI + Scalar. |
| 7 | **Sem Machine Learning.** Flutter só faz o que o manual pede (consumir a Web API + funcionalidade complementar). |
| 8 | Não há outro modelo de documento: o manual (ABNT, 20–30 páginas de desenvolvimento) é o template. |
| 9 | Trigger de exclusão de usuário: o Repositório define o `ExecutorID` na sessão antes de excluir. `HasTrigger` nas tabelas com trigger. |
| 10 | Nada complicado: nomes simples, comentários em português. |
| 11 | .NET 9. |
| 12 | Assunto de IA/autoria é do grupo: **não escrever nada sobre isso** na documentação nem nos roteiros. |
| 13 | Mantemos o tema **SUNSET**. |

Regra de ouro do `CLAUDE.md`: se a documentação antiga e o código divergirem, a documentação vale; cada divergência vai para a seção "Resumo dos tópicos alterados" da documentação final.

## 2. Estrutura técnica (resumo)

- Solution `Sunset.sln`: `Sunset.Modelos`, `Sunset.Repositorio`, `Sunset.Negocio`, `Sunset.Web` (MVC), `Sunset.Api`. Referências: `Web/Api -> Negocio -> Repositorio -> Modelos`.
- EF Core 9 (SQL Server) para cadastros e consultas; Dapper para a procedure `CadastrarAgendamento` (`CommandType.StoredProcedure`). Connection string só no `appsettings.json` (`Server=localhost\SQLEXPRESS;Database=Sunset;...`).
- Triggers e procedure escritas à mão dentro da migration `Inicial`. Admin inicial: `admin@sunset.com` / `Admin@1234`. Usuários de teste (script `banco/dados_exemplo.sql`): `amanda.r@gmail.com`, `bruno.os@outlook.com`, `mariana.costa@hotmail.com`, senha `Senha1234`.
- Web: login por cookie (claims id, nome, e-mail, perfil), visual roxo, responsivo (Bootstrap), VLibras.
- Api: sem token (limitação assumida). CORS liberado para o app no navegador.
- Flutter: `sunset_app/`, pacote `http`. Telas: login, quadras, horários, Pix, meus agendamentos.

## 3. Divergências / mudanças em relação à documentação antiga (para a seção "Resumo dos tópicos alterados")

- `Usuario.Senha` deixou de ser UNIQUE (hash sem sal revelaria senhas iguais).
- `Pagamentos.ID_usuario` NOT NULL documentado (faltava no dicionário e o INSERT antigo falharia).
- Novo status `Cancelado` em Agendamentos; o conflito de horário ignora cancelados (antes: "falar com um administrador").
- `PrecoHora` e `QuadraId` NOT NULL.
- `trg_ExclusaoSomenteAdmin` não faz mais ROLLBACK: só RAISERROR e RETURN (INSTEAD OF DELETE).
- A procedure agora devolve o id (`SELECT IdAgendamento`) em vez de `PRINT`, usa transação com `UPDLOCK, HOLDLOCK`.
- CPF com validação dos dígitos verificadores (antes só 11 caracteres).
- Textos sem acento antes de gravar (os CHECKs do banco só aceitam letras sem acento, números e espaços).
- O `banco_dados/README.md` do legado estava desatualizado.
- Login: "Usuário" = e-mail.
- Pix simulado; reserva fica Pendente até pagar (5 minutos); paga vira "Em andamento"; "Finalizado" é calculado quando o horário acaba.
- Sem Machine Learning (decisão do grupo); Flutter mínimo.
- Horários fixos de 1 hora, 08h–22h, até 60 dias à frente.

## 4. Passo a passo de execução

1. ~~Instalar dependências~~ (ver seção 9).
2. ~~Pasta 1~~ concluída. 3. ~~Pasta 2~~ concluída. 4. ~~Pasta 3~~ concluída. 5. ~~Pasta 4~~ concluída. 6. ~~Pasta 5 (app Flutter)~~ concluída.
7. **Documentação final e roteiros** (pendente).
8. **Verificação final** (pendente).

## 5. Como foi testado (para a seção de limitações)

- Todas as pastas compilaram (`dotnet build`, 0 erros). Web e Api sobem; páginas protegidas redirecionam para o login; OpenAPI lista todos os endpoints.
- Regras de negócio testadas por um programa de teste local com repositórios falsos (sem banco): cadastro, CPF, senha, login, reserva, conflito, cancelamento, permissões, Pix, expiração de 5 minutos, exclusão pelo Admin, relatório. Tudo passou.
- Flutter: `flutter analyze` sem problemas, `flutter test` com 3 testes passando, e teste no navegador (Chromium) contra uma **Api de mentira** (login, quadras, horários, reserva, Pix, cancelamento). Não foi testado contra a Api real, nem no Android.
- **NÃO testado:** migration, triggers e procedure num SQL Server real (a nuvem não tem SQL Server); exclusão de usuário com `sp_set_session_context`; Android. O grupo deve rodar `banco/testes.sql` e as telas no PC.

## 6. Documentação final (`DOCUMENTACAO_SUNSET.docx`)

- Estrutura do manual: Capa, Resumo, Abstract, Sumário, Introdução, Desenvolvimento (20 a 30 páginas), Conclusão, Referências ABNT. Fonte 12, espaçamento 1,5, margens 3/3/2/2.
- O manual pede as Etapas 1–10 (negócio, engenharia ágil, banco, C#, web responsivo, UX/UI, ML e dados, comunicação/LIBRAS, mobile, integração e avaliação). A Etapa 7 (ML) fica de fora por decisão do grupo (registrar a decisão e o que foi feito de análise de dados: o relatório TXT). Etapa 8: comunicação/liderança/negociação ficam para o grupo escrever; LIBRAS = VLibras no site.
- Manter o que continua valendo da documentação antiga (banco, requisitos, personas, stakeholders, UX). Acrescentar: arquitetura em camadas, diagrama (camadas, banco, Flutter), trechos comentados (repositório, negócio, controller MVC, controller API, chamada da procedure), como instalar e rodar, decisões técnicas.
- Seção **Resumo dos tópicos alterados** (seção 3 deste plano).
- Prints da solution e das telas: o grupo tira; deixar marcado onde entram.
- Não escrever nada sobre IA/autoria.

## 7. Roteiros dos 6 integrantes (`ROTEIROS_APRESENTACAO.md`)

Cada um: tema, tempo de fala (equilibrado), pontos-chave com arquivos e trechos, 3 a 5 perguntas prováveis com respostas curtas, transições. Divisão proposta:

1. Negócio, requisitos e UX
2. Banco de dados (tabelas, triggers, procedure)
3. Camadas, Repositório e injeção de dependência
4. Negócio, login e segurança (SHA256, CPF)
5. Web MVC, telas, agendamento, Pix e responsividade (VLibras)
6. Web API, Flutter, painel Admin, conclusão e limitações

## 8. Verificação (última tarefa)

- Compilar cada pasta (1 a 5) de novo; `flutter analyze` e `flutter test`.
- Requisitos do PIM: 5 projetos, referências no sentido certo, DI no `Program.cs`, consultas parametrizadas, connection string no `appsettings.json`, OpenAPI, Flutter consumindo a Api, `HasTrigger`, procedure chamada pelo sistema.
- Atenção: o manual diz que Web e Api não devem acessar o Repositório diretamente. Hoje os `Program.cs` só registram os repositórios na injeção de dependência (como no template). Conferir se isso está igual ao template e explicar na documentação.
- Telas batem com a documentação antiga; documento segue o manual; roteiros batem com o código.
- Lista honesta do que ficou sem teste (seção 5).

## 9. Ambiente e instalações

- **Nuvem (onde o Claude trabalha):** agora com .NET 9, Flutter 3.47 e NuGet liberados (você liberou os domínios). Sem SQL Server. O ambiente da nuvem some entre sessões: por isso as pastas ficam salvas aqui em `entrega/`. Para continuar, a nova sessão deve copiar `entrega/05-flutter-final` para a nuvem e usá-la como base (`dotnet build`).
- **Seu Windows:** tem Git, .NET SDK 10.0.201 (o projeto usa net9 com `RollForward`), Flutter em `C:\src\flutter` (`flutter doctor`: Chrome OK, sem Android SDK, sem Windows 10 SDK) e `dotnet-ef`.
- **SQL Server Express: ainda NÃO instalado** (o `sqlcmd` não achou `localhost\SQLEXPRESS`). Dê um duplo clique em `instalar_dependencias_2.bat` (baixa e instala como instância `SQLEXPRESS`). Depois: `dotnet ef database update --project Sunset.Repositorio --startup-project Sunset.Api` na pasta da solution, rode `banco/dados_exemplo.sql` e `banco/testes.sql` no SSMS/Azure Data Studio e confira os resultados esperados.
- Para testar o app Flutter no navegador: suba a Api (`dotnet run --project Sunset.Api`, porta 5218), depois `cd sunset_app`, `flutter pub get`, `flutter run -d chrome`.
- Arquivos soltos que podem ser apagados: `_template.tgz`, `instalar_dependencias.ps1/.bat` (versão winget que não funcionou).
- Pasta `ferramentas_teste/`: programa de teste das regras de negócio, Api de mentira e roteiro de teste do Flutter (não fazem parte da entrega; ver `LEIAME.md` dentro).
