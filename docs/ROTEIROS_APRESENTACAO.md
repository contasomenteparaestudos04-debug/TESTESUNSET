# Roteiros de apresentação – SUNSET (PIM IV)

Tempo total sugerido: 30 minutos (5 minutos de fala por integrante) + perguntas dos professores. A ordem segue a evolução das 5 pastas do GitHub. Todos devem saber responder sobre o projeto inteiro: leiam também os roteiros dos colegas.

| Ordem | Integrante | Tema | Pasta(s) | Tempo |
|---|---|---|---|---|
| 1 | Nicholas Jones Teixeira | Problema, requisitos e UX | documentação (2.4 e 2.5) | 5 min |
| 2 | Leonardo Manini de Souza | Banco de dados | 01 | 5 min |
| 3 | Pedro Geordane Melão Rodrigues | Camadas, Repositório e injeção de dependência | 01 | 5 min |
| 4 | Vinicius Fortunato Freitas | Negócio, login e segurança | 02 | 5 min |
| 5 | Pedro Henrique de Paiva | Site MVC, agendamento e Pix | 03 e 04 | 5 min |
| 6 | Patrick Pelegi Mariano | Web API, Flutter, painel Admin, limitações | 04 e 05 | 5 min |

Comandos para ter prontos antes de começar (Api na 5218, site na 5157): `dotnet run --project Sunset.Api` e `dotnet run --project Sunset.Web`. Logins: `amanda.r@gmail.com` / `Senha1234` (cliente), `mariana.costa@hotmail.com` / `Senha1234` (gestora), `admin@sunset.com` / `Admin@1234` (admin).

---

## 1. Nicholas – Problema, requisitos e UX (5 min)

**Pontos-chave**
- O problema: pequenos empreendedores de quadras (Beach Tennis e Vôlei) não têm sistema de gestão como os grandes complexos.
- Objetivo do SUNSET: agendar quadra com pagamento rápido e gerenciamento simples.
- Perfis: Cliente, Gestor (dono de quadra) e Admin. Personas e stakeholders estão na documentação (2.5.3.4 e 2.5.3.5).
- Requisitos: funcionais e não funcionais de front, back e banco (2.5.1) e critérios de aceite (2.5.2.2).
- UX: cor roxa, telas simples, responsivo, VLibras para acessibilidade (2.4). Mostrar que as telas novas mantêm campos e nomes da documentação.
- O que mudou no PIM IV: protótipo de console virou site + API + app (seção 2.6.1 e tabela da 2.7).
- Organização do grupo: Kanban e Scrum (2.5.5 e 2.5.6).

**Perguntas prováveis**
1. *Por que SUNSET é diferente dos sistemas existentes?* Foca no pequeno empreendedor, com pagamento por Pix e agenda simples.
2. *Quem usa o sistema?* Cliente reserva, gestor acompanha a agenda das quadras dele, admin cuida de usuários e agendamentos.
3. *Como vocês pensaram na acessibilidade?* VLibras em todas as páginas e tela responsiva.
4. *Por que a cor roxa?* Passa criatividade e tecnologia; está explicado na documentação (2.2.1).
5. *O que mudou em relação ao semestre passado?* Banco de verdade, camadas, site, API e app; regras de negócio mantidas.

**Transição:** "Agora o Leonardo mostra onde ficam guardados esses dados: o banco."

---

## 2. Leonardo – Banco de dados (5 min)

**Arquivos:** `banco/script_banco.sql`, `banco/dados_exemplo.sql`, `banco/testes.sql`, `Sunset.Repositorio/Migrations/..._Inicial.cs`.

**Pontos-chave**
- SQL Server Express. 6 tabelas: Usuario, Enderecos, Quadras, Pagamentos, Agendamentos, LogExclusaoQuadras (dicionário na 2.1.3).
- Restrições: CHECK de CPF (só números e não repetido), e-mail com formato válido, Admin só com e-mail @sunset.com, status permitidos, HoraFim > HoraInicio.
- 3 triggers: `trg_LogExclusaoQuadras` (grava log ao excluir quadra), `trg_VirarGestor` (quem cadastra quadra vira Gestor), `trg_ExclusaoSomenteAdmin` (só Admin exclui usuário; usa `SESSION_CONTEXT('ExecutorID')`).
- Procedure `CadastrarAgendamento`: transação, `UPDLOCK/HOLDLOCK`, confere conflito de horário (ignora cancelados), devolve o id do agendamento, trata erro de vínculo.
- Chaves estrangeiras sem cascata: o SQL Server recusou a cascata em Agendamentos, então ficou Restrict.
- Demonstração: rodar `banco/testes.sql` e mostrar um sucesso, um conflito e a exclusão sem permissão.

**Perguntas prováveis**
1. *Para que serve a procedure se o EF Core já grava dados?* Concentra a conferência de conflito dentro de uma transação, evita reserva dupla, e o PIM pede o uso de procedure com Dapper.
2. *O que acontece se duas pessoas reservam o mesmo horário juntas?* O `UPDLOCK/HOLDLOCK` faz uma esperar a outra; a segunda recebe erro de conflito.
3. *Como a trigger sabe quem está excluindo?* O repositório grava o `ExecutorID` na sessão antes do DELETE e a trigger lê esse valor.
4. *Por que a senha deixou de ser UNIQUE?* Com hash sem sal, uma regra única revelaria senhas iguais de pessoas diferentes.
5. *O que acontece ao excluir um usuário com agendamentos?* O banco recusa (FK sem cascata) e o sistema mostra mensagem simples.

**Transição:** "Para o código falar com esse banco existem as camadas. O Pedro explica."

---

## 3. Pedro Geordane – Camadas, Repositório e injeção de dependência (5 min)

**Arquivos:** `Sunset.sln`, `Sunset.Repositorio/Contexto/AppDbContext.cs`, `AgendamentoRepositorio.cs`, `Interfaces/`, `Program.cs` (Web e Api), `INJECAO-DE-DEPENDENCIA.md`.

**Pontos-chave**
- 5 projetos e o sentido das referências: Web/Api → Negocio → Repositorio → Modelos. Mesmo desenho do template do professor.
- `AppDbContext`: tabelas, CHECKs, chaves, `OnDelete(Restrict)` e `HasTrigger` nas tabelas com trigger.
- Migration `Inicial`: cria tabelas, triggers e procedure; `dotnet ef database update`.
- EF Core para consultas e cadastros; Dapper só em `CadastrarPelaProcedure` (`CommandType.StoredProcedure`, parâmetros separados, trata `SqlException` 50000).
- Injeção de dependência: `AddScoped<Interface, Classe>` no `Program.cs`. Os controllers pedem só classes de Negócio no construtor; o `Program.cs` só registra os repositórios.
- Connection string só no `appsettings.json`.

**Perguntas prováveis**
1. *Por que separar em camadas?* Site e API usam a mesma regra; fica mais fácil testar e trocar uma parte.
2. *O que é injeção de dependência?* O .NET entrega pronta a classe que o construtor pede, sem usar `new` dentro do controller.
3. *Por que usar interface no repositório?* Para o Negócio não depender do banco; nos testes usamos repositórios de mentira.
4. *Por que EF Core e Dapper juntos?* EF é prático no dia a dia; Dapper chama a procedure de forma direta e simples.
5. *Como evitam SQL Injection?* Parâmetros no Dapper e consultas do EF; nunca juntamos texto do usuário no comando.

**Transição:** "Em cima do repositório fica a camada de Negócio. O Vinicius continua."

---

## 4. Vinicius – Negócio, login e segurança (5 min)

**Arquivos:** `Sunset.Negocio/UsuarioNegocio.cs`, `Seguranca.cs`, `Validacao.cs`, `Texto.cs`, `RegraDeNegocioException.cs`, `Sunset.Modelos/Constantes.cs`.

**Pontos-chave**
- Cadastro: nome, sobrenome, e-mail, CPF e senha (mínimo 8). Erros viram `RegraDeNegocioException` com mensagem clara.
- `Validacao.CpfValido`: confere os 2 dígitos verificadores e recusa CPF repetido.
- `Seguranca.GerarHash`: SHA256 em hexadecimal; no login a senha digitada é transformada e comparada com o hash do banco.
- Login pelo e-mail; perfis Admin, Gestor e Cliente (`Constantes.Perfis`).
- Texto sem acento antes de gravar (`Texto.cs`), porque os CHECKs do banco só aceitam letras sem acento.
- Ligação com o semestre passado: classes Seguranca e Validacao do módulo SistemaLogin (2.3.5.6 e 2.3.5.7) foram aproveitadas.

**Perguntas prováveis**
1. *O que é hash e por que não guardar a senha?* Hash é uma conta de um sentido só; se o banco vazar, a senha em si não aparece.
2. *SHA256 é o mais seguro?* Não; faltam sal e custo de processamento (ex.: bcrypt). Mantivemos SHA256 porque está na documentação; é uma limitação que reconhecemos.
3. *Como funciona a validação de CPF?* Calcula os dois dígitos verificadores com pesos decrescentes e compara com os dois últimos números.
4. *Onde ficam as regras: no banco ou no código?* Nos dois: o código dá mensagens amigáveis e o banco é a última barreira com CHECK e triggers.
5. *Como o Admin é criado?* E-mail terminando em @sunset.com (regra do banco); o admin inicial vem na migration.

**Transição:** "Com cadastro e login prontos, o Pedro Henrique mostra o uso no site."

---

## 5. Pedro Henrique – Site MVC, agendamento e Pix (5 min)

**Arquivos:** `Sunset.Web/Controllers/ContaController.cs`, `ReservasController.cs`, `PagamentosController.cs`, `Views/Reservas/Horarios.cshtml`, `Views/Pagamentos/Pix.cshtml`, `Views/Shared/_Layout.cshtml`, `Views/Reservas/Agenda.cshtml`, `Sunset.Negocio/AgendamentoNegocio.cs`.

**Pontos-chave**
- MVC: Controller recebe, chama o Negócio e devolve uma View Razor. Login por cookie; `[Authorize]` e `Roles` por perfil.
- Fluxo do cliente: esporte → quadra → data → horário livre/ocupado → Pix → Meus agendamentos (demonstrar ao vivo).
- `AgendamentoNegocio.Reservar`: valida horário (08–22h, 1h, até 60 dias, não passado), cria pagamento Pendente e chama a procedure.
- Pix simulado: QR de exemplo, cronômetro de 5 minutos conferido no servidor (`ExpirarPagamentosVencidos`), botão "Simular pagamento" muda a reserva para Em andamento.
- Cancelamento pelo cliente libera o horário. Gestor vê a agenda de 5 dias das quadras dele e baixa o relatório TXT delas, escolhendo o período (`ReservasController.Relatorio` e `RelatorioNegocio.GerarDoGestor`).
- Visual: Bootstrap roxo, responsivo (mostrar com a janela estreita), VLibras no canto.

**Perguntas prováveis**
1. *O Pix é real?* Não, é simulado por decisão do grupo; a regra dos 5 minutos é real e conferida no servidor.
2. *Por que conferir o prazo no servidor e não só na tela?* O cronômetro da tela pode ser alterado; o servidor é quem manda.
3. *O que impede um cliente de cancelar a reserva de outro?* O Negócio compara o id do dono ou o gestor da quadra antes de cancelar.
4. *Como garantem que o gestor só baixa o relatório das quadras dele?* A rota `Relatorio` exige perfil Gestor ou Admin e usa o id de quem está logado (guardado no cookie) para buscar só as quadras dele; não existe parâmetro de quadra na URL para trocar.
5. *Por que o relatório é TXT e não PDF?* O requisito RNF02 da documentação pede TXT; o "Exportar PDF" que aparece em um fluxo antigo não foi implementado e está registrado no resumo dos tópicos alterados.
4. *O que é `ValidateAntiForgeryToken`?* Protege os formulários contra envio falso de outro site.
5. *Como o site fica responsivo?* Grid e componentes do Bootstrap e a tag viewport no `_Layout`.

**Transição:** "O mesmo Negócio também atende a API e o app, e fecha com o painel Admin. Patrick."

---

## 6. Patrick – Web API, Flutter, painel Admin e limitações (5 min)

**Arquivos:** `Sunset.Api/Controllers/*.cs`, `Sunset.Api/Program.cs` (OpenAPI/Scalar e CORS), `Sunset.Web/Controllers/AdminController.cs`, `Sunset.Negocio/RelatorioNegocio.cs`, `sunset_app/lib/servico_api.dart` e `lib/telas/`.

**Pontos-chave**
- API REST com os mesmos serviços de Negócio; endpoints de usuários, quadras, agendamentos e pagamentos (tabela na 2.6.7). Mostrar `/scalar` (documentação interativa).
- App Flutter: `ServicoApi` (pacote `http`) e as telas login, quadras, horários, Pix e meus agendamentos. Mostrar rodando contra a Api.
- Painel Admin: usuários, agendamentos com filtro por nome e data, exclusões (só Admin; a trigger confere) e relatório geral em TXT (análise de dados simples). O gestor tem o relatório só das quadras dele (explicado pelo Pedro Henrique).
- Decisões: sem Machine Learning, Pix simulado.
- Limitações honestas: Api sem token (o app manda o id do usuário); cancelamento de reserva paga sem estorno; agenda do gestor mostra como Livre horários de hoje que já passaram; app não testado no Android; `testes.sql` precisa de banco limpo.
- Fechamento: o que o grupo aprendeu e próximos passos (token JWT, Pix real, sal no hash).

**Perguntas prováveis**
1. *Por que a Api não tem token?* Para manter o projeto simples neste semestre; o próximo passo seria JWT.
2. *O que é CORS e por que ligaram?* Permite que o app rodando em outro endereço do navegador chame a Api.
3. *Por que o Flutter usa 10.0.2.2 no Android?* No emulador Android esse endereço aponta para o computador; `localhost` seria o próprio celular.
4. *Por que não fizeram Machine Learning?* Decisão do grupo para entregar o restante bem testado; a análise de dados ficou no relatório TXT.
5. *O que o relatório mostra?* Usuários por perfil, quadras por esporte, agendamentos por status, faturamento dos pagamentos aprovados e lista de agendamentos. O do gestor mostra o mesmo, mas só das quadras dele e no período escolhido.

**Encerramento:** agradecer e abrir para perguntas, passando a palavra a quem for o assunto da pergunta.

---

## Dicas para todos

- Cada um treina o trecho de código que cita; abram o arquivo antes e saibam apontar a linha principal.
- Se a pergunta for de outro tema, responda o básico e passe ao colega com "o Fulano aprofunda isso".
- Se algo falhar na demonstração, diga o que deveria acontecer e siga; não improvise explicações sobre o que não sabe.
