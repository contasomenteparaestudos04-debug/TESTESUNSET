using Microsoft.AspNetCore.Mvc;
using Sunset.Modelos.Dtos;
using Sunset.Negocio;

namespace Sunset.Api.Controllers;

// Pix simulado pela Api (usado pelo app).
[ApiController]
[Route("api/[controller]")]
public class PagamentosController : ControllerBase
{
    private readonly AgendamentoNegocio _negocio;

    public PagamentosController(AgendamentoNegocio negocio)
    {
        _negocio = negocio;
    }

    // GET api/pagamentos/7/pix?usuarioId=3   → valor, código e segundos que faltam
    [HttpGet("{id:int}/pix")]
    public IActionResult Pix(int id, [FromQuery] int usuarioId)
    {
        try
        {
            return Ok(_negocio.ObterPix(id, usuarioId));
        }
        catch (RegraDeNegocioException ex)
        {
            return NotFound(new { erro = ex.Message });
        }
    }

    // POST api/pagamentos/7/simular   → "Simular pagamento"
    [HttpPost("{id:int}/simular")]
    public IActionResult Simular(int id, CancelarDto dto)
    {
        try
        {
            _negocio.SimularPagamento(id, dto.UsuarioId);
            return NoContent();
        }
        catch (RegraDeNegocioException ex)
        {
            return BadRequest(new { erro = ex.Message });
        }
    }
}
