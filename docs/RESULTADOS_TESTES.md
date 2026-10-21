# Resultados dos testes – SUNSET (18/10/2026)

Resumo do que foi testado no computador do grupo (SQL Server 2022 Express, instância SQLEXPRESS).

| Parte | Como foi testado | Resultado |
|---|---|---|
| Migration e banco | `dotnet ef database update` | Criou tabelas, triggers e procedure. Erro de cascata em `Agendamentos` corrigido (FK sem cascata) |
| Banco | `banco/testes.sql` (13 testes) na primeira execução com banco limpo | Resultados esperados conferidos |
| Regras de negócio | Programa de teste local com repositórios de mentira (`ferramentas_teste/`) | Passou |
| Api | Chamadas reais contra o SQL Server (login, quadras, horários, reserva, Pix, cancelamento) | Passou |
| Site | Navegador, perfis Cliente, Gestor e Admin | Passou |
| Relatório do gestor (19/10) | Programa de teste local com repositórios de mentira: só as quadras do gestor, filtro de período, Admin vê todas, gestor sem quadra | Passou (7 testes novos). Compilou sem erros nas Pastas 4 e 5 |
| App Flutter | Chrome contra a Api real; `flutter analyze` e `flutter test` | Passou; bug de `setState` achado e corrigido |

Não testado: app no Android; botão "Baixar TXT" (Admin) e o formulário de período do relatório do gestor na tela; exclusão de usuário com agendamentos pela tela do Admin.
Observação: rodar `testes.sql` uma segunda vez dá falhas nos testes 1, 2 e 10 porque os dados já existem (esperado). Use banco limpo.
Prints dos testes: o grupo deve colocar na documentação (marcação PRINT 6).
