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
}
