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

    // A injeção de dependência entrega o QuadraNegocio pronto (ver Program.cs).
    public QuadrasController(QuadraNegocio negocio)
    {
        _negocio = negocio;
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
}
