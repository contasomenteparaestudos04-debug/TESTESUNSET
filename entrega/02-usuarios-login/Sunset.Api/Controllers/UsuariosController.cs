using Microsoft.AspNetCore.Mvc;
using Sunset.Modelos.Dtos;
using Sunset.Negocio;

namespace Sunset.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsuariosController : ControllerBase
{
    private readonly UsuarioNegocio _negocio;

    public UsuariosController(UsuarioNegocio negocio)
    {
        _negocio = negocio;
    }

    // POST api/usuarios   → cadastro de cliente
    [HttpPost]
    public IActionResult Cadastrar(CadastroDto dto)
    {
        try
        {
            var usuario = _negocio.Cadastrar(dto.Nome, dto.Sobrenome, dto.Email, dto.Cpf, dto.Senha);
            return Created($"api/usuarios/{usuario.Id}", UsuarioDto.De(usuario));
        }
        catch (RegraDeNegocioException ex)
        {
            return BadRequest(new { erro = ex.Message });
        }
    }

    // POST api/usuarios/login   → devolve os dados do usuário (sem senha).
    // Simplificação: não gera token; o app guarda o id. Fica registrado como limitação.
    [HttpPost("login")]
    public IActionResult Login(LoginDto dto)
    {
        try
        {
            var usuario = _negocio.Login(dto.Email, dto.Senha);
            return Ok(UsuarioDto.De(usuario));
        }
        catch (RegraDeNegocioException ex)
        {
            return Unauthorized(new { erro = ex.Message });
        }
    }
}
