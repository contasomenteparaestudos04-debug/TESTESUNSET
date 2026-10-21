using Microsoft.AspNetCore.Mvc;
using Sunset.Modelos.Dtos;
using Sunset.Negocio;

namespace Sunset.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AgendamentosController : ControllerBase
{
    private readonly AgendamentoNegocio _negocio;

    public AgendamentosController(AgendamentoNegocio negocio)
    {
        _negocio = negocio;
    }

    // POST api/agendamentos   → chama a stored procedure CadastrarAgendamento
    // 201 quando dá certo, 400 quando a regra recusa (ex.: horário já ocupado).
    [HttpPost]
    public IActionResult Agendar(AgendamentoSalvarDto dto)
    {
        try
        {
            var id = _negocio.Agendar(dto.DataReserva, dto.HoraInicio, dto.HoraFim,
                                      dto.UsuarioId, dto.QuadraId, dto.PagamentoId);
            return Created($"api/agendamentos/{id}", new { id });
        }
        catch (RegraDeNegocioException ex)
        {
            return BadRequest(new { erro = ex.Message });
        }
    }

    // POST api/agendamentos/reservar   → fluxo do cliente: cria o pagamento pendente e agenda 1 hora
    [HttpPost("reservar")]
    public IActionResult Reservar(ReservaDto dto)
    {
        try
        {
            var reserva = _negocio.Reservar(dto.UsuarioId, dto.QuadraId, dto.Data, dto.HoraInicio);
            return Created($"api/agendamentos/{reserva.AgendamentoId}", reserva);
        }
        catch (RegraDeNegocioException ex)
        {
            return BadRequest(new { erro = ex.Message });
        }
    }

    // GET api/agendamentos?usuarioId=3   → agendamentos do cliente
    [HttpGet]
    public IActionResult Listar([FromQuery] int usuarioId) =>
        Ok(_negocio.MeusAgendamentos(usuarioId));

    // POST api/agendamentos/5/cancelar   → o cliente (ou o gestor da quadra) cancela
    [HttpPost("{id}/cancelar")]
    public IActionResult Cancelar(int id, CancelarDto dto)
    {
        try
        {
            _negocio.Cancelar(id, dto.UsuarioId);
            return NoContent();
        }
        catch (RegraDeNegocioException ex)
        {
            return BadRequest(new { erro = ex.Message });
        }
    }
}
