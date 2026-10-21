using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sunset.Modelos;
using Sunset.Modelos.Dtos;
using Sunset.Negocio;

namespace Sunset.Web.Controllers;

// Controller MVC das quadras: recebe a requisição, chama o Negocio e devolve uma view.
// Nenhuma regra de negócio aqui; elas estão em QuadraNegocio.
[Authorize]
public class QuadrasController : Controller
{
    private readonly QuadraNegocio _negocio;
    private readonly UsuarioNegocio _usuarios;

    public QuadrasController(QuadraNegocio negocio, UsuarioNegocio usuarios)
    {
        _negocio = negocio;
        _usuarios = usuarios;
    }

    // GET /Quadras   -> "Minhas quadras": o gestor vê as dele, o Admin vê todas
    public IActionResult Index()
    {
        var quadras = (User.Perfil() == Perfis.Admin ? _negocio.Listar() : _negocio.ListarDoGestor(User.Id()))
            .Select(QuadraDto.De).ToList();
        return View(quadras);
    }

    // GET /Quadras/Criar
    public IActionResult Criar() => View(new QuadraSalvarDto());

    // POST /Quadras/Criar
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Criar(QuadraSalvarDto modelo)
    {
        if (!ModelState.IsValid) return View(modelo);

        try
        {
            // O usuário logado vira o gestor da quadra (a trigger do banco muda o perfil dele).
            _negocio.Cadastrar(modelo.ParaQuadra(), User.Id());

            // A trigger do banco promoveu o usuário a Gestor: atualiza o cookie com o perfil novo.
            var usuario = _usuarios.ObterPorId(User.Id());
            if (usuario != null) await HttpContext.EntrarNoSistema(usuario);

            TempData["Mensagem"] = "Quadra cadastrada com sucesso.";
            return RedirectToAction(nameof(Index));
        }
        catch (RegraDeNegocioException ex)
        {
            // A mensagem da regra aparece no topo do formulário.
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(modelo);
        }
    }

    // GET /Quadras/Editar/5   (só o gestor dono da quadra ou o Admin)
    public IActionResult Editar(int id)
    {
        var quadra = _negocio.ObterPorId(id);
        if (quadra is null) return NotFound();
        if (!_negocio.PodeGerenciar(quadra, User.Id(), User.Perfil())) return Forbid();

        ViewData["Id"] = id;
        return View(QuadraSalvarDto.De(quadra));
    }

    // POST /Quadras/Editar/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Editar(int id, QuadraSalvarDto modelo)
    {
        var existente = _negocio.ObterPorId(id);
        if (existente is null) return NotFound();
        if (!_negocio.PodeGerenciar(existente, User.Id(), User.Perfil())) return Forbid();

        ViewData["Id"] = id;
        if (!ModelState.IsValid) return View(modelo);

        try
        {
            _negocio.Atualizar(modelo.ParaQuadra(id));
            TempData["Mensagem"] = "Quadra atualizada.";
            return RedirectToAction(nameof(Index));
        }
        catch (RegraDeNegocioException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(modelo);
        }
    }

    // GET /Quadras/Excluir/5  -> tela de confirmação
    public IActionResult Excluir(int id)
    {
        var quadra = _negocio.ObterPorId(id);
        if (quadra is null) return NotFound();
        if (!_negocio.PodeGerenciar(quadra, User.Id(), User.Perfil())) return Forbid();
        return View(QuadraDto.De(quadra));
    }

    // POST /Quadras/Excluir/5
    [HttpPost, ActionName("Excluir")]
    [ValidateAntiForgeryToken]
    public IActionResult ConfirmarExclusao(int id)
    {
        var quadra = _negocio.ObterPorId(id);
        if (quadra is null) return NotFound();
        if (!_negocio.PodeGerenciar(quadra, User.Id(), User.Perfil())) return Forbid();

        try
        {
            _negocio.Remover(id);
            TempData["Mensagem"] = "Quadra excluída.";
        }
        catch (RegraDeNegocioException ex)
        {
            TempData["Erro"] = ex.Message;
        }
        return RedirectToAction(nameof(Index));
    }
}
