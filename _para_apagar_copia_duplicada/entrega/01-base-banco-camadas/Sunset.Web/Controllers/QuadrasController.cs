using Microsoft.AspNetCore.Mvc;
using Sunset.Modelos.Dtos;
using Sunset.Negocio;

namespace Sunset.Web.Controllers;

// Controller MVC das quadras: recebe a requisição, chama o Negocio e devolve uma view.
// Nenhuma regra de negócio aqui; elas estão em QuadraNegocio.
public class QuadrasController : Controller
{
    private readonly QuadraNegocio _negocio;

    public QuadrasController(QuadraNegocio negocio)
    {
        _negocio = negocio;
    }

    // GET /Quadras
    public IActionResult Index()
    {
        var quadras = _negocio.Listar().Select(QuadraDto.De).ToList();
        return View(quadras);
    }

    // GET /Quadras/Criar
    public IActionResult Criar() => View(new QuadraSalvarDto());

    // POST /Quadras/Criar
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Criar(QuadraSalvarDto modelo)
    {
        if (!ModelState.IsValid) return View(modelo);

        try
        {
            _negocio.Cadastrar(modelo.ParaQuadra());
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

    // GET /Quadras/Editar/5
    public IActionResult Editar(int id)
    {
        var quadra = _negocio.ObterPorId(id);
        if (quadra is null) return NotFound();

        ViewData["Id"] = id;
        return View(QuadraSalvarDto.De(quadra));
    }

    // POST /Quadras/Editar/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Editar(int id, QuadraSalvarDto modelo)
    {
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
        return View(QuadraDto.De(quadra));
    }

    // POST /Quadras/Excluir/5
    [HttpPost, ActionName("Excluir")]
    [ValidateAntiForgeryToken]
    public IActionResult ConfirmarExclusao(int id)
    {
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
