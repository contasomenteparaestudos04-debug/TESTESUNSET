# Resultados dos testes – SUNSET (atualizado em 21/10/2026)

Resumo do que foi testado no computador do grupo (SQL Server 2022 Express, instância SQLEXPRESS).

| Parte | Como foi testado | Resultado |
|---|---|---|
| Migration e banco | `dotnet ef database update` | Criou tabelas, triggers e procedure. Erro de cascata em `Agendamentos` corrigido (FK sem cascata) |
| Banco | `banco/testes.sql` (13 testes) na primeira execução com banco limpo | Resultados esperados conferidos |
| Regras de negócio | Programa de teste local com repositórios de mentira (`ferramentas_teste/`) | Passou |
| Api | Chamadas reais contra o SQL Server (login, quadras, horários, reserva, Pix, cancelamento) | Passou |
| Site | Navegador, perfis Cliente, Gestor e Admin | Passou |
| Relatório do gestor (19/10) | Programa de teste local com repositórios de mentira: só as quadras do gestor, filtro de período, Admin vê todas, gestor sem quadra | Passou (7 testes novos). Compilou sem erros nas Pastas 4 e 5 |
| Relatório TXT na tela (21/10) | Site rodando no GitHub Codespaces (cópia igual à Pasta 5), com SQL Server: pagamento de uma reserva, botão de baixar o relatório e perfis de gestor diferentes | Passou: o relatório atualizou com o pagamento e cada gestor vê só as quadras dele |
| Exclusão de usuário pelo painel Admin (21/10) | Codespaces com SQL Server: excluir usuário com agendamentos e excluir usuário sem vínculos | Passou: o com vínculos foi recusado com mensagem e continuou na lista; o sem vínculos foi excluído |
| App Flutter | Chrome contra a Api real; `flutter analyze` e `flutter test` | Passou; bug de `setState` achado e corrigido |
| Responsividade do site e do app (21/10) | Chrome sem interface em 1280px e 390px, todas as telas do site nos perfis Cliente, Gestor e Admin e o app Flutter em 390px; site e Api rodando com dados em memória (sem SQL Server) | Sem rolagem lateral na página em nenhuma tela. Tabelas ajustadas para cartões no celular e prints refeitos (`prints/`). O banco real não entrou nesse teste |

Não testado: app no Android (sem Android SDK no computador usado; foi testado no navegador).
Observação: rodar `testes.sql` uma segunda vez dá falhas nos testes 1, 2 e 10 porque os dados já existem (esperado). Use banco limpo.
Prints dos testes: o grupo deve colocar na documentação (marcação PRINT 6).
Prints das telas (desktop 1280px, celular 390px e app): pasta `prints/`.
