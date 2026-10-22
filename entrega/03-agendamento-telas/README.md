# SUNSET - Pasta 3: agendamento, cancelamento e telas de cliente e gestor

Terceiro incremento do PIM IV. Inclui tudo das Pastas 1 e 2.

## O que foi entregue

**Cliente (Web)**
- `Agendar`: abas por esporte (Vôlei e Beach Tennis), cada quadra com a marca "Livre agora" ou "Ocupada agora".
- `Ver horários`: escolhe o dia e vê a grade de 08h às 22h, de hora em hora (livre, ocupado ou encerrado). Clicar num horário livre reserva 1 hora.
- `Meus agendamentos`: lista os agendamentos com status e botão **Cancelar** (decisão do grupo: o cliente cancela). Só dá para cancelar o que ainda não começou.

**Gestor (Web)**
- `Minhas quadras`: o gestor só vê, edita e exclui as quadras **dele** (Admin vê todas). Tentar abrir a quadra de outro gestor dá "acesso negado".
- `Agenda`: próximos 5 dias de cada quadra, com horários livres/agendados, nome do cliente e botão Cancelar.
- Quando um cliente cadastra a primeira quadra, a trigger do banco o promove a Gestor e o cookie de login é atualizado na hora.

**Regras (AgendamentoNegocio)**
- Reservar cria um pagamento **Pendente** (valor de 1 hora da quadra) e chama a stored procedure `CadastrarAgendamento` (Dapper). Se a procedure recusar (horário ocupado), o pagamento criado é apagado.
- Não aceita horário que já passou, fora de 08h-22h, quebrado (ex.: 18:30) ou com mais de 60 dias de antecedência.
- Cancelar: pode o cliente que reservou, o gestor da quadra e o Admin. O status vira `Cancelado`, o horário volta a ficar livre e o pagamento pendente também é cancelado.
- O pagamento por Pix (QR code e prazo de 5 minutos) chega na Pasta 4.

**Api (Scalar em `/scalar`)**
- `GET /api/quadras/{id}/horarios?data=`
- `POST /api/agendamentos/reservar`
- `GET /api/agendamentos?usuarioId=`
- `POST /api/agendamentos/{id}/cancelar`
- Exemplos prontos em `Sunset.Api/Sunset.Api.http`.

## Como rodar

Igual às pastas anteriores (connection string, `dotnet ef database update`, `dotnet run`).

1. Rode `banco/dados_exemplo.sql` para ter quadras e usuários.
2. Entre com `bruno.os@outlook.com` / `Senha1234`, abra `Agendar`, escolha uma quadra e reserve um horário.
3. Tente reservar o mesmo horário com outra conta: deve aparecer como ocupado.
4. Cancele em `Meus agendamentos` e veja o horário voltar a ficar livre.

## Arquivos principais para explicar

- `Sunset.Negocio/AgendamentoNegocio.cs`: `Reservar`, `Cancelar`, `HorariosDoDia`, `AgendaDoGestor`.
- `Sunset.Repositorio/AgendamentoRepositorio.cs`: procedure com Dapper + consultas com EF Core.
- `Sunset.Repositorio/PagamentoRepositorio.cs`.
- `Sunset.Web/Controllers/ReservasController.cs` e as views em `Views/Reservas`.
- `Sunset.Web/Controllers/QuadrasController.cs`: checagem do dono da quadra (`PodeGerenciar`).
- `Sunset.Modelos/Dtos/AgendamentoDtos.cs`.

## O que vem depois

Pasta 4: pagamento por Pix simulado, painel do administrador e relatório em TXT.

## Publicação

- Ordem: 3 de 5.
- Commit sugerido: `feat: agendamento, cancelamento e telas de cliente e gestor`

## Limitações desta pasta

- As regras (reservar, conflito, cancelar, permissões) foram testadas com um teste local sem banco. A procedure no SQL Server real ainda precisa ser conferida pelo grupo.
- Nesta pasta o pagamento fica sempre `Pendente`; a aprovação só existe a partir da Pasta 4.
- Reserva de 1 hora por vez, em horários cheios (simplificação).
- Devolução de dinheiro de pagamento aprovado não está no escopo.
- A Api não valida token: o `usuarioId` enviado é confiado (simplificação assumida).

## Responsividade no celular

Nas telas `Minhas quadras` e `Meus agendamentos`, a tabela vira uma lista de cartões quando a tela tem menos de 768px (antes ela rolava para o lado e escondia colunas e botões). A regra está no final de `Sunset.Web/wwwroot/css/site.css` (`.tabela-celular`) e cada `<td>` das views tem um `data-label` com o nome da coluna. Em tela larga nada mudou.
