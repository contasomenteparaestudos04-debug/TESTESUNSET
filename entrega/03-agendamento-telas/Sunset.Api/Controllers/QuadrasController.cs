using Microsoft.AspNetCore.Mvc;
using Sunset.Modelos.Dtos;
using Sunset.Negocio;

namespace Sunset.Api.Controllers;

// Endpoints REST de quadras, consumidos pelo aplicativo Flutter.
// O controller só recebe a requisição, chama o Negocio e escolhe o código HTTP.
[ApiController]
[Route("api/[controller]")]
public class QuadrasController : ControllerBase
{
    private readonly QuadraNegocio _negocio;
    private readonly AgendamentoNegocio _agendamentos;

    // A injeção de dependência entrega os objetos de negócio prontos (ver Program.cs).
    public QuadrasController(QuadraNegocio negocio, AgendamentoNegocio agendamentos)
    {
        _negocio = negocio;
        _agendamentos = agendamentos;
    }

    // GET api/quadras
    [HttpGet]
    public ActionResult<IEnumerable<QuadraDto>> Listar()
    {
        return Ok(_negocio.Listar().Select(QuadraDto.De));
    }

    // GET api/quadras/5
    [HttpGet("{id:int}")]
    public ActionResult<QuadraDto> Obter(int id)
    {
        var quadra = _negocio.ObterPorId(id);
        if (quadra is null) return NotFound();
        return Ok(QuadraDto.De(quadra));
    }

    // GET api/quadras/5/horarios?data=2026-12-20   → horários do dia, livres e ocupados
    [HttpGet("{id:int}/horarios")]
    public ActionResult<QuadraHorariosDto> Horarios(int id, [FromQuery] DateTime? data)
    {
        var dia = (data ?? DateTime.Today).Date;
        var modelo = _agendamentos.HorariosDaQuadra(id, dia);
        if (modelo is null) return NotFound();
        return Ok(modelo);
    }
}
