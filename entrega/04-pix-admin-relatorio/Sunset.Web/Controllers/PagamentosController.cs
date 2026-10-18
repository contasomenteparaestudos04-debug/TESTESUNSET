using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sunset.Negocio;

namespace Sunset.Web.Controllers;

// Tela do Pix simulado. O prazo de 5 minutos é conferido no AgendamentoNegocio (servidor);
// o relógio da tela é só visual.
[Authorize]
public class PagamentosController : Controller
{
    private readonly AgendamentoNegocio _negocio;

    public PagamentosController(AgendamentoNegocio negocio)
    {
        _negocio = negocio;
    }

    // GET /Pagamentos/Pix/7
    public IActionResult Pix(int id)
    {
        try
        {
            return View(_negocio.ObterPix(id, User.Id()));
        }
        catch (RegraDeNegocioException ex)
        {
            TempData["Erro"] = ex.Message;
            return RedirectToAction("Minhas", "Reservas");
        }
    }

    // POST /Pagamentos/Simular/7   (botão "Simular pagamento")
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Simular(int id)
    {
        try
        {
            _negocio.SimularPagamento(id, User.Id());
            TempData["Mensagem"] = "Pagamento aprovado! Sua reserva está confirmada.";
        }
        catch (RegraDeNegocioException ex)
        {
            TempData["Erro"] = ex.Message;
        }
        return RedirectToAction("Minhas", "Reservas");
    }
}
