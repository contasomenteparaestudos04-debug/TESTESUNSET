# SUNSET - Pasta 4: pagamento Pix simulado, painel do administrador e relatório

Quarto incremento do PIM IV. Inclui tudo das Pastas 1, 2 e 3.

## O que foi entregue

**Pix simulado**
- Depois de escolher o horário, o cliente cai na tela do Pix (`/Pagamentos/Pix/{id}`): valor, QR Code de exemplo, código "copia e cola" de exemplo e relógio de 5 minutos.
- Botão **Simular pagamento**: o pagamento vira `Aprovado` e a reserva vira `Em andamento` (confirmada). Reserva paga cujo horário já acabou aparece como `Finalizado`.
- O prazo de 5 minutos é conferido **no servidor** (`AgendamentoNegocio.ExpirarPagamentosVencidos`). Passou do prazo sem pagar: pagamento e reserva viram `Cancelado` e o horário volta a ficar livre. O relógio da tela é só visual.
- Em `Meus agendamentos` aparece o botão "Pagar com Pix" enquanto o prazo não acabou.

**Painel do Admin (`/Admin`, só perfil Admin)**
- Usuários: busca por nome ou e-mail e exclusão. A exclusão usa `sp_set_session_context 'ExecutorID'` para a trigger `trg_ExclusaoSomenteAdmin` conferir se quem exclui é Admin (`UsuarioRepositorio.Excluir`).
- Agendamentos: filtro por cliente/quadra e por período, e exclusão (apaga também o pagamento).
- Quadras: o Admin vê e gerencia todas.
- Relatório do Admin: botão que baixa `relatorio-sunset.txt` (usuários por perfil, quadras por esporte, agendamentos por status, faturamento dos pagamentos aprovados e lista de agendamentos). Código em `RelatorioNegocio.Gerar`.

**Relatório do gestor (`/Reservas/Relatorio`, perfil Gestor ou Admin)**
- Na tela `Agenda`, o gestor escolhe um período (De / Até, opcional) e baixa `relatorio-quadras.txt` só com as quadras dele: agendamentos por status, faturamento dos pagamentos aprovados e a lista de agendamentos. Código em `RelatorioNegocio.GerarDoGestor`.
- O relatório usa o id do usuário logado (`ReservasController.Relatorio`), então o gestor não consegue pedir dados de outra quadra pela URL. O Admin que abrir essa rota vê todas as quadras, igual à agenda.
- Vem da documentação antiga: o gestor "gera relatórios" (CA2) e o relatório sai em TXT (RNF02). O "Exportar PDF" do fluxo 2.0.2 não foi feito.

**Api**
- `GET /api/pagamentos/{id}/pix?usuarioId=`
- `POST /api/pagamentos/{id}/simular`
- `POST /api/agendamentos/reservar` agora devolve `agendamentoId` e `pagamentoId`.
- Exemplos em `Sunset.Api/Sunset.Api.http`.

## Como rodar

Igual às pastas anteriores.

1. Entre com `bruno.os@outlook.com` / `Senha1234`, reserve um horário e veja a tela do Pix.
2. Clique em "Simular pagamento" e confira o status em `Meus agendamentos`.
3. Faça outra reserva e espere 5 minutos sem pagar: a página recarrega e a reserva aparece cancelada.
4. Entre com `admin@sunset.com` / `Admin@1234` e abra `Painel Admin`.
5. Entre com `mariana.costa@hotmail.com` / `Senha1234` (gestora), abra `Agenda` e baixe o relatório das quadras dela, com e sem período.

## Arquivos principais para explicar

- `Sunset.Negocio/AgendamentoNegocio.cs`: `ObterPix`, `SimularPagamento`, `ExpirarPagamentosVencidos`, `ExcluirComoAdmin`.
- `Sunset.Repositorio/UsuarioRepositorio.cs`: `Excluir` com o contexto de sessão.
- `Sunset.Negocio/RelatorioNegocio.cs` (`Gerar` e `GerarDoGestor`) e `AgendamentoRepositorio.ListarDasQuadras`.
- `Sunset.Web/Controllers/ReservasController.cs` (`Relatorio`) e `Views/Reservas/Agenda.cshtml` (formulário do período).
- `Sunset.Web/Controllers/PagamentosController.cs` e `Views/Pagamentos/Pix.cshtml` (relógio em JavaScript).
- `Sunset.Web/Controllers/AdminController.cs` e `Views/Admin`.

## O que vem depois

Pasta 5: aplicativo Flutter (consome a Api) e fechamento do projeto.

## Publicação

- Ordem: 4 de 5.
- Commit sugerido: `feat: pix simulado, painel do administrador e relatorio txt`

## Limitações desta pasta

- Não existe cobrança real: o Pix é uma simulação (QR Code e código de exemplo).
- A exclusão de usuário com a trigger e o `sp_set_session_context` **não foi executada num SQL Server real**; as regras de negócio foram testadas com um teste local sem banco. Teste no SSMS/aplicação antes da apresentação.
- Usuário que já tem agendamentos, pagamentos ou quadras não pode ser excluído (o banco recusa por causa das chaves estrangeiras) e o painel mostra a mensagem.
- A expiração acontece quando alguém consulta horários ou reservas, não por um serviço rodando em segundo plano (mais simples).
- Devolução de dinheiro continua fora do escopo.
